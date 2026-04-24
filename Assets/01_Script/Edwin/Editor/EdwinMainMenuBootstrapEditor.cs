#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EdwinMainMenuBootstrap))]
public sealed class EdwinMainMenuBootstrapEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Bake: guarda Edwin.unity sin borrar MainMenu_Root (posiciones, hijos extra del START, CREDITS, etc.). " +
            "Sincroniza referencias del bootstrap (audio, fuente título) y sprites por ruta en Background, TitleImage y el Image del botón llamado StartButton. " +
            "Reconstrucción completa (destructiva): menú Edwin → Build Main Menu In Edwin Scene.",
            MessageType.Info);

        if (GUILayout.Button("Bake: guardar menú en Edwin.unity"))
            EdwinMainMenuSceneBuilder.BakePreserveMenuInEdwinScene();
    }
}
#endif
