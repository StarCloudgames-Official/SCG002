// Assets/_Project/1. Scripts/Atlas/Editor/AtlasEnumGenerateMenu.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AtlasEnumGenerateMenu
{
    [MenuItem("SCG/Tools/Generate/Generate Atlas")]
    private static void Generate()
    {
        var atlasBox = FindAtlasBox();
        if (atlasBox == null)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerate", "AtlasBox 찾을 수 없음", "OK");
            return;
        }

        AtlasEnumGenerator.Generate(atlasBox);
    }

    private static AtlasBox FindAtlasBox()
    {
        var guids = AssetDatabase.FindAssets("t:AtlasBox");
        if (guids.Length == 0) return null;

        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AtlasBox>(path);
    }
}
#endif