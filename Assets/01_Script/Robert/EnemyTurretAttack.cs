// EnemyTurretAttack.cs
// VERSION CORREGIDA COMPLETA
// Cambia de torreta cuando una está destruida
// Ataca componentes individuales correctamente

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
        [SerializeField] private TurretController_Arandia currentTurret;
        [SerializeField] private TurretComponent_Arandia targetComponent;

        private Coroutine attackCoroutine;

        void Update()
        {
            // Buscar nueva torreta si no hay
            if (currentTurret == null)
            {
                FindNearestTurret();
                return;
            }

            // Si la torreta ya está destruida → cambiar
            if (currentTurret.IsDestroyed)
            {
                ResetTarget();
                FindNearestTurret();
                return;
            }

            // Si no hay componente objetivo → resolver
            if (targetComponent == null)
            {
                ResolveTargetComponent();
            }

            // Si el componente está roto → intentar otro
            if (targetComponent == null || targetComponent.IsBroken)
            {
                if (!TryPickLivingTarget(currentTurret))
                {
                    // No quedan piezas → cambiar de torreta
                    ResetTarget();
                    FindNearestTurret();
                    return;
                }
            }

            // Distancia al componente real
            Vector3 targetPos = targetComponent.transform.position;
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

        void ResetTarget()
        {
            StopAttacking();
            currentTurret = null;
            targetComponent = null;
        }

        void FindNearestTurret()
        {
            TurretController_Arandia[] all = FindObjectsByType<TurretController_Arandia>(FindObjectsSortMode.None);

            float closest = Mathf.Infinity;
            currentTurret = null;

            foreach (var t in all)
            {
                if (t == null || t.IsDestroyed) continue;

                float d = Vector3.Distance(transform.position, t.transform.position);
                if (d < closest)
                {
                    closest = d;
                    currentTurret = t;
                }
            }

            if (currentTurret != null)
                ResolveTargetComponent();
        }

        void ResolveTargetComponent()
        {
            if (currentTurret == null || currentTurret.IsDestroyed) return;

            switch (enemyType)
            {
                case EnemyAttackType.Sensor:
                    targetComponent = currentTurret.sensor;
                    break;

                case EnemyAttackType.Cañon:
                    targetComponent = currentTurret.canon;
                    break;

                case EnemyAttackType.Motor:
                    targetComponent = currentTurret.motor;
                    break;

                case EnemyAttackType.Random:
                    int r = Random.Range(0, 3);
                    if (r == 0) targetComponent = currentTurret.sensor;
                    else if (r == 1) targetComponent = currentTurret.canon;
                    else targetComponent = currentTurret.motor;
                    break;
            }

            // Si salió uno roto → cambiar
            if (targetComponent != null && targetComponent.IsBroken)
            {
                TryPickLivingTarget(currentTurret);
            }
        }

        bool TryPickLivingTarget(TurretController_Arandia controller)
        {
            if (controller.sensor != null && !controller.sensor.IsBroken)
            {
                targetComponent = controller.sensor;
                return true;
            }

            if (controller.canon != null && !controller.canon.IsBroken)
            {
                targetComponent = controller.canon;
                return true;
            }

            if (controller.motor != null && !controller.motor.IsBroken)
            {
                targetComponent = controller.motor;
                return true;
            }

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
                if (currentTurret == null || targetComponent == null)
                {
                    StopAttacking();
                    yield break;
                }

                if (currentTurret.IsDestroyed)
                {
                    StopAttacking();
                    yield break;
                }

                // Si el componente se rompió
                if (targetComponent.IsBroken || targetComponent.CurrentHP <= 0f)
                {
                    if (!TryPickLivingTarget(currentTurret))
                    {
                        StopAttacking();
                        yield break;
                    }
                }

                float dist = Vector3.Distance(transform.position, targetComponent.transform.position);

                if (dist > attackRange)
                {
                    StopAttacking();
                    yield break;
                }

                // APLICAR DAÑO
                ComponentType tipo = GetComponentType();

                if (!targetComponent.IsBroken)
                {
                    currentTurret.ReceiveEnemyDamage(tipo, damage);

                    Debug.Log($"[EnemyAttack] {gameObject.name} golpeó {tipo} — daño: {damage}");
                }

                yield return new WaitForSeconds(attackInterval);
            }
        }

        ComponentType GetComponentType()
        {
            if (targetComponent == currentTurret.sensor)
                return ComponentType.Sensor;

            if (targetComponent == currentTurret.motor)
                return ComponentType.Motor;

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