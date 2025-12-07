using UnityEngine;

[CreateAssetMenu(fileName = "GoogleOAuthConfig", menuName = "SCG/Google Spreadsheet/OAuth Config")]
public class GoogleOAuthConfig : ScriptableObject
{
    [Header("OAuth Credentials")]
    public string clientId;
    public string clientSecret;

    [Header("OAuth Tokens (auto filled)")]
    public string accessToken;
    public string refreshToken;

    [Header("Google Sheet Info")]
    [Tooltip("Google Sheet ID (the part between /d/ and /edit in the URL)")]
    public string sheetId;
}

