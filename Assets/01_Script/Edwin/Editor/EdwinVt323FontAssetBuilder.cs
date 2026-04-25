#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

/// <summary>
/// Genera el TMP Font Asset (SDF dinámico) para VT323, igual que Assets → Create → TextMeshPro → Font Asset → SDF.
/// Batch: -executeMethod EdwinVt323FontAssetBuilder.GenerateAndSave
/// </summary>
public static class EdwinVt323FontAssetBuilder
{
    const string FontTtfPath = "Assets/05_Assets/Edwin/Fonts/VT323/VT323-Regular.ttf";

    [MenuItem("Edwin/Generate VT323 TMP Font Asset (SDF)")]
    public static void GenerateFromMenu()
    {
        GenerateAndSave();
    }

    public static void GenerateAndSave()
    {
        if (TMP_Settings.instance == null)
        {
            Debug.LogError("TMP_Settings missing. Import TextMeshPro Essential Resources first.");
            return;
        }

        var font = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
        if (font == null)
        {
            Debug.LogError("VT323 font not found at: " + FontTtfPath);
            return;
        }

        string sourceFontFilePath = FontTtfPath;
        string folderPath = Path.GetDirectoryName(sourceFontFilePath)?.Replace('\\', '/') ?? "";
        string assetName = Path.GetFileNameWithoutExtension(sourceFontFilePath);
        string newAssetFilePathWithName = folderPath + "/" + assetName + " SDF.asset";

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetFilePathWithName) != null)
        {
            Debug.Log("VT323 SDF ya existe; no se regenera (evita cambiar el GUID y romper referencias en EdwinHUD). Para forzar, borra el .asset en el Project y vuelve a ejecutar este menú.");
            return;
        }

        FontEngine.InitializeFontEngine();

        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        if (fontAsset == null)
        {
            Debug.LogWarning("Unable to create TMP font asset for VT323. Enable Include Font Data on the .ttf importer.", font);
            return;
        }

        fontAsset.name = assetName + " SDF";

        if (fontAsset.atlasTextures is { Length: > 0 } && fontAsset.atlasTextures[0] != null)
            fontAsset.atlasTextures[0].name = assetName + " SDF Atlas";
        if (fontAsset.material != null)
            fontAsset.material.name = assetName + " SDF Material";

        int atlasPadding = fontAsset.atlasPadding;
        string sourceGuid = AssetDatabase.AssetPathToGUID(sourceFontFilePath);
        // El constructor con parámetros es internal en Unity.TextMeshPro; se rellenan los campos públicos del struct.
        fontAsset.creationSettings = new FontAssetCreationSettings
        {
            sourceFontFileName = string.Empty,
            sourceFontFileGUID = sourceGuid,
            faceIndex = 0,
            pointSizeSamplingMode = 0,
            pointSize = (int)fontAsset.faceInfo.pointSize,
            padding = atlasPadding,
            paddingMode = 2,
            packingMode = 0,
            atlasWidth = 1024,
            atlasHeight = 1024,
            characterSetSelectionMode = 7,
            characterSequence = string.Empty,
            renderMode = (int)GlyphRenderMode.SDFAA,
            referencedFontAssetGUID = string.Empty,
            referencedTextAssetGUID = string.Empty,
            fontStyle = 0,
            fontStyleModifier = 0,
            includeFontFeatures = false
        };

        AssetDatabase.CreateAsset(fontAsset, newAssetFilePathWithName);
        if (fontAsset.atlasTextures is { Length: > 0 } && fontAsset.atlasTextures[0] != null)
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        if (fontAsset.material != null)
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("VT323 TMP font asset created: " + newAssetFilePathWithName);
    }
}
#endif
