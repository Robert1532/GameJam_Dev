using UnityEngine;

namespace LastMachine.Arandia
{
    public class EnemyHealth_Arandia : MonoBehaviour
    {
        public int maxHealth = 50;
        private int currentHealth;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log("Enemigo herido: " + currentHealth + " HP");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            Debug.Log("¡Enemigo DESTRUIDO!");
            Destroy(gameObject);
        }
    }
}
