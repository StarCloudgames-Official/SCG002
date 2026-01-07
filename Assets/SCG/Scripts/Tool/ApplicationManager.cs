using System;
using Cysharp.Threading.Tasks;
using StarCloudgamesLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class ApplicationManager : Singleton<ApplicationManager>
{
    public event Action OnBackPressed;
    public event Action<bool> OnPausedChanged;
    public event Action<bool> OnFocusChanged;

    public bool IsPaused { get; private set; }
    public bool HasFocus { get; private set; } = true;
    public bool IsQuitting { get; private set; }

    private const int TargetFps = 60;
    private const float BackPressMinInterval = 0.3f;

    private float lastBackPressTime;

    #region Unity Lifecycle

    public override async UniTask Initialize()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFps;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        await UniTask.CompletedTask;
    }

    private void Update()
    {
        if (IsQuitting)
            return;

        CheckBackSpace();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        IsPaused = pauseStatus;
        OnPausedChanged?.Invoke(pauseStatus);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        HasFocus = hasFocus;
        OnFocusChanged?.Invoke(hasFocus);
    }

    private void OnApplicationQuit()
    {
        IsQuitting = true;
    }

    #endregion

    #region Input Handling

    private void CheckBackSpace()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            HandleBackTriggered();
        }
    }

    private void HandleBackTriggered()
    {
        var now = Time.unscaledTime;

        if (now - lastBackPressTime < BackPressMinInterval)
            return;

        lastBackPressTime = now;
        OnBackPressed?.Invoke();
    }

    #endregion

    #region Listener Registration

    public void AddBackListener(Action listener)
    {
        OnBackPressed += listener;
    }

    public void RemoveBackListener(Action listener)
    {
        OnBackPressed -= listener;
    }

    public void AddPauseListener(Action<bool> listener)
    {
        OnPausedChanged += listener;
    }

    public void RemovePauseListener(Action<bool> listener)
    {
        OnPausedChanged -= listener;
    }

    public void AddFocusListener(Action<bool> listener)
    {
        OnFocusChanged += listener;
    }

    public void RemoveFocusListener(Action<bool> listener)
    {
        OnFocusChanged -= listener;
    }

    #endregion
}
