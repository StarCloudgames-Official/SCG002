using UnityEngine;

public abstract class CachedMonoBehaviour : MonoBehaviour
{
    private Transform _cachedTransform;
    public Transform CachedTransform  => _cachedTransform ??= transform;

    private GameObject _cachedGameObject;
    public GameObject CachedGameObject => _cachedGameObject ??= gameObject;

    private RectTransform _cachedRectTransform;
    public RectTransform CachedRectTransform => _cachedRectTransform ??= GetComponent<RectTransform>();
}