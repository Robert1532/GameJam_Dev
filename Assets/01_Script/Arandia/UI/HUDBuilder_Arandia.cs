// HUDBuilder_Arandia.cs
// Responsable: Arandia
// Descripcion: Genera TODO el HUD automáticamente por código.
//              Solo agrega este script a un Empty "HUD_Manager" en la escena.
//              No necesitas crear NADA en el Canvas manualmente.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public class HUDBuilder_Arandia : MonoBehaviour
    {
        [Header("Referencias - Arandia")]
        [Tooltip("Arrastra las 4 torretas aquí")]
        public TurretController_Arandia[] turrets;

        [Tooltip("Arrastra el Player aquí")]
        public RepairSystem_Arandia repairSystem;

        [Tooltip("Arrastra el WaveManager aquí (opcional)")]
        public WaveManager_Arandia waveManager;

        [Tooltip("Arrastra el GameManager aquí (opcional)")]
        public GameManager_Arandia gameManager;

        // Colores del tema LAST MACHINE
        private Color bgDark      = new Color(0.08f, 0.08f, 0.10f, 0.92f);
        private Color bgPanel     = new Color(0.12f, 0.12f, 0.15f, 0.90f);
        private Color greenOK     = new Color(0.114f, 0.851f, 0.459f);
        private Color orangeWarn  = new Color(0.729f, 0.459f, 0.090f);
        private Color redBad      = new Color(0.890f, 0.141f, 0.290f);
        private Color textWhite   = new Color(0.9f, 0.9f, 0.92f);
        private Color textGray    = new Color(0.6f, 0.6f, 0.65f);

        // Referencias generadas
        private Canvas canvas;
        private GameObject turretPanel;
        private TextMeshProUGUI turretNameLabel;
        private TextMeshProUGUI turretStatusLabel;
        private Image[] compBars = new Image[3];
        private TextMeshProUGUI[] compLabels = new TextMeshProUGUI[3];
        private TextMeshProUGUI[] compPercents = new TextMeshProUGUI[3];
        private GameObject repairBarGroup;
        private Image repairProgressBar;
        private TextMeshProUGUI repairProgressLabel;
        private TextMeshProUGUI promptLabel;

        // Wave UI
        private TextMeshProUGUI waveLabel;
        private TextMeshProUGUI timerLabel;
        private TextMeshProUGUI statusLabel;

        // Pieces UI
        private TextMeshProUGUI piecesLabel;

        // Game Over / Victory
        private GameObject gameOverPanel;
        private TextMeshProUGUI gameOverTitle;
        private TextMeshProUGUI gameOverScore;
        private Button retryBtn;
        private GameObject victoryPanel;
        private TextMeshProUGUI victoryTitle;
        private TextMeshProUGUI victoryScore;

        // Estado
        private TurretController_Arandia activeTurret;
        private PieceInventory_Arandia inventory;
        private bool isShowingTurretHUD = false;

        void Start()
        {
            BuildCanvas();
            BuildTurretPanel();
            BuildWaveInfoPanel();
            BuildPiecesCounter();
            BuildGameOverPanel();
            BuildVictoryPanel();

            // Ocultar paneles iniciales
            turretPanel.SetActive(false);
            repairBarGroup.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);

            // Suscribir eventos de torretas
            foreach (var t in turrets)
            {
                if (t == null) continue;
                t.OnPlayerEnterRange += OnEnterTurret;
                t.OnPlayerExitRange += OnExitTurret;
                t.OnTurretDestroyed += OnTurretDestroyed;
            }

            // Suscribir eventos de reparación
            if (repairSystem != null)
            {
                repairSystem.OnRepairStarted += OnRepairStarted;
                repairSystem.OnRepairProgress += OnRepairProgress;
                repairSystem.OnRepairComplete += (c, a) => OnRepairEnded();
                repairSystem.OnRepairCancelled += OnRepairEnded;
                inventory = repairSystem.GetComponent<PieceInventory_Arandia>();
            }

            // Suscribir wave manager
            if (waveManager != null)
            {
                waveManager.OnGameWon += ShowVictory;
            }

            // Suscribir game manager (para game over con UI)
            if (gameManager != null)
            {
                // Sobreescribir el panel de GameManager con el nuestro
                gameManager.gameOverPanel = gameOverPanel;
                gameManager.gameOverTitle = gameOverTitle;
                gameManager.gameOverScore = gameOverScore;
                gameManager.retryButton = retryBtn;
                gameManager.victoryPanel = victoryPanel;
                gameManager.victoryTitle = victoryTitle;
                gameManager.victoryScore = victoryScore;
            }

            // Wave manager UI
            if (waveManager != null)
            {
                waveManager.waveText = waveLabel;
                waveManager.timerText = timerLabel;
                waveManager.statusText = statusLabel;
            }
        }

        void Update()
        {
            if (isShowingTurretHUD && activeTurret != null)
                RefreshTurretPanel();

            // Piezas
            if (inventory != null && piecesLabel != null)
            {
                piecesLabel.text = $"Piezas: {inventory.CurrentPieces}";
                piecesLabel.color = inventory.CurrentPieces <= 2 ? redBad : textWhite;
            }
        }

        // ══════════════════════════════════════════════
        //  CONSTRUIR UI
        // ══════════════════════════════════════════════

        private void BuildCanvas()
        {
            GameObject canvasGO = new GameObject("HUD_Canvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void BuildTurretPanel()
        {
            // Panel principal — esquina inferior izquierda
            turretPanel = CreatePanel(canvas.transform, "TurretPanel",
                new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(20, 20), new Vector2(360, 280), bgDark);

            float y = -15;

            // Nombre de torreta
            turretNameLabel = CreateText(turretPanel.transform, "TurretName",
                new Vector2(15, y), new Vector2(250, 30), "TORRETA NORTE", 18, textWhite, FontStyles.Bold);
            y -= 22;

            // Status
            turretStatusLabel = CreateText(turretPanel.transform, "TurretStatus",
                new Vector2(15, y), new Vector2(250, 22), "en rango", 13, greenOK, FontStyles.Normal);
            y -= 30;

            // Barras de componentes
            string[] names = { "Sensor", "Cañón", "Motor" };
            Color[] barColors = { new Color(0.2f, 0.6f, 1f), orangeWarn, greenOK };

            for (int i = 0; i < 3; i++)
            {
                // Nombre del componente
                compLabels[i] = CreateText(turretPanel.transform, $"CompLabel_{i}",
                    new Vector2(15, y), new Vector2(80, 20), names[i], 12, textGray, FontStyles.Normal);

                // Porcentaje
                compPercents[i] = CreateText(turretPanel.transform, $"CompPercent_{i}",
                    new Vector2(280, y), new Vector2(60, 20), "100%", 12, textWhite, FontStyles.Bold);

                y -= 18;

                // Barra background
                Image barBg = CreateImage(turretPanel.transform, $"BarBg_{i}",
                    new Vector2(15, y), new Vector2(310, 12), new Color(0.2f, 0.2f, 0.22f));

                // Barra fill
                compBars[i] = CreateImage(barBg.transform, $"BarFill_{i}",
                    Vector2.zero, Vector2.zero, barColors[i]);
                compBars[i].rectTransform.anchorMin = Vector2.zero;
                compBars[i].rectTransform.anchorMax = Vector2.one;
                compBars[i].rectTransform.offsetMin = Vector2.zero;
                compBars[i].rectTransform.offsetMax = Vector2.zero;
                compBars[i].type = Image.Type.Filled;
                compBars[i].fillMethod = Image.FillMethod.Horizontal;
                compBars[i].fillAmount = 1f;

                y -= 22;
            }

            y -= 5;

            // Prompt de reparación
            promptLabel = CreateText(turretPanel.transform, "RepairPrompt",
                new Vector2(15, y), new Vector2(330, 22),
                "E+1 Sensor   E+2 Cañón   E+3 Motor", 11, textGray, FontStyles.Italic);

            y -= 28;

            // Barra de progreso de reparación
            repairBarGroup = new GameObject("RepairBarGroup");
            repairBarGroup.transform.SetParent(turretPanel.transform, false);

            CreateText(repairBarGroup.transform, "RepairLabel",
                new Vector2(15, y), new Vector2(200, 18), "Reparando...", 11, orangeWarn, FontStyles.Normal);

            y -= 18;

            Image repairBg = CreateImage(repairBarGroup.transform, "RepairBarBg",
                new Vector2(15, y), new Vector2(310, 10), new Color(0.2f, 0.2f, 0.22f));

            repairProgressBar = CreateImage(repairBg.transform, "RepairBarFill",
                Vector2.zero, Vector2.zero, orangeWarn);
            repairProgressBar.rectTransform.anchorMin = Vector2.zero;
            repairProgressBar.rectTransform.anchorMax = Vector2.one;
            repairProgressBar.rectTransform.offsetMin = Vector2.zero;
            repairProgressBar.rectTransform.offsetMax = Vector2.zero;
            repairProgressBar.type = Image.Type.Filled;
            repairProgressBar.fillMethod = Image.FillMethod.Horizontal;
            repairProgressBar.fillAmount = 0f;

            repairProgressLabel = repairBarGroup.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildWaveInfoPanel()
        {
            // Panel esquina superior derecha
            GameObject wavePanel = CreatePanel(canvas.transform, "WavePanel",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-320, -20), new Vector2(300, 90), bgDark);

            waveLabel = CreateText(wavePanel.transform, "WaveLabel",
                new Vector2(15, -10), new Vector2(270, 30), "OLEADA 1", 22, textWhite, FontStyles.Bold);

            timerLabel = CreateText(wavePanel.transform, "TimerLabel",
                new Vector2(200, -10), new Vector2(80, 30), "00:45", 22, orangeWarn, FontStyles.Bold);

            statusLabel = CreateText(wavePanel.transform, "StatusLabel",
                new Vector2(15, -45), new Vector2(270, 25), "Preparando...", 13, textGray, FontStyles.Italic);
        }

        private void BuildPiecesCounter()
        {
            // Esquina superior izquierda
            GameObject piecesPanel = CreatePanel(canvas.transform, "PiecesPanel",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -20), new Vector2(160, 50), bgDark);

            piecesLabel = CreateText(piecesPanel.transform, "PiecesLabel",
                new Vector2(15, -10), new Vector2(130, 30), "Piezas: 5", 20, textWhite, FontStyles.Bold);
        }

        private void BuildGameOverPanel()
        {
            // Panel central — Game Over
            gameOverPanel = CreatePanel(canvas.transform, "GameOverPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-250, -150), new Vector2(500, 300),
                new Color(0.15f, 0.02f, 0.02f, 0.95f));

            gameOverTitle = CreateText(gameOverPanel.transform, "GOTitle",
                new Vector2(20, -30), new Vector2(460, 50),
                "LA FÁBRICA HA CAÍDO", 32, redBad, FontStyles.Bold);
            gameOverTitle.alignment = TextAlignmentOptions.Center;

            gameOverScore = CreateText(gameOverPanel.transform, "GOScore",
                new Vector2(20, -100), new Vector2(460, 60),
                "Oleada: 0\nTiempo: 00:00", 18, textGray, FontStyles.Normal);
            gameOverScore.alignment = TextAlignmentOptions.Center;

            // Botón Retry
            GameObject btnGO = new GameObject("RetryButton");
            btnGO.transform.SetParent(gameOverPanel.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0);
            btnRT.anchorMax = new Vector2(0.5f, 0);
            btnRT.anchoredPosition = new Vector2(0, 60);
            btnRT.sizeDelta = new Vector2(200, 50);

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = redBad;
            retryBtn = btnGO.AddComponent<Button>();

            TextMeshProUGUI btnText = CreateText(btnGO.transform, "BtnText",
                Vector2.zero, Vector2.zero, "REINTENTAR", 18, textWhite, FontStyles.Bold);
            btnText.rectTransform.anchorMin = Vector2.zero;
            btnText.rectTransform.anchorMax = Vector2.one;
            btnText.rectTransform.offsetMin = Vector2.zero;
            btnText.rectTransform.offsetMax = Vector2.zero;
            btnText.alignment = TextAlignmentOptions.Center;

            if (gameManager != null)
                retryBtn.onClick.AddListener(gameManager.RetryGame);
        }

        private void BuildVictoryPanel()
        {
            victoryPanel = CreatePanel(canvas.transform, "VictoryPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-250, -150), new Vector2(500, 300),
                new Color(0.02f, 0.12f, 0.05f, 0.95f));

            victoryTitle = CreateText(victoryPanel.transform, "VTitle",
                new Vector2(20, -30), new Vector2(460, 50),
                "LAST MACHINE OPERATIONAL", 28, greenOK, FontStyles.Bold);
            victoryTitle.alignment = TextAlignmentOptions.Center;

            victoryScore = CreateText(victoryPanel.transform, "VScore",
                new Vector2(20, -100), new Vector2(460, 60),
                "10 oleadas sobrevividas", 18, textGray, FontStyles.Normal);
            victoryScore.alignment = TextAlignmentOptions.Center;
        }

        // ══════════════════════════════════════════════
        //  EVENTOS
        // ══════════════════════════════════════════════

        private void OnEnterTurret(TurretController_Arandia t)
        {
            activeTurret = t;
            isShowingTurretHUD = true;
            turretPanel.SetActive(true);
            turretNameLabel.text = t.turretName;
            RefreshTurretPanel();
        }

        private void OnExitTurret(TurretController_Arandia t)
        {
            if (activeTurret == t)
            {
                isShowingTurretHUD = false;
                turretPanel.SetActive(false);
                repairBarGroup.SetActive(false);
                activeTurret = null;
            }
        }

        private void OnTurretDestroyed(TurretController_Arandia t)
        {
            // Revisar si es game over
            int destroyed = 0;
            foreach (var turret in turrets)
                if (turret != null && turret.IsDestroyed) destroyed++;

            if (destroyed >= 4)
            {
                gameOverPanel.SetActive(true);
                int wave = waveManager != null ? waveManager.CurrentWave : 0;
                float time = gameManager != null ? gameManager.SurvivalTime : 0;
                int min = Mathf.FloorToInt(time / 60f);
                int sec = Mathf.FloorToInt(time % 60f);
                gameOverScore.text = $"Oleada alcanzada: {wave}\nTiempo: {min:00}:{sec:00}";
            }
        }

        private void OnRepairStarted(TurretComponent_Arandia comp)
        {
            repairBarGroup.SetActive(true);
            if (repairProgressLabel != null)
                repairProgressLabel.text = $"Reparando {comp.componentType}...";
            repairProgressBar.fillAmount = 0f;
        }

        private void OnRepairProgress(float p)
        {
            if (repairProgressBar != null)
                repairProgressBar.fillAmount = p;
        }

        private void OnRepairEnded()
        {
            repairBarGroup.SetActive(false);
        }

        private void ShowVictory()
        {
            victoryPanel.SetActive(true);
            float time = gameManager != null ? gameManager.SurvivalTime : 0;
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            victoryScore.text = $"10 oleadas sobrevividas\nTiempo total: {min:00}:{sec:00}";
        }

        // ══════════════════════════════════════════════
        //  REFRESH
        // ══════════════════════════════════════════════

        private void RefreshTurretPanel()
        {
            if (activeTurret == null) return;

            TurretComponent_Arandia[] comps = {
                activeTurret.sensor, activeTurret.canon, activeTurret.motor
            };

            for (int i = 0; i < 3; i++)
            {
                if (comps[i] == null) continue;
                float pct = comps[i].HPPercent;
                compBars[i].fillAmount = pct;
                compBars[i].color = comps[i].GetStateColor();
                compPercents[i].text = Mathf.RoundToInt(pct * 100f) + "%";
                compPercents[i].color = comps[i].GetStateColor();
            }

            // Status
            if (activeTurret.IsDestroyed)
            {
                turretStatusLabel.text = "DESTRUIDA";
                turretStatusLabel.color = redBad;
            }
            else if (!activeTurret.IsActive)
            {
                turretStatusLabel.text = "INACTIVA";
                turretStatusLabel.color = orangeWarn;
            }
            else
            {
                turretStatusLabel.text = "en rango";
                turretStatusLabel.color = greenOK;
            }
        }

        // ══════════════════════════════════════════════
        //  HELPERS — Crear elementos UI
        // ══════════════════════════════════════════════

        private GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name,
            Vector2 position, Vector2 size, string text, int fontSize, Color color, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            return tmp;
        }

        private Image CreateImage(Transform parent, string name,
            Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = color;

            return img;
        }
    }
}
