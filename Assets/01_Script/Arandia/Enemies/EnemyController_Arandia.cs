using UnityEngine;
using System.Collections;

namespace LastMachine.Arandia
{
    public enum EnemyType { Scrapper, Jammer, Drainer, Rusher }

    public class EnemyController_Arandia : MonoBehaviour
    {
        [Header("Configuracion de IA")]
        public EnemyType type;
        public float speed = 3f;
        public float attackRange = 2f;
        public int damagePerHit = 10;
        public float attackInterval = 1.5f;

        private GameObject targetTurret;
        private TurretComponent_Arandia targetComponent;
        private bool isAttacking = false;

        void Start()
        {
            FindNearestTurret();
        }

        void Update()
        {
            if (targetTurret == null)
            {
                FindNearestTurret();
                return;
            }

            var controller = targetTurret.GetComponent<TurretController_Arandia>();
            if (controller == null || controller.IsDestroyed)
            {
                targetTurret = null;
                targetComponent = null;
                FindNearestTurret();
                return;
            }

            if (targetComponent == null || targetComponent.IsBroken)
            {
                AssignTargetComponent(controller);
                if (targetComponent == null || targetComponent.IsBroken)
                {
                    targetTurret = null;
                    FindNearestTurret();
                    return;
                }
            }

            // Moverse hacia el componente específico, no solo hacia la torreta
            Vector3 targetPos = targetComponent != null ? targetComponent.transform.position : targetTurret.transform.position;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > attackRange)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
            }
            else if (!isAttacking && targetComponent != null && !targetComponent.IsBroken)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        void FindNearestTurret()
        {
            GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");
            float closestDist = Mathf.Infinity;
            targetTurret = null;
            targetComponent = null;

            foreach (GameObject t in turrets)
            {
                var c = t.GetComponent<TurretController_Arandia>();
                if (c == null || c.IsDestroyed) continue;

                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetTurret = t;
                    AssignTargetComponent(c);
                }
            }
        }

        void AssignTargetComponent(TurretController_Arandia controller)
        {
            if (controller == null) return;

            switch (type)
            {
                case EnemyType.Jammer:
                    targetComponent = PickFirstAlive(controller.sensor, controller.canon, controller.motor);
                    break;
                case EnemyType.Scrapper:
                case EnemyType.Rusher:
                    targetComponent = PickFirstAlive(controller.canon, controller.sensor, controller.motor);
                    break;
                case EnemyType.Drainer:
                    targetComponent = PickFirstAlive(controller.motor, controller.sensor, controller.canon);
                    break;
            }
        }

        static TurretComponent_Arandia PickFirstAlive(params TurretComponent_Arandia[] order)
        {
            foreach (var c in order)
            {
                if (c != null && !c.IsBroken) return c;
            }
            return null;
        }

        IEnumerator AttackRoutine()
        {
            isAttacking = true;
            while (targetTurret != null && targetComponent != null && !targetComponent.IsBroken
                   && Vector3.Distance(transform.position, targetComponent.transform.position) <= attackRange)
            {
                var ctrl = targetTurret.GetComponent<TurretController_Arandia>();
                if (ctrl == null || ctrl.IsDestroyed)
                    break;

                Debug.Log($"[IA] {name} ({type}) atacando {targetComponent.gameObject.name}");
                targetComponent.TakeDamage(damagePerHit);
                yield return new WaitForSeconds(attackInterval);
            }
            isAttacking = false;
        }
    }
}
