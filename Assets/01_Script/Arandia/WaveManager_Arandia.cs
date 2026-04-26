using UnityEngine;
using System.Collections;
using TMPro;

namespace LastMachine.Arandia
{
    public class WaveManager_Arandia : MonoBehaviour
    {
        [Header("Configuracion de Oleadas - Arandia")]
        public int totalWaves = 10;
        public float waveDuration = 45f;
        public float truceDuration = 30f;
        public float waveCountdownTime = 5f;

        [Header("Referencias")]
        public TurretDegradation_Arandia[] turretDegradations;
        public GameManager_Arandia gameManager;

        [Header("Spawner de Enemigos")]
        public EnemySpawner_Arandia enemySpawner;

        [Header("UI")]
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI timerText;        // 🔥 TIEMPO TOTAL
        public TextMeshProUGUI nextWaveText;     // 🔥 NUEVO
        public TextMeshProUGUI statusText;
        public GameObject waveAnnouncementPanel;

        // Estado
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool waveActive = false;
        [SerializeField] private float waveTimer = 0f;

        private float totalGameTime = 0f; // 🔥 NUEVO

        // Eventos
        public System.Action<int> OnWaveStart;
        public System.Action<int> OnWaveEnd;
        public System.Action OnTruceStart;
        public System.Action OnGameWon;

        public int CurrentWave => currentWave;
        public bool IsWaveActive => waveActive;

        void Start()
        {
            StartCoroutine(GameLoop());
        }

        void Update()
        {
            // 🔥 TIEMPO TOTAL SIEMPRE CORRIENDO
            if (gameManager != null && gameManager.IsGameOver) return;

            totalGameTime += Time.deltaTime;
            UpdateTotalTimerUI();
        }

        private IEnumerator GameLoop()
        {
            currentWave = 0;

            ShowStatus(
                "La Fábrica necesita un mecánico.\n" +
                "Muévete con WASD. Presiona E para reparar."
            );

            // 🔥 Cuenta regresiva inicial
            yield return StartCoroutine(CountdownNextWave(waveCountdownTime, 1));

            for (int wave = 1; wave <= totalWaves; wave++)
            {
                if (gameManager != null && gameManager.IsGameOver)
                    yield break;

                currentWave = wave;

                StartWave(wave);

                waveTimer = 0f;

                while (waveTimer < waveDuration)
                {
                    if (gameManager != null && gameManager.IsGameOver)
                        yield break;

                    waveTimer += Time.deltaTime;

                    UpdateNextWaveText(waveDuration - waveTimer, "FIN DE OLEADA");

                    yield return null;
                }

                EndWave(wave);

                if (wave >= totalWaves)
                {
                    OnGameWon?.Invoke();

                    ShowStatus("LAST MACHINE OPERATIONAL\n¡Has sobrevivido!");

                    if (waveText != null)
                        waveText.text = "VICTORIA";

                    if (nextWaveText != null)
                        nextWaveText.text = "";

                    yield break;
                }

                yield return StartCoroutine(TrucePhase(wave + 1));
            }
        }

        // ==================================================
        // 🔥 CUENTA REGRESIVA A SIGUIENTE OLEADA
        // ==================================================

        private IEnumerator CountdownNextWave(float duration, int nextWave)
        {
            float timer = duration;

            while (timer > 0f)
            {
                UpdateNextWaveText(timer, $"OLEADA {nextWave}");

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        // ==================================================
        // FASES
        // ==================================================

        private void StartWave(int wave)
        {
            waveActive = true;

            ShowStatus($"OLEADA {wave} — ¡Defiende!");

            if (waveText != null)
                waveText.text = $"OLEADA {wave}";

            foreach (var deg in turretDegradations)
                if (deg != null)
                    deg.StartDegrading(wave);

            if (enemySpawner != null)
                StartCoroutine(enemySpawner.SpawnWave(wave));

            OnWaveStart?.Invoke(wave);

            Debug.Log($"[Arandia] === OLEADA {wave} INICIADA ===");
        }

        private void EndWave(int wave)
        {
            waveActive = false;

            foreach (var deg in turretDegradations)
                if (deg != null)
                    deg.PauseDegrade();

            OnWaveEnd?.Invoke(wave);

            Debug.Log($"[Arandia] === OLEADA {wave} COMPLETADA ===");
        }

        private IEnumerator TrucePhase(int nextWave)
        {
            OnTruceStart?.Invoke();

            float timer = truceDuration;

            while (timer > 0f)
            {
                if (gameManager != null && gameManager.IsGameOver)
                    yield break;

                UpdateNextWaveText(timer, $"OLEADA {nextWave}");

                ShowStatus($"TREGUA — Repara ({Mathf.CeilToInt(timer)}s)");

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        // ==================================================
        // UI HELPERS
        // ==================================================

        private void ShowStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        private void UpdateTotalTimerUI()
        {
            if (timerText == null) return;

            int min = Mathf.FloorToInt(totalGameTime / 60f);
            int sec = Mathf.FloorToInt(totalGameTime % 60f);

            timerText.text = $"{min:00}:{sec:00}";
        }

        private void UpdateNextWaveText(float time, string label)
        {
            if (nextWaveText == null) return;

            int sec = Mathf.CeilToInt(time);

            nextWaveText.text = $"{label} EN: {sec}s";
        }
    }
}