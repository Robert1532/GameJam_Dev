using System.Collections;
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
    const string BgAssetPath = "Assets/05_Assets/Edwin/Backgrounds/bg_2_preview.png";
    const string BtnAssetPath = "Assets/05_Assets/Edwin/UI/button_start-preview.png";
    const string TitleAssetPath = "Assets/05_Assets/Edwin/UI/title-preview.png";
    const string MenuMusicAssetPath = "Assets/05_Assets/Edwin/Audio/Music/LAST MACHINE - Main Menu.mp3";
    const string StartClickAssetPath = "Assets/05_Assets/Edwin/Audio/SFX/button-start-click.mp3";
    const string StartHoverAssetPath = "Assets/05_Assets/Edwin/Audio/SFX/button-start-hover.wav";
    const string TitleLineFontAssetPath = "Assets/05_Assets/Edwin/Fonts/RussoOne-Regular.ttf";

    /// <summary>
    /// Unity 6+ ya no admite <c>Arial.ttf</c> como built-in; usar solo Legacy para UI generada por código.
    /// </summary>
    public static Font MenuBuiltinFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    static Color ByteRgba(byte r, byte g, byte b, byte a = 255) => new Color(r / 255f, g / 255f, b / 255f, a / 255f);

    /// <summary>Color LAST / MACHINE / START (#fcae0eff).</summary>
    public static readonly Color MenuTitleLastColor = ByteRgba(0xFC, 0xAE, 0x0E);

    /// <summary>Igual que LAST (#fcae0eff).</summary>
    public static readonly Color MenuTitleMachineColor = ByteRgba(0xFC, 0xAE, 0x0E);

    /// <summary>Color del texto START (#fcae0eff).</summary>
    public static readonly Color MenuButtonStartTextColor = ByteRgba(0xFC, 0xAE, 0x0E);

    /// <summary>Color del texto del botón CREDITS.</summary>
    public static readonly Color MenuButtonCreditsTextColor = ByteRgba(0x2B, 0x9F, 0xAD);

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
    /// <param name="titleLineFont">LAST / MACHINE; si es null se usa <see cref="MenuBuiltinFont"/>.</param>
    public static void AddTitleBlock(Transform canvas, int uiLayer, Sprite titleSprite, Font titleLineFont)
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
            TitleLastAnchoredPosition, TitleLastSizeDelta, TitleLastFontSize, titleLineFont);
        AddTitleLineAnchored(
            overlayGo.transform, uiLayer, "TitleTextMachine", "MACHINE",
            TitleMachineAnchoredPosition, TitleMachineSizeDelta, TitleMachineFontSize, titleLineFont);

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
        int fontSize,
        Font titleLineFont)
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
        SetupTitleLineText(go.AddComponent<Text>(), line, fontSize, TextAnchor.MiddleCenter, titleLineFont);
    }

    static void SetupTitleLineText(Text t, string line, int fontSize, TextAnchor alignment, Font titleLineFont)
    {
        t.text = line;
        t.alignment = alignment;
        t.color = line == "LAST" || line == "MACHINE"
            ? MenuTitleLastColor
            : new Color(0.96f, 0.9f, 0.58f, 1f);
        var f = titleLineFont != null ? titleLineFont : MenuBuiltinFont();
        t.font = f;

        t.fontSize = fontSize;
        t.fontStyle = titleLineFont != null ? FontStyle.Normal : FontStyle.Bold;
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

    [SerializeField] AudioClip menuBackgroundMusic;
    [SerializeField] AudioClip startButtonClickSfx;
    [SerializeField] AudioClip startButtonHoverSfx;
    [SerializeField] Font titleLineFont;
    [SerializeField, Range(0f, 1f)] float menuMusicVolume = 0.20f;
    [SerializeField, Range(0f, 1f)] float startClickVolume = 1f;
    [SerializeField, Range(0f, 1f)] float startHoverVolume = 1f;

    [SerializeField] string nextSceneName = "Robert";

    AudioSource _musicSource;
    AudioSource _uiSfxSource;
    static Sprite _cachedUiWhiteSprite;

    void Awake()
    {
        EnsureEventSystem();
        ResolveMenuAudioClips();
        ResolveTitleLineFont();
        EnsureMenuAudioPlayback();

        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
            WireStartButton(canvas.transform);
        else
            BuildUiFromSprites();

        canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            ApplyTitleOverlayLineFonts(canvas.transform);
            ApplyMenuActionButtonLabelColors(canvas.transform);
        }

        SetupMenuMusicVolumeSlider();
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
        var startButton = FindChildButtonByGameObjectName(canvasTransform, "StartButton");
        if (startButton == null)
            startButton = canvasTransform.GetComponentInChildren<Button>(true);

        if (startButton == null)
        {
            Debug.LogWarning("[EdwinMainMenuBootstrap] Hay Canvas pero no se encontró ningún Button.");
            return;
        }

        ConfigureMenuButtonAudio(startButton);
        startButton.onClick.RemoveListener(OnStartClicked);
        startButton.onClick.AddListener(OnStartClicked);

        var creditButton = FindChildButtonByGameObjectName(canvasTransform, "CreditButton")
            ?? FindChildButtonByGameObjectName(canvasTransform, "CreditsButton");
        if (creditButton == null)
        {
            foreach (var b in canvasTransform.GetComponentsInChildren<Button>(true))
            {
                var tx = b.GetComponentInChildren<Text>(true);
                if (tx != null && tx.text != null && tx.text.Trim() == "CREDITS")
                {
                    creditButton = b;
                    break;
                }
            }
        }

        if (creditButton != null)
        {
            ConfigureMenuButtonAudio(creditButton);
            creditButton.onClick.RemoveListener(OnCreditsClicked);
            creditButton.onClick.AddListener(OnCreditsClicked);
        }

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

            AddTitleBlock(canvasTransform, layer, titleSprite, titleLineFont);
        }

        EnsureTitleIdleAnim(canvasTransform);
    }

    static Button FindChildButtonByGameObjectName(Transform canvas, string gameObjectName)
    {
        foreach (var button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name == gameObjectName)
                return button;
        }

        return null;
    }

    void ConfigureMenuButtonAudio(Button button)
    {
        button.transition = Selectable.Transition.None;
        var hover = button.GetComponent<EdwinStartButtonHoverAnim>();
        if (hover == null)
            hover = button.gameObject.AddComponent<EdwinStartButtonHoverAnim>();
        hover.SetUiSfxSource(_uiSfxSource, startButtonHoverSfx, startHoverVolume);
    }

    void OnCreditsClicked()
    {
        PlayStartClickSound();
    }

    void BuildUiFromSprites()
    {
        var bgSprite = LoadSprite(BgAssetPath, "bg_2_preview");
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

        AddTitleBlock(canvasGo.transform, uiLayer, titleSprite, titleLineFont);

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
        text.color = MenuButtonStartTextColor;
        text.font = MenuBuiltinFont();
        text.fontSize = 40;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        ConfigureMenuButtonAudio(button);
        button.onClick.AddListener(OnStartClicked);
        AddPressStartHint(canvasGo.transform, uiLayer);
    }

    void OnStartClicked()
    {
        PlayStartClickSound();
        if (string.IsNullOrWhiteSpace(nextSceneName))
            return;
        StartCoroutine(LoadNextSceneAfterClickSfx());
    }

    IEnumerator LoadNextSceneAfterClickSfx()
    {
        var clip = startButtonClickSfx;
        // Clips muy cortos: Unity a veces reporta duración casi 0; damos margen mínimo para el DSP.
        var len = clip != null ? clip.length : 0f;
        var wait = Mathf.Max(0.22f, len + 0.12f);
        yield return new WaitForSecondsRealtime(wait);
        SceneManager.LoadScene(nextSceneName);
    }

    void ResolveMenuAudioClips()
    {
#if UNITY_EDITOR
        if (menuBackgroundMusic == null)
            menuBackgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>(MenuMusicAssetPath);
        if (startButtonClickSfx == null)
            startButtonClickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(StartClickAssetPath);
        if (startButtonHoverSfx == null)
            startButtonHoverSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(StartHoverAssetPath);
#else
        if (menuBackgroundMusic == null)
            menuBackgroundMusic = Resources.Load<AudioClip>("Audio/EdwinMenuMusic");
        if (startButtonClickSfx == null)
            startButtonClickSfx = Resources.Load<AudioClip>("Audio/EdwinStartClick");
        if (startButtonHoverSfx == null)
            startButtonHoverSfx = Resources.Load<AudioClip>("Audio/EdwinStartHover");
#endif
    }

    void ResolveTitleLineFont()
    {
#if UNITY_EDITOR
        if (titleLineFont == null)
            titleLineFont = AssetDatabase.LoadAssetAtPath<Font>(TitleLineFontAssetPath);
#else
        if (titleLineFont == null)
            titleLineFont = Resources.Load<Font>("Fonts/RussoOne-Regular");
#endif
    }

    void ApplyTitleOverlayLineFonts(Transform canvasTransform)
    {
        ResolveTitleLineFont();
        var f = titleLineFont != null ? titleLineFont : MenuBuiltinFont();
        var overlay = canvasTransform.Find("TitleRoot/TitleTextOverlay");
        if (overlay == null)
            return;
        foreach (var tr in overlay.GetComponentsInChildren<Transform>(true))
        {
            if (tr.name != "TitleTextLast" && tr.name != "TitleTextMachine")
                continue;
            var text = tr.GetComponent<Text>();
            if (text == null)
                continue;
            text.font = f;
            text.fontStyle = titleLineFont != null ? FontStyle.Normal : FontStyle.Bold;
            if (tr.name == "TitleTextLast")
                text.color = MenuTitleLastColor;
            else if (tr.name == "TitleTextMachine")
                text.color = MenuTitleMachineColor;
        }
    }

    void ApplyMenuActionButtonLabelColors(Transform canvasTransform)
    {
        foreach (var text in canvasTransform.GetComponentsInChildren<Text>(true))
        {
            var s = text.text != null ? text.text.Trim() : string.Empty;
            if (s == "START")
                text.color = MenuButtonStartTextColor;
            else if (s == "CREDITS")
                text.color = MenuButtonCreditsTextColor;
        }

        foreach (var hover in canvasTransform.GetComponentsInChildren<EdwinStartButtonHoverAnim>(true))
            hover.SyncBaseTextColorFromLabel();
    }

    void EnsureMenuAudioPlayback()
    {
        var holder = transform.Find("MenuAudio_Root");
        if (holder == null)
        {
            var root = new GameObject("MenuAudio_Root");
            root.transform.SetParent(transform, false);

            var mGo = new GameObject("MenuMusic");
            mGo.transform.SetParent(root.transform, false);
            _musicSource = mGo.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;

            var sGo = new GameObject("MenuUiSfx");
            sGo.transform.SetParent(root.transform, false);
            _uiSfxSource = sGo.AddComponent<AudioSource>();
            _uiSfxSource.playOnAwake = false;
            _uiSfxSource.loop = false;
            _uiSfxSource.spatialBlend = 0f;
            _uiSfxSource.volume = 1f;
            _uiSfxSource.dopplerLevel = 0f;
            _uiSfxSource.ignoreListenerPause = true;
        }
        else
        {
            _musicSource = holder.Find("MenuMusic")?.GetComponent<AudioSource>();
            _uiSfxSource = holder.Find("MenuUiSfx")?.GetComponent<AudioSource>();
            if (_uiSfxSource == null)
            {
                var sGo = new GameObject("MenuUiSfx");
                sGo.transform.SetParent(holder, false);
                _uiSfxSource = sGo.AddComponent<AudioSource>();
                _uiSfxSource.playOnAwake = false;
                _uiSfxSource.loop = false;
                _uiSfxSource.spatialBlend = 0f;
                _uiSfxSource.volume = 1f;
                _uiSfxSource.dopplerLevel = 0f;
                _uiSfxSource.ignoreListenerPause = true;
            }
        }

        if (_uiSfxSource != null)
            _uiSfxSource.volume = 1f;

        if (_musicSource != null)
        {
            _musicSource.volume = menuMusicVolume;
            _musicSource.clip = menuBackgroundMusic;
            if (menuBackgroundMusic != null && !_musicSource.isPlaying)
                _musicSource.Play();
        }
    }

    void PlayStartClickSound()
    {
        EnsureMenuAudioPlayback();
        ResolveMenuAudioClips();
        if (startButtonClickSfx == null)
            return;

        if (startButtonClickSfx.loadState == AudioDataLoadState.Unloaded)
            startButtonClickSfx.LoadAudioData();

        if (_uiSfxSource == null)
            return;

        var vol = Mathf.Clamp01(startClickVolume);
        _uiSfxSource.enabled = true;
        _uiSfxSource.playOnAwake = false;
        _uiSfxSource.loop = false;
        _uiSfxSource.spatialBlend = 0f;
        _uiSfxSource.panStereo = 0f;
        _uiSfxSource.priority = 0;
        _uiSfxSource.ignoreListenerPause = true;
        _uiSfxSource.mute = false;
        _uiSfxSource.volume = 1f;
        _uiSfxSource.PlayOneShot(startButtonClickSfx, vol);
    }

    void OnMenuMusicVolumeSliderChanged(float value)
    {
        menuMusicVolume = value;
        if (_musicSource != null)
            _musicSource.volume = menuMusicVolume;
    }

    /// <summary>
    /// Crea el bloque de volumen (misma jerarquía / estilo que <c>PressStartHint</c> en Edwin.unity).
    /// <paramref name="siblingIndex"/> = posición entre hijos del Canvas (p. ej. 1 = justo debajo del fondo).
    /// No registra listeners; el bootstrap los enlaza en <see cref="SetupMenuMusicVolumeSlider"/>.
    /// </summary>
    public static Slider BuildMenuMusicVolumeSliderUi(Transform canvas, int uiLayer, int siblingIndex)
    {
        if (canvas == null)
            return null;

        var rootGo = new GameObject("MenuMusicVolumeRoot");
        rootGo.transform.SetParent(canvas, false);
        rootGo.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, canvas.childCount - 1));
        rootGo.layer = uiLayer;

        var rootRt = rootGo.AddComponent<RectTransform>();
        // Misma anchura que PressStartHint (800); anclaje inferior como el resto del menú horneado.
        rootRt.anchorMin = new Vector2(0.5f, 0.08f);
        rootRt.anchorMax = new Vector2(0.5f, 0.08f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.sizeDelta = new Vector2(800f, 56f);
        rootRt.anchoredPosition = new Vector2(0f, 10f);

        var capGo = new GameObject("Caption");
        capGo.transform.SetParent(rootGo.transform, false);
        capGo.layer = uiLayer;
        var capRt = capGo.AddComponent<RectTransform>();
        capRt.anchorMin = new Vector2(0f, 0.52f);
        capRt.anchorMax = new Vector2(1f, 1f);
        capRt.offsetMin = Vector2.zero;
        capRt.offsetMax = Vector2.zero;
        var capText = capGo.AddComponent<Text>();
        capText.text = "Menu music volume";
        capText.font = MenuBuiltinFont();
        capText.fontSize = 22;
        capText.fontStyle = FontStyle.Normal;
        capText.alignment = TextAnchor.UpperCenter;
        capText.color = new Color(0.82f, 0.76f, 0.64f, 0.9f);
        capText.horizontalOverflow = HorizontalWrapMode.Overflow;
        capText.verticalOverflow = VerticalWrapMode.Overflow;
        capText.raycastTarget = false;

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(rootGo.transform, false);
        sliderGo.layer = uiLayer;
        var sliderRt = sliderGo.AddComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 0f);
        sliderRt.anchorMax = new Vector2(1f, 0.46f);
        sliderRt.offsetMin = new Vector2(0f, 2f);
        sliderRt.offsetMax = new Vector2(0f, -2f);

        var white = UiWhiteSprite();
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(sliderGo.transform, false);
        bgGo.layer = uiLayer;
        var bgRt = bgGo.AddComponent<RectTransform>();
        StretchFull(bgRt);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = white;
        bgImg.color = new Color(0.12f, 0.12f, 0.14f, 0.88f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        fillArea.layer = uiLayer;
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(6f, 4f);
        fillAreaRt.offsetMax = new Vector2(-6f, -4f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        fillGo.layer = uiLayer;
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.sizeDelta = Vector2.zero;
        fillRt.anchoredPosition = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = white;
        fillImg.type = Image.Type.Simple;
        fillImg.color = new Color(0.82f, 0.68f, 0.28f, 0.95f);

        var handleSlide = new GameObject("Handle Slide Area");
        handleSlide.transform.SetParent(sliderGo.transform, false);
        handleSlide.layer = uiLayer;
        var hsRt = handleSlide.AddComponent<RectTransform>();
        hsRt.anchorMin = Vector2.zero;
        hsRt.anchorMax = Vector2.one;
        hsRt.offsetMin = new Vector2(6f, 4f);
        hsRt.offsetMax = new Vector2(-6f, -4f);

        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleSlide.transform, false);
        handleGo.layer = uiLayer;
        var handleRt = handleGo.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(22f, 0f);
        handleRt.anchorMin = new Vector2(0f, 0f);
        handleRt.anchorMax = new Vector2(0f, 1f);
        handleRt.pivot = new Vector2(0.5f, 0.5f);
        handleRt.anchoredPosition = Vector2.zero;
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.sprite = white;
        handleImg.color = new Color(0.98f, 0.82f, 0.45f, 1f);

        var slider = sliderGo.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        return slider;
    }

    void SetupMenuMusicVolumeSlider()
    {
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
            return;

        var root = canvas.transform.Find("MenuMusicVolumeRoot");
        Slider slider;
        if (root == null)
        {
            slider = BuildMenuMusicVolumeSliderUi(canvas.transform, canvas.gameObject.layer, 1);
            if (slider == null)
                return;
        }
        else
            slider = root.GetComponentInChildren<Slider>(true);

        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(OnMenuMusicVolumeSliderChanged);
        slider.value = menuMusicVolume;
        slider.onValueChanged.AddListener(OnMenuMusicVolumeSliderChanged);
    }

    static Sprite UiWhiteSprite()
    {
        if (_cachedUiWhiteSprite != null)
            return _cachedUiWhiteSprite;
        var tex = Texture2D.whiteTexture;
        _cachedUiWhiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return _cachedUiWhiteSprite;
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
