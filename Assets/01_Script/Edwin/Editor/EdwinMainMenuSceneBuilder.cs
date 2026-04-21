#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Construye el menú principal en Edwin.unity (Canvas + fondo + botón START).
/// Menú: Edwin → Build Main Menu In Edwin Scene
/// Batch: -executeMethod EdwinMainMenuSceneBuilder.BuildAndSave
/// </summary>
public static class EdwinMainMenuSceneBuilder
{
    const string ScenePath = "Assets/00_Scenes/Edwin.unity";
    const string BgPath = "Assets/05_Assets/Edwin/Backgrounds/bg_preview.png";
    const string BtnPath = "Assets/05_Assets/Edwin/UI/button_start-preview.png";
    const string TitlePath = "Assets/05_Assets/Edwin/UI/title-preview.png";
    const string MenuMusicAssetPath =
        "Assets/05_Assets/Edwin/Audio/Music/monume-retro-arcade-game-music/monume-retro-arcade-game-music-509489.mp3";
    const string StartClickAssetPath = "Assets/05_Assets/Edwin/Audio/SFX/button-start-click.mp3";
    const string StartHoverAssetPath = "Assets/05_Assets/Edwin/Audio/SFX/button-start-hover.wav";

    [MenuItem("Edwin/Build Main Menu In Edwin Scene")]
    public static void BuildFromMenu()
    {
        BuildInternal(save: true);
    }

    /// <summary>
    /// Añade <c>MenuMusicVolumeRoot</c> al Canvas de Edwin.unity sin borrar el menú (misma UI que en runtime horneado).
    /// </summary>
    [MenuItem("Edwin/Ensure Menu Music Slider In Edwin Scene")]
    public static void EnsureMenuMusicSliderInEdwinScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var bootstrap = Object.FindFirstObjectByType<EdwinMainMenuBootstrap>();
        if (bootstrap == null)
        {
            Debug.LogError("[EdwinMainMenuSceneBuilder] No hay EdwinMainMenuBootstrap en la escena.");
            return;
        }

        var canvas = bootstrap.GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("[EdwinMainMenuSceneBuilder] No hay Canvas bajo el bootstrap.");
            return;
        }

        if (canvas.transform.Find("MenuMusicVolumeRoot") != null)
        {
            Debug.Log("[EdwinMainMenuSceneBuilder] MenuMusicVolumeRoot ya existe; no se duplica.");
            return;
        }

        const int uiLayer = 5;
        var slider = EdwinMainMenuBootstrap.BuildMenuMusicVolumeSliderUi(canvas.transform, uiLayer, 1);
        if (slider == null)
            return;

        var soBoot = new SerializedObject(bootstrap);
        var pVol = soBoot.FindProperty("menuMusicVolume");
        slider.value = pVol != null ? pVol.floatValue : 0.2f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[EdwinMainMenuSceneBuilder] Añadido MenuMusicVolumeRoot (volumen menú) al Canvas.");
    }

    public static void BuildAndSave()
    {
        BuildInternal(save: true);
        EditorApplication.Exit(0);
    }

    static void BuildInternal(bool save)
    {
        EnsureSpriteTextureImport(BgPath);
        EnsureSpriteTextureImport(BtnPath);
        EnsureSpriteTextureImport(TitlePath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var bgSprite = LoadSpriteFromTextureAsset(BgPath);
        var btnSprite = LoadSpriteFromTextureAsset(BtnPath);
        var titleSprite = LoadSpriteFromTextureAsset(TitlePath);
        if (bgSprite == null || btnSprite == null)
        {
            Debug.LogError(
                "[EdwinMainMenuSceneBuilder] No se pudieron cargar Sprites en " + BgPath + " / " + BtnPath +
                ". Comprueba que sean PNG válidos y que la ruta exista.");
            return;
        }

        if (titleSprite == null)
        {
            Debug.LogWarning(
                "[EdwinMainMenuSceneBuilder] No se cargó " + TitlePath + "; se horneará sólo LAST / MACHINE (sin imagen).");
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        const int uiLayer = 5;

        var existing = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in existing)
        {
            if (go.name == "MainMenu_Root")
                Object.DestroyImmediate(go);
        }

        var root = new GameObject("MainMenu_Root");
        var bootstrap = root.AddComponent<EdwinMainMenuBootstrap>();
        AssignMenuAudioClips(bootstrap);

        var esGo = new GameObject("EventSystem");
        esGo.transform.SetParent(root.transform, false);
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(root.transform, false);
        canvasGo.layer = uiLayer;

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
        canvasRt.localScale = Vector3.one;

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

        EdwinMainMenuBootstrap.AddTitleBlock(canvasGo.transform, uiLayer, titleSprite);

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
        text.font = EdwinMainMenuBootstrap.MenuBuiltinFont();
        text.fontSize = 40;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        button.transition = Selectable.Transition.None;
        btnGo.AddComponent<EdwinStartButtonHoverAnim>();

        EdwinMainMenuBootstrap.AddPressStartHint(canvasGo.transform, uiLayer);

        var volumeSlider = EdwinMainMenuBootstrap.BuildMenuMusicVolumeSliderUi(canvasGo.transform, uiLayer, 1);
        if (volumeSlider != null)
        {
            var soBoot = new SerializedObject(bootstrap);
            var pVol = soBoot.FindProperty("menuMusicVolume");
            volumeSlider.value = pVol != null ? pVol.floatValue : 0.2f;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (save)
            EditorSceneManager.SaveOpenScenes();
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static void AssignMenuAudioClips(EdwinMainMenuBootstrap bootstrap)
    {
        if (bootstrap == null)
            return;

        var music = AssetDatabase.LoadAssetAtPath<AudioClip>(MenuMusicAssetPath);
        var click = AssetDatabase.LoadAssetAtPath<AudioClip>(StartClickAssetPath);
        var hover = AssetDatabase.LoadAssetAtPath<AudioClip>(StartHoverAssetPath);
        var so = new SerializedObject(bootstrap);
        var m = so.FindProperty("menuBackgroundMusic");
        var s = so.FindProperty("startButtonClickSfx");
        var h = so.FindProperty("startButtonHoverSfx");
        if (m != null)
            m.objectReferenceValue = music;
        if (s != null)
            s.objectReferenceValue = click;
        if (h != null)
            h.objectReferenceValue = hover;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (music == null)
            Debug.LogWarning("[EdwinMainMenuSceneBuilder] No se encontró música en " + MenuMusicAssetPath);
        if (click == null)
            Debug.LogWarning("[EdwinMainMenuSceneBuilder] No se encontró SFX clic en " + StartClickAssetPath);
        if (hover == null)
            Debug.LogWarning("[EdwinMainMenuSceneBuilder] No se encontró SFX hover en " + StartHoverAssetPath);
    }

    /// <summary>
    /// Fuerza Sprite 2D (Single) para que <see cref="AssetDatabase"/> pueda obtener un <see cref="Sprite"/>.
    /// </summary>
    static void EnsureSpriteTextureImport(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    static Sprite LoadSpriteFromTextureAsset(string assetPath)
    {
        var main = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (main is Sprite sMain)
            return sMain;

        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (obj is Sprite s)
                return s;
        }

        return null;
    }
}
#endif
