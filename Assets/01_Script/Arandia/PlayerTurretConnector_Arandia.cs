// PlayerTurretConnector_Arandia.cs
// Responsable: Arandia
// Descripcion: Componente del jugador que detecta la torreta mas cercana en rango,
//              activa el HUD correcto y conecta el RepairSystem con ella.
//              Coloca este script en el GameObject del Jugador.

using UnityEngine;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    public class PlayerTurretConnector_Arandia : MonoBehaviour
    {
        [Header("Referencias del Jugador - Arandia")]
        public RepairSystem_Arandia repairSystem;
        public PieceInventory_Arandia inventory;

        [Header("Configuracion")]
        [Tooltip("Radio para detectar torretas. Debe ser igual al SphereCollider de la torreta.")]
        public float interactRadius = 3.5f;
        public LayerMask turretLayer;

        [Header("Feedback Visual")]
        [Tooltip("Texto que aparece en pantalla con el nombre de la torreta y el prompt E")]
        public UnityEngine.UI.Text promptText;   // Opcional: si usas UGUI simple

        // Estado
        private TurretController_Arandia currentTurret;

        void Start()
        {

            // Validar referencias
            if (repairSystem == null)
                repairSystem = GetComponent<RepairSystem_Arandia>();

            if (inventory == null)
                inventory = GetComponent<PieceInventory_Arandia>();
        }

        void Update()
        {
            DetectNearestTurret();

            // Recoger piezas con F (opcional, drop de enemigos lo hace automático)
            // Esta es la versión de emergencia para debug
#if UNITY_EDITOR
            bool fDown = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) fDown = Keyboard.current.fKey.wasPressedThisFrame;
#else
            fDown = Input.GetKeyDown(KeyCode.F);
#endif
            if (fDown)
                inventory?.AddPieces(3);
#endif
        }

        // ──────────────────────────────────────────────
        //  Detección de torreta más cercana
        // ──────────────────────────────────────────────

        private void DetectNearestTurret()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, turretLayer);

            TurretController_Arandia nearest = null;
            float closestDist = float.MaxValue;

            foreach (Collider col in hits)
            {
                TurretController_Arandia tc = col.GetComponent<TurretController_Arandia>();
                if (tc == null)
                    tc = col.GetComponentInParent<TurretController_Arandia>();

                if (tc == null || tc.IsDestroyed) continue;

                float dist = Vector3.Distance(transform.position, tc.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearest = tc;
                }
            }

            // Cambio de torreta
            if (nearest != currentTurret)
            {
                if (currentTurret != null)
                    ExitTurret(currentTurret);

                if (nearest != null)
                    EnterTurret(nearest);

                currentTurret = nearest;
            }

            // Actualizar prompt
            UpdatePrompt();
        }

        private void EnterTurret(TurretController_Arandia turret)
        {
            repairSystem?.SetCurrentTurret(turret);
            Debug.Log($"[Arandia] Jugador entró en rango de {turret.turretName}");
        }

        private void ExitTurret(TurretController_Arandia turret)
        {
            repairSystem?.ClearCurrentTurret();
            Debug.Log($"[Arandia] Jugador salió del rango de {turret.turretName}");
        }

        private void UpdatePrompt()
        {
            if (promptText == null) return;

            if (currentTurret != null && !currentTurret.IsDestroyed)
            {
                promptText.text = $"{currentTurret.turretName} | E+1 Sensor  E+2 Cañón  E+3 Motor";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }

        // ──────────────────────────────────────────────
        //  Getter público
        // ──────────────────────────────────────────────

        /// <summary>Retorna la torreta actualmente en rango del jugador.</summary>
        public TurretController_Arandia CurrentTurret => currentTurret;

        /// <summary>True si el jugador está en rango de alguna torreta.</summary>
        public bool IsNearTurret => currentTurret != null;

        // ──────────────────────────────────────────────
        //  Gizmos debug
        // ──────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
