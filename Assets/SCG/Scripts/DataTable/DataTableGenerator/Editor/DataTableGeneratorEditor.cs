using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Text;
using MemoryPack;
using SCG.GoogleSheets;

[CustomEditor(typeof(DataTableGenerator))]
public class DataTableGeneratorEditor : Editor
{
    private DataTableGenerator gen;

    private readonly List<SheetResponse> loadedSheets = new();
    private readonly List<string> loadedSheetNames = new();

    private void OnEnable() => gen = (DataTableGenerator)target;

    public override void OnInspectorGUI()
    {
        // Only show GeneratedCodePath (default inspector) and the generate button
        DrawDefaultInspector();
        GUILayout.Space(8);
        if (GUILayout.Button("Generate Data Table"))
            BuildAll().Forget();
    }

    public async Task BuildAll(bool skipCodeGeneration = false)
    {
        bool keepProgressForSecondPhase = false;
        var cfg = FindGoogleConfig();
        if (cfg == null || (string.IsNullOrEmpty(cfg.accessToken) && string.IsNullOrEmpty(cfg.refreshToken)))
        {
            EditorUtility.DisplayDialog("Authentication Required", "Google OAuth is required. Please create/configure GoogleOAuthConfig.", "OK");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Google Sheet Build", "Downloading sheets...", 0.20f);
            await LoadAllSheets(cfg);

            if (!skipCodeGeneration)
            {
                EditorUtility.DisplayProgressBar("Google Sheet Build", "Generating classes...", 0.40f);
                GenerateAllClasses();

                EditorUtility.DisplayProgressBar("Google Sheet Build", "Generating enums...", 0.55f);
                GenerateEnums();

                EditorUtility.DisplayProgressBar("Google Sheet Build", "Refreshing scripts...", 0.60f);
                AssetDatabase.Refresh();

                if (!AllTableClassesExist())
                {
                    keepProgressForSecondPhase = true;
                    EditorUtility.DisplayProgressBar("Google Sheet Build", "Waiting for recompile (will continue)...", 0.65f);
                    DataTableGeneratorPostCompileHelper.ScheduleSecondPhase();
                    return;
                }
            }

            EditorUtility.DisplayProgressBar("Google Sheet Build", "Serializing DataTable...", 0.80f);
            var dataTableFile = BuildDataTableFile();

            EditorUtility.DisplayProgressBar("Google Sheet Build", "Writing file...", 0.95f);
            SaveDataTableFile(dataTableFile);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Done", "All table data have been generated and saved.", "OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataTable] BuildAll exception:\n{e}");
            EditorUtility.DisplayDialog("Error", "Build failed. Check Console.", "OK");
        }
        finally
        {
            if (!keepProgressForSecondPhase)
                EditorUtility.ClearProgressBar();
        }
    }

    private GoogleOAuthConfig FindGoogleConfig()
    {
        var guids = AssetDatabase.FindAssets("t:GoogleOAuthConfig");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GoogleOAuthConfig>(path);
        }
        return null;
    }

    // OAuth UI intentionally omitted; uses existing GoogleOAuthConfig silently.

    private async Task LoadAllSheets(GoogleOAuthConfig cfg)
    {
        loadedSheets.Clear();
        loadedSheetNames.Clear();

        if (string.IsNullOrEmpty(cfg.sheetId))
        {
            Debug.LogError("[Sheets] Sheet ID is empty. Check GoogleOAuthConfig.");
            return;
        }

        var meta = await GoogleSheetsClient.LoadMeta(cfg);
        if (meta?.sheets == null)
        {
            Debug.LogError("[Sheets] Failed to load sheet metadata.");
            return;
        }

        foreach (var s in meta.sheets)
        {
            var sheetName = s.properties.title;
            if (string.IsNullOrEmpty(sheetName)) continue;
            if (sheetName.StartsWith("#", StringComparison.Ordinal) || sheetName == "Localization")
                continue;

            var values = await GoogleSheetsClient.LoadSheetValues(cfg, $"{sheetName}!A:Z");
            if (values?.values == null)
            {
                Debug.LogWarning($"[Sheets] Sheet '{sheetName}' is empty.");
                continue;
            }

            loadedSheets.Add(values);
            loadedSheetNames.Add(sheetName);
        }

        Debug.Log($"[Sheets] Loaded sheets: {loadedSheets.Count}");
    }

    private void GenerateAllClasses()
    {
        if (!Directory.Exists(gen.generatedCodePath))
            Directory.CreateDirectory(gen.generatedCodePath);

        for (int i = 0; i < loadedSheets.Count; i++)
        {
            var name = loadedSheetNames[i];
            if (name == "EnumDataTable" || name == "Localization" || name.Contains("#"))
                continue;

            var sheet = loadedSheets[i];
            if (sheet.values == null || sheet.values.Count < 3)
            {
                Debug.LogWarning($"[ClassGen] '{name}' needs at least 3 rows (header/types/data)");
                continue;
            }

            var headerRow = sheet.values[0];
            var typeRow = sheet.values[1];
            string[] fieldNames = headerRow.ToArray();
            string[] fieldTypes = typeRow.ToArray();
            SheetClassGenerator.GenerateClass(name, fieldNames, fieldTypes, gen.generatedCodePath);
        }
    }

    private void GenerateEnums()
    {
        int index = loadedSheetNames.IndexOf("EnumDataTable");
        if (index == -1)
        {
            Debug.Log("[EnumGen] 'EnumDataTable' not found. Skipping.");
            return;
        }

        var sheet = loadedSheets[index];
        if (sheet.values == null || sheet.values.Count < 2)
        {
            Debug.LogWarning("[EnumGen] 'EnumDataTable' requires at least 2 rows.");
            return;
        }

        var headers = sheet.values[0];
        int headerCount = headers.Count;
        string outputPath = Path.Combine(gen.generatedCodePath, "DataTableEnum.cs");

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public static partial class DataTableEnum");
        sb.AppendLine("{");

        int rowCount = sheet.values.Count;

        for (int col = 0; col < headerCount; col++)
        {
            string enumName = headers[col];
            if (string.IsNullOrWhiteSpace(enumName)) continue;
            if (enumName.Equals("value", StringComparison.OrdinalIgnoreCase) || enumName.Equals("index", StringComparison.OrdinalIgnoreCase))
                continue;

            int valueCol = -1;
            if (col + 1 < headerCount)
            {
                string nextHeader = headers[col + 1];
                if (!string.IsNullOrWhiteSpace(nextHeader) &&
                    (nextHeader.Equals("value", StringComparison.OrdinalIgnoreCase) || nextHeader.Equals("index", StringComparison.OrdinalIgnoreCase)))
                {
                    valueCol = col + 1;
                }
            }

            sb.AppendLine($"    public enum {enumName}");
            sb.AppendLine("    {");

            for (int r = 1; r < rowCount; r++)
            {
                var row = sheet.values[r];
                if (col >= row.Count) continue;
                string rawName = row[col];
                if (string.IsNullOrWhiteSpace(rawName)) continue;
                string memberName = SanitizeEnum(rawName);

                if (valueCol >= 0 && r < sheet.values.Count && valueCol < row.Count)
                {
                    string rawValue = row[valueCol];
                    if (!string.IsNullOrWhiteSpace(rawValue) && int.TryParse(rawValue, out int enumValue))
                    {
                        sb.AppendLine($"        {memberName} = {enumValue},");
                        continue;
                    }
                }
                sb.AppendLine($"        {memberName},");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString());
        Debug.Log($"[EnumGen] Generated: {outputPath}");
    }

    private DataTableFile BuildDataTableFile()
    {
        var result = new DataTableFile();
        var serializerType = typeof(MemoryPack.MemoryPackSerializer);
        var serializeMethods = serializerType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Serialize" && m.IsGenericMethodDefinition)
            .ToArray();
        var serializeOneParam = serializeMethods.FirstOrDefault(m => m.GetParameters().Length == 1);
        var serializeTwoParams = serializeMethods.FirstOrDefault(m => m.GetParameters().Length == 2);

        for (int i = 0; i < loadedSheets.Count; i++)
        {
            string sheetName = loadedSheetNames[i];
            if (sheetName == "EnumDataTable" || sheetName == "Localization")
                continue;

            var sheet = loadedSheets[i];
            var rows = sheet.values;
            if (rows == null || rows.Count < 3)
            {
                Debug.LogWarning($"[DataTable] '{sheetName}' has insufficient rows.");
                continue;
            }

            string[] headers = rows[0].ToArray();
            string[] types = rows[1].ToArray();

            Type classType = FindTypeByName(sheetName);
            if (classType == null)
            {
                Debug.LogError($"[DataTable] Type '{sheetName}' not found.");
                continue;
            }

            Type listType = typeof(List<>).MakeGenericType(classType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType);

            for (int r = 2; r < rows.Count; r++)
            {
                var row = rows[r];
                if (row.All(string.IsNullOrWhiteSpace)) continue;

                object instance = Activator.CreateInstance(classType);
                for (int c = 0; c < headers.Length; c++)
                {
                    if (c >= row.Count) continue;
                    string header = headers[c];
                    if (string.IsNullOrWhiteSpace(header)) continue;

                    string typeStr = c < types.Length ? types[c] : "string";
                    string value = row[c];
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    FieldInfo field = classType.GetField(header, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field == null) continue;

                    object parsed;

                    // 🔹 Enum 타입이면 Enum.Parse로 처리
                    if (field.FieldType.IsEnum)
                    {
                        try
                        {
                            parsed = Enum.Parse(field.FieldType, value, ignoreCase: true);
                        }
                        catch (Exception)
                        {
                            Debug.LogError($"[DataTable] Enum parse failed: field={field.Name}, type={field.FieldType}, value={value}");
                            // 기본값(0)으로 세팅
                            parsed = Enum.GetValues(field.FieldType).GetValue(0);
                        }
                    }
                    else
                    {
                        // 기존 Primitive/배열 파싱 로직
                        parsed = ParseValue(typeStr, value);
                    }

                    field.SetValue(instance, parsed);
                }
                list.Add(instance);
            }

            byte[] payload;
            if (serializeOneParam != null)
            {
                var m = serializeOneParam.MakeGenericMethod(listType);
                payload = (byte[])m.Invoke(null, new object[] { list });
            }
            else if (serializeTwoParams != null)
            {
                var m = serializeTwoParams.MakeGenericMethod(listType);
                payload = (byte[])m.Invoke(null, new object[] { list, null });
            }
            else
            {
                throw new InvalidOperationException("MemoryPackSerializer.Serialize<T> overload not found.");
            }

            result.Tables.Add(new DataTableEntry { TableName = sheetName, Payload = payload });
            Debug.Log($"[DataTable] Serialized: {sheetName} rows={list.Count}");
        }

        return result;
    }

    private void SaveDataTableFile(DataTableFile dataTableFile)
    {
        string dataDir = Path.Combine("Assets/Resources");
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        string filePath = Path.Combine(dataDir, "DataTable.bytes");
        try
        {
            byte[] bytes = MemoryPack.MemoryPackSerializer.Serialize(dataTableFile);
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"[DataTable] Saved: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataTable] Save failed: {e}");
        }
        AssetDatabase.Refresh();
    }

    private bool AllTableClassesExist()
    {
        for (int i = 0; i < loadedSheets.Count; i++)
        {
            string sheetName = loadedSheetNames[i];
            if (string.IsNullOrEmpty(sheetName)) continue;
            if (sheetName == "EnumDataTable" || sheetName == "Localization") continue;
            if (sheetName.StartsWith("#", StringComparison.Ordinal)) continue;
            if (FindTypeByName(sheetName) == null)
            {
                Debug.LogWarning($"[DataTable] Missing generated type for '{sheetName}'.");
                return false;
            }
        }
        return true;
    }

    private static object ParseValue(string type, string value)
    {
        try
        {
            var ci = CultureInfo.InvariantCulture;
            switch (type)
            {
                case "int": return int.Parse(value, ci);
                case "long": return long.Parse(value, ci);
                case "float": return float.Parse(value, ci);
                case "double": return double.Parse(value, ci);
                case "bool": return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";
                case "string": return value;
                case "int[]": return value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s.Trim(), ci)).ToArray();
                case "float[]": return value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => float.Parse(s.Trim(), ci)).ToArray();
                case "long[]": return value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => long.Parse(s.Trim(), ci)).ToArray();
                case "double[]": return value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => double.Parse(s.Trim(), ci)).ToArray();
                default: return value;
            }
        }
        catch
        {
            return type switch
            {
                "int" => 0,
                "long" => 0L,
                "float" => 0f,
                "double" => 0d,
                "bool" => false,
                "string" => string.Empty,
                "int[]" => Array.Empty<int>(),
                "float[]" => Array.Empty<float>(),
                "long[]" => Array.Empty<long>(),
                "double[]" => Array.Empty<double>(),
                _ => null
            };
        }
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
            foreach (var t in types)
            {
                if (t == null) continue;
                if (t.Name == typeName) return t;
                if (!string.IsNullOrEmpty(t.FullName) && (t.FullName.EndsWith("." + typeName, StringComparison.Ordinal) || t.FullName.EndsWith("+" + typeName, StringComparison.Ordinal)))
                    return t;
            }
        }
        return null;
    }

    private static string SanitizeEnum(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name)) return "_";
        if (char.IsDigit(name[0])) name = "_" + name;
        name = name.Replace(" ", "_").Replace("-", "_");
        return name;
    }
}

public static class DataTableGeneratorMenu
{
    [MenuItem("SCG/Tools/Generate/Generate Data Table")]
    public static void BuildAllFromMenu()
    {
        string[] guids = AssetDatabase.FindAssets("t:DataTableGenerator");
        if (guids == null || guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Not Found", "No DataTableGenerator asset found. Create one via Create > Tools > DataTableGenerator.", "OK");
            return;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var asset = AssetDatabase.LoadAssetAtPath<DataTableGenerator>(path);
        if (asset == null)
        {
            EditorUtility.DisplayDialog("Load Error", $"Path: {path}\nCould not load DataTableGenerator asset.", "OK");
            return;
        }
        var editor = Editor.CreateEditor(asset) as DataTableGeneratorEditor;
        if (editor == null)
        {
            EditorUtility.DisplayDialog("Editor Error", "Failed to create DataTableGeneratorEditor instance.", "OK");
            return;
        }
        editor.BuildAll().Forget();
    }
}

public static class TaskExtensions
{
    public static async void Forget(this Task task) => await task;
}
