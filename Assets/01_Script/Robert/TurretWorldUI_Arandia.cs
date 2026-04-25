using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LastMachine.Arandia
{
    public class TurretWorldUI_Arandia : MonoBehaviour
    {
        public TurretController_Arandia turret;

        [Header("Sensor")]
        public RectTransform sensorFill;
        public TextMeshProUGUI sensorText;

        [Header("Canon")]
        public RectTransform canonFill;
        public TextMeshProUGUI canonText;

        [Header("Motor")]
        public RectTransform motorFill;
        public TextMeshProUGUI motorText;

        void Update()
        {
            if (turret == null) return;

            UpdateBar(sensorFill, sensorText, turret.sensor);
            UpdateBar(canonFill, canonText, turret.canon);
            UpdateBar(motorFill, motorText, turret.motor);
        }

        void UpdateBar(RectTransform bar, TextMeshProUGUI txt, TurretComponent_Arandia comp)
        {
            if (comp == null) return;

            float pct = comp.HPPercent;

            // Cambiar tamaño
            bar.sizeDelta = new Vector2(120 * pct, bar.sizeDelta.y);

            // Texto
            txt.text = Mathf.RoundToInt(pct * 100) + "%";

            // Color
            if (pct > 0.5f)
                bar.GetComponent<Image>().color = Color.green;
            else if (pct > 0.2f)
                bar.GetComponent<Image>().color = Color.yellow;
            else
                bar.GetComponent<Image>().color = Color.red;
        }
    }
}