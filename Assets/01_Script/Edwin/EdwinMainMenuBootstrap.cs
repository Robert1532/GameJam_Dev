using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Menú principal (LAST MACHINE). Si la UI ya está guardada bajo este objeto (Canvas hijo),
/// enlaza START y añade título / hint si faltan. Si no hay Canvas, la construye en Awake (fallback).
/// Para hornear la UI en la escena sin Play: menu Edwin - Build Main Menu In Edwin Scene.
/// </summary>
[DisallowMultipleComponent]
public sealed class EdwinMainMenuBootstrap : MonoBehaviour
{
    const string BgAssetPath = "Assets/05_Assets/Edwin/Backgrounds/bg_preview.png";
    const string BtnAssetPath = "Assets/05_Assets/Edwin/UI/button_start-preview.png";
    const string TitleAssetPath = "Assets/05_Assets/Edwin/UI/title-preview.png";

    /// <summary>
    /// Unity 6+ ya no admite <c>Arial.ttf</c> como built-in; usar solo Legacy para UI generada por código.
    /// </summary>
    public static Font MenuBuiltinFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    /// <summary>
    /// Rects y fuentes del título: copiados de <c>Assets/00_Scenes/Edwin.unity</c> (TitleTextOverlay + LAST/MACHINE a mano).
    /// Sin <see cref="VerticalLayoutGroup"/> para que en el editor puedas mover libremente; al hornear se recrean estos valores.
    /// </summary>
    const float TitleRootMaxWidth = 1100f;

    static readonly Vector2 TitleOverlayAnchoredPosition = new Vector2(0f, 98f);
    static readonly Vector2 TitleOverlaySizeDelta = new Vector2(1100f, 456f);

    static readonly Vector2 TitleLastAnchoredPosition = new Vector2(550f, -210f);
    static readonly Vector2 TitleLastSizeDelta = new Vector2(1076f, 72f);
    const int TitleLastFontSize = 140;

    static readonly Vector2 TitleMachineAnchoredPosition = new Vector2(550f, -328f);
    static readonly Vector2 TitleMachineSizeDelta = new Vector2(1076f, 64f);
    const int TitleMachineFontSize = 150;

    /// <summary>
    /// <c>TitleRoot</c> = cartel. <c>TitleImage</c> = sprite a pantalla completo del root. Textos en <c>TitleTextOverlay</c> con posiciones fijas (como en la escena).
    /// </summary>
    public static void AddTitleBlock(Transform canvas, int uiLayer, Sprite titleSprite)
    {
        var rootGo = new GameObject("TitleRoot");
        rootGo.transform.SetParent(canvas, false);
        rootGo.layer = uiLayer;
        var rootRt = rootGo.AddComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.88f);
        rootRt.anchorMax = new Vector2(0.5f, 0.88f);
        rootRt.pivot = new Vector2(0.5f, 1f);
        rootRt.anchoredPosition = Vector2.zero;

        float slotW = TitleRootMaxWidth;
        float slotH = TitleOverlaySizeDelta.y;
        if (titleSprite != null)
        {
            var rect = titleSprite.rect;
            var w = Mathf.Max(1f, rect.width);
            var h = Mathf.Max(1f, rect.height);
            slotW = Mathf.Min(TitleRootMaxWidth, w);
            slotH = slotW * (h / w);

            var imgGo = new GameObject("TitleImage");
            imgGo.transform.SetParent(rootGo.transform, false);
            imgGo.layer = uiLayer;
            var irt = imgGo.AddComponent<RectTransform>();
            StretchFull(irt);
            var img = imgGo.AddComponent<Image>();
            img.sprite = titleSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        rootRt.sizeDelta = new Vector2(slotW, slotH);

        var overlayGo = new GameObject("TitleTextOverlay");
        overlayGo.transform.SetParent(rootGo.transform, false);
        overlayGo.layer = uiLayer;
        var overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRt.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRt.pivot = new Vector2(0.5f, 0.5f);
        overlayRt.anchoredPosition = TitleOverlayAnchoredPosition;
        overlayRt.sizeDelta = TitleOverlaySizeDelta;

        AddTitleLineAnchored(
            overlayGo.transform, uiLayer, "TitleTextLast", "LAST",
            TitleLastAnchoredPosition, TitleLastSizeDelta, TitleLastFontSize);
        AddTitleLineAnchored(
            overlayGo.transform, uiLayer, "TitleTextMachine", "MACHINE",
            TitleMachineAnchoredPosition, TitleMachineSizeDelta, TitleMachineFontSize);

        rootGo.AddComponent<EdwinTitleIdleAnim>();
    }

