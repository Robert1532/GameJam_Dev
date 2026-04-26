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

        // Timer
        private float degradeTimer = 0f;
        private const float TICK_INTERVAL = 1f;

        void Awake()
        {
            turret = GetComponent<TurretController_Arandia>();

            if (turret == null)
                Debug.LogError("[Arandia] Falta TurretController en el objeto.");
        }

        void Update()
        {
            if (!isDegrading) return;
            if (turret == null) return; // 🔥 YA NO BLOQUEAMOS POR IsDestroyed

            degradeTimer += Time.deltaTime;

            if (degradeTimer >= TICK_INTERVAL)
            {
                degradeTimer -= TICK_INTERVAL;
                ApplyTimeDegradation();
            }
        }

        // =========================
        // API PUBLICA
        // =========================

        public void StartDegrading(int waveNumber)
        {
            currentWave = waveNumber;
            isDegrading = true;
            degradeTimer = 0f;

            Debug.Log($"[Degradacion] {turret.turretName} activa (Wave {waveNumber})");
        }

        public void PauseDegrade()
        {
            isDegrading = false;
        }

        public void ResumeDegrade(int waveNumber)
        {
            StartDegrading(waveNumber);
        }

        public void ReceiveEnemyDamage(ComponentType type, float damage)
        {
            if (turret == null) return;

            TurretComponent_Arandia target = GetTurretComponent(type);
            if (target == null) return;

            // 🔥 NO BLOQUEAR POR TORRETA DESTRUIDA
            target.TakeDamage(damage);

            Debug.Log($"[EnemyDamage] {turret.turretName} -> {type} (-{damage})");
        }

        // =========================
        // INTERNOS
        // =========================

        private void ApplyTimeDegradation()
        {
            if (turret.sensor == null || turret.canon == null || turret.motor == null)
                return;

            float dmg = GetCurrentDps();

            // Distribución de desgaste
            turret.sensor.TakeDamage(dmg * 0.8f);
            turret.canon.TakeDamage(dmg * 1.2f);
            turret.motor.TakeDamage(dmg * 0.9f);
        }

        private float GetCurrentDps()
        {
            return baseDamagePerSecond + (waveScaling * (currentWave - 1));
        }

        private TurretComponent_Arandia GetTurretComponent(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Sensor: return turret.sensor;
                case ComponentType.Canon: return turret.canon;
                case ComponentType.Motor: return turret.motor;
                default: return null;
            }
        }

        // =========================
        // DEBUG
        // =========================

        void OnDrawGizmosSelected()
        {
            if (!isDegrading) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
        }
    }
}