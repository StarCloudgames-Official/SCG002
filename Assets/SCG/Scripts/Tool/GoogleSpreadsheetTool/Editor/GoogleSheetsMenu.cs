using UnityEditor;
using UnityEngine;

public static class GoogleSheetsMenu
{
    [MenuItem("SCG/Tools/Google/Create OAuth Config Asset")]
    public static void CreateConfig()
    {
        var path = "Assets/_Project/1. Scripts/Tool/GoogleSpreadsheetTool/GoogleOAuthConfig.asset";
        var cfg = AssetDatabase.LoadAssetAtPath<GoogleOAuthConfig>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<GoogleOAuthConfig>();
            AssetDatabase.CreateAsset(cfg, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cfg;
            Debug.Log($"[GoogleSheets] Created config asset at {path}");
        }
        else
        {
            Selection.activeObject = cfg;
            Debug.Log($"[GoogleSheets] Config asset already exists at {path}");
        }
    }
}

