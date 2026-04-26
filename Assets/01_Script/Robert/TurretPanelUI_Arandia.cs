using UnityEngine;
using TMPro;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    public class TurretPanelUI_Arandia : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;

        [Header("Textos")]
        public TextMeshProUGUI turretNameText;
        public TextMeshProUGUI piecesText;
        public TextMeshProUGUI repairAllText;

        [Header("Botones")]
        public Button btnSensor;
        public Button btnCanon;
        public Button btnMotor;
        public Button btnRepairAll;
        public Button btnClose;

        private TurretController_Arandia currentTurret;
        private RepairSystem_Arandia repairSystem;
        private PieceInventory_Arandia inventory;

        private bool isOpen = false;
        public bool IsOpen => isOpen;

        public void Init(RepairSystem_Arandia repair, PieceInventory_Arandia inv)
        {
            repairSystem = repair;
            inventory = inv;
        }

        void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        void Update()
        {
            if (!isOpen) return;

            UpdatePieces();
            UpdateRepairAllText();
            UpdateTurretName();

            bool closePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                closePressed = Keyboard.current.xKey.wasPressedThisFrame ||
                               Keyboard.current.eKey.wasPressedThisFrame;
            }
#else
            closePressed = Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E);
#endif

            if (closePressed)
                Hide();
        }

        public void Show(TurretController_Arandia turret)
        {
            if (turret == null || panelRoot == null) return;

            currentTurret = turret;
            panelRoot.SetActive(true);
            isOpen = true;

            UpdateTurretName();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            btnSensor.onClick.RemoveAllListeners();
            btnSensor.onClick.AddListener(() =>
                repairSystem.StartRepair(turret.sensor));

            btnCanon.onClick.RemoveAllListeners();
            btnCanon.onClick.AddListener(() =>
                repairSystem.StartRepair(turret.canon));

            btnMotor.onClick.RemoveAllListeners();
            btnMotor.onClick.AddListener(() =>
                repairSystem.StartRepair(turret.motor));

            btnRepairAll.onClick.RemoveAllListeners();
            btnRepairAll.onClick.AddListener(RepairAll);

            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Hide);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            currentTurret = null;
            isOpen = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void UpdateTurretName()
        {
            if (currentTurret != null && turretNameText != null)
                turretNameText.text = currentTurret.turretName;
        }

        void UpdatePieces()
        {
            if (inventory != null && piecesText != null)
                piecesText.text = "Piezas: " + inventory.CurrentPieces;
        }

        void UpdateRepairAllText()
        {
            if (currentTurret == null || repairAllText == null) return;

            int needed = CalculateTotalPiecesNeeded();

            repairAllText.text = needed <= 0
                ? "TODO REPARADO"
                : $"-{needed} PIEZAS REQUERIDAS";
        }

        int CalculateTotalPiecesNeeded()
        {
            int total = 0;

            total += GetPiecesNeeded(currentTurret.sensor);
            total += GetPiecesNeeded(currentTurret.canon);
            total += GetPiecesNeeded(currentTurret.motor);

            return total;
        }

        int GetPiecesNeeded(TurretComponent_Arandia comp)
        {
            if (comp == null || repairSystem == null) return 0;

            float missingHP = comp.maxHP - comp.CurrentHP;
            if (missingHP <= 0) return 0;

            int repairsNeeded = Mathf.CeilToInt(missingHP / repairSystem.repairAmount);
            return repairsNeeded * repairSystem.piecesPerRepair;
        }

        void RepairAll()
        {
            if (currentTurret == null) return;

            repairSystem.StartRepair(currentTurret.sensor);
            repairSystem.StartRepair(currentTurret.canon);
            repairSystem.StartRepair(currentTurret.motor);
        }
    }
}