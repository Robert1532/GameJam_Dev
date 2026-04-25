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

            // Moverse hacia el componente específico, no solo hacia la torreta
            Vector3 targetPos = targetComponent != null ? targetComponent.transform.position : targetTurret.transform.position;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > attackRange)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
            }
            else if (!isAttacking && targetComponent != null)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        void FindNearestTurret()
        {
            GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");
            float closestDist = Mathf.Infinity;

            foreach (GameObject t in turrets)
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetTurret = t;
                    AssignTargetComponent(t.GetComponent<TurretController_Arandia>());
                }
            }
        }

        void AssignTargetComponent(TurretController_Arandia controller)
        {
            if (controller == null) return;

            switch (type)
            {
                case EnemyType.Jammer:
                    targetComponent = controller.sensor;
                    break;
                case EnemyType.Scrapper:
                case EnemyType.Rusher:
                    targetComponent = controller.canon;
                    break;
                case EnemyType.Drainer:
                    targetComponent = controller.motor;
                    break;
            }
        }

        IEnumerator AttackRoutine()
        {
            isAttacking = true;
            while (targetComponent != null && !targetComponent.IsBroken && Vector3.Distance(transform.position, targetComponent.transform.position) <= attackRange)
            {
                Debug.Log($"[IA] {name} ({type}) atacando {targetComponent.gameObject.name}");
                targetComponent.TakeDamage(damagePerHit);
                yield return new WaitForSeconds(attackInterval);
            }
            isAttacking = false;
        }
    }
}
