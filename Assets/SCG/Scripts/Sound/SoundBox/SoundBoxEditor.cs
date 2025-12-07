// SoundBoxEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundBox))]
public class SoundBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (GUILayout.Button("Generate Sound"))
        {
            var soundBox = (SoundBox)target;
            SoundEnumGenerator.Generate(soundBox);
        }
    }
}
#endif