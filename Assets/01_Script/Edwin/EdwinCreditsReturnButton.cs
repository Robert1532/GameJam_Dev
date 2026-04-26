using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Botón "volver" en la escena de créditos.
/// Por defecto carga la escena en el índice 0 del Build Settings (<c>Assets/00_Scenes/Edwin.unity</c>).
/// </summary>
[DisallowMultipleComponent]
public sealed class EdwinCreditsReturnButton : MonoBehaviour
{
    [Tooltip("Índice en File > Build Settings. 0 = primera escena (menú Edwin en Assets/00_Scenes/Edwin.unity).")]
    [SerializeField] int mainMenuBuildIndex;

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuBuildIndex, LoadSceneMode.Single);
    }
}
