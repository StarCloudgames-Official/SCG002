#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SoundEnumGenerateMenu
{
    [MenuItem("SCG/Tools/Generate/Generate Sound")]
    private static void Generate()
    {
        var soundBox = FindSoundBox();
        if (soundBox == null)
        {
            EditorUtility.DisplayDialog("SoundEnumGenerate", "SoundBox 찾을 수 없음", "OK");
            return;
        }

        SoundEnumGenerator.Generate(soundBox);
    }

    private static SoundBox FindSoundBox()
    {
        var guids = AssetDatabase.FindAssets("t:SoundBox");
        if (guids.Length == 0) return null;

        return AssetDatabase.LoadAssetAtPath<SoundBox>(
            AssetDatabase.GUIDToAssetPath(guids[0])
        );
    }
}

public static class SoundEnumGenerator
{
    public static void Generate(SoundBox soundBox)
    {
        var soPath = AssetDatabase.GetAssetPath(soundBox);
        var dir = Path.GetDirectoryName(soPath);

        const string enumName = "SoundId";
        var enumPath = Path.Combine(dir, enumName + ".cs");

        var folders = soundBox.Folders;
        if (folders == null || folders.Length == 0)
        {
            EditorUtility.DisplayDialog("SoundBox", "폴더 미등록", "OK");
            return;
        }

        var keys = new HashSet<string>();
        var dict = new SerializedDictionary<SoundId, AudioClip>();

        foreach (var folder in folders)
        {
            var path = AssetDatabase.GetAssetPath(folder);
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { path });

            foreach (var guid in guids)
            {
                var clipPath = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
                var key = Sanitize(clip.name);

                keys.Add(key);

                if (Enum.TryParse(key, out SoundId parsedId))
                    dict[parsedId] = clip; // 기존 enum 값 있으면 세팅
            }
        }

        // enum 작성
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated");
        sb.AppendLine($"public enum {enumName}");
        sb.AppendLine("{");
        sb.AppendLine("    None = 0,");

        foreach (var key in keys)
            sb.AppendLine($"    {key},");

        sb.AppendLine("}");

        File.WriteAllText(enumPath, sb.ToString(), Encoding.UTF8);

        soundBox.SetMapping(dict);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "SoundBox",
            $"완료: {enumName} 생성 + 매핑 적용\n→ {enumPath}",
            "OK"
        );
    }

    private static string Sanitize(string raw)
    {
        var sb = new StringBuilder();
        bool first = true;

        foreach (var c in raw)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append('_');

            if (first && char.IsDigit(sb[0]))
                sb.Insert(0, '_');

            first = false;
        }

        return sb.ToString();
    }
}
#endif