    /// <summary>
    /// Asegura animación idle en <c>TitleRoot</c> si existe (escenas horneadas antes de añadir el script).
    /// </summary>
    public static void EnsureTitleIdleAnim(Transform canvas)
    {
        var titleRoot = canvas.Find("TitleRoot");
        if (titleRoot == null || titleRoot.GetComponent<EdwinTitleIdleAnim>() != null)
            return;
        titleRoot.gameObject.AddComponent<EdwinTitleIdleAnim>();
    }

    static void AddTitleLineAnchored(
        Transform parent,
        int uiLayer,
        string objectName,
        string line,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.layer = uiLayer;
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
        SetupTitleLineText(go.AddComponent<Text>(), line, fontSize, TextAnchor.MiddleCenter);
    }

    static void SetupTitleLineText(Text t, string line, int fontSize, TextAnchor alignment)
    {
        t.text = line;
        t.alignment = alignment;
        t.color = new Color(0.96f, 0.9f, 0.58f, 1f);
        t.font = MenuBuiltinFont();

        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
    }

    /// <summary>
    /// Texto bajo el botón START (misma posición que <c>StartButton</c>: mitad de altura 130 + separación).
    /// </summary>
    public static void AddPressStartHint(Transform canvas, int uiLayer)
    {
        var hintGo = new GameObject("PressStartHint");
        hintGo.transform.SetParent(canvas, false);
        hintGo.layer = uiLayer;
        var rt = hintGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.32f);
        rt.anchorMax = new Vector2(0.5f, 0.32f);
        rt.pivot = new Vector2(0.5f, 1f);
        const float startButtonHalfHeight = 65f;
        const float gapBelowButton = 14f;
        rt.anchoredPosition = new Vector2(0f, -(startButtonHalfHeight + gapBelowButton));
        rt.sizeDelta = new Vector2(800f, 40f);

        var hint = hintGo.AddComponent<Text>();
        hint.text = "Press START to begin";
        hint.alignment = TextAnchor.UpperCenter;
        hint.color = new Color(0.82f, 0.76f, 0.64f, 0.9f);
        hint.font = MenuBuiltinFont();
        hint.fontSize = 22;
        hint.fontStyle = FontStyle.Normal;
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        hint.verticalOverflow = VerticalWrapMode.Overflow;
        hint.raycastTarget = false;
    }

    [SerializeField] string nextSceneName = "Main";

    void Awake()
    {
        EnsureEventSystem();

        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            WireStartButton(canvas.transform);
            return;
        }

        BuildUiFromSprites();
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;
        var es = new GameObject("EventSystem");
        es.transform.SetParent(transform, false);
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    void WireStartButton(Transform canvasTransform)
    {
        var button = canvasTransform.GetComponentInChildren<Button>(true);
        if (button == null)
        {
            Debug.LogWarning("[EdwinMainMenuBootstrap] Hay Canvas pero no se encontró Button.");
            return;
        }

        ConfigureStartButton(button);
        button.onClick.RemoveListener(OnStartClicked);
        button.onClick.AddListener(OnStartClicked);

        if (canvasTransform.Find("PressStartHint") == null)
        {
            var layer = canvasTransform.gameObject.layer;
            AddPressStartHint(canvasTransform, layer);
        }

        var titleRoot = canvasTransform.Find("TitleRoot");
        if (titleRoot != null && titleRoot.Find("TitleTextOverlay") == null)
            Object.Destroy(titleRoot.gameObject);

        if (canvasTransform.Find("TitleRoot") == null)
        {
            var legacyTitle = canvasTransform.Find("TitleText");
            if (legacyTitle != null)
                Object.Destroy(legacyTitle.gameObject);

            var layer = canvasTransform.gameObject.layer;
            var titleSprite = LoadSprite(TitleAssetPath, "title-preview");
            if (titleSprite == null)
            {
                Debug.LogWarning(
                    "[EdwinMainMenuBootstrap] No se cargó title-preview; se añaden sólo las líneas LAST / MACHINE.");
            }

            AddTitleBlock(canvasTransform, layer, titleSprite);
        }

        EnsureTitleIdleAnim(canvasTransform);
    }

    void ConfigureStartButton(Button button)
    {
        button.transition = Selectable.Transition.None;
        if (button.GetComponent<EdwinStartButtonHoverAnim>() == null)
            button.gameObject.AddComponent<EdwinStartButtonHoverAnim>();
    }

    void BuildUiFromSprites()
    {
        var bgSprite = LoadSprite(BgAssetPath, "bg_preview");
        var btnSprite = LoadSprite(BtnAssetPath, "button_start-preview");
        var titleSprite = LoadSprite(TitleAssetPath, "title-preview");
        if (bgSprite == null || btnSprite == null)
        {
            Debug.LogError(
                "[EdwinMainMenuBootstrap] Faltan sprites. Revisa import (Sprite 2D and UI) y Resources para builds.");
            return;
        }

        if (titleSprite == null)
        {
            Debug.LogWarning(
                "[EdwinMainMenuBootstrap] title-preview no encontrado; el menú mostrará sólo LAST / MACHINE (sin imagen).");
        }

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var canvasRt = canvasGo.GetComponent<RectTransform>();
        canvasRt.anchorMin = Vector2.zero;
        canvasRt.anchorMax = Vector2.one;
        canvasRt.offsetMin = Vector2.zero;
        canvasRt.offsetMax = Vector2.zero;

        const int uiLayer = 5;
        canvasGo.layer = uiLayer;

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(canvasGo.transform, false);
        bgGo.layer = uiLayer;
        var bgRt = bgGo.AddComponent<RectTransform>();
        StretchFull(bgRt);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = true;
        bgImg.raycastTarget = false;

        AddTitleBlock(canvasGo.transform, uiLayer, titleSprite);

        var btnGo = new GameObject("StartButton");
        btnGo.transform.SetParent(canvasGo.transform, false);
        btnGo.layer = uiLayer;
        var btnRt = btnGo.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.32f);
        btnRt.anchorMax = new Vector2(0.5f, 0.32f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(420, 130);
        btnRt.anchoredPosition = Vector2.zero;

        var btnImg = btnGo.AddComponent<Image>();
        btnImg.sprite = btnSprite;
        btnImg.type = Image.Type.Simple;
        var button = btnGo.AddComponent<Button>();
        button.targetGraphic = btnImg;

        var textGo = new GameObject("StartText");
        textGo.transform.SetParent(btnGo.transform, false);
        textGo.layer = uiLayer;
        var textRt = textGo.AddComponent<RectTransform>();
        StretchFull(textRt);
        var text = textGo.AddComponent<Text>();
        text.text = "START";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.98f, 0.82f, 0.45f);
        text.font = MenuBuiltinFont();
        text.fontSize = 40;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        ConfigureStartButton(button);
        button.onClick.AddListener(OnStartClicked);
        AddPressStartHint(canvasGo.transform, uiLayer);
    }

    void OnStartClicked()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
            return;
        SceneManager.LoadScene(nextSceneName);
    }

    static Sprite LoadSprite(string assetPath, string resourcesName)
    {
#if UNITY_EDITOR
        EnsureSpriteTextureImportForPlay(assetPath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        var main = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (main is Sprite sMain)
            return sMain;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (obj is Sprite s)
                return s;
        }

        return null;
#else
        return Resources.Load<Sprite>(resourcesName);
#endif
    }

#if UNITY_EDITOR
    static void EnsureSpriteTextureImportForPlay(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }
#endif

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
