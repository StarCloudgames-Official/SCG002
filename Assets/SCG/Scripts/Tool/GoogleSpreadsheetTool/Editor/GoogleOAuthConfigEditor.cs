using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoogleOAuthConfig))]
public class GoogleOAuthConfigEditor : Editor
{
    private string authCode = string.Empty;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var cfg = (GoogleOAuthConfig)target;

        GUILayout.Space(8);
        EditorGUILayout.LabelField("OAuth", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Google Consent Page"))
        {
            GoogleSheetsClient.OpenAuthLink(cfg);
        }

        authCode = EditorGUILayout.TextField("Auth Code", authCode);

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(authCode));
        if (GUILayout.Button("Request Tokens"))
        {
            RequestTokens(cfg, authCode);
        }
        EditorGUI.EndDisabledGroup();

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

    private async void RequestTokens(GoogleOAuthConfig cfg, string code)
    {
        var ok = await GoogleSheetsClient.RequestTokens(cfg, code);
        if (ok) Debug.Log("[GoogleSheets] Token request OK");
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

