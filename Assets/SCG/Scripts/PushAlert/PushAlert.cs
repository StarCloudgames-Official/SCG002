using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public static class PushAlert
{
#if UNITY_ANDROID
    private const string ChannelId = "default_channel";
#endif

    private static bool initialized = false;
    public static bool IsInitialized => initialized;

    public static async Awaitable Initialize()
    {
        if (initialized) return;

#if UNITY_ANDROID
        // Android 13 (API 33)+ requires POST_NOTIFICATIONS permission
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            
            // Wait for permission dialog to be dismissed
            await Awaitable.WaitForSecondsAsync(0.5f);
            while (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS") 
                   && IsPermissionDialogOpen())
            {
                await Awaitable.NextFrameAsync();
            }
        }

        var channel = new AndroidNotificationChannel
        {
            Id = ChannelId,
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "General Notifications"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#elif UNITY_IOS

        // iOS requires authorization request
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Sound | AuthorizationOption.Badge;
        using var req = new AuthorizationRequest(authorizationOption, true);
        
        while (!req.IsFinished)
        {
            await Awaitable.NextFrameAsync();
        }

        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

        initialized = true;
        Debug.Log("[PushAlert] Initialized");
    }

#if UNITY_ANDROID
    private static bool IsPermissionDialogOpen()
    {
        // Simple heuristic: if app is not focused, dialog might be open
        return !Application.isFocused;
    }
#endif

    public static void Push(string title, string description, float publishTimeSecond)
    {
        if (!initialized)
        {
            Debug.LogWarning("[PushAlert] Not initialized. Call Initialize() first.");
            return;
        }

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = description,
            FireTime = System.DateTime.Now.AddSeconds(publishTimeSecond),
            SmallIcon = "icon_0",
            LargeIcon = "icon_0"
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);

#elif UNITY_IOS
        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = System.TimeSpan.FromSeconds(publishTimeSecond),
            Repeats = false
        };

        var notification = new iOSNotification
        {
            Identifier = System.Guid.NewGuid().ToString(),
            Title = title,
            Body = description,
            ShowInForeground = true,
            // 앱 켜져 있을 때도 배너/사운드 나오게 할지
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);

#else
        // 에디터 / 기타 플랫폼에선 그냥 로그만
        Debug.Log($"[PushAlert] {publishTimeSecond}초 후 알림: {title} - {description}");
#endif
    }

    public static void CancelAll()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
#else
        Debug.Log("[PushAlert] CancelAll (No-op on this platform)");
#endif
    }
}
