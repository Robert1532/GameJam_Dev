using UnityEngine;
using TMPro;

namespace LastMachine.Arandia
{
    public class WorldPromptUI_Arandia : MonoBehaviour
    {
        public static WorldPromptUI_Arandia Instance;

        private TextMeshProUGUI text;

        void Awake()
        {
            Instance = this;

            // Crear Canvas
            GameObject canvasGO = new GameObject("WorldPromptCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Crear texto
            GameObject textGO = new GameObject("PromptText");
            textGO.transform.SetParent(canvasGO.transform);

            text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rt = text.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 80);
            rt.sizeDelta = new Vector2(600, 100);
        }

        public void Show(string message)
        {
            text.text = message;
        }

        public void Hide()
        {
            text.text = "";
        }
    }
}