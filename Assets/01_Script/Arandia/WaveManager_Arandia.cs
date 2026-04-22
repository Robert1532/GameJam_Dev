// WaveManager_Arandia.cs
// Responsable: Arandia
// Descripcion: Maneja oleadas de enemigos. Controla la degradacion de las torretas,
//              tiempos de tregua, y escala la dificultad por oleada.
//              Coloca en un Empty "GameSystems" en la escena.

using UnityEngine;
using System.Collections;
using TMPro;

namespace LastMachine.Arandia
{
    public class WaveManager_Arandia : MonoBehaviour
    {
        [Header("Configuracion de Oleadas - Arandia")]
        public int totalWaves = 10;              // 10 oleadas = victoria
        public float waveDuration = 45f;         // segundos por oleada
        public float truceDuration = 30f;        // segundos de tregua entre oleadas
        public float waveCountdownTime = 5f;     // cuenta regresiva antes de oleada

        [Header("Referencias")]
        public TurretDegradation_Arandia[] turretDegradations; // 4 torretas
        public GameManager_Arandia gameManager;

        [Header("UI (opcional)")]
        public TextMeshProUGUI waveText;         // "OLEADA 3"
        public TextMeshProUGUI timerText;        // "00:32"
        public TextMeshProUGUI statusText;       // "TREGUA - Repara tus torretas"
        public GameObject waveAnnouncementPanel; // Panel que aparece 5 seg

        // Estado
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool waveActive = false;
        [SerializeField] private float waveTimer = 0f;

        // Eventos — otros sistemas pueden escuchar
        public System.Action<int> OnWaveStart;     // oleada numero
        public System.Action<int> OnWaveEnd;       // oleada numero
        public System.Action OnTruceStart;
        public System.Action OnGameWon;

        public int CurrentWave => currentWave;
        public bool IsWaveActive => waveActive;
        public float WaveTimeRemaining => Mathf.Max(0f, waveDuration - waveTimer);

        void Start()
        {
            // Empezar con oleada 0 (tutorial)
            StartCoroutine(GameLoop());
        }

        private IEnumerator GameLoop()
        {
            // ─── Oleada 0: Tutorial (60 seg de gracia) ───
            currentWave = 0;
            ShowStatus("La Fábrica necesita un mecánico.\nMuévete con WASD. Acércate a una torreta y presiona E para reparar.");
            yield return new WaitForSeconds(10f);

            // ─── Loop principal ───
            for (int wave = 1; wave <= totalWaves; wave++)
            {
                if (gameManager != null && gameManager.IsGameOver) yield break;

                currentWave = wave;

                // Cuenta regresiva
                yield return StartCoroutine(WaveCountdown(wave));

                // Iniciar oleada
                StartWave(wave);

                // Esperar duración de oleada
                waveTimer = 0f;
                while (waveTimer < waveDuration)
                {
                    if (gameManager != null && gameManager.IsGameOver) yield break;
                    waveTimer += Time.deltaTime;
                    UpdateTimerUI();
                    yield return null;
                }

                // Fin de oleada
                EndWave(wave);

                // ¿Última oleada sobrevivida?
                if (wave >= totalWaves)
                {
                    OnGameWon?.Invoke();
                    ShowStatus("LAST MACHINE OPERATIONAL\n¡Has sobrevivido!");
                    if (waveText != null) waveText.text = "VICTORIA";
                    yield break;
                }

                // Tregua
                yield return StartCoroutine(TrucePhase());
            }
        }

        // ──────────────────────────────────────────────
        //  Fases
        // ──────────────────────────────────────────────

        private IEnumerator WaveCountdown(int wave)
        {
            if (waveAnnouncementPanel != null)
                waveAnnouncementPanel.SetActive(true);

            for (int i = (int)waveCountdownTime; i > 0; i--)
            {
                ShowStatus($"OLEADA {wave} en {i}...");
                if (waveText != null) waveText.text = $"OLEADA {wave}";
                yield return new WaitForSeconds(1f);
            }

            if (waveAnnouncementPanel != null)
                waveAnnouncementPanel.SetActive(false);
        }

        private void StartWave(int wave)
        {
            waveActive = true;
            ShowStatus($"OLEADA {wave} — ¡Defiende las torretas!");

            // Activar degradación en todas las torretas
            foreach (var deg in turretDegradations)
            {
                if (deg != null)
                    deg.StartDegrading(wave);
            }

            OnWaveStart?.Invoke(wave);
            Debug.Log($"[Arandia] === OLEADA {wave} INICIADA ===");
        }

        private void EndWave(int wave)
        {
            waveActive = false;

            // Pausar degradación
            foreach (var deg in turretDegradations)
            {
                if (deg != null)
                    deg.PauseDegrade();
            }

            OnWaveEnd?.Invoke(wave);
            Debug.Log($"[Arandia] === OLEADA {wave} COMPLETADA ===");
        }

        private IEnumerator TrucePhase()
        {
            OnTruceStart?.Invoke();
            float truceTimer = truceDuration;

            while (truceTimer > 0f)
            {
                truceTimer -= Time.deltaTime;
                ShowStatus($"TREGUA — Repara tus torretas ({Mathf.CeilToInt(truceTimer)}s)");
                yield return null;
            }
        }

        // ──────────────────────────────────────────────
        //  UI helpers
        // ──────────────────────────────────────────────

        private void ShowStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        private void UpdateTimerUI()
        {
            if (timerText == null) return;
            float remaining = WaveTimeRemaining;
            int min = Mathf.FloorToInt(remaining / 60f);
            int sec = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{min:00}:{sec:00}";
        }
    }
}
