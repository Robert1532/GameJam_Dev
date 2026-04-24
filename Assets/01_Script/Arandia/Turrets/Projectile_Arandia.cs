// Projectile_Arandia.cs
// Responsable: Arandia
// Descripcion: Proyectil disparado por la torreta. Se mueve hacia adelante,
//              impacta en enemigos y se autodestruye.

using UnityEngine;

namespace LastMachine.Arandia
{
    public class Projectile_Arandia : MonoBehaviour
    {
        [Header("Configuracion - Arandia")]
        public float speed = 15f;
        public float damage = 25f;
        public float lifeTime = 4f;         // segundos antes de autodestruirse
        public string enemyTag = "Enemy";

        [Header("Efectos")]
        public GameObject impactVFXPrefab;  // Opcional: partículas de impacto

        private float timer;

        void Update()
        {
            // Mover hacia adelante
            transform.position += transform.forward * speed * Time.deltaTime;

            // Auto-destruir si no impacta
            timer += Time.deltaTime;
            if (timer >= lifeTime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignorar la propia torreta
            if (!other.CompareTag(enemyTag)) return;

            // Aplicar daño al enemigo
            // Los enemigos deberán tener un componente con un método TakeDamage(float)
            // Intentamos con la interfaz genérica primero
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"[Arandia] Proyectil impactó en {other.name} por {damage} daño.");
            }

            SpawnImpactVFX();
            Destroy(gameObject);
        }

        private void SpawnImpactVFX()
        {
            if (impactVFXPrefab != null)
                Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
        }
    }

    // ──────────────────────────────────────────────
    //  Interfaz compartida para recibir daño
    //  Los enemigos del GameJam deben implementarla
    // ──────────────────────────────────────────────
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}
