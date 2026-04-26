using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public class GameManager_Arandia : MonoBehaviour
    {
        [Header("Torretas")]
        public List<TurretController_Arandia> turrets = new List<TurretController_Arandia>();

        [Header("Wave Manager")]
        public WaveManager_Arandia waveManager;

        [Header("UI Game Over")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverTitle;
        public TextMeshProUGUI gameOverScore;
        public Button retryButton;

        [Header("UI Victoria")]
        public GameObject victoryPanel;
        public TextMeshProUGUI victoryTitle;
        public TextMeshProUGUI victoryScore;
        public bool IsGameOver => isGameOver;
        private bool isGameOver = false;
        private float survivalTime = 0f;

        void Start()
        {
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

            CheckAllTurretsDead();
        }

        // 🔥 NUEVA LÓGICA CORRECTA
        void CheckAllTurretsDead()
        {
            if (turrets == null || turrets.Count == 0) return;

            int validTurrets = 0;

            foreach (var t in turrets)
            {
                if (t == null) continue;
                if (t.sensor == null || t.canon == null || t.motor == null) continue;

                validTurrets++;

                if (!IsDead(t.sensor)) return;
                if (!IsDead(t.canon)) return;
                if (!IsDead(t.motor)) return;
            }

            if (validTurrets == 0) return;

            Debug.Log("GAME OVER: todas las torretas destruidas");
            GameOver();
        }

        bool IsDead(TurretComponent_Arandia comp)
        {
            return comp != null && comp.CurrentHP <= 0f;
        }

        // ───────────── GAME OVER ─────────────
        void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 0f;

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
                        $"Oleada: {wave}\nTiempo: {min:00}:{sec:00}";
                }
            }
        }

        // ───────────── VICTORIA ─────────────
        void ShowVictory()
        {
            if (isGameOver) return;

            Time.timeScale = 0f;

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
                        $"10 oleadas sobrevividas\nTiempo: {min:00}:{sec:00}";
                }
            }
        }

        // ───────────── RETRY ─────────────
        public void RetryGame()
        {
            Time.timeScale = 1f;

            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}