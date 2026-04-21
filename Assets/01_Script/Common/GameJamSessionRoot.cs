using UnityEngine;

/// <summary>
/// Punto de enganche en escena para lógica compartida del game jam (arranque, servicios, etc.).
/// </summary>
public sealed class GameJamSessionRoot : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"{nameof(GameJamSessionRoot)}: escena '{gameObject.scene.name}' lista.");
    }
}
