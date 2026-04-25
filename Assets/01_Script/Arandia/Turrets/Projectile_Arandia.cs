using UnityEngine;

namespace LastMachine.Arandia
{
    public class Projectile_Arandia : MonoBehaviour
    {
        public float speed = 15f;
        public float damage = 25f;
        public float lifeTime = 4f;
        public string enemyTag = "Enemy";

        public GameObject impactVFXPrefab;

        private float timer;

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            timer += Time.deltaTime;
            if (timer >= lifeTime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(enemyTag)) return;

            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"💥 Impacto a {other.name}");
            }
            else
            {
                Debug.LogWarning($"⚠ {other.name} NO tiene IDamageable");
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

    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}