// TurretAnimator_Arandia.cs
// Responsable: Arandia
// Descripcion: Controla las animaciones de la torreta por código:
//   - Rotacion suave del cañon hacia el objetivo
//   - Movimiento idle del sensor (giro de radar)
//   - Recoil de disparo
//   - Efecto visual de componente dañado (parpadeo)

using UnityEngine;
using System.Collections;

namespace LastMachine.Arandia
{
    public class TurretAnimator_Arandia : MonoBehaviour
    {
        [Header("Partes del modelo - Arandia")]
        [Tooltip("Transform del cañón (rota hacia el objetivo)")]
        public Transform canonPivot;

        [Tooltip("Transform del sensor/radar (gira constantemente en idle)")]
        public Transform sensorPivot;

        [Tooltip("Transform del motor (vibra cuando está dañado)")]
        public Transform motorPivot;

        [Tooltip("Punto desde donde sale el proyectil (hijo del cañón)")]
        public Transform firePoint;

        [Header("Configuracion de Rotacion")]
        [Tooltip("Velocidad de rotacion del cañón hacia el objetivo (grados/seg)")]
        public float canonRotationSpeed = 90f;

        [Tooltip("Velocidad de giro del radar en idle (grados/seg)")]
        public float sensorIdleSpeed = 45f;

        public Vector3 rotationOffset = new Vector3(0, 180, 0); // Por defecto 180 para corregir el "disparo por la espalda"

        [Header("Recoil de Disparo")]
        public float recoilDistance = 0.15f;
        public float recoilDuration = 0.08f;
        public float recoilReturnDuration = 0.2f;

        [Header("Parpadeo de Dano")]
        public Renderer[] componentRenderers; // Renderers de los 3 componentes
        public float blinkInterval = 0.4f;

        // Referencias
        private TurretController_Arandia turret;
        private TurretComponent_Arandia sensorComp;
        private TurretComponent_Arandia canonComp;
        private TurretComponent_Arandia motorComp;

        // Estado interno
        private Vector3 canonLocalOrigin;
        private Vector3 sensorLocalOrigin;
        private Quaternion sensorLocalRotationOrigin;
        private Vector3 motorLocalOrigin;

        private bool isDoingRecoil = false;
        private Coroutine[] blinkCoroutines = new Coroutine[3];

        void Awake()
        {
            turret     = GetComponent<TurretController_Arandia>();
            sensorComp = turret?.sensor;
            canonComp  = turret?.canon;
            motorComp  = turret?.motor;

            // GUARDAR POSICIONES ORIGINALES DEL EDITOR
            if (canonPivot != null)
                canonLocalOrigin = canonPivot.localPosition;
            
            if (sensorPivot != null)
            {
                sensorLocalOrigin = sensorPivot.localPosition;
                sensorLocalRotationOrigin = sensorPivot.localRotation;
            }

            if (motorPivot != null)
                motorLocalOrigin = motorPivot.localPosition;
        }

        void Start()
        {
            // Suscribirse a eventos de componentes para efectos visuales
            if (sensorComp != null)
            {
                sensorComp.OnComponentDamaged  += (c, d) => OnComponentDamaged(c);
                sensorComp.OnComponentBroken   += OnComponentBroken;
                sensorComp.OnComponentRepaired += OnComponentRepaired;
            }
            if (canonComp != null)
            {
                canonComp.OnComponentDamaged  += (c, d) => OnComponentDamaged(c);
                canonComp.OnComponentBroken   += OnComponentBroken;
                canonComp.OnComponentRepaired += OnComponentRepaired;
            }
            if (motorComp != null)
            {
                motorComp.OnComponentDamaged  += (c, d) => OnComponentDamaged(c);
                motorComp.OnComponentBroken   += OnComponentBroken;
                motorComp.OnComponentRepaired += OnComponentRepaired;
            }
        }

        void Update()
        {
            AnimateSensor();
            AnimateMotor();
            HandleIdleBreathing();
        }

        private void HandleIdleBreathing()
        {
            if (canonPivot == null || turret.PlayerInRange) return; // No respirar si el jugador está operando o no hay pivote

            // Si no hay un objetivo actual, el cañón oscila levemente
            // Usamos un pequeño offset en el eje X para simular respiración
            float breathe = Mathf.Sin(Time.time * 1.5f) * 2f; 
            Quaternion breatheRot = Quaternion.Euler(breathe, 0f, 0f);
            
            // Solo aplicamos si no estamos apuntando (esto se pisa en AimAt si hay objetivo)
            canonPivot.localRotation = Quaternion.Slerp(canonPivot.localRotation, breatheRot, Time.deltaTime * 1f);
        }

        // ──────────────────────────────────────────────
        //  Animaciones de idle
        // ──────────────────────────────────────────────

        private void AnimateSensor()
        {
            if (sensorPivot == null || sensorComp == null) return;

            if (sensorComp.IsBroken)
            {
                // Sensor roto: oscila erráticamente sobre su rotación original
                float jitter = Mathf.Sin(Time.time * 8f) * 15f;
                sensorPivot.localRotation = sensorLocalRotationOrigin * Quaternion.Euler(0f, jitter, 0f);
            }
            else
            {
                // Sensor OK: giro continuo del radar
                float speed = sensorComp.IsDamaged ? sensorIdleSpeed * 0.4f : sensorIdleSpeed;
                sensorPivot.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
            }
        }

        private void AnimateMotor()
        {
            if (motorPivot == null || motorComp == null) return;

            if (motorComp.IsDamaged && !motorComp.IsBroken)
            {
                // Motor dañado: vibración sobre su posición original
                float vibX = Mathf.Sin(Time.time * 20f) * 0.05f; // Un poco más de vibración
                float vibZ = Mathf.Cos(Time.time * 17f) * 0.05f;
                motorPivot.localPosition = motorLocalOrigin + new Vector3(vibX, 0f, vibZ);
            }
            else
            {
                motorPivot.localPosition = motorLocalOrigin;
            }
        }

        // ──────────────────────────────────────────────
        //  Rotación del cañón hacia objetivo
        // ──────────────────────────────────────────────

        /// <summary>
        /// Llamar desde TurretController cada frame que tenga objetivo.
        /// </summary>
        private Vector3 currentAimTarget;
        private bool hasTarget = false;

        public void AimAt(Vector3 targetPos)
        {
            currentAimTarget = targetPos;
            hasTarget = true;
        }

        private void LateUpdate()
        {
            if (!hasTarget || canonPivot == null) return;
            if (canonComp != null && canonComp.IsBroken) return;

            // Calcular dirección
            Vector3 direction = (currentAimTarget - canonPivot.position).normalized;
            if (direction == Vector3.zero) return;

            // Rotación forzada después de las animaciones + Corrección de eje
            Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
            canonPivot.rotation = Quaternion.Slerp(canonPivot.rotation, targetRot, Time.deltaTime * canonRotationSpeed * 5f);
            
            // Resetear para el siguiente frame
            hasTarget = false;
        }

        // ──────────────────────────────────────────────
        //  Animación de disparo (recoil)
        // ──────────────────────────────────────────────

        /// <summary>Llamar desde TurretController.Fire()</summary>
        public void PlayShoot()
        {
            if (!isDoingRecoil && canonPivot != null)
                StartCoroutine(RecoilRoutine());
        }

        private IEnumerator RecoilRoutine()
        {
            isDoingRecoil = true;
            Vector3 recoilPos = canonLocalOrigin - Vector3.forward * recoilDistance;

            // Empuje hacia atrás
            float t = 0f;
            while (t < recoilDuration)
            {
                t += Time.deltaTime;
                canonPivot.localPosition = Vector3.Lerp(canonLocalOrigin, recoilPos, t / recoilDuration);
                yield return null;
            }

            // Retorno suave
            t = 0f;
            while (t < recoilReturnDuration)
            {
                t += Time.deltaTime;
                canonPivot.localPosition = Vector3.Lerp(recoilPos, canonLocalOrigin, t / recoilReturnDuration);
                yield return null;
            }

            canonPivot.localPosition = canonLocalOrigin;
            isDoingRecoil = false;
        }

        // ──────────────────────────────────────────────
        //  Efectos de estado
        // ──────────────────────────────────────────────

        private void OnComponentDamaged(TurretComponent_Arandia comp)
        {
            // Destello rápido (flash blanco)
            Renderer r = GetRendererForComponent(comp);
            if (r != null)
                StartCoroutine(FlashWhite(r, 0.1f));
        }

        private void OnComponentBroken(TurretComponent_Arandia comp)
        {
            int index = GetIndexForComponent(comp);
            if (index == -1) return;

            Renderer r = GetRendererForComponent(comp);
            if (r != null)
            {
                if (blinkCoroutines[index] != null) StopCoroutine(blinkCoroutines[index]);
                blinkCoroutines[index] = StartCoroutine(BlinkRed(r));
            }
        }

        private void OnComponentRepaired(TurretComponent_Arandia comp)
        {
            int index = GetIndexForComponent(comp);
            if (index == -1) return;

            if (blinkCoroutines[index] != null)
            {
                StopCoroutine(blinkCoroutines[index]);
                blinkCoroutines[index] = null;
            }

            // Restaurar color original
            Renderer r = GetRendererForComponent(comp);
            if (r != null)
                r.material.color = Color.white;
        }

        private IEnumerator FlashWhite(Renderer r, float duration)
        {
            Color original = r.material.color;
            r.material.color = Color.white * 2f; // HDR flash
            yield return new WaitForSeconds(duration);
            r.material.color = original;
        }

        private IEnumerator BlinkRed(Renderer r)
        {
            Color original = r.material.color;
            Color broken   = new Color(0.890f, 0.141f, 0.290f);
            bool toggle    = false;

            while (true)
            {
                r.material.color = toggle ? broken : original;
                toggle = !toggle;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private int GetIndexForComponent(TurretComponent_Arandia comp)
        {
            if (comp == null) return -1;
            switch (comp.componentType)
            {
                case ComponentType.Sensor: return 0;
                case ComponentType.Canon:  return 1;
                case ComponentType.Motor:  return 2;
                default: return -1;
            }
        }

        private Renderer GetRendererForComponent(TurretComponent_Arandia comp)
        {
            int index = GetIndexForComponent(comp);
            if (index >= 0 && index < componentRenderers.Length)
                return componentRenderers[index];

            return null;
        }
    }
}
