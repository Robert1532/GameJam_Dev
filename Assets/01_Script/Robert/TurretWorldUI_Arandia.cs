using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LastMachine.Arandia
{
    public class TurretWorldUI_Arandia : MonoBehaviour
    {
        public TurretController_Arandia turret;

        [Header("Sensor")]
        public Image sensorFill;
        public TextMeshProUGUI sensorText;

        [Header("Canon")]
        public Image canonFill;
        public TextMeshProUGUI canonText;

        [Header("Motor")]
        public Image motorFill;
        public TextMeshProUGUI motorText;

        void Awake()
        {
            // 🔥 Auto-detectar torreta si no está asignada
            if (turret == null)
                turret = GetComponentInParent<TurretController_Arandia>();
        }

        void Update()
        {
            if (turret == null) return;

            UpdateBar(sensorFill, sensorText, turret.sensor);
            UpdateBar(canonFill, canonText, turret.canon);
            UpdateBar(motorFill, motorText, turret.motor);
        }

        void UpdateBar(Image bar, TextMeshProUGUI txt, TurretComponent_Arandia comp)
        {
            if (comp == null || bar == null || txt == null) return;

            float pct = Mathf.Clamp01(comp.HPPercent);

            // 🔹 Fill
            bar.fillAmount = pct;

            // 🔹 Texto
            txt.text = Mathf.RoundToInt(pct * 100) + "%";

            // 🔥 Color PROGRESIVO SUAVE (verde → amarillo → rojo REAL)
            Color color;

            if (pct > 0.5f)
            {
                float t = (pct - 0.5f) / 0.5f;
                color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                float t = pct / 0.5f;
                color = Color.Lerp(Color.red, Color.yellow, t);
            }

            bar.color = color;
        }
    }
}