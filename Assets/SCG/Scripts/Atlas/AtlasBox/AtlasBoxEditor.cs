// Assets/_Project/1. Scripts/Atlas/Editor/AtlasBoxEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AtlasBox))]
public class AtlasBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (GUILayout.Button("Generate Atlas"))
        {
            var atlasBox = (AtlasBox)target;
            AtlasEnumGenerator.Generate(atlasBox);
        }
    }
}
#endif