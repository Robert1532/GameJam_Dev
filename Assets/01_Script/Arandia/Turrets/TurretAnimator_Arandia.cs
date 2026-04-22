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
        private bool isDoingRecoil = false;
        private Coroutine blinkCoroutine;

        void Awake()
        {
            turret     = GetComponent<TurretController_Arandia>();
            sensorComp = turret?.sensor;
            canonComp  = turret?.canon;
            motorComp  = turret?.motor;

            if (canonPivot != null)
                canonLocalOrigin = canonPivot.localPosition;
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
        }

        // ──────────────────────────────────────────────
        //  Animaciones de idle
        // ──────────────────────────────────────────────

        private void AnimateSensor()
        {
            if (sensorPivot == null || sensorComp == null) return;

            if (sensorComp.IsBroken)
            {
                // Sensor roto: oscila erráticamente
                float jitter = Mathf.Sin(Time.time * 8f) * 15f;
                sensorPivot.localRotation = Quaternion.Euler(0f, jitter, 0f);
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
                // Motor dañado: vibración leve
                float vibX = Mathf.Sin(Time.time * 20f) * 0.02f;
                float vibZ = Mathf.Cos(Time.time * 17f) * 0.02f;
                motorPivot.localPosition = new Vector3(vibX, 0f, vibZ);
            }
            else
            {
                motorPivot.localPosition = Vector3.zero;
            }
        }

        // ──────────────────────────────────────────────
        //  Rotación del cañón hacia objetivo
        // ──────────────────────────────────────────────

        /// <summary>
        /// Llamar desde TurretController cada frame que tenga objetivo.
        /// </summary>
        public void AimAt(Vector3 worldTarget)
        {
            if (canonPivot == null || canonComp == null) return;
            if (canonComp.IsBroken) return;

            Vector3 direction = (worldTarget - canonPivot.position).normalized;
            if (direction == Vector3.zero) return;

            Quaternion targetRot = Quaternion.LookRotation(direction);
            float speed = canonComp.IsDamaged ? canonRotationSpeed * 0.5f : canonRotationSpeed;
            canonPivot.rotation = Quaternion.RotateTowards(canonPivot.rotation, targetRot, speed * Time.deltaTime);
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
            // Parpadeo rojo continuo hasta que se repare
            Renderer r = GetRendererForComponent(comp);
            if (r != null)
            {
                if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
                blinkCoroutine = StartCoroutine(BlinkRed(r));
            }
        }

        private void OnComponentRepaired(TurretComponent_Arandia comp)
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
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

        private Renderer GetRendererForComponent(TurretComponent_Arandia comp)
        {
            if (comp == null || componentRenderers == null) return null;

            int index = -1;
            switch (comp.componentType)
            {
                case ComponentType.Sensor: index = 0; break;
                case ComponentType.Canon:  index = 1; break;
                case ComponentType.Motor:  index = 2; break;
            }

            if (index >= 0 && index < componentRenderers.Length)
                return componentRenderers[index];

            return null;
        }
    }
}
