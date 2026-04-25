using UnityEngine;
using TMPro;
using LastMachine.Arandia;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// HUD superior izquierdo: OLA, VIDA BASE (integridad media de torretas), PIEZAS.
/// Referencias opcionales; si faltan, intenta FindFirstObjectByType en Awake.
/// </summary>
#if UNITY_EDITOR
[ExecuteAlways]
#endif
[DisallowMultipleComponent]
public sealed class EdwinGameplayHudController : MonoBehaviour
{
#if UNITY_EDITOR
    const string EditorVt323SdfAssetPath = "Assets/05_Assets/Edwin/Fonts/VT323/VT323-Regular SDF.asset";
#endif

    [Header("Textos (TextMeshProUGUI)")]
    [SerializeField] TextMeshProUGUI olaText;
    [SerializeField] TextMeshProUGUI vidaBaseText;
    [SerializeField] TextMeshProUGUI piezasText;

    [Header("Tipografía HUD")]
    [Tooltip("Opcional: asigna el TMP Font Asset SDF (p. ej. VT323-Regular SDF) para la misma fuente en editor y en builds.")]
    [SerializeField] TMP_FontAsset hudTmpFontAsset;
    [Tooltip("Si no hay TMP Font Asset, se genera uno dinámico a partir de esta fuente (.ttf).")]
    [SerializeField] Font hudSourceFont;

    [Header("Datos (opcional)")]
    [SerializeField] WaveManager_Arandia waveManager;
    [SerializeField] PieceInventory_Arandia pieceInventory;
    [SerializeField] TurretController_Arandia[] turrets;

    static TMP_FontAsset s_cachedHudFontAsset;
    static Font s_cachedHudFontSource;

    /// <summary>
    /// Estos TMP llevan la fuente serializada en la escena (VT323 SDF); no se sobrescriben desde código.
    /// </summary>
    static bool IsStatsHudTextSerializedInScene(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;
        var n = tmp.gameObject.name;
        return n == "HudOla" || n == "HudVidaBase" || n == "HudPiezas";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ApplyHudFontFromSource();
    }
#endif

    void OnEnable()
    {
        ApplyHudFontFromSource();
    }

    void Awake()
    {
        ApplyHudFontFromSource();
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager_Arandia>();
        if (pieceInventory == null)
            pieceInventory = FindFirstObjectByType<PieceInventory_Arandia>();
        if (turrets == null || turrets.Length == 0)
            turrets = FindObjectsByType<TurretController_Arandia>(FindObjectsSortMode.None);
    }

    TMP_FontAsset ResolveHudFontAsset()
    {
        if (hudTmpFontAsset != null)
            return hudTmpFontAsset;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var fromDisk = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EditorVt323SdfAssetPath);
            if (fromDisk != null)
                return fromDisk;
        }
#endif

        if (hudSourceFont == null)
            return null;

        if (s_cachedHudFontAsset == null || s_cachedHudFontSource != hudSourceFont)
        {
            s_cachedHudFontSource = hudSourceFont;
            s_cachedHudFontAsset = TMP_FontAsset.CreateFontAsset(hudSourceFont);
        }

        return s_cachedHudFontAsset;
    }

    void ApplyHudFontFromSource()
    {
        var fa = ResolveHudFontAsset();
        if (fa == null)
            return;

        foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp == null) continue;
            if (IsStatsHudTextSerializedInScene(tmp))
                continue;
            tmp.font = fa;
            tmp.ForceMeshUpdate(true);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Avoid calling ForceUpdateCanvases during layout callbacks (it internally uses SendMessage).
            // Defer to the next editor tick.
            EditorApplication.delayCall += Canvas.ForceUpdateCanvases;
        }
#endif
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (olaText != null)
        {
            var w = waveManager != null ? waveManager.CurrentWave : 0;
            olaText.text = w > 0 ? $"OLA: {w}" : "OLA: —";
        }

        if (vidaBaseText != null)
            vidaBaseText.text = $"VIDA BASE: {ComputeBaseIntegrityPercent():0}%";

        if (piezasText != null)
        {
            var n = pieceInventory != null ? pieceInventory.CurrentPieces : 0;
            piezasText.text = $"PIEZAS: {n}";
        }
    }

    float ComputeBaseIntegrityPercent()
    {
        if (turrets == null || turrets.Length == 0)
            return 100f;

        float sum = 0f;
        int count = 0;
        foreach (var t in turrets)
        {
            if (t == null) continue;
            Accumulate(t.sensor, ref sum, ref count);
            Accumulate(t.canon, ref sum, ref count);
            Accumulate(t.motor, ref sum, ref count);
        }

        if (count == 0)
            return 100f;
        return Mathf.Clamp01(sum / count) * 100f;
    }

    static void Accumulate(TurretComponent_Arandia c, ref float sum, ref int count)
    {
        if (c == null) return;
        sum += Mathf.Clamp01(c.HPPercent);
        count++;
    }
}
