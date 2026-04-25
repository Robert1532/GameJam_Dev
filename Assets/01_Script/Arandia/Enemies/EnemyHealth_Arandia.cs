using UnityEngine;

namespace LastMachine.Arandia
{
    public class EnemyHealth_Arandia : MonoBehaviour, IDamageable
    {
        public float maxHealth = 50f;
        private float currentHealth;

        void Start()
        {
            currentHealth = maxHealth;
        }

        // 🔥 ESTE ES EL MÉTODO QUE TU PROYECTIL USA
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            Debug.Log($"💥 {gameObject.name} recibió {damage} daño. HP: {currentHealth}");

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        void Die()
        {
            Debug.Log($"🔥 {gameObject.name} DESTRUIDO");
            Destroy(gameObject);
        }
    }
}