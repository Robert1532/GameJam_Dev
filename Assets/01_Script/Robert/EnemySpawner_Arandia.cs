using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace LastMachine.Arandia
{
    public class EnemySpawner_Arandia : MonoBehaviour
    {
        [System.Serializable]
        public class EnemyData
        {
            public string enemyName;
            public GameObject prefab;
            [Range(1, 100)] public int spawnWeight = 10;
        }

        public Transform[] spawnPoints;
        public List<EnemyData> enemies = new List<EnemyData>();
        public GameObject bossPrefab;

        public float timeBetweenSpawn = 1.2f;
        public int baseEnemies = 6;
        public int extraEnemiesPerWave = 2;
        public int bossEveryWave = 5;

        public IEnumerator SpawnWave(int wave)
        {
            int totalEnemies = baseEnemies + (wave * extraEnemiesPerWave);

            for (int i = 0; i < totalEnemies; i++)
            {
                SpawnRandomEnemy();
                yield return new WaitForSeconds(timeBetweenSpawn);
            }

            if (wave % bossEveryWave == 0)
            {
                yield return new WaitForSeconds(2f);
                SpawnBoss();
            }
        }

        private void SpawnRandomEnemy()
        {
            if (spawnPoints.Length == 0 || enemies.Count == 0) return;

            GameObject prefab = GetWeightedEnemy();
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            NavMeshHit hit;

            if (NavMesh.SamplePosition(point.position, out hit, 10f, NavMesh.AllAreas))
            {
                Instantiate(prefab, hit.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("❌ No hay NavMesh en spawn");
            }
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null || spawnPoints.Length == 0) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            NavMeshHit hit;

            if (NavMesh.SamplePosition(point.position, out hit, 10f, NavMesh.AllAreas))
            {
                Instantiate(bossPrefab, hit.position, Quaternion.identity);
                Debug.Log("🔥 BOSS GENERADO");
            }
        }

        private GameObject GetWeightedEnemy()
        {
            int totalWeight = 0;

            foreach (var e in enemies)
                if (e.prefab != null)
                    totalWeight += e.spawnWeight;

            int random = Random.Range(0, totalWeight);
            int current = 0;

            foreach (var e in enemies)
            {
                if (e.prefab == null) continue;

                current += e.spawnWeight;
                if (random < current)
                    return e.prefab;
            }

            return enemies[0].prefab;
        }
    }
}