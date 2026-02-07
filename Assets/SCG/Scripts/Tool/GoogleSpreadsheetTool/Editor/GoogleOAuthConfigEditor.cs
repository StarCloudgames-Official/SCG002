using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoogleOAuthConfig))]
public class GoogleOAuthConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var cfg = (GoogleOAuthConfig)target;

        GUILayout.Space(8);
        EditorGUILayout.LabelField("OAuth", EditorStyles.boldLabel);

        if (GUILayout.Button("Authorize (Open Browser)"))
        {
            Authorize(cfg);
        }

        if (GUILayout.Button("Refresh Access Token"))
        {
            RefreshAccessToken(cfg);
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Test: List Sheet Titles"))
        {
            ListSheetTitles(cfg);
        }
    }

    private async void Authorize(GoogleOAuthConfig cfg)
    {
        var ok = await GoogleSheetsClient.AuthorizeWithListener(cfg);
        if (ok) Debug.Log("[GoogleSheets] Authorization complete!");
    }

    private async void RefreshAccessToken(GoogleOAuthConfig cfg)
    {
        var ok = await GoogleSheetsClient.RefreshAccessToken(cfg);
        if (ok) Debug.Log("[GoogleSheets] Access token refreshed");
    }

    private async void ListSheetTitles(GoogleOAuthConfig cfg)
    {
        try
        {
            var meta = await GoogleSheetsClient.LoadMeta(cfg);
            if (meta?.sheets == null)
            {
                Debug.LogWarning("[GoogleSheets] No sheets");
                return;
            }

            foreach (var s in meta.sheets)
            {
                Debug.Log($"[GoogleSheets] Sheet: {s.properties.title}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GoogleSheets] Meta load failed: {e}");
        }
    }
}
