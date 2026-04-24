// PiecePickup_Arandia.cs
// Responsable: Arandia
// Descripcion: Objeto recolectable que añade piezas al inventario del jugador.

using UnityEngine;

namespace LastMachine.Arandia
{
    public class PiecePickup_Arandia : MonoBehaviour
    {
        [Header("Configuracion")]
        public int piecesAmount = 3;
        public float rotationSpeed = 100f;
        public float floatSpeed = 2f;
        public float floatAmount = 0.2f;

        private Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
            // Destruirse tras 20 segundos si no se recoge
            Destroy(gameObject, 20f);
        }

        void Update()
        {
            // Animacion: girar y flotar
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PieceInventory_Arandia inventory = other.GetComponent<PieceInventory_Arandia>();
                if (inventory != null)
                {
                    inventory.AddPieces(piecesAmount);
                    // Podríamos instanciar un efecto de partículas aquí
                    Destroy(gameObject);
                }
            }
        }
    }
}
