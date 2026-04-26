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

            // Torreta destruida (los 3 componentes rotos): dejar de atacar y buscar otra o quedar idle
            TurretController_Arandia ctrl = currentTurret.GetComponent<TurretController_Arandia>();
            if (ctrl == null || ctrl.IsDestroyed)
            {
                StopAttacking();
                currentTurret = null;
                targetComponent = null;
                FindNearestTurret();
                return;
            }

            // Medir distancia contra el COMPONENTE objetivo, no el centro
            if (targetComponent == null)
            {
                ResolveTargetComponent();
            }

            // Objetivo roto sin piezas vivas en esta torreta (ResolveTargetComponent pudo quedar obsoleto)
            if (targetComponent != null && targetComponent.IsBroken)
            {
                if (!TryPickLivingTarget(ctrl))
                {
                    StopAttacking();
                    currentTurret = null;
                    targetComponent = null;
                    FindNearestTurret();
                    return;
                }
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
            currentTurret = null;

            foreach (var t in all)
            {
                if (t == null) continue;
                var c = t.GetComponent<TurretController_Arandia>();
                if (c == null || c.IsDestroyed) continue;

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
            if (controller == null || controller.IsDestroyed) return;

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

            if (targetComponent != null && targetComponent.IsBroken)
                TryPickLivingTarget(controller);
        }

        /// <summary>Elige el primer componente de esta torreta que aún no esté roto.</summary>
        bool TryPickLivingTarget(TurretController_Arandia controller)
        {
            if (controller == null) return false;
            if (controller.sensor != null && !controller.sensor.IsBroken) { targetComponent = controller.sensor; return true; }
            if (controller.canon != null && !controller.canon.IsBroken) { targetComponent = controller.canon; return true; }
            if (controller.motor != null && !controller.motor.IsBroken) { targetComponent = controller.motor; return true; }
            targetComponent = null;
            return false;
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

                TurretController_Arandia controller = currentTurret.GetComponent<TurretController_Arandia>();
                if (controller == null || controller.IsDestroyed)
                {
                    StopAttacking();
                    yield break;
                }

                // Si el componente objetivo ya está roto, buscar otro
                if (targetComponent.IsBroken || targetComponent.CurrentHP <= 0f)
                {
                    if (!TryPickLivingTarget(controller))
                    {
                        StopAttacking();
                        yield break;
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

                // APLICAR DAÑO (ReceiveEnemyDamage no hace nada si torreta destruida o pieza rota)
                ComponentType tipo = GetComponentType();
                if (!targetComponent.IsBroken && targetComponent.CurrentHP > 0f)
                {
                    currentTurret.ReceiveEnemyDamage(tipo, damage);
                    Debug.Log($"[EnemyAttack] {gameObject.name} golpeó {tipo} — daño: {damage}");
                }

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