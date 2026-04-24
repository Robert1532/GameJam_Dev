// =======================================================
// EnemySpawner_Arandia.cs
// Responsable: Arandia
// Descripcion:
// Genera enemigos aleatorios de varios tipos.
// Soporta cualquier cantidad de enemigos.
// Genera Boss cada X oleadas.
// =======================================================

using UnityEngine;
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

            [Range(1, 100)]
            public int spawnWeight = 10; // probabilidad
        }

        [Header("Spawn Points")]
        public Transform[] spawnPoints; // tus 4 puntos

        [Header("Enemigos Normales")]
        public List<EnemyData> enemies = new List<EnemyData>();

        [Header("Boss")]
        public GameObject bossPrefab;

        [Header("Configuracion")]
        public float timeBetweenSpawn = 1.2f;

        public int baseEnemies = 6;
        public int extraEnemiesPerWave = 2;

        public int bossEveryWave = 5;

        public bool randomBossPoint = true;

        // =====================================================
        // SPAWN DE OLEADA
        // =====================================================

        public IEnumerator SpawnWave(int wave)
        {
            int totalEnemies = baseEnemies + (wave * extraEnemiesPerWave);

            for (int i = 0; i < totalEnemies; i++)
            {
                SpawnRandomEnemy();
                yield return new WaitForSeconds(timeBetweenSpawn);
            }

            // Boss cada X oleadas
            if (wave % bossEveryWave == 0)
            {
                yield return new WaitForSeconds(2f);
                SpawnBoss();
            }
        }

        // =====================================================
        // ENEMIGOS NORMALES
        // =====================================================

        private void SpawnRandomEnemy()
        {
            if (spawnPoints.Length == 0) return;
            if (enemies.Count == 0) return;

            GameObject prefabToSpawn = GetWeightedEnemy();

            if (prefabToSpawn == null) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(
                prefabToSpawn,
                point.position,
                Quaternion.identity
            );
        }

        // =====================================================
        // BOSS
        // =====================================================

        private void SpawnBoss()
        {
            if (bossPrefab == null) return;
            if (spawnPoints.Length == 0) return;

            Transform point;

            if (randomBossPoint)
                point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            else
                point = spawnPoints[0];

            Instantiate(
                bossPrefab,
                point.position,
                Quaternion.identity
            );

            Debug.Log("BOSS GENERADO");
        }

        // =====================================================
        // SISTEMA DE PROBABILIDAD
        // =====================================================

        private GameObject GetWeightedEnemy()
        {
            int totalWeight = 0;

            foreach (EnemyData enemy in enemies)
            {
                if (enemy.prefab != null)
                    totalWeight += enemy.spawnWeight;
            }

            int random = Random.Range(0, totalWeight);

            int current = 0;

            foreach (EnemyData enemy in enemies)
            {
                if (enemy.prefab == null) continue;

                current += enemy.spawnWeight;

                if (random < current)
                {
                    return enemy.prefab;
                }
            }

            return enemies[0].prefab;
        }
    }
}