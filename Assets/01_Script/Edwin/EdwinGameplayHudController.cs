using UnityEngine;
using TMPro;
using LastMachine.Arandia;

/// <summary>
/// HUD superior izquierdo: OLA, VIDA BASE (integridad media de torretas), PIEZAS.
/// Referencias opcionales; si faltan, intenta FindFirstObjectByType en Awake.
/// </summary>
[DisallowMultipleComponent]
public sealed class EdwinGameplayHudController : MonoBehaviour
{
    [Header("Textos (TextMeshProUGUI)")]
    [SerializeField] TextMeshProUGUI olaText;
    [SerializeField] TextMeshProUGUI vidaBaseText;
    [SerializeField] TextMeshProUGUI piezasText;

    [Header("Datos (opcional)")]
    [SerializeField] WaveManager_Arandia waveManager;
    [SerializeField] PieceInventory_Arandia pieceInventory;
    [SerializeField] TurretController_Arandia[] turrets;

    void Awake()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager_Arandia>();
        if (pieceInventory == null)
            pieceInventory = FindFirstObjectByType<PieceInventory_Arandia>();
        if (turrets == null || turrets.Length == 0)
            turrets = FindObjectsByType<TurretController_Arandia>(FindObjectsSortMode.None);
    }

    void Update()
    {
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
