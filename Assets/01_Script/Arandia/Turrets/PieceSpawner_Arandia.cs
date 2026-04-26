// PieceSpawner_Arandia.cs
// Responsable: Arandia
// Descripcion: Spawner de
//
// que aparecen durante la tregua para que el jugador pueda reparar.

using UnityEngine;

namespace LastMachine.Arandia
{
    public class PieceSpawner_Arandia : MonoBehaviour
    {
        [Header("Configuracion - Arandia")]
        public GameObject piecePrefab;
        public float spawnRadius = 15f;
        public int piecesPerWave = 4;

        [Header("Referencias")]
        public WaveManager_Arandia waveManager;

        void Start()
        {
            if (waveManager != null)
            {
                waveManager.OnTruceStart += SpawnPieces;
            }
        }

        public void SpawnPieces()
        {
            for (int i = 0; i < piecesPerWave; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = new Vector3(randomCircle.x, 0.5f, randomCircle.y);
                
                if (piecePrefab != null)
                {
                    Instantiate(piecePrefab, spawnPos, Quaternion.identity);
                }
            }
            Debug.Log($"[Arandia] Generadas {piecesPerWave} piezas en el campo.");
        }
    }
}
