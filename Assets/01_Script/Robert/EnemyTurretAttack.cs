// EnemyTurretAttack.cs
// Mide distancia contra el COMPONENTE objetivo (Sensor/Canon/Motor),
// no contra el centro de la torreta. Así el Jammer ataca cuando está
// cerca del Sensor aunque el centro de la torreta esté lejos.

using UnityEngine;
using System.Collections;
using LastMachine.Arandia;

namespace LastMachine
{
    public class EnemyTurretAttack : MonoBehaviour
    {
        [Header("Tipo de Enemigo")]
        public EnemyAttackType enemyType = EnemyAttackType.Cañon;

        [Header("Estadísticas de Ataque")]
        public float damage = 10f;
        public float attackInterval = 1.5f;
        public float attackRange = 2.5f;

        [Header("Debug")]
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private TurretDegradation_Arandia currentTurret;
        [SerializeField] private TurretComponent_Arandia targetComponent; // el hijo concreto

        private Coroutine attackCoroutine;

        void Update()
        {
            // Buscar torreta si no tenemos
            if (currentTurret == null)
            {
                FindNearestTurret();
                return;
            }

            // Medir distancia contra el COMPONENTE objetivo, no el centro
            if (targetComponent == null)
            {
                ResolveTargetComponent();
            }

            Vector3 targetPos = targetComponent != null
                ? targetComponent.transform.position
                : currentTurret.transform.position;

            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist <= attackRange && !isAttacking)
            {
                StartAttacking();
            }
            else if (dist > attackRange && isAttacking)
            {
                StopAttacking();
            }
        }

        void FindNearestTurret()
        {
            TurretDegradation_Arandia[] all = FindObjectsByType<TurretDegradation_Arandia>(FindObjectsSortMode.None);
            float closest = Mathf.Infinity;

            foreach (var t in all)
            {
                if (t == null) continue;
                float d = Vector3.Distance(transform.position, t.transform.position);
                if (d < closest)
                {
                    closest = d;
                    currentTurret = t;
                }
            }

            // Una vez encontrada la torreta, resolver el componente objetivo
            ResolveTargetComponent();
        }

        void ResolveTargetComponent()
        {
            if (currentTurret == null) return;

            // Obtener TurretController del mismo GameObject
            TurretController_Arandia controller = currentTurret.GetComponent<TurretController_Arandia>();
            if (controller == null) return;

            switch (enemyType)
            {
                case EnemyAttackType.Sensor: targetComponent = controller.sensor; break;
                case EnemyAttackType.Cañon: targetComponent = controller.canon; break;
                case EnemyAttackType.Motor: targetComponent = controller.motor; break;
                case EnemyAttackType.Random:
                    // Aleatorio pero consistente: elige uno y lo mantiene
                    int r = Random.Range(0, 3);
                    if (r == 0) targetComponent = controller.sensor;
                    else if (r == 1) targetComponent = controller.canon;
                    else targetComponent = controller.motor;
                    break;
            }
        }

        void StartAttacking()
        {
            if (isAttacking) return;
            isAttacking = true;
            attackCoroutine = StartCoroutine(AttackLoop());
        }

        void StopAttacking()
        {
            isAttacking = false;
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        IEnumerator AttackLoop()
        {
            while (true)
            {
                // Validar que todo sigue en pie
                if (currentTurret == null || targetComponent == null)
                {
                    StopAttacking();
                    yield break;
                }

                // Si el componente objetivo ya está roto, buscar otro
                if (targetComponent.IsBroken)
                {
                    // Intentar cambiar al siguiente componente disponible
                    TurretController_Arandia controller = currentTurret.GetComponent<TurretController_Arandia>();
                    if (controller != null)
                    {
                        if (!controller.sensor.IsBroken) targetComponent = controller.sensor;
                        else if (!controller.canon.IsBroken) targetComponent = controller.canon;
                        else if (!controller.motor.IsBroken) targetComponent = controller.motor;
                        else
                        {
                            // Torreta destruida, dejar de atacar
                            StopAttacking();
                            yield break;
                        }
                    }
                }

                // Verificar distancia contra el componente actual
                float dist = Vector3.Distance(transform.position, targetComponent.transform.position);
                if (dist > attackRange)
                {
                    // Salir del loop, Update() lo reiniciará cuando vuelva a estar cerca
                    StopAttacking();
                    yield break;
                }

                // APLICAR DAÑO
                ComponentType tipo = GetComponentType();
                currentTurret.ReceiveEnemyDamage(tipo, damage);

                Debug.Log($"[EnemyAttack] {gameObject.name} golpeó {tipo} — daño: {damage}");

                yield return new WaitForSeconds(attackInterval);
            }
        }

        ComponentType GetComponentType()
        {
            // Devolver el tipo según el componente que tenemos actualmente
            if (targetComponent == null) return ComponentType.Canon;

            TurretController_Arandia controller = currentTurret?.GetComponent<TurretController_Arandia>();
            if (controller == null) return ComponentType.Canon;

            if (targetComponent == controller.sensor) return ComponentType.Sensor;
            if (targetComponent == controller.motor) return ComponentType.Motor;
            return ComponentType.Canon;
        }

        void OnDisable()
        {
            StopAttacking();
        }

        void OnDestroy()
        {
            StopAttacking();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    public enum EnemyAttackType
    {
        Sensor,
        Cañon,
        Motor,
        Random
    }
}