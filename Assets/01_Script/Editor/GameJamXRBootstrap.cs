#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// One-shot OpenXR setup for XR Plug-in Management (Standalone + Android).
/// Batch: <c>-executeMethod GameJamXRBootstrap.ConfigureOpenXR</c>
/// </summary>
public static class GameJamXRBootstrap
{
    const string OpenXRLoaderTypeName = "UnityEngine.XR.OpenXR.OpenXRLoader";

    [MenuItem("GameJam/XR/Configure OpenXR")]
    public static void ConfigureOpenXR()
    {
        try
        {
            var perBuild = EnsurePerBuildTargetAsset();
            EnsureOpenXR(BuildTargetGroup.Standalone, perBuild);
            EnsureOpenXR(BuildTargetGroup.Android, perBuild);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameJamXRBootstrap] OpenXR loader assigned for Standalone and Android.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[GameJamXRBootstrap] " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            return;
        }

        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    static XRGeneralSettingsPerBuildTarget EnsurePerBuildTargetAsset()
    {
        if (EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget existing) && existing != null)
            return existing;

        const string assetPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
        if (!AssetDatabase.IsValidFolder("Assets/XR"))
            AssetDatabase.CreateFolder("Assets", "XR");

        var asset = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, asset, true);
        AssetDatabase.SaveAssets();
        return asset;
    }

    static void EnsureOpenXR(BuildTargetGroup group, XRGeneralSettingsPerBuildTarget perBuild)
    {
        if (!perBuild.HasSettingsForBuildTarget(group))
            perBuild.CreateDefaultSettingsForBuildTarget(group);

        var gen = perBuild.SettingsForBuildTarget(group);
        if (gen != null)
            gen.InitManagerOnStart = true;

        if (!perBuild.HasManagerSettingsForBuildTarget(group))
            perBuild.CreateDefaultManagerSettingsForBuildTarget(group);

        var mgr = perBuild.ManagerSettingsForBuildTarget(group);
        if (mgr == null)
            throw new InvalidOperationException("XRManagerSettings missing for " + group);

        if (XRPackageMetadataStore.IsLoaderAssigned(OpenXRLoaderTypeName, group))
            return;

        if (!XRPackageMetadataStore.AssignLoader(mgr, OpenXRLoaderTypeName, group))
            throw new InvalidOperationException("AssignLoader failed for " + group);
    }
}
#endif
