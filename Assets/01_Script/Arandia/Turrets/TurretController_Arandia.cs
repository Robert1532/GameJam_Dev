using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    public enum TurretDirection { Norte, Sur, Este, Oeste }

    public class TurretController_Arandia : MonoBehaviour
    {
        [Header("Identificacion")]
        public TurretDirection direction;
        public string turretName => "TORRETA " + direction.ToString().ToUpper();

        [Header("Componentes")]
        public TurretComponent_Arandia sensor;
        public TurretComponent_Arandia canon;
        public TurretComponent_Arandia motor;

        [Header("Combate")]
        public float baseFireRate = 1.2f;
        public float detectionRange = 20f;
        public float baseDamage = 15f;
        public GameObject projectilePrefab;
        public Transform firePoint;

        [Header("Visuales")]
        public TurretAnimator_Arandia turretAnimator;

        private float fireTimer = 0;
        private bool isActive = true;
        private bool playerInRange = false;

        private Transform currentTarget;

        public bool IsActive => isActive;
        public bool IsDestroyed => AllComponentsBroken();
        public bool PlayerInRange => playerInRange;

        void Start()
        {
            Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            SubscribeToComponents();

            if (turretAnimator == null)
                turretAnimator = GetComponent<TurretAnimator_Arandia>();
        }

        void Update()
        {
            // 🔥 REACTIVAR AUTOMÁTICAMENTE SI YA NO ESTÁ DESTRUIDA
            if (!isActive && !AllComponentsBroken())
            {
                isActive = true;
                Debug.Log($"[REACTIVADA] {turretName}");
            }

            if (isActive)
                HandleCombat();

            HandleInteraction();
        }

        void HandleCombat()
        {
            if (!sensor.IsBroken)
            {
                UpdateTarget();

                if (currentTarget != null && turretAnimator != null)
                    turretAnimator.AimAt(currentTarget.position);
            }

            if (!canon.IsBroken)
            {
                float fireRate = GetAdjustedFireRate();

                fireTimer += Time.deltaTime;

                if (fireTimer >= 1f / fireRate && currentTarget != null)
                {
                    Fire();
                    fireTimer = 0;
                }
            }
        }

        void UpdateTarget()
        {
            float closestDist = float.MaxValue;
            currentTarget = null;

            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentTarget = hit.transform;
                }
            }
        }

        void Fire()
        {
            if (firePoint == null || projectilePrefab == null) return;

            if (!sensor.IsBroken && currentTarget != null)
            {
                Vector3 dir = (currentTarget.position - firePoint.position).normalized;
                firePoint.rotation = Quaternion.LookRotation(dir);
            }

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            var projScript = proj.GetComponent<Projectile_Arandia>();
            if (projScript != null)
                projScript.damage = baseDamage;

            turretAnimator?.PlayShoot();

            if (canon.componentAnimator != null)
                canon.componentAnimator.SetTrigger("Shoot");
        }

        float GetAdjustedFireRate()
        {
            if (motor.IsBroken) return baseFireRate * 0.2f;
            if (motor.IsDamaged) return baseFireRate * 0.6f;
            return baseFireRate;
        }

        // =========================
        // DAÑO
        // =========================
        public void ReceiveEnemyDamage(ComponentType type, float damage)
        {
            switch (type)
            {
                case ComponentType.Sensor: sensor.TakeDamage(damage); break;
                case ComponentType.Canon: canon.TakeDamage(damage); break;
                case ComponentType.Motor: motor.TakeDamage(damage); break;
            }
        }

        void SubscribeToComponents()
        {
            sensor.OnComponentBroken += OnComponentChanged;
            canon.OnComponentBroken += OnComponentChanged;
            motor.OnComponentBroken += OnComponentChanged;

            sensor.OnComponentRepaired += OnComponentChanged;
            canon.OnComponentRepaired += OnComponentChanged;
            motor.OnComponentRepaired += OnComponentChanged;
        }

        void OnComponentChanged(TurretComponent_Arandia comp)
        {
            CheckTurretStatus();
        }

        void CheckTurretStatus()
        {
            if (AllComponentsBroken())
            {
                isActive = false;
                Debug.Log($"[DESTRUIDA] {turretName}");
            }
            else
            {
                isActive = true; // 🔥 CLAVE: se reactiva si ya no está completamente rota
            }
        }

        bool AllComponentsBroken()
        {
            return sensor.IsBroken && canon.IsBroken && motor.IsBroken;
        }

        // =========================
        // INTERACCIÓN
        // =========================
        void HandleInteraction()
        {
            if (!playerInRange) return;

            if (IsInteractPressed())
            {
                var repair = FindFirstObjectByType<RepairSystem_Arandia>();
                var panel = FindFirstObjectByType<TurretPanelUI_Arandia>();

                if (repair != null && panel != null)
                {
                    repair.SetCurrentTurret(this);
                    panel.Show(this);
                }
            }
        }

        static bool IsInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = true;
            WorldPromptUI_Arandia.Instance?.Show("Presiona E para reparar");
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = false;
            WorldPromptUI_Arandia.Instance?.Hide();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}