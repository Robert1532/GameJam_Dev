using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastMachine.Arandia
{
    /// <summary>
    /// Adds a HUD top bar (HudTopBar/HudOla/HudVidaBase/HudPiezas) to scenes that use HUDBuilder_Arandia,
    /// and wires it to EdwinGameplayHudController for dynamic values.
    /// </summary>
    [ExecuteAlways]
    public sealed class HudTopBarInstaller_Arandia : MonoBehaviour
    {
        const string RuntimeCanvasName = "HUD_Canvas";
        const string PreviewCanvasName = "HUD_Preview";

        [Header("Layout")]
        [SerializeField] Vector2 topBarAnchoredPosition = new Vector2(16, -12);
        [SerializeField] Vector2 topBarSize = new Vector2(632, 54);

        [Header("TMP Font")]
        [SerializeField] TMP_FontAsset vt323FontAsset;

        [Header("Optional wiring overrides")]
        [SerializeField] WaveManager_Arandia waveManager;
        [SerializeField] PieceInventory_Arandia pieceInventory;

        void OnEnable()
        {
            if (!Application.isPlaying)
                EnsureInEditMode();
        }

        IEnumerator Start()
        {
            if (!Application.isPlaying)
                yield break;

            // Ensure HUDBuilder has created HUD_Canvas.
            yield return null;

            var builder = FindFirstObjectByType<HUDBuilder_Arandia>();
            if (builder == null)
                yield break;

            var canvas = builder.transform.Find(RuntimeCanvasName);
            if (canvas == null)
                yield break;

            var existing = canvas.Find("HudTopBar");
            if (existing != null)
                yield break;

            var hudTopBar = CreateHudTopBar(canvas);

            var ola = hudTopBar.transform.Find("HudOla")?.GetComponent<TextMeshProUGUI>();
            var vida = hudTopBar.transform.Find("HudVidaBase")?.GetComponent<TextMeshProUGUI>();
            var piezas = hudTopBar.transform.Find("HudPiezas")?.GetComponent<TextMeshProUGUI>();

            var controller = hudTopBar.AddComponent<EdwinGameplayHudController>();
            controller.SendMessage("ApplyHudFontFromSource", SendMessageOptions.DontRequireReceiver);

            // Set fields via serialized backing (private) using Unity serialization:
            // We can't access private fields directly; expose via reflection safely.
            var t = typeof(EdwinGameplayHudController);
            SetField(t, controller, "olaText", ola);
            SetField(t, controller, "vidaBaseText", vida);
            SetField(t, controller, "piezasText", piezas);
            SetField(t, controller, "hudTmpFontAsset", vt323FontAsset);

            var wm = waveManager != null ? waveManager : builder.waveManager != null ? builder.waveManager : FindFirstObjectByType<WaveManager_Arandia>();
            var inv = pieceInventory != null ? pieceInventory : builder.repairSystem != null ? builder.repairSystem.GetComponent<PieceInventory_Arandia>() : FindFirstObjectByType<PieceInventory_Arandia>();

            SetField(t, controller, "waveManager", wm);
            SetField(t, controller, "pieceInventory", inv);
            // Leave turrets empty: controller auto-finds in Awake if needed.
        }

        static void SetField(System.Type t, object instance, string fieldName, object value)
        {
            var f = t.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f != null)
                f.SetValue(instance, value);
        }

        void EnsureInEditMode()
        {
            var builder = FindFirstObjectByType<HUDBuilder_Arandia>();
            if (builder == null)
                return;

            // If the scene already has a runtime-style HUD in edit mode, don't create a preview.
            if (builder.transform.Find(RuntimeCanvasName) != null)
                return;

            var previewCanvas = builder.transform.Find(PreviewCanvasName);
            if (previewCanvas == null)
            {
                var go = new GameObject(PreviewCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                go.transform.SetParent(builder.transform, false);
                previewCanvas = go.transform;

                var canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (previewCanvas.Find("HudTopBar") == null)
                CreateHudTopBar(previewCanvas);
        }

        GameObject CreateHudTopBar(Transform parent)
        {
            var hudTopBar = new GameObject("HudTopBar", typeof(RectTransform));
            hudTopBar.transform.SetParent(parent, false);

            var rt = (RectTransform)hudTopBar.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = topBarAnchoredPosition;
            rt.sizeDelta = topBarSize;

            CreateHudText(hudTopBar.transform, "HudOla", new Vector2(10, -8), new Vector2(190, 40), "OLA: —");
            CreateHudText(hudTopBar.transform, "HudVidaBase", new Vector2(220, -8), new Vector2(190, 40), "VIDA BASE: 100%");
            CreateHudText(hudTopBar.transform, "HudPiezas", new Vector2(430, -8), new Vector2(192, 40), "PIEZAS: 0");

            return hudTopBar;
        }

        TextMeshProUGUI CreateHudText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, string initialText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = initialText;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            if (vt323FontAsset != null)
                tmp.font = vt323FontAsset;

            return tmp;
        }
    }
}

