using UnityEngine;

namespace LastMachine.Arandia
{
    public class EnemyHealth_Arandia : MonoBehaviour, IDamageable
    {
        public float maxHealth = 100f;
        private float currentHealth;
        private bool isDead = false;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            Debug.Log($"💥 {gameObject.name} recibió {damage} daño. HP: {currentHealth}");

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"🔥 {gameObject.name} DESTRUIDO");

            // Llama al script del enemigo si tiene uno
            SendMessage("Morir", SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject);
        }
    }
}