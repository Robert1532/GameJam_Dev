// HUDBuilder_Arandia.cs
// Responsable: Arandia
// Descripcion: Genera el HUD automáticamente con estética Pixel-Art Retro de Guerra.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public class HUDBuilder_Arandia : MonoBehaviour
    {
        [Header("Referencias - Arandia")]
        public TurretController_Arandia[] turrets;
        public RepairSystem_Arandia repairSystem;
        public WaveManager_Arandia waveManager;
        public GameManager_Arandia gameManager;

        // Colores del tema LAST MACHINE (Pixel-Art Retro de Guerra)
        private Color bgDark      = new Color(0.05f, 0.07f, 0.1f, 0.95f);
        private Color borderBlue  = new Color(0.15f, 0.5f, 0.9f, 1f);
        private Color greenOK     = new Color(0f, 1f, 0.4f, 1f);
        private Color orangeWarn  = new Color(1f, 0.6f, 0f, 1f);
        private Color redBad      = new Color(1f, 0.1f, 0.2f, 1f);
        private Color textWhite   = new Color(0.95f, 1f, 0.95f);
        private Color textGray    = new Color(0.54f, 0.55f, 0.57f);

        // Referencias generadas
        private Canvas canvas;
        private GameObject turretPanel;
        private TextMeshProUGUI turretNameLabel;
        private Image[] compIconBorders = new Image[3];
        private TextMeshProUGUI[] compStatusText = new TextMeshProUGUI[3];
        private Button repairCriticalBtn;
        private TextMeshProUGUI repairCriticalText;
        
        private TextMeshProUGUI waveLabel;
        private TextMeshProUGUI timerLabel;
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI piecesLabel;
        private GameObject promptOverlay;

        private GameObject gameOverPanel;
        private TextMeshProUGUI gameOverTitle;
        private TextMeshProUGUI gameOverScore;
        private Button retryBtn;
        private GameObject victoryPanel;
        private TextMeshProUGUI victoryTitle;
        private TextMeshProUGUI victoryScore;

        private TurretController_Arandia activeTurret;
        private PieceInventory_Arandia inventory;
        private bool isShowingTurretHUD = false;

        void Start()
        {
            BuildCanvas();
            BuildTurretPanel();
            BuildWaveInfoPanel();
            BuildPiecesCounter();
            BuildPromptOverlay();
            BuildGameOverPanel();
            BuildVictoryPanel();

            turretPanel.SetActive(false);
            promptOverlay.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);

            foreach (var t in turrets)
            {
                if (t == null) continue;
                t.OnPlayerEnterRange += OnEnterTurret;
                t.OnPlayerExitRange += OnExitTurret;
            }

            if (repairSystem != null)
                inventory = repairSystem.GetComponent<PieceInventory_Arandia>();

            if (waveManager != null)
            {
                waveManager.OnGameWon += ShowVictory;
                waveManager.waveText = waveLabel;
                waveManager.timerText = timerLabel;
                waveManager.statusText = statusLabel;
            }

            if (gameManager != null)
            {
                gameManager.gameOverPanel = gameOverPanel;
                gameManager.gameOverTitle = gameOverTitle;
                gameManager.gameOverScore = gameOverScore;
                gameManager.retryButton = retryBtn;
                gameManager.victoryPanel = victoryPanel;
                gameManager.victoryTitle = victoryTitle;
                gameManager.victoryScore = victoryScore;
            }
        }

        void Update()
        {
            bool ePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                ePressed = true;
#else
            if (Input.GetKeyDown(KeyCode.E))
                ePressed = true;
#endif

            // Detectar tecla E para abrir/cerrar el menú si estamos cerca de una torreta
            if (activeTurret != null && ePressed)
            {
                isShowingTurretHUD = !isShowingTurretHUD;
                if (turretPanel != null) turretPanel.SetActive(isShowingTurretHUD);
                
                // GESTIÓN DEL RATÓN: Mostrarlo si el menú está abierto
                if (isShowingTurretHUD)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            if (isShowingTurretHUD && activeTurret != null)
                RefreshTurretPanel();

            if (inventory != null && piecesLabel != null)
            {
                piecesLabel.text = $"MATERIALES: {inventory.CurrentPieces}";
                piecesLabel.color = inventory.CurrentPieces < 2 ? redBad : greenOK;
            }
        }

        private void BuildCanvas()
        {
            // Asegurar que existe un EventSystem para los clics
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();

                // COMPATIBILIDAD CON NUEVO INPUT SYSTEM
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
            }

            GameObject go = new GameObject("HUD_Canvas");
            go.transform.SetParent(transform);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Ajustamos a 1280x720 para que los elementos se vean más grandes por defecto
            CanvasScaler cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1280, 720);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
        }

        private void BuildPromptOverlay()
        {
            promptOverlay = CreatePanel(canvas.transform, "Prompt", new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(400, 60), new Color(0,0,0,0));
            TextMeshProUGUI t = CreateText(promptOverlay.transform, "T", Vector2.zero, new Vector2(400, 60), "[E] ACCEDER A TORRETA", 28, greenOK, FontStyles.Bold);
            t.alignment = TextAlignmentOptions.Center;
            promptOverlay.SetActive(false);
        }

        [Header("Iconos de Componentes")]
        public Sprite sensorIcon;
        public Sprite canonIcon;
        public Sprite motorIcon;

        private void BuildTurretPanel()
        {
            turretPanel = CreatePanel(canvas.transform, "TurretPanel", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30, 0), new Vector2(450, 560), bgDark);
            RectTransform rt = turretPanel.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 0.5f);
            
            AddOutline(turretPanel, borderBlue, 4);
            turretNameLabel = CreateText(turretPanel.transform, "Title", new Vector2(25, -20), new Vector2(400, 40), "SISTEMA DE MANTENIMIENTO", 18, textWhite, FontStyles.Bold);
            
            float y = -80;
            string[] names = { "SENSOR", "CAÑÓN", "MOTOR" };
            Sprite[] icons = { sensorIcon, canonIcon, motorIcon };
            
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                GameObject box = CreatePanel(turretPanel.transform, "Box"+i, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, y), new Vector2(410, 95), new Color(0.12f, 0.18f, 0.25f));
                RectTransform boxRT = box.GetComponent<RectTransform>();
                boxRT.pivot = new Vector2(0, 1);
                
                compIconBorders[i] = box.GetComponent<Image>();
                AddOutline(box, greenOK, 2);
                
                // ICONO (Bien pegado a la izquierda y centrado verticalmente)
                if (icons[i] != null)
                {
                    GameObject iconGo = new GameObject("Icon");
                    iconGo.transform.SetParent(box.transform);
                    Image iconImg = iconGo.AddComponent<Image>();
                    iconImg.sprite = icons[i];
                    
                    RectTransform iconRT = iconGo.GetComponent<RectTransform>();
                    iconRT.pivot = new Vector2(0, 0.5f);
                    iconRT.anchorMin = new Vector2(0, 0.5f);
                    iconRT.anchorMax = new Vector2(0, 0.5f);
                    iconRT.anchoredPosition = new Vector2(15, 0); // 15 píxeles desde la izquierda
                    iconRT.sizeDelta = new Vector2(65, 65);
                }

                // TEXTO (Más a la derecha para no tocar el icono)
                float textX = icons[i] != null ? 95 : 20;
                CreateText(box.transform, "L", new Vector2(textX, -20), new Vector2(160, 25), names[i], 15, textWhite, FontStyles.Bold);
                compStatusText[i] = CreateText(box.transform, "S", new Vector2(textX, -50), new Vector2(160, 25), "ESTADO: 100%", 12, greenOK, FontStyles.Normal);
                
                // BOTÓN REPARAR (A la derecha)
                Button btn; TextMeshProUGUI txt;
                CreateButton(box.transform, "Fix"+i, new Vector2(270, -22), new Vector2(125, 50), borderBlue, "REPARAR", out btn, out txt);
                RectTransform btnRT = btn.GetComponent<RectTransform>();
                btnRT.pivot = new Vector2(0, 1);
                txt.fontSize = 13;
                btn.onClick.AddListener(() => {
                    if (activeTurret != null && repairSystem != null) {
                        TurretComponent_Arandia comp = (index == 0) ? activeTurret.sensor : (index == 1 ? activeTurret.canon : activeTurret.motor);
                        repairSystem.StartRepair(comp);
                    }
                });

                y -= 110;
            }

            CreateButton(turretPanel.transform, "RepAllBtn", new Vector2(20, -380), new Vector2(400, 50), borderBlue, "REPARAR COMPONENTE CRÍTICO", out repairCriticalBtn, out repairCriticalText);
            repairCriticalBtn.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            repairCriticalText.fontSize = 14;
            
            Button simBtn; TextMeshProUGUI simTxt;
            CreateButton(turretPanel.transform, "SimBtn", new Vector2(20, -440), new Vector2(400, 35), new Color(0.3f, 0.1f, 0.1f), "SIMULAR DAÑO (DEBUG)", out simBtn, out simTxt);
            simBtn.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            simTxt.fontSize = 11;
            simBtn.onClick.AddListener(() => {
                if (activeTurret != null) {
                    activeTurret.sensor.TakeDamage(30);
                    activeTurret.canon.TakeDamage(25);
                    activeTurret.motor.TakeDamage(20);
                }
            });
        }

        private void BuildWaveInfoPanel()
        {
            GameObject p = CreatePanel(canvas.transform, "Wave", new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero, new Vector2(500, 80), bgDark);
            AddOutline(p, borderBlue, 3);
            waveLabel = CreateText(p.transform, "W", new Vector2(20, -10), new Vector2(200, 40), "OLEADA 1", 24, textWhite, FontStyles.Bold);
            timerLabel = CreateText(p.transform, "T", new Vector2(350, -10), new Vector2(130, 40), "00:45", 24, orangeWarn, FontStyles.Bold);
            statusLabel = CreateText(p.transform, "S", new Vector2(20, -50), new Vector2(460, 25), "INICIANDO DEFENSA...", 14, textGray, FontStyles.Italic);
        }

        private void BuildPiecesCounter()
        {
            GameObject p = CreatePanel(canvas.transform, "Inv", new Vector2(0.95f, 0.05f), new Vector2(0.95f, 0.05f), Vector2.zero, new Vector2(250, 60), bgDark);
            AddOutline(p, greenOK, 3);
            piecesLabel = CreateText(p.transform, "P", new Vector2(20, -15), new Vector2(210, 30), "MATERIALES: 0", 18, greenOK, FontStyles.Bold);
        }

        private void BuildGameOverPanel() {
            gameOverPanel = CreatePanel(canvas.transform, "GO", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 400), new Color(0.2f, 0, 0));
            gameOverTitle = CreateText(gameOverPanel.transform, "T", new Vector2(0, -50), new Vector2(600, 60), "CONEXIÓN PERDIDA", 40, redBad, FontStyles.Bold);
            gameOverTitle.alignment = TextAlignmentOptions.Center;
            gameOverScore = CreateText(gameOverPanel.transform, "S", new Vector2(0, -150), new Vector2(600, 100), "", 20, textWhite, FontStyles.Normal);
            gameOverScore.alignment = TextAlignmentOptions.Center;
            CreateButton(gameOverPanel.transform, "R", new Vector2(200, -300), new Vector2(200, 50), redBad, "REINTENTAR", out retryBtn, out _);
        }

        private void BuildVictoryPanel() {
            victoryPanel = CreatePanel(canvas.transform, "V", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 400), new Color(0, 0.2f, 0.1f));
            victoryTitle = CreateText(victoryPanel.transform, "T", new Vector2(0, -50), new Vector2(600, 60), "MISIÓN CUMPLIDA", 40, greenOK, FontStyles.Bold);
            victoryTitle.alignment = TextAlignmentOptions.Center;
            victoryScore = CreateText(victoryPanel.transform, "S", new Vector2(0, -150), new Vector2(600, 100), "", 20, textWhite, FontStyles.Normal);
            victoryScore.alignment = TextAlignmentOptions.Center;
        }

        private void OnEnterTurret(TurretController_Arandia t) { activeTurret = t; promptOverlay.SetActive(true); }
        private void OnExitTurret(TurretController_Arandia t) { 
            if(activeTurret == t) { activeTurret = null; promptOverlay.SetActive(false); turretPanel.SetActive(false); isShowingTurretHUD = false; }
        }
        private void ShowVictory() { victoryPanel.SetActive(true); }

        private void RefreshTurretPanel()
        {
            if (!isShowingTurretHUD || activeTurret == null) return;

            TurretComponent_Arandia[] comps = { activeTurret.sensor, activeTurret.canon, activeTurret.motor };
            for (int i = 0; i < 3; i++)
            {
                float pct = comps[i].HPPercent;
                Color c = pct > 0.5f ? greenOK : (pct > 0.2f ? orangeWarn : redBad);
                compStatusText[i].text = $"ESTADO: {Mathf.RoundToInt(pct*100)}% - {(pct > 0.5f ? "OPTIMAL" : (pct > 0.2f ? "CRÍTICO" : "FALLO"))}";
                compStatusText[i].color = c;
                compIconBorders[i].GetComponent<Outline>().effectColor = c;
            }
        }

        private GameObject CreatePanel(Transform p, string n, Vector2 min, Vector2 max, Vector2 pos, Vector2 s, Color c) {
            GameObject g = new GameObject(n); g.transform.SetParent(p, false);
            RectTransform r = g.AddComponent<RectTransform>(); r.anchorMin = min; r.anchorMax = max; r.anchoredPosition = pos; r.sizeDelta = s;
            g.AddComponent<Image>().color = c; return g;
        }

        private TextMeshProUGUI CreateText(Transform p, string n, Vector2 pos, Vector2 s, string t, int f, Color c, FontStyles st) {
            GameObject g = new GameObject(n); g.transform.SetParent(p, false);
            RectTransform r = g.AddComponent<RectTransform>(); r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1); r.anchoredPosition = pos; r.sizeDelta = s;
            TextMeshProUGUI tmp = g.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = f; tmp.color = c; tmp.fontStyle = st; return tmp;
        }

        private void CreateButton(Transform p, string n, Vector2 pos, Vector2 s, Color c, string t, out Button b, out TextMeshProUGUI tmp) {
            GameObject g = CreatePanel(p, n, new Vector2(0, 1), new Vector2(0, 1), pos, s, c);
            b = g.AddComponent<Button>(); tmp = CreateText(g.transform, "T", Vector2.zero, s, t, 16, textWhite, FontStyles.Bold); tmp.alignment = TextAlignmentOptions.Center;
        }

        private void AddOutline(GameObject g, Color c, float s) { Outline o = g.AddComponent<Outline>(); o.effectColor = c; o.effectDistance = new Vector2(s, -s); }
    }
}
