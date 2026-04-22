// TurretHUD_Arandia.cs
// Responsable: Arandia
// Descripcion: Panel de HUD que muestra % de HP de cada componente cuando el jugador esta cerca.
//              Incluye barra de progreso de reparación y estados visuales.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastMachine.Arandia
{
    public class TurretHUD_Arandia : MonoBehaviour
    {
        [Header("Referencias de Torreta - Arandia")]
        public TurretController_Arandia turret;

        [Header("Panel Principal")]
        public GameObject hudPanel;
        public TextMeshProUGUI turretNameText;
        public TextMeshProUGUI statusText;
        public Image panelBackground;     // Para cambiar color en destrucción

        [Header("Sensor UI")]
        public Image sensorBar;
        public TextMeshProUGUI sensorPercentText;
        public Image sensorIcon;

        [Header("Canon UI")]
        public Image canonBar;
        public TextMeshProUGUI canonPercentText;
        public Image canonIcon;

        [Header("Motor UI")]
        public Image motorBar;
        public TextMeshProUGUI motorPercentText;
        public Image motorIcon;

        [Header("Indicador de Reparacion")]
        public GameObject repairPrompt;       // "E = Reparar"
        public TextMeshProUGUI repairKeyText; // E+1 Sensor / E+2 Canon / E+3 Motor

        [Header("Barra de Progreso de Reparacion")]
        public GameObject repairProgressGroup;   // Contenedor de la barra de progreso
        public Image repairProgressBar;          // Image.fillAmount (0-1)
        public TextMeshProUGUI repairProgressText; // "Reparando Canon... 75%"

        // Colores del panel según estado
        private Color normalPanelColor = new Color(0.1f, 0.1f, 0.12f, 0.85f);
        private Color destroyedPanelColor = new Color(0.4f, 0.05f, 0.05f, 0.9f);

        private bool isVisible = false;

        void Start()
        {
            if (turret == null) turret = GetComponentInParent<TurretController_Arandia>();

            turret.OnPlayerEnterRange += ShowHUD;
            turret.OnPlayerExitRange += HideHUD;

            hudPanel.SetActive(false);
            if (repairPrompt != null) repairPrompt.SetActive(false);
            if (repairProgressGroup != null) repairProgressGroup.SetActive(false);
        }

        /// <summary>
        /// Conectar a un RepairSystem para recibir eventos de progreso.
        /// Llamar una vez al inicio desde PlayerTurretConnector o manualmente.
        /// </summary>
        public void BindRepairSystem(RepairSystem_Arandia repair)
        {
            if (repair == null) return;
            repair.OnRepairProgress += UpdateRepairProgress;
            repair.OnRepairStarted += OnRepairStarted;
            repair.OnRepairComplete += OnRepairFinished;
            repair.OnRepairCancelled += OnRepairCancelled;
        }

        void Update()
        {
            if (!isVisible) return;
            RefreshBars();
        }

        private void ShowHUD(TurretController_Arandia t)
        {
            isVisible = true;
            hudPanel.SetActive(true);
            if (repairPrompt != null) repairPrompt.SetActive(true);
            turretNameText.text = turret.turretName;
            RefreshBars();
        }

        private void HideHUD(TurretController_Arandia t)
        {
            isVisible = false;
            hudPanel.SetActive(false);
            if (repairPrompt != null) repairPrompt.SetActive(false);
            if (repairProgressGroup != null) repairProgressGroup.SetActive(false);
        }

        private void RefreshBars()
        {
            UpdateBar(sensorBar, sensorPercentText, sensorIcon, turret.sensor);
            UpdateBar(canonBar, canonPercentText, canonIcon, turret.canon);
            UpdateBar(motorBar, motorPercentText, motorIcon, turret.motor);

            // Estado general
            if (turret.IsDestroyed)
            {
                statusText.text = "DESTRUIDA";
                statusText.color = new Color(0.890f, 0.141f, 0.290f);
                if (panelBackground != null) panelBackground.color = destroyedPanelColor;
            }
            else if (!turret.IsActive)
            {
                statusText.text = "INACTIVA";
                statusText.color = new Color(0.729f, 0.459f, 0.090f);
                if (panelBackground != null) panelBackground.color = normalPanelColor;
            }
            else
            {
                statusText.text = "en rango";
                statusText.color = new Color(0.114f, 0.851f, 0.459f);
                if (panelBackground != null) panelBackground.color = normalPanelColor;
            }

            // Texto de reparacion
            if (repairKeyText != null)
                repairKeyText.text = "E+1 Sensor   E+2 Cañón   E+3 Motor";
        }

        private void UpdateBar(Image bar, TextMeshProUGUI percentText, Image icon, TurretComponent_Arandia component)
        {
            if (component == null) return;

            float pct = component.HPPercent;
            bar.fillAmount = pct;
            bar.color = component.GetStateColor();
            percentText.text = Mathf.RoundToInt(pct * 100f) + "%";

            if (icon != null)
                icon.color = component.GetStateColor();
        }

        // ──────────────────────────────────────────────
        //  Callbacks de RepairSystem
        // ──────────────────────────────────────────────

        private void OnRepairStarted(TurretComponent_Arandia comp)
        {
            if (repairProgressGroup != null)
                repairProgressGroup.SetActive(true);

            if (repairProgressText != null)
                repairProgressText.text = $"Reparando {comp.componentType}... 0%";
        }

        private void UpdateRepairProgress(float progress)
        {
            if (repairProgressBar != null)
                repairProgressBar.fillAmount = progress;

            if (repairProgressText != null)
            {
                int pct = Mathf.RoundToInt(progress * 100f);
                // Mantener el texto previo pero actualizar %
                string current = repairProgressText.text;
                int dotsIndex = current.IndexOf("...");
                if (dotsIndex >= 0)
                    repairProgressText.text = current.Substring(0, dotsIndex + 3) + $" {pct}%";
            }
        }

        private void OnRepairFinished(TurretComponent_Arandia comp, float amount)
        {
            if (repairProgressGroup != null)
                repairProgressGroup.SetActive(false);
        }

        private void OnRepairCancelled()
        {
            if (repairProgressGroup != null)
                repairProgressGroup.SetActive(false);
        }
    }
}

