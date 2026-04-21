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

    [MenuItem("Edwin/Build Main Menu In Edwin Scene")]
    public static void BuildFromMenu()
    {
        BuildInternal(save: true);
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
        root.AddComponent<EdwinMainMenuBootstrap>();

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
