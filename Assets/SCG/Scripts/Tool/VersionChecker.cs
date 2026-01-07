using System;
using Cysharp.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine;

public static class VersionChecker
{
    private const string KeyAppVersion = "app_version";

    private static bool initialized;

    public static bool IsUpdateRequired { get; private set; }

    public static UniTask<bool> CheckVersion()
    {
        if (initialized) return UniTask.FromResult(!IsUpdateRequired);

#if UNITY_EDITOR
        Debug.Log("[VersionChecker] Skipped in Editor");
        initialized = true;
        return UniTask.FromResult(true);
#else
        return CheckVersionInternal();
#endif
    }

#if !UNITY_EDITOR
    private static async UniTask<bool> CheckVersionInternal()
    {
        try
        {
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

            await remoteConfig.SetDefaultsAsync(new System.Collections.Generic.Dictionary<string, object>
            {
                { KeyAppVersion, Application.version }
            });

            await remoteConfig.FetchAsync(TimeSpan.Zero);
            await remoteConfig.ActivateAsync();

            var remoteVersion = remoteConfig.GetValue(KeyAppVersion).StringValue;
            var currentVersion = Application.version;

            IsUpdateRequired = remoteVersion != currentVersion;

            initialized = true;

            Debug.Log($"[VersionChecker] Current: {currentVersion}, Remote: {remoteVersion}, UpdateRequired: {IsUpdateRequired}");

            return !IsUpdateRequired;
        }
        catch (Exception e)
        {
            Debug.LogError($"[VersionChecker] Failed: {e.Message}");
            return true;
        }
    }
#endif

    public static void OpenStore()
    {
#if UNITY_ANDROID
        Application.OpenURL($"market://details?id={Application.identifier}");
#elif UNITY_IOS
        Application.OpenURL($"itms-apps://itunes.apple.com/app/{Application.identifier}");
#endif
    }

    public static void HandleUpdateRequired()
    {
        if (!IsUpdateRequired) return;

        Debug.Log("[VersionChecker] Update required, opening store...");
        OpenStore();
    }
}
