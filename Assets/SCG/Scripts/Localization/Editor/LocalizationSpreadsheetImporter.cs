#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using IosAppInfo = UnityEngine.Localization.Platform.iOS.AppInfo;
using AndroidAppInfo = UnityEngine.Localization.Platform.Android.AppInfo;

public static class LocalizationSpreadsheetImporter
{
    private const string StringTableDirectory = "Assets/Localization/StringTables";
    private const string LocaleDirectory      = "Assets/Localization/Locales";
    private const string DefaultCollectionName = "LocalizationTable";

    [MenuItem("SCG/Tools/Localization/Import From Google Sheet")]
    private static void ImportFromGoogle()
    {
        ImportFromGoogle(DefaultCollectionName);
    }

    // Additional variants/menu items can be added if needed.

    private static async void ImportFromGoogle(string collectionName)
    {
        var cfg = GoogleConfigFinder.FindConfig();
        if (cfg == null || string.IsNullOrEmpty(cfg.sheetId))
        {
            EditorUtility.DisplayDialog("[Localization]", "Google OAuth config or Sheet ID missing.", "OK");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Localization", "Downloading 'Localization' sheet...", 0.2f);

            var values = await GoogleSheetsClient.LoadSheetValues(cfg, "Localization!A:Z");
            if (values?.values == null || values.values.Count < 2)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("[Localization]", "Localization sheet is empty or invalid.", "OK");
                return;
            }

            var table = ConvertToDictionary(values.values);

            EditorUtility.DisplayProgressBar("Localization", "Applying to StringTables...", 0.6f);
            ApplyToStringTables(table, collectionName);

            EditorUtility.DisplayProgressBar("Localization", "Cleaning up unused Locales...", 0.9f);
            RemoveUnusedLocales();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("[Localization]", "Localization import complete.", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"[Localization] Import failed: {e}");
            EditorUtility.DisplayDialog("[Localization]", "Failed to import from Google.", "OK");
        }
    }

    private static Dictionary<string, Dictionary<string, string>> ConvertToDictionary(List<List<string>> rows)
    {
        var dict = new Dictionary<string, Dictionary<string, string>>();
        if (rows == null || rows.Count == 0) return dict;

        var header = rows[0];
        int keyIndex = -1;
        var localeColumns = new List<(int index, string code)>();

        for (int i = 0; i < header.Count; i++)
        {
            string h = (header[i] ?? string.Empty).Trim();
            if (string.Equals(h, "key", StringComparison.OrdinalIgnoreCase))
            {
                keyIndex = i;
            }
            else if (!string.IsNullOrEmpty(h))
            {
                localeColumns.Add((i, h));
            }
        }

        if (keyIndex < 0)
            throw new Exception("Header must contain 'key' column.");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row == null || row.Count == 0) continue;

            string key = keyIndex < row.Count ? row[keyIndex] : string.Empty;
            if (string.IsNullOrWhiteSpace(key)) continue;

            var rowDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (index, code) in localeColumns)
            {
                string value = index < row.Count ? (row[index] ?? string.Empty) : string.Empty;
                rowDict[code] = value;
            }

            dict[key] = rowDict;
        }

        return dict;
    }

    private static void ApplyToStringTables(Dictionary<string, Dictionary<string, string>> table, string collectionName)
    {
        if (table == null || table.Count == 0)
            return;

        EnsureFolder(StringTableDirectory);
        EnsureFolder(LocaleDirectory);

        var collection = LocalizationEditorSettings.GetStringTableCollection(collectionName) as StringTableCollection;
        if (collection == null)
        {
            collection = LocalizationEditorSettings.CreateStringTableCollection(collectionName, StringTableDirectory);
            Debug.Log($"[Localization] Created StringTableCollection: {collectionName}");
        }

        var csvKeys = new HashSet<string>(table.Keys);

        // Determine locales from first row's dict
        var sampleRow = table.Values.FirstOrDefault();
        var localeIdToTable = new Dictionary<string, StringTable>(StringComparer.OrdinalIgnoreCase);
        var csvLocaleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in sampleRow.Keys)
        {
            if (string.Equals(header, "key", StringComparison.OrdinalIgnoreCase))
                continue;

            var localeId = header;
            csvLocaleIds.Add(localeId);

            var locale = GetOrCreateLocale(localeId);
            var locTable = collection.GetTable(locale.Identifier) as StringTable;
            if (locTable == null)
            {
                locTable = collection.AddNewTable(locale.Identifier) as StringTable;
            }

            localeIdToTable[locale.Identifier.Code] = locTable;
        }

        // Upsert entries
        foreach (var kv in table)
        {
            string key = kv.Key;
            var perLocale = kv.Value;

            foreach (var (localeId, localized) in perLocale)
            {
                if (!localeIdToTable.TryGetValue(localeId, out var locTable))
                    continue;

                var entry = locTable.GetEntry(key);
                if (entry == null)
                {
                    entry = locTable.AddEntry(key, localized ?? string.Empty);
                }
                else
                {
                    entry.Value = localized ?? string.Empty;
                }
                EditorUtility.SetDirty(locTable);
            }
        }

        // Remove stale entries not in CSV
        var shared = collection.SharedData;
        if (shared != null)
        {
            var allKeys = shared.Entries.Select(e => e.Key).ToArray();
            foreach (var key in allKeys)
            {
                if (!csvKeys.Contains(key))
                {
                    TableEntryReference entryRef = key;
                    collection.RemoveEntry(entryRef);
                }
            }

            EditorUtility.SetDirty(shared);
        }

        // Optional: link iOS/Android app name metadata to a common key
        SetupAppNameMetadataIfNeeded(collectionName, "app_name");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
        if (parts.Length < 2 || parts[0] != "Assets")
            throw new Exception($"Invalid path: {path}");

        var current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static void RemoveUnusedLocales()
    {
        var usedLocaleCodes = new HashSet<string>();
        var collections = LocalizationEditorSettings.GetStringTableCollections();
        foreach (var col in collections)
        {
            if (col is not StringTableCollection stCol) continue;
            foreach (var table in stCol.StringTables)
            {
                if (table == null) continue;
                usedLocaleCodes.Add(table.LocaleIdentifier.Code);
            }
        }

        var localeGuids = AssetDatabase.FindAssets("t:Locale", new[] { LocaleDirectory });
        foreach (var guid in localeGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null) continue;

            var code = locale.Identifier.Code;
            if (!usedLocaleCodes.Contains(code))
            {
                LocalizationEditorSettings.RemoveLocale(locale);
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[Localization] Removed unused Locale: {code} ({path})");
            }
        }
    }

    private static Locale GetOrCreateLocale(string localeId)
    {
        var locale = LocalizationEditorSettings.GetLocale(localeId);
        if (locale != null)
            return locale;

        EnsureFolder(LocaleDirectory);
        locale = Locale.CreateLocale(localeId);
        var assetPath = $"{LocaleDirectory}/{localeId}.asset";
        AssetDatabase.CreateAsset(locale, assetPath);
        LocalizationEditorSettings.AddLocale(locale);
        Debug.Log($"[Localization] Created Locale: {localeId} ({assetPath})");
        return locale;
    }

    private static void SetupAppNameMetadataIfNeeded(string tableName, string appNameKey)
    {
        var settings = LocalizationSettings.Instance;
        if (settings == null)
        {
            Debug.LogWarning("[Localization] LocalizationSettings asset not found. AppName metadata setup skipped.");
            return;
        }

        var metadata = LocalizationSettings.Metadata;

        var iosInfo = metadata.GetMetadata<IosAppInfo>();
        if (iosInfo == null)
        {
            iosInfo = new IosAppInfo();
            metadata.AddMetadata(iosInfo);
        }
        if (iosInfo.ShortName == null || iosInfo.ShortName.IsEmpty)
            iosInfo.ShortName = new LocalizedString(tableName, appNameKey);
        if (iosInfo.DisplayName == null || iosInfo.DisplayName.IsEmpty)
            iosInfo.DisplayName = new LocalizedString(tableName, appNameKey);

        var androidInfo = metadata.GetMetadata<AndroidAppInfo>();
        if (androidInfo == null)
        {
            androidInfo = new AndroidAppInfo();
            metadata.AddMetadata(androidInfo);
        }
        if (androidInfo.DisplayName == null || androidInfo.DisplayName.IsEmpty)
            androidInfo.DisplayName = new LocalizedString(tableName, appNameKey);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}

public static class GoogleConfigFinder
{
    public static GoogleOAuthConfig FindConfig()
    {
        var guids = AssetDatabase.FindAssets("t:GoogleOAuthConfig");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var cfg = AssetDatabase.LoadAssetAtPath<GoogleOAuthConfig>(path);
            if (cfg != null) return cfg;
        }
        return null;
    }
}
#endif

