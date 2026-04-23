// TurretDegradation_Arandia.cs
// Responsable: Arandia
// Descripcion: Reduce HP de los componentes con el tiempo y recibe daño de enemigos.
//              Se pausa entre oleadas (llamar PauseDegrade / ResumeDegrade desde GameManager).

using UnityEngine;

namespace LastMachine.Arandia
{
    public class TurretDegradation_Arandia : MonoBehaviour
    {
        [Header("Degradacion por Tiempo - Arandia")]
        [Tooltip("Daño por segundo base cuando la oleada esta activa")]
        public float baseDamagePerSecond = 2f;

        [Tooltip("Multiplicador que escala con el numero de oleada")]
        public float waveScaling = 0.5f;

        [Header("Estado")]
        [SerializeField] private bool isDegrading = false;
        [SerializeField] private int currentWave = 1;

        // Referencias
        private TurretController_Arandia turret;

        // Timer acumulado
        private float degradeTimer = 0f;
        private const float TICK_INTERVAL = 1f; // cada 1 segundo aplica daño

        void Awake()
        {
            turret = GetComponent<TurretController_Arandia>();
            if (turret == null)
                Debug.LogError("[Arandia] TurretDegradation necesita TurretController en el mismo GameObject.");
        }

        void Update()
        {
            if (!isDegrading) return;
            if (turret == null || turret.IsDestroyed) return;

            degradeTimer += Time.deltaTime;

            if (degradeTimer >= TICK_INTERVAL)
            {
                degradeTimer -= TICK_INTERVAL;
                ApplyTimeDegradation();
            }
        }

        // ──────────────────────────────────────────────
        //  API pública — llamar desde GameManager / WaveManager
        // ──────────────────────────────────────────────

        /// <summary>Activa la degradación al comenzar una oleada.</summary>
        public void StartDegrading(int waveNumber)
        {
            currentWave = waveNumber;
            isDegrading = true;
            degradeTimer = 0f;
            Debug.Log($"[Arandia] {turret.turretName}: degradación activa (oleada {waveNumber})");
        }

        /// <summary>Pausa la degradación en tregua entre oleadas.</summary>
        public void PauseDegrade()
        {
            isDegrading = false;
        }

        /// <summary>Reanuda la degradación cuando comienza la siguiente oleada.</summary>
        public void ResumeDegrade(int waveNumber)
        {
            StartDegrading(waveNumber);
        }

        /// <summary>
        /// Aplica daño directo de un enemigo a un componente específico.
        /// Llamar desde el script de enemigo cuando ataca la torreta.
        /// </summary>
        /// <param name="type">Componente objetivo (Sensor, Canon, Motor)</param>
        /// <param name="damage">Cantidad de daño</param>
        public void ReceiveEnemyDamage(ComponentType type, float damage)
        {
            if (turret == null || turret.IsDestroyed) return;

            TurretComponent_Arandia target = GetComponent(type);
            if (target == null) return;

            target.TakeDamage(damage);
            Debug.Log($"[Arandia] {turret.turretName}: {type} recibió {damage} daño de enemigo. HP: {target.HPPercent * 100f:F0}%");
        }

        // ──────────────────────────────────────────────
        //  Internos
        // ──────────────────────────────────────────────

        private void ApplyTimeDegradation()
        {
            float dmg = GetCurrentDps();

            // Daño distribuido: el cañón se desgasta más rápido (es la pieza más usada)
            turret.sensor.TakeDamage(dmg * 0.8f);
            turret.canon.TakeDamage(dmg * 1.2f);
            turret.motor.TakeDamage(dmg * 0.9f);
        }

        private float GetCurrentDps()
        {
            return baseDamagePerSecond + (waveScaling * (currentWave - 1));
        }

        private TurretComponent_Arandia GetComponent(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Sensor: return turret.sensor;
                case ComponentType.Canon:  return turret.canon;
                case ComponentType.Motor:  return turret.motor;
                default: return null;
            }
        }

        // Gizmos para debug
        private void OnDrawGizmosSelected()
        {
            if (!isDegrading) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
        }
    }
}
