// TurretController_Arandia.cs
// Responsable: Arandia
// Descripcion: Controla la torreta completa (3 componentes), detecta enemigos y dispara

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public enum TurretDirection { Norte, Sur, Este, Oeste }

    public class TurretController_Arandia : MonoBehaviour
    {
        [Header("Identificacion - Arandia")]
        public TurretDirection direction;
        public string turretName => "TORRETA " + direction.ToString().ToUpper();

        [Header("Componentes de la Torreta")]
        public TurretComponent_Arandia sensor;
        public TurretComponent_Arandia canon;
        public TurretComponent_Arandia motor;

        [Header("Configuracion de Combate")]
        public float baseFireRate = 1f;
        public float detectionRange = 30f;
        public float baseDamage = 10f;
        public GameObject projectilePrefab;
        public Transform firePoint;

        [Header("Visuales")]
        public TurretAnimator_Arandia turretAnimator;

        [Header("Estado")]
        private float fireTimer = 0;
        private bool isActive = true;
        [SerializeField] private bool playerInRange = false;

        // Referencias internas
        private Transform currentTarget;
        private List<Transform> enemiesInRange = new List<Transform>();

        // Eventos
        public System.Action<TurretController_Arandia> OnTurretDestroyed;
        public System.Action<TurretController_Arandia> OnPlayerEnterRange;
        public System.Action<TurretController_Arandia> OnPlayerExitRange;

        public bool IsActive => isActive;
        // FIXED: la torreta está destruida cuando los 3 componentes están rotos
        public bool IsDestroyed => AllComponentsBroken();
        public bool PlayerInRange => playerInRange;

        void Start()
        {
            // Registrarse en el HUD automaticamente si no estamos en la lista
            if (HUDBuilder_Arandia.Instance != null)
            {
                if (!HUDBuilder_Arandia.Instance.turrets.Contains(this))
                {
                    HUDBuilder_Arandia.Instance.turrets.Add(this);
                    Debug.Log($"<color=green>[Auto-Registro] {turretName} se ha unido al HUD Manager</color>");
                }
            }
            // FORZAR QUE NINGUNA PIEZA SE MUEVA POR FISICAS (incluyendo hijos)
            Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log($"[Arandia] {turretName} bloqueada físicamente en: {transform.position}");

            SubscribeToComponents();
            // Auto-buscar animador si no está asignado
            if (turretAnimator == null)
                turretAnimator = GetComponent<TurretAnimator_Arandia>();
        }

        void Update()
        {
            if (!isActive) return;

            CleanEnemyList();

            // INTERACCIÓN CON JUGADOR
            if (playerInRange && IsInteractPressed())
            {
                Debug.Log($"[Interaccion] Abriendo HUD de {turretName}");
                HUDBuilder_Arandia.Instance?.ShowTurretPanel(this);
            }

            // LÓGICA DEL SENSOR (APUNTADO)
            if (!sensor.IsBroken)
            {
                UpdateTarget();
                if (currentTarget != null && turretAnimator != null)
                {
                    turretAnimator.AimAt(currentTarget.position);
                }
            }
            else
            {
                // FALLO DEL SENSOR
                if (turretAnimator != null)
                {
                    Vector3 randomPos = transform.position + transform.forward * 10f + transform.right * Mathf.Sin(Time.time * 2f) * 5f;
                    turretAnimator.AimAt(randomPos);
                }
            }

            // LÓGICA DEL MOTOR Y CAÑÓN (DISPARO)
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

        private void SubscribeToComponents()
        {
            sensor.OnComponentBroken += OnSensorBroken;
            canon.OnComponentBroken += OnCanonBroken;
            motor.OnComponentBroken += OnMotorBroken;
        }

        private static bool IsInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        private void UpdateTarget()
        {
            if (sensor.IsBroken) return;
            if (detectionRange <= 0) detectionRange = 30f; // Forzar rango si está en 0

            float closestDist = float.MaxValue;
            currentTarget = null;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        currentTarget = hit.transform;
                    }
                }
            }
            
            if (currentTarget != null) Debug.Log($"[Radar] {turretName} ha fijado objetivo: {currentTarget.name}");
        }

        private void Fire()
        {
            if (firePoint == null || projectilePrefab == null) return;

            // Apuntar
            if (!sensor.IsBroken && currentTarget != null)
            {
                Vector3 dir = (currentTarget.position - firePoint.position).normalized;
                firePoint.rotation = Quaternion.LookRotation(dir);
            }

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Configurar daño del proyectil según estado del cañón
            Projectile_Arandia projScript = proj.GetComponent<Projectile_Arandia>();
            if (projScript != null)
                projScript.damage = baseDamage;

            // Animación de recoil
            turretAnimator?.PlayShoot();

            if (canon.componentAnimator != null)
                canon.componentAnimator.SetTrigger("Shoot");
        }

        private float GetAdjustedFireRate()
        {
            if (motor.IsBroken) return baseFireRate * 0.2f;     // Motor roto: 20% cadencia
            if (motor.IsDamaged) return baseFireRate * 0.6f;    // Motor danado: 60%
            return baseFireRate;
        }

        // Callbacks de componentes rotos
        private void OnSensorBroken(TurretComponent_Arandia comp)
        {
            Debug.Log($"[Arandia] {turretName}: SENSOR ROTO - disparo aleatorio activado");
        }

        private void OnCanonBroken(TurretComponent_Arandia comp)
        {
            Debug.Log($"[Arandia] {turretName}: CANON ROTO - torreta inactiva");
            CheckTurretStatus();
        }

        private void OnMotorBroken(TurretComponent_Arandia comp)
        {
            Debug.Log($"[Arandia] {turretName}: MOTOR ROTO - cadencia al 20%");
            CheckTurretStatus();
        }

        private void CheckTurretStatus()
        {
            if (AllComponentsBroken())
            {
                isActive = false;
                OnTurretDestroyed?.Invoke(this);
                Debug.Log($"[Arandia] {turretName}: DESTRUIDA");
            }
        }

        private bool AllComponentsBroken()
        {
            return sensor.IsBroken && canon.IsBroken && motor.IsBroken;
        }

        // Registro de enemigos en rango (llamado por los enemigos al entrar/salir)
        public void RegisterEnemy(Transform enemy)
        {
            if (!enemiesInRange.Contains(enemy))
                enemiesInRange.Add(enemy);
        }

        public void UnregisterEnemy(Transform enemy)
        {
            enemiesInRange.Remove(enemy);
        }

        private void CleanEnemyList()
        {
            enemiesInRange.RemoveAll(e => e == null);
        }

        // Deteccion de jugador para HUD
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;

                RepairSystem_Arandia repair = other.GetComponent<RepairSystem_Arandia>();
                if (repair != null)
                {
                    repair.SetCurrentTurret(this);
                }

                HUDBuilder_Arandia.Instance?.ShowPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                RepairSystem_Arandia repair = other.GetComponent<RepairSystem_Arandia>();
                if (repair != null)
                {
                    repair.ClearCurrentTurret();
                }

                HUDBuilder_Arandia.Instance?.HidePrompt();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
