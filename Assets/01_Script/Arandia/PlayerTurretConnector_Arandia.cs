using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    public class PlayerTurretConnector_Arandia : MonoBehaviour
    {
        public RepairSystem_Arandia repairSystem;
        public PieceInventory_Arandia inventory;
        public TurretPanelUI_Arandia turretUI;

        public float interactRadius = 3.5f;
        public LayerMask turretLayer;

        public UnityEngine.UI.Text promptText;

        private TurretController_Arandia currentTurret;

        void Start()
        {
            if (repairSystem == null)
                repairSystem = GetComponent<RepairSystem_Arandia>();

            if (inventory == null)
                inventory = GetComponent<PieceInventory_Arandia>();

            turretUI.Init(repairSystem, inventory);
        }

        void Update()
        {
            DetectNearestTurret();
            HandleInput();
        }

        void HandleInput()
        {
            bool ePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                ePressed = Keyboard.current.eKey.wasPressedThisFrame;
#else
            ePressed = Input.GetKeyDown(KeyCode.E);
#endif

            if (ePressed && currentTurret != null)
            {
                if (turretUI.IsOpen)
                    turretUI.Hide();
                else
                    turretUI.Show(currentTurret);
            }
        }

        private void DetectNearestTurret()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, turretLayer);

            TurretController_Arandia nearest = null;
            float closestDist = float.MaxValue;

            foreach (Collider col in hits)
            {
                var tc = col.GetComponent<TurretController_Arandia>() ??
                         col.GetComponentInParent<TurretController_Arandia>();

                if (tc == null || tc.IsDestroyed) continue;

                float dist = Vector3.Distance(transform.position, tc.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearest = tc;
                }
            }

            // 🔥 Si cambias de torreta o te alejas → cerrar panel
            if (nearest != currentTurret)
            {
                if (turretUI.IsOpen)
                    turretUI.Hide();

                repairSystem.ClearCurrentTurret();

                if (nearest != null)
                    repairSystem.SetCurrentTurret(nearest);

                currentTurret = nearest;
            }

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            if (promptText == null) return;

            if (currentTurret != null)
            {
                promptText.text = currentTurret.turretName + " | Presiona E";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}