// TurretComponent_Arandia.cs
// Responsable: Arandia
// Descripcion: HP independiente por componente de torreta (Sensor, Canon, Motor)

using UnityEngine;

namespace LastMachine.Arandia
{
    public enum ComponentType { Sensor, Canon, Motor }

    public enum ComponentState { OK, Damaged, Broken }

    public class TurretComponent_Arandia : MonoBehaviour
    {
        [Header("Configuracion - Arandia")]
        public ComponentType componentType;
        public float maxHP = 100f;

        [Header("Estado")]
        [SerializeField] private float currentHP;
        [SerializeField] private ComponentState state;

        [Header("Referencias de Animacion")]
        public Animator componentAnimator;

        // Umbrales de estado
        private const float DAMAGED_THRESHOLD = 0.5f; // 50% HP = Damaged
        private const float BROKEN_THRESHOLD = 0f;    // 0% HP = Broken

        // Eventos
        public System.Action<TurretComponent_Arandia> OnComponentBroken;
        public System.Action<TurretComponent_Arandia> OnComponentRepaired;
        public System.Action<TurretComponent_Arandia, float> OnComponentDamaged;

        public float CurrentHP => currentHP;
        public float HPPercent => currentHP / maxHP;
        public ComponentState State => state;
        public bool IsBroken => state == ComponentState.Broken;
        public bool IsDamaged => state == ComponentState.Damaged;

        void Awake()
        {
            // La escena/prefab suele guardar currentHP en 0; inicializar antes que otros Awake/Update
            // evita un frame donde el GameManager cree que la fábrica ya está destruida.
            if (maxHP > 0f && currentHP <= 0f)
                currentHP = maxHP;
        }

        void Start()
        {
            currentHP = maxHP;
            state = ComponentState.OK;
            UpdateAnimationState();
        }

        public void TakeDamage(float amount)
        {
            if (state == ComponentState.Broken) return;

            currentHP = Mathf.Max(0f, currentHP - amount);
            UpdateState();
            OnComponentDamaged?.Invoke(this, amount);
            UpdateAnimationState();
        }

        public void Repair(float amount)
        {
            bool wasBroken = state == ComponentState.Broken;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            UpdateState();

            if (wasBroken && state != ComponentState.Broken)
                OnComponentRepaired?.Invoke(this);

            UpdateAnimationState();
        }

        private void UpdateState()
        {
            ComponentState newState;

            if (currentHP <= BROKEN_THRESHOLD)
                newState = ComponentState.Broken;
            else if (HPPercent <= DAMAGED_THRESHOLD)
                newState = ComponentState.Damaged;
            else
                newState = ComponentState.OK;

            if (newState != state)
            {
                state = newState;
                if (state == ComponentState.Broken)
                    OnComponentBroken?.Invoke(this);
            }
        }

        private void UpdateAnimationState()
        {
            if (componentAnimator == null) return;

            switch (state)
            {
                case ComponentState.OK:
                    componentAnimator.SetTrigger("Idle");
                    break;
                case ComponentState.Damaged:
                    componentAnimator.SetTrigger("Damaged");
                    break;
                case ComponentState.Broken:
                    componentAnimator.SetTrigger("Broken");
                    break;
            }
        }

        // Retorna color segun estado para el HUD
        public Color GetStateColor()
        {
            switch (state)
            {
                case ComponentState.OK:      return new Color(0.114f, 0.851f, 0.459f); // verde #1D9E75
                case ComponentState.Damaged: return new Color(0.729f, 0.459f, 0.090f); // naranja #BA7517
                case ComponentState.Broken:  return new Color(0.890f, 0.141f, 0.290f); // rojo #E24B4A
                default: return Color.white;
            }
        }
    }
}
