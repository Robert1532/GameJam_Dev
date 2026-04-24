// TurretController_Arandia.cs
// Responsable: Arandia
// Descripcion: Controla la torreta completa (3 componentes), detecta enemigos y dispara

using UnityEngine;
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

        [Header("Configuracion de Disparo")]
        public Transform firePoint;
        public GameObject projectilePrefab;
        public float baseFireRate = 1f;      // disparos por segundo
        public float detectionRange = 10f;
        public float baseDamage = 25f;

        [Header("Estado")]
        [SerializeField] private bool isActive = true;
        [SerializeField] private bool playerInRange = false;

        [Header("Animacion")]
        public TurretAnimator_Arandia turretAnimator;

        // Referencias internas
        private Transform currentTarget;
        private float fireTimer;
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
            UpdateTarget();
            HandleFiring();
            HandleAiming();
        }

        private void HandleAiming()
        {
            if (turretAnimator == null || currentTarget == null) return;
            turretAnimator.AimAt(currentTarget.position);
        }

        private void SubscribeToComponents()
        {
            sensor.OnComponentBroken += OnSensorBroken;
            canon.OnComponentBroken += OnCanonBroken;
            motor.OnComponentBroken += OnMotorBroken;
        }

        private void UpdateTarget()
        {
            if (sensor.IsBroken)
            {
                // Sensor roto: dispara en direccion aleatoria
                if (enemiesInRange.Count > 0)
                    currentTarget = enemiesInRange[Random.Range(0, enemiesInRange.Count)];
                return;
            }

            // Sensor OK: apunta al enemigo mas cercano
            float closestDist = float.MaxValue;
            currentTarget = null;

            foreach (Transform enemy in enemiesInRange)
            {
                if (enemy == null) continue;
                float dist = Vector3.Distance(transform.position, enemy.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentTarget = enemy;
                }
            }
        }

        private void HandleFiring()
        {
            if (canon.IsBroken || currentTarget == null) return;

            float fireRate = GetAdjustedFireRate();
            fireTimer += Time.deltaTime;

            if (fireTimer >= 1f / fireRate)
            {
                fireTimer = 0f;
                Fire();
            }
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
                OnPlayerEnterRange?.Invoke(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                OnPlayerExitRange?.Invoke(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
