using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using SCG.GoogleSheets;

/// <summary>
/// Editor-only Google Sheets helper (OAuth + fetch APIs).
/// </summary>
public static class GoogleSheetsClient
{
    private const string Scope = "https://www.googleapis.com/auth/spreadsheets.readonly";
    private const string RedirectUri = "http://127.0.0.1:3000";
    private const string ListenerPrefix = "http://127.0.0.1:3000/";

    /// <summary>
    /// Opens the Google consent page AND starts a local HTTP listener to
    /// automatically capture the auth code and exchange it for tokens.
    /// </summary>
    public static async Task<bool> AuthorizeWithListener(GoogleOAuthConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[GoogleSheets] Config is null.");
            return false;
        }

        HttpListener listener = null;
        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(ListenerPrefix);
            listener.Start();
            Debug.Log("[GoogleSheets] Local listener started on " + RedirectUri);

            // Open the consent page in the browser
            string authUrl =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                "?client_id=" + config.clientId +
                "&redirect_uri=" + RedirectUri +
                "&response_type=code" +
                "&scope=" + Scope +
                "&access_type=offline" +
                "&prompt=consent";

            Application.OpenURL(authUrl);

            // Wait for Google to redirect back
            var context = await listener.GetContextAsync();
            string code = context.Request.QueryString["code"];
            string error = context.Request.QueryString["error"];

            // Send a response page to the browser
            string responseHtml;
            if (!string.IsNullOrEmpty(error))
            {
                responseHtml = "<html><body><h2>Authorization failed</h2><p>" + error + "</p><p>You can close this tab.</p></body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseHtml);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
                Debug.LogError("[GoogleSheets] Authorization denied: " + error);
                return false;
            }

            if (string.IsNullOrEmpty(code))
            {
                responseHtml = "<html><body><h2>No auth code received</h2><p>You can close this tab.</p></body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseHtml);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
                Debug.LogError("[GoogleSheets] No auth code in redirect.");
                return false;
            }

            // Exchange the code for tokens
            bool ok = await RequestTokens(config, code);

            responseHtml = ok
                ? "<html><body><h2>Authorization successful!</h2><p>You can close this tab and return to Unity.</p></body></html>"
                : "<html><body><h2>Token exchange failed</h2><p>Check the Unity console for details.</p></body></html>";
            var buf = Encoding.UTF8.GetBytes(responseHtml);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buf.Length;
            await context.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
            context.Response.Close();

            return ok;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GoogleSheets] AuthorizeWithListener exception: {e}");
            return false;
        }
        finally
        {
            listener?.Stop();
            listener?.Close();
        }
    }

    public static async Task<bool> RequestTokens(GoogleOAuthConfig config, string code)
    {
        try
        {
            using var client = new HttpClient();
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("code", code),
                new KeyValuePair<string,string>("client_id", config.clientId),
                new KeyValuePair<string,string>("client_secret", config.clientSecret),
                new KeyValuePair<string,string>("redirect_uri", RedirectUri),
                new KeyValuePair<string,string>("grant_type", "authorization_code"),
            });

            var response = await client.PostAsync("https://oauth2.googleapis.com/token", form);
            var json = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<GoogleOAuthToken>(json);

            if (token == null || string.IsNullOrEmpty(token.access_token))
            {
                Debug.LogError("[GoogleSheets] Token request failed. Check code and credentials.");
                return false;
            }

            config.accessToken = token.access_token;
            if (!string.IsNullOrEmpty(token.refresh_token))
                config.refreshToken = token.refresh_token;

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GoogleSheets] RequestTokens exception: {e}");
            return false;
        }
    }

    public static async Task<bool> RefreshAccessToken(GoogleOAuthConfig config)
    {
        if (config == null || string.IsNullOrEmpty(config.refreshToken))
        {
            Debug.LogWarning("[GoogleSheets] No refresh token available.");
            return false;
        }

        using var client = new HttpClient();
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("client_id", config.clientId),
            new KeyValuePair<string,string>("client_secret", config.clientSecret),
            new KeyValuePair<string,string>("refresh_token", config.refreshToken),
            new KeyValuePair<string,string>("grant_type", "refresh_token"),
        });

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", form);
        if (!response.IsSuccessStatusCode)
        {
            Debug.LogError($"[GoogleSheets] RefreshAccessToken failed: {response.StatusCode}");
            return false;
        }

        var json = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<GoogleOAuthToken>(json);
        if (token == null || string.IsNullOrEmpty(token.access_token))
        {
            Debug.LogError("[GoogleSheets] RefreshAccessToken parse failed.");
            return false;
        }

        config.accessToken = token.access_token;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        return true;
    }

    public static async Task<string> GetStringWithAuth(GoogleOAuthConfig config, string url)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.accessToken);

        var response = await client.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await RefreshAccessToken(config);
            if (!refreshed)
                throw new Exception("Unauthorized and token refresh failed.");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.accessToken);
            response = await client.GetAsync(url);
        }

        if (!response.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)response.StatusCode} : {await response.Content.ReadAsStringAsync()}");

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<SpreadSheetMeta> LoadMeta(GoogleOAuthConfig config)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.sheetId}";
        var raw = await GetStringWithAuth(config, url);
        return JsonConvert.DeserializeObject<SpreadSheetMeta>(raw);
    }

    public static async Task<SheetResponse> LoadSheetValues(GoogleOAuthConfig config, string sheetNameOrRange)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.sheetId}/values/{sheetNameOrRange}";
        var raw = await GetStringWithAuth(config, url);
        return JsonConvert.DeserializeObject<SheetResponse>(raw);
    }

    [Serializable] public class GoogleOAuthToken { public string access_token; public string refresh_token; }
}
