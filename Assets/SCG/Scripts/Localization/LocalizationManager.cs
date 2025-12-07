using System;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class LocalizationManager
{
    private static string tableName = "LocalizationTable";
    private static bool initialized = false;

    public static void Initialize() => EnsureInitialized();

    public static string Localize(this string key) => Get(key);

    public static string Get(string key)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(key)) return string.Empty;

        var db = LocalizationSettings.StringDatabase;
        if (db == null) return string.Empty;

        try
        {
            TableReference tableRef = tableName;
            TableEntryReference entryRef = key;
            return db.GetLocalizedString(tableRef, entryRef);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string LocalizeFormat(this string key, params object[] args) => Format(key, args);

    public static string Format(string key, params object[] args)
    {
        var raw = Get(key);
        if (string.IsNullOrEmpty(raw)) return raw;

        try
        {
            return ZString.Format(raw, args);
        }
        catch
        {
            return raw;
        }
    }

    private static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        SetupLocaleFromSystemLanguage();
    }

    private static void SetupLocaleFromSystemLanguage()
    {
        var locales = LocalizationSettings.AvailableLocales;
        if (locales == null) return;

        var target = MapSystemLanguageToCode(Application.systemLanguage);
        Locale selected = null;

        foreach (var locale in locales.Locales)
        {
            if (locale.Identifier.Code.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                selected = locale;
                break;
            }
        }

        if (selected == null)
        {
            foreach (var locale in locales.Locales)
            {
                if (locale.Identifier.Code.StartsWith(target, StringComparison.OrdinalIgnoreCase))
                {
                    selected = locale;
                    break;
                }
            }
        }

        if (selected == null)
        {
            foreach (var locale in locales.Locales)
            {
                if (locale.Identifier.Code.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                {
                    selected = locale;
                    break;
                }
            }
        }

        if (selected == null && locales.Locales.Count > 0)
        {
            selected = locales.Locales[0];
        }

        if (selected != null)
        {
            LocalizationSettings.SelectedLocale = selected;
        }
    }

    private static string MapSystemLanguageToCode(SystemLanguage lang)
    {
        return lang switch
        {
            SystemLanguage.Korean => "ko",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Chinese => "zh-CN",
            SystemLanguage.ChineseSimplified => "zh-CN",
            SystemLanguage.ChineseTraditional => "zh-TW",
            _ => "en"
        };
    }
}
