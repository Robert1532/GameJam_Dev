//using UnityEngine;
//using TMPro;

//namespace LastMachine.Arandia
//{
//    public class HUDBuilder_Arandia : MonoBehaviour
//    {
//        public static HUDBuilder_Arandia Instance;

//        [Header("Prompt")]
//        public TextMeshProUGUI promptText;

//        void Awake()
//        {
//            Instance = this;
//        }

//        public void ShowPrompt()
//        {
//            if (promptText == null) return;

//            promptText.gameObject.SetActive(true);
//            promptText.text = "Presiona [E] para reparar la torreta";
//        }

//        public void HidePrompt()
//        {
//            if (promptText == null) return;

//            promptText.gameObject.SetActive(false);
//        }
//    }
//}