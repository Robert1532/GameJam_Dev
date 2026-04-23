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
            "Guarda Canvas + fondo + botón en Edwin.unity para verlos en el editor sin Play. " +
            "Abre la escena Edwin antes de pulsar.",
            MessageType.Info);

        if (GUILayout.Button("Bake: guardar menú en Edwin.unity"))
            EdwinMainMenuSceneBuilder.BuildFromMenu();
    }
}
#endif
