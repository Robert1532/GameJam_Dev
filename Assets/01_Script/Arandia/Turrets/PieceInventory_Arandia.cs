// PieceInventory_Arandia.cs
// Responsable: Arandia
// Descripcion: Inventario de piezas del jugador (drop de enemigos, consumidas al reparar)

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastMachine.Arandia
{
    public class PieceInventory_Arandia : MonoBehaviour
    {
        [Header("Inventario - Arandia")]
        public int startingPieces = 5;
        public int maxPieces = 20;

        [Header("UI")]
        public TextMeshProUGUI piecesCountText;
        public Image piecesIcon;

        private int currentPieces;

        public int CurrentPieces => currentPieces;

        void Start()
        {
            currentPieces = startingPieces;
            RefreshUI();
        }

        public bool HasPieces(int amount)
        {
            return currentPieces >= amount;
        }

        public void ConsumePieces(int amount)
        {
            currentPieces = Mathf.Max(0, currentPieces - amount);
            RefreshUI();
        }

        public void AddPieces(int amount)
        {
            currentPieces = Mathf.Min(maxPieces, currentPieces + amount);
            RefreshUI();
            Debug.Log($"[Arandia] +{amount} piezas. Total: {currentPieces}");
        }

        private void RefreshUI()
        {
            if (piecesCountText != null)
                piecesCountText.text = currentPieces.ToString();

            // Color de alerta cuando quedan pocas piezas
            if (piecesIcon != null)
                piecesIcon.color = currentPieces <= 2
                    ? new Color(0.890f, 0.141f, 0.290f)  // rojo #E24B4A
                    : Color.white;
        }
    }
}
