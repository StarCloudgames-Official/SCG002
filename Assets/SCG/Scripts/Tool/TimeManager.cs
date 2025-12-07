using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public static class TimeManager
{
    private const string GoogleUrl = "https://www.google.com";

    private static DateTime? serverTime;
    private static float lastSyncRealtimeSinceStartup;
    
    private static bool initialized;
    private static bool isSynced;
    
    public static DateTime UtcNow => GetCurrentUtcTime();
    public static DateTime TodayMidnightUtc => UtcNow.Date;
    public static DateTime TomorrowMidnightUtc => UtcNow.Date.AddDays(1);
    
    #region Initialize

    public static async Awaitable Initialize()
    {
        if (initialized) return;
        initialized = true;
        
        await Sync();
        
        ApplicationManager.Instance.AddPauseListener(OnApplicationPause);
    }

    #endregion

    #region Sync

    public static async Awaitable<bool> Sync()
    {
        var time = await FetchTimeFromGoogle();
        
        if (time.HasValue)
        {
            serverTime = time.Value;
            lastSyncRealtimeSinceStartup = Time.realtimeSinceStartup;
            isSynced = true;
            Debug.Log($"[TimeManager] Synced: {serverTime.Value:yyyy-MM-dd HH:mm:ss} UTC");
            return true;
        }
        
        Debug.LogWarning("[TimeManager] Failed to sync server time");
        return false;
    }

    private static async Awaitable<DateTime?> FetchTimeFromGoogle()
    {
        try
        {
            using var request = UnityWebRequest.Head(GoogleUrl);
            request.timeout = 10;
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var dateHeader = request.GetResponseHeader("Date");
                if (!string.IsNullOrEmpty(dateHeader))
                {
                    if (DateTime.TryParseExact(dateHeader, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                    {
                        return parsed.ToUniversalTime();
                    }
                    
                    if (DateTime.TryParse(dateHeader, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed2))
                    {
                        return parsed2.ToUniversalTime();
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[TimeManager] Google request error: {request.error}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TimeManager] Exception: {e.Message}");
        }
        
        return null;
    }

    #endregion

    #region Time Calculation

    private static DateTime GetCurrentUtcTime()
    {
        if (!serverTime.HasValue)
        {
            return DateTime.UtcNow;
        }
        
        var elapsed = Time.realtimeSinceStartup - lastSyncRealtimeSinceStartup;
        return serverTime.Value.AddSeconds(elapsed);
    }

    #endregion

    #region Background Handling

    private static void OnApplicationPause(bool paused)
    {
        if (!paused)
        {
            Sync().Forget();
        }
    }

    #endregion

    #region Utility

    public static double SecondsSince(DateTime utcTime)
    {
        return (UtcNow - utcTime).TotalSeconds;
    }

    public static bool HasPassed(DateTime utcTime)
    {
        return UtcNow >= utcTime;
    }



    #endregion
}