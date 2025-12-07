#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEditor.U2D;

public static class AtlasEnumGenerator
{
    // EditorPrefs 키
    private const string PendingRemapKey = "AtlasEnumGenerator_PendingRemap";
    private const string PendingAtlasBoxPathKey = "AtlasEnumGenerator_PendingAtlasBoxPath";

    [InitializeOnLoadMethod]
    private static void OnEditorReload()
    {
        if (!EditorPrefs.HasKey(PendingRemapKey)) return;
        if (!EditorPrefs.GetBool(PendingRemapKey, false)) return;

        EditorPrefs.DeleteKey(PendingRemapKey);

        var atlasBoxPath = EditorPrefs.GetString(PendingAtlasBoxPathKey, string.Empty);
        EditorPrefs.DeleteKey(PendingAtlasBoxPathKey);

        if (string.IsNullOrEmpty(atlasBoxPath)) return;

        var atlasBox = AssetDatabase.LoadAssetAtPath<AtlasBox>(atlasBoxPath);
        if (atlasBox == null) return;

        Generate(atlasBox, isFromReload: true);
    }

    public static void Generate(AtlasBox atlasBox) => Generate(atlasBox, isFromReload: false);

    private static void Generate(AtlasBox atlasBox, bool isFromReload)
    {
        if (atlasBox == null)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "AtlasBox가 null입니다.", "OK");
            return;
        }

        var folders = atlasBox.Folders;
        if (folders == null || folders.Length == 0)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "AtlasBox에 폴더가 등록되어 있지 않습니다.", "OK");
            return;
        }

        var atlasBoxPath = AssetDatabase.GetAssetPath(atlasBox);
        if (string.IsNullOrEmpty(atlasBoxPath))
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "AtlasBox의 에셋 경로를 찾을 수 없습니다.", "OK");
            return;
        }

        var atlasBoxDir = Path.GetDirectoryName(atlasBoxPath);
        if (string.IsNullOrEmpty(atlasBoxDir))
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "AtlasBox의 폴더 경로를 찾을 수 없습니다.", "OK");
            return;
        }

        var enumFilePath = Path.Combine(atlasBoxDir, "AtlasType.cs").Replace("\\", "/");
        var rawGroupDict = new Dictionary<string, SerializedDictionary<string, Sprite>>();

        foreach (var folder in folders)
        {
            if (folder == null) continue;

            var folderPath = AssetDatabase.GetAssetPath(folder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[AtlasEnumGenerator] 유효하지 않은 폴더: {folderPath}");
                continue;
            }

            var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { folderPath });

            foreach (var guid in atlasGuids)
            {
                var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null) continue;

                var atlasNameRaw = atlas.name;
                var atlasKey = Sanitize(atlasNameRaw);

                if (!rawGroupDict.TryGetValue(atlasKey, out var spriteDict))
                {
                    spriteDict = new SerializedDictionary<string, Sprite>();
                    rawGroupDict.Add(atlasKey, spriteDict);
                }

                var packables = SpriteAtlasExtensions.GetPackables(atlas);
                foreach (var packable in packables)
                {
                    if (packable == null) continue;

                    var packablePath = AssetDatabase.GetAssetPath(packable);

                    if (AssetDatabase.IsValidFolder(packablePath))
                    {
                        var spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { packablePath });
                        foreach (var sGuid in spriteGuids)
                        {
                            var sPath = AssetDatabase.GUIDToAssetPath(sGuid);
                            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sPath);
                            if (sprite == null) continue;

                            spriteDict[sprite.name] = sprite;
                        }
                    }
                    else
                    {
                        if (packable is Texture2D || packable is Sprite)
                        {
                            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(packablePath);
                            if (sprite == null) continue;

                            spriteDict[sprite.name] = sprite;
                        }
                    }
                }
            }
        }

        if (rawGroupDict.Count == 0)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "SpriteAtlas에서 가져올 Sprite를 찾지 못했습니다.", "OK");
            return;
        }

        var atlasNames = new HashSet<string>(rawGroupDict.Keys);

        var currentNames = new HashSet<string>(StringComparer.Ordinal);
        var atlasType = Type.GetType("AtlasType");
        if (atlasType != null && atlasType.IsEnum)
        {
            currentNames = Enum.GetNames(atlasType).ToHashSet(StringComparer.Ordinal);
        }

        var needEnumUpdate = atlasType == null || atlasNames.Any(n => !currentNames.Contains(n)) || currentNames.Any(n => n != "None" && !atlasNames.Contains(n));

        if (needEnumUpdate)
        {
            GenerateEnumFile(enumFilePath, atlasNames);

            AssetDatabase.Refresh();

            if (isFromReload) return;
            
            EditorPrefs.SetBool(PendingRemapKey, true);
            EditorPrefs.SetString(PendingAtlasBoxPathKey, atlasBoxPath);

            return;
        }

        if (atlasType == null || !atlasType.IsEnum)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", "AtlasType enum 타입을 찾을 수 없습니다. 컴파일 에러를 확인해주세요.", "OK");
            return;
        }

        var entries = new List<AtlasBox.AtlasEntry>();

        foreach (var kv in rawGroupDict)
        {
            var atlasKey = kv.Key;
            var spriteDict = kv.Value;

            try
            {
                var parsed = Enum.Parse(atlasType, atlasKey);
                var parsedType = (AtlasType)parsed;

                var entry = new AtlasBox.AtlasEntry
                {
                    type = parsedType,
                    sprites = spriteDict
                };
                entries.Add(entry);
            }
            catch
            {
                Debug.LogError($"[AtlasEnumGenerator] AtlasType에 '{atlasKey}' 항목이 없습니다. Enum 갱신이 제대로 안 된 상태입니다.");
            }
        }

        atlasBox.SetMapping(entries.ToArray());
        EditorUtility.SetDirty(atlasBox);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!isFromReload)
        {
            EditorUtility.DisplayDialog("AtlasEnumGenerator", $"매핑 갱신 완료.\n아틀라스 개수: {entries.Count}", "OK");
        }
        else
        {
            Debug.Log($"[AtlasEnumGenerator] 재컴파일 후 자동 매핑 갱신 완료. 아틀라스 개수: {entries.Count}");
        }
    }

    private static void GenerateEnumFile(string enumFilePath, HashSet<string> atlasNames)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated by AtlasEnumGenerator");
        sb.AppendLine("public enum AtlasType");
        sb.AppendLine("{");
        sb.AppendLine("    None = 0,");

        int index = 1;
        foreach (var name in atlasNames)
        {
            sb.AppendLine($"    {name} = {index},");
            index++;
        }

        sb.AppendLine("}");

        var dir = Path.GetDirectoryName(enumFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(enumFilePath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[AtlasEnumGenerator] AtlasType enum 생성/갱신 완료: {enumFilePath}");
    }

    private static string Sanitize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Atlas";

        raw = raw.Trim();
        var sb = new StringBuilder();

        if (char.IsDigit(raw[0]))
            sb.Append('_');

        foreach (var c in raw)
        {
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        }

        return sb.ToString();
    }
}
#endif
