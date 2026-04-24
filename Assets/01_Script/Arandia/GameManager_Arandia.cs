// GameManager_Arandia.cs
// Responsable: Arandia
// Descripcion: Maneja el estado general del juego: detecta Game Over cuando las 4
//              torretas caen, muestra pantallas de derrota/victoria, y lleva el score.
//              Coloca en un Empty "GameSystems" en la escena.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace LastMachine.Arandia
{
    public class GameManager_Arandia : MonoBehaviour
    {
        [Header("Torretas - Arandia")]
        public TurretController_Arandia[] turrets;  // Las 4 torretas

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
            // Suscribirse a destrucción de torretas
            foreach (var turret in turrets)
            {
                if (turret != null)
                    turret.OnTurretDestroyed += OnTurretDestroyed;
            }

            // Suscribirse a victoria
            if (waveManager != null)
                waveManager.OnGameWon += ShowVictory;

            // Esconder paneles
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Botón retry
            if (retryButton != null)
                retryButton.onClick.AddListener(RetryGame);
        }

        void Update()
        {
            if (!isGameOver)
                survivalTime += Time.deltaTime;
        }

        // ──────────────────────────────────────────────
        //  Callback: una torreta cayó
        // ──────────────────────────────────────────────

        private void OnTurretDestroyed(TurretController_Arandia turret)
        {
            turretsDestroyed++;
            Debug.Log($"[Arandia] TORRETA DESTRUIDA: {turret.turretName}. Total caídas: {turretsDestroyed}/4");

            // Game Over: las 4 fueron destruidas
            if (turretsDestroyed >= 4)
            {
                GameOver();
            }
        }

        // ──────────────────────────────────────────────
        //  Game Over
        // ──────────────────────────────────────────────

        private void GameOver()
        {
            isGameOver = true;
            Time.timeScale = 0f; // Pausar el juego

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
                    gameOverScore.text = $"Oleada alcanzada: {wave}\nTiempo: {min:00}:{sec:00}";
                }
            }
        }

        // ──────────────────────────────────────────────
        //  Victoria
        // ──────────────────────────────────────────────

        private void ShowVictory()
        {
            if (isGameOver) return; // No mostrar victoria si ya es game over

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
                    victoryScore.text = $"10 oleadas sobrevividas\nTiempo total: {min:00}:{sec:00}";
                }
            }
        }

        // ──────────────────────────────────────────────
        //  Retry
        // ──────────────────────────────────────────────

        public void RetryGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
