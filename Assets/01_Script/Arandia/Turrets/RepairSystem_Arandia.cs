// RepairSystem_Arandia.cs
// Responsable: Arandia
// Descripcion: Sistema de reparacion - jugador presiona E+1/2/3 para reparar Sensor/Canon/Motor
//              Incluye evento de progreso para barra HUD y cancelacion por daño recibido.

using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    public class RepairSystem_Arandia : MonoBehaviour
    {
        [Header("Configuracion de Reparacion - Arandia")]
        public float repairAmount = 30f;
        public float repairDuration = 1.5f;     // segundos de animacion de reparar
        public int piecesPerRepair = 2;         // piezas que consume cada reparacion

        [Header("Referencias del Jugador")]
        public PieceInventory_Arandia inventory;
        public Animator playerAnimator;

        [Header("Audio (opcional)")]
        public AudioSource audioSource;
        public AudioClip repairStartClip;
        public AudioClip repairCompleteClip;
        public AudioClip repairFailClip;

        // Torreta actual en rango
        private TurretController_Arandia currentTurret;
        private bool isRepairing = false;
        private bool cancelRepair = false;
        private Coroutine repairCoroutine;
        private float currentProgress = 0f;

        // ──────────────────────────────────────────────
        //  Eventos — el HUD se suscribe a estos
        // ──────────────────────────────────────────────

        /// <summary>Progreso de reparación actual (0-1). El HUD puede usar esto para una barra.</summary>
        public System.Action<float> OnRepairProgress;

        /// <summary>Se invoca cuando la reparación se completa (componente, cantidadReparada).</summary>
        public System.Action<TurretComponent_Arandia, float> OnRepairComplete;

        /// <summary>Se invoca cuando la reparación se cancela (por salir de rango o recibir daño).</summary>
        public System.Action OnRepairCancelled;

        /// <summary>Se invoca al iniciar reparación (componente que se está reparando).</summary>
        public System.Action<TurretComponent_Arandia> OnRepairStarted;

        void Update()
        {
            if (currentTurret == null || isRepairing) return;

            bool ePressed = false;
            bool num1Down = false, num2Down = false, num3Down = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                ePressed = Keyboard.current.eKey.isPressed;
                num1Down = Keyboard.current.digit1Key.wasPressedThisFrame;
                num2Down = Keyboard.current.digit2Key.wasPressedThisFrame;
                num3Down = Keyboard.current.digit3Key.wasPressedThisFrame;
            }
#else
            ePressed = Input.GetKey(KeyCode.E);
            num1Down = Input.GetKeyDown(KeyCode.Alpha1);
            num2Down = Input.GetKeyDown(KeyCode.Alpha2);
            num3Down = Input.GetKeyDown(KeyCode.Alpha3);
#endif

            if (ePressed)
            {
                if (num1Down)
                    StartRepair(currentTurret.sensor);
                else if (num2Down)
                    StartRepair(currentTurret.canon);
                else if (num3Down)
                    StartRepair(currentTurret.motor);
            }
        }

        public void StartRepair(TurretComponent_Arandia component)
        {
            if (component == null) return;

            // Ya esta al maximo
            if (component.HPPercent >= 1f)
            {
                Debug.Log($"[Arandia] {component.componentType} ya esta al maximo.");
                return;
            }

            // Sin piezas
            if (!inventory.HasPieces(piecesPerRepair))
            {
                Debug.Log("[Arandia] Sin piezas suficientes para reparar.");
                PlayAudio(repairFailClip);
                return;
            }

            repairCoroutine = StartCoroutine(RepairRoutine(component));
        }

        private IEnumerator RepairRoutine(TurretComponent_Arandia component)
        {
            isRepairing = true;
            cancelRepair = false;
            currentProgress = 0f;

            OnRepairStarted?.Invoke(component);
            PlayAudio(repairStartClip);

            if (playerAnimator != null)
                playerAnimator.SetTrigger("Repair");

            Debug.Log($"[Arandia] Reparando {component.componentType}...");

            // Barra de progreso durante repairDuration
            float elapsed = 0f;
            while (elapsed < repairDuration)
            {
                // Cancelar si el jugador sale del rango
                if (currentTurret == null || !currentTurret.PlayerInRange || cancelRepair)
                {
                    CancelCurrentRepair();
                    yield break;
                }

                elapsed += Time.deltaTime;
                currentProgress = Mathf.Clamp01(elapsed / repairDuration);
                OnRepairProgress?.Invoke(currentProgress);
                yield return null;
            }

            // Aplicar reparacion
            inventory.ConsumePieces(piecesPerRepair);
            component.Repair(repairAmount);

            currentProgress = 0f;
            OnRepairProgress?.Invoke(0f);
            OnRepairComplete?.Invoke(component, repairAmount);
            PlayAudio(repairCompleteClip);

            Debug.Log($"[Arandia] {component.componentType} reparado. HP: {component.HPPercent * 100f:F0}%");

            isRepairing = false;
        }

        // ──────────────────────────────────────────────
        //  API pública
        // ──────────────────────────────────────────────

        /// <summary>Llamado cuando el jugador entra en rango de una torreta.</summary>
        public void SetCurrentTurret(TurretController_Arandia turret)
        {
            currentTurret = turret;
        }

        /// <summary>Llamado cuando el jugador sale del rango.</summary>
        public void ClearCurrentTurret()
        {
            if (repairCoroutine != null)
                StopCoroutine(repairCoroutine);

            isRepairing = false;
            cancelRepair = false;
            currentProgress = 0f;
            OnRepairProgress?.Invoke(0f);
            currentTurret = null;
        }

        /// <summary>
        /// Cancela la reparación en curso (ej: el jugador recibe daño).
        /// Llamar desde el sistema de daño del jugador.
        /// </summary>
        public void InterruptRepair()
        {
            if (!isRepairing) return;
            cancelRepair = true;
        }

        private void CancelCurrentRepair()
        {
            isRepairing = false;
            cancelRepair = false;
            currentProgress = 0f;
            OnRepairProgress?.Invoke(0f);
            OnRepairCancelled?.Invoke();
            PlayAudio(repairFailClip);
            Debug.Log("[Arandia] Reparación cancelada.");
        }

        // ──────────────────────────────────────────────
        //  Getters
        // ──────────────────────────────────────────────

        public bool IsRepairing => isRepairing;
        public float RepairProgress => currentProgress;

        // ──────────────────────────────────────────────
        //  Audio helper
        // ──────────────────────────────────────────────

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
