using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using UnityEngine;

public static class AppEvent
{
    private static bool initialized;

    #region Initialize

    public static async UniTask Initialize()
    {
        if (initialized)
            return;

        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        await dependencyTask;

        if (dependencyTask.Result == DependencyStatus.Available)
        {
            var app = FirebaseApp.DefaultInstance;

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

#if UNITY_EDITOR
            Crashlytics.IsCrashlyticsCollectionEnabled = false;
#else
            Crashlytics.IsCrashlyticsCollectionEnabled = true;
#endif

            initialized = true;
            Debug.Log("[AppEvent] Firebase Initialized");
        }
        else
        {
            Debug.LogError($"[AppEvent] Firebase dependency error: {dependencyTask.Result}");
        }
    }

    #endregion

    #region Log

    public static void LogEvent(string eventName)
    {
        if (!initialized)
            return;

        FirebaseAnalytics.LogEvent(eventName);
#if UNITY_EDITOR
        Debug.Log($"[Analytics] {eventName}");
#endif
    }

    public static void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (!initialized || parameters == null)
            return;

        var paramList = new List<Parameter>();

        foreach (var p in parameters)
        {
            switch (p.Value)
            {
                case string value:
                    paramList.Add(new Parameter(p.Key, value));
                    break;

                case int:
                case long:
                    paramList.Add(new Parameter(p.Key, System.Convert.ToInt64(p.Value)));
                    break;

                case float:
                case double:
                    paramList.Add(new Parameter(p.Key, System.Convert.ToDouble(p.Value)));
                    break;
            }
        }

        FirebaseAnalytics.LogEvent(eventName, paramList.ToArray());

#if UNITY_EDITOR
        Debug.Log($"[Analytics] {eventName} | {parameters.Count} params");
#endif
    }

    #endregion

    #region Crashlytics

    public static void SetUser(string userId)
    {
        if (!initialized) return;
        Crashlytics.SetUserId(userId);
    }

    public static void LogError(string message)
    {
        if (!initialized) return;
        Crashlytics.Log(message);
#if UNITY_EDITOR
        Debug.LogWarning($"[Crashlytics Log] {message}");
#endif
    }

    public static void ForceCrash()
    {
#if !UNITY_EDITOR
        throw new System.Exception("🔥 Crashlytics Test Crash");
#else
        Debug.LogWarning("[Crash Test] Editor에서는 크래시 안 됨");
#endif
    }

    #endregion
}
