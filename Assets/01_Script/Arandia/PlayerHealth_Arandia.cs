// PlayerHealth_Arandia.cs
// Responsable: Arandia
// Fecha: 26/04/2026
// Actualizado por: Edwin Heredia
// Descripcion: Salud del jugador. Al recibir daño cancela cualquier reparación en curso.

using UnityEngine;

namespace LastMachine.Arandia
{
    public class PlayerHealth_Arandia : MonoBehaviour, IDamageable
    {
        [Header("Salud - Arandia")]
        public float maxHealth = 100f;

        [Header("Referencias")]
        [SerializeField] RepairSystem_Arandia repairSystem;

        public float CurrentHealth { get; private set; }

        public System.Action<float> OnHealthChanged;
        public System.Action OnPlayerDied;

        void Awake()
        {
            CurrentHealth = maxHealth;

            if (repairSystem == null)
                repairSystem = GetComponent<RepairSystem_Arandia>();
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth / maxHealth);

            repairSystem?.InterruptRepair();

            if (CurrentHealth <= 0f)
                OnPlayerDied?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
        }
    }
}
