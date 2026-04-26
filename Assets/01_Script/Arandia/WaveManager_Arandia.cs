using UnityEngine;
using System.Collections;
using TMPro;

namespace LastMachine.Arandia
{
    public class WaveManager_Arandia : MonoBehaviour
    {
        [Header("Configuracion de Oleadas")]
        public int totalWaves = 10;
        public float waveDuration = 45f;
        public float truceDuration = 30f;
        public float waveCountdownTime = 5f;

        [Header("Referencias")]
        public TurretDegradation_Arandia[] turretDegradations;
        public GameManager_Arandia gameManager;

        [Header("Spawner")]
        public EnemySpawner_Arandia enemySpawner;

        [Header("UI")]
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI nextWaveText;
        public TextMeshProUGUI statusText;

        private int currentWave = 0;
        private bool waveActive = false;
        private float waveTimer = 0f;
        private float totalGameTime = 0f;

        public System.Action<int> OnWaveStart;
        public System.Action<int> OnWaveEnd;
        public System.Action OnTruceStart;
        public System.Action OnGameWon;

        public int CurrentWave => currentWave;
        public bool IsWaveActive => waveActive;

        void Start()
        {
            // 🔥 AUTO BUSCAR GAME MANAGER (SOLUCIÓN AL ERROR)
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager_Arandia>();

            StartCoroutine(GameLoop());
        }

        void Update()
        {
            // 🔥 PROTECCIÓN TOTAL
            if (gameManager != null && gameManager.IsGameOver)
                return;

            totalGameTime += Time.deltaTime;
            UpdateTotalTimerUI();
        }

        IEnumerator GameLoop()
        {
            currentWave = 0;

            ShowStatus("Muévete con WASD. Presiona E para reparar.");

            yield return StartCoroutine(CountdownNextWave(waveCountdownTime, 1));

            for (int wave = 1; wave <= totalWaves; wave++)
            {
                if (IsGameOver()) yield break;

                currentWave = wave;

                StartWave(wave);

                waveTimer = 0f;

                while (waveTimer < waveDuration)
                {
                    if (IsGameOver()) yield break;

                    waveTimer += Time.deltaTime;

                    UpdateNextWaveText(waveDuration - waveTimer, "FIN DE OLEADA");

                    yield return null;
                }

                EndWave(wave);

                if (wave >= totalWaves)
                {
                    OnGameWon?.Invoke();

                    ShowStatus("VICTORIA");

                    if (waveText != null)
                        waveText.text = "VICTORIA";

                    if (nextWaveText != null)
                        nextWaveText.text = "";

                    yield break;
                }

                yield return StartCoroutine(TrucePhase(wave + 1));
            }
        }

        // 🔥 MÉTODO CENTRAL (EVITA ERRORES)
        bool IsGameOver()
        {
            return gameManager != null && gameManager.IsGameOver;
        }

        IEnumerator CountdownNextWave(float duration, int nextWave)
        {
            float timer = duration;

            while (timer > 0f)
            {
                UpdateNextWaveText(timer, $"OLEADA {nextWave}");

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        void StartWave(int wave)
        {
            waveActive = true;

            ShowStatus($"OLEADA {wave}");

            if (waveText != null)
                waveText.text = $"OLEADA {wave}";

            foreach (var deg in turretDegradations)
                if (deg != null)
                    deg.StartDegrading(wave);

            if (enemySpawner != null)
                StartCoroutine(enemySpawner.SpawnWave(wave));

            OnWaveStart?.Invoke(wave);
        }

        void EndWave(int wave)
        {
            waveActive = false;

            foreach (var deg in turretDegradations)
                if (deg != null)
                    deg.PauseDegrade();

            OnWaveEnd?.Invoke(wave);
        }

        IEnumerator TrucePhase(int nextWave)
        {
            OnTruceStart?.Invoke();

            float timer = truceDuration;

            while (timer > 0f)
            {
                if (IsGameOver()) yield break;

                UpdateNextWaveText(timer, $"OLEADA {nextWave}");

                ShowStatus($"TREGUA ({Mathf.CeilToInt(timer)}s)");

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        void ShowStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        void UpdateTotalTimerUI()
        {
            if (timerText == null) return;

            int min = Mathf.FloorToInt(totalGameTime / 60f);
            int sec = Mathf.FloorToInt(totalGameTime % 60f);

            timerText.text = $"{min:00}:{sec:00}";
        }

        void UpdateNextWaveText(float time, string label)
        {
            if (nextWaveText == null) return;

            int sec = Mathf.CeilToInt(time);

            nextWaveText.text = $"{label} EN: {sec}s";
        }
    }
}