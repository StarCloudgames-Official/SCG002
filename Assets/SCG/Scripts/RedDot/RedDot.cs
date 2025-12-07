using UnityEngine;

public class RedDot : CachedMonoBehaviour
{
    [Tooltip("레드닷 경로. 예) Mail, Mission/Daily, Mission/Weekly")]
    [SerializeField] private string path;

    public string Path => path;

    private bool registered;

    private void OnEnable()
    {
        RegisterIfNeeded();
    }

    private void OnDisable()
    {
        UnregisterIfNeeded();
    }

    internal void SetVisual(bool on)
    {
        CachedGameObject.SetActive(on);
    }

    public void SetPath(string newPath)
    {
        if (newPath == path) return;

        UnregisterIfNeeded();

        path = newPath;

        RegisterIfNeeded();
    }

    public void ClearPath()
    {
        UnregisterIfNeeded();
        path = null;
        SetVisual(false);
    }

    private void RegisterIfNeeded()
    {
        if (!isActiveAndEnabled) return;
        if (string.IsNullOrEmpty(path)) return;
        if (registered) return;

        RedDotSystem.Register(this);
        registered = true;
    }

    private void UnregisterIfNeeded()
    {
        if (!registered) return;
        registered = false;

        if (string.IsNullOrEmpty(path)) return;

        RedDotSystem.Unregister(this);
    }
}