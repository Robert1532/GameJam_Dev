using UnityEngine;
using System.Collections;

namespace LastMachine.Arandia
{
    public class EnemyController_Arandia : MonoBehaviour
    {
        public float speed = 3f;
        public float attackRange = 2f;
        public int damagePerHit = 10;
        public float attackInterval = 1.5f;

        private GameObject targetTurret;
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

            float distance = Vector3.Distance(transform.position, targetTurret.transform.position);

            if (distance > attackRange)
            {
                // MOVERSE HACIA LA TORRETA
                Vector3 direction = (targetTurret.transform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                transform.LookAt(new Vector3(targetTurret.transform.position.x, transform.position.y, targetTurret.transform.position.z));
            }
            else if (!isAttacking)
            {
                // ATACAR
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
                }
            }
        }

        IEnumerator AttackRoutine()
        {
            isAttacking = true;
            while (targetTurret != null && Vector3.Distance(transform.position, targetTurret.transform.position) <= attackRange)
            {
                var controller = targetTurret.GetComponent<TurretController_Arandia>();
                if (controller != null)
                {
                    Debug.Log("¡Enemigo atacando CAÑÓN de " + targetTurret.name + "!");
                    controller.canon.TakeDamage(damagePerHit);
                }
                yield return new WaitForSeconds(attackInterval);
            }
            isAttacking = false;
        }
    }
}
