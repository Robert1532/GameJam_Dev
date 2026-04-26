using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public class GameManager_Arandia : MonoBehaviour
    {
        [Header("Torretas - Arandia")]
        public List<TurretController_Arandia> turrets = new List<TurretController_Arandia>();

        [Header("Referencia al WaveManager")]
        public WaveManager_Arandia waveManager;

        [Header("UI - Game Over")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverTitle;
        public TextMeshProUGUI gameOverScore;
        public Button retryButton;

        [Header("UI - Victoria")]
        public GameObject victoryPanel;
        public TextMeshProUGUI victoryTitle;
        public TextMeshProUGUI victoryScore;

        // Estado
        [SerializeField] private bool isGameOver = false;
        [SerializeField] private int turretsDestroyed = 0;
        private float survivalTime = 0f;

        public bool IsGameOver => isGameOver;
        public float SurvivalTime => survivalTime;

        void Start()
        {
            foreach (var turret in turrets)
            {
                if (turret != null)
                    turret.OnTurretDestroyed += OnTurretDestroyed;
            }

            if (waveManager != null)
                waveManager.OnGameWon += ShowVictory;

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (retryButton != null)
                retryButton.onClick.AddListener(RetryGame);
        }

        void Update()
        {
            if (isGameOver) return;

            survivalTime += Time.deltaTime;

            // 🔥 NUEVO: chequeo global de componentes en 0
            CheckAllTurretsDead();
        }

        // ──────────────────────────────────────────────
        // TORRETAS DESTRUIDAS (sistema original)
        // ──────────────────────────────────────────────

        private void OnTurretDestroyed(TurretController_Arandia turret)
        {
            if (isGameOver) return;

            turretsDestroyed++;

            Debug.Log($"[Arandia] TORRETA DESTRUIDA: {turret.turretName}. Total: {turretsDestroyed}/4");

            if (turretsDestroyed >= 4)
            {
                GameOver();
            }
        }

        // ──────────────────────────────────────────────
        // 🔥 NUEVO: TODOS LOS COMPONENTES EN 0
        // ──────────────────────────────────────────────

        void CheckAllTurretsDead()
        {
            if (turrets == null || turrets.Count == 0) return;

            int turretsEvaluated = 0;

            foreach (var t in turrets)
            {
                if (t == null) continue;

                // Referencia sin asignar no cuenta como "pieza rota" (evita game over instantáneo).
                if (t.sensor == null || t.canon == null || t.motor == null)
                    continue;

                turretsEvaluated++;

                if (!IsComponentDead(t.sensor)) return;
                if (!IsComponentDead(t.canon)) return;
                if (!IsComponentDead(t.motor)) return;
            }

            if (turretsEvaluated == 0)
                return;

            Debug.Log("[Arandia] Todos los componentes de todas las torretas están en 0");
            GameOver();
        }

        static bool IsComponentDead(TurretComponent_Arandia comp)
        {
            return comp != null && comp.CurrentHP <= 0f;
        }

        // ──────────────────────────────────────────────
        // GAME OVER
        // ──────────────────────────────────────────────

        private void GameOver()
        {
            if (isGameOver) return; // 🔥 evita duplicado

            isGameOver = true;
            Time.timeScale = 0f;

            Debug.Log("[Arandia] ═══ GAME OVER — La Fábrica ha caído ═══");

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverTitle != null)
                    gameOverTitle.text = "LA FÁBRICA HA CAÍDO";

                if (gameOverScore != null)
                {
                    int wave = waveManager != null ? waveManager.CurrentWave : 0;

                    int min = Mathf.FloorToInt(survivalTime / 60f);
                    int sec = Mathf.FloorToInt(survivalTime % 60f);

                    gameOverScore.text =
                        $"Oleada alcanzada: {wave}\nTiempo: {min:00}:{sec:00}";
                }
            }
        }

        // ──────────────────────────────────────────────
        // VICTORIA
        // ──────────────────────────────────────────────

        private void ShowVictory()
        {
            if (isGameOver) return;

            Time.timeScale = 0f;

            Debug.Log("[Arandia] ═══ VICTORIA — LAST MACHINE OPERATIONAL ═══");

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);

                if (victoryTitle != null)
                    victoryTitle.text = "LAST MACHINE OPERATIONAL";

                if (victoryScore != null)
                {
                    int min = Mathf.FloorToInt(survivalTime / 60f);
                    int sec = Mathf.FloorToInt(survivalTime % 60f);

                    victoryScore.text =
                        $"10 oleadas sobrevividas\nTiempo total: {min:00}:{sec:00}";
                }
            }
        }

        // ──────────────────────────────────────────────
        // RETRY
        // ──────────────────────────────────────────────

        public void RetryGame()
        {
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.name))
                SceneManager.LoadScene(scene.name);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}