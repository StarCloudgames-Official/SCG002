using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public abstract class PooledScrollViewBase<TData, TCell> : CachedMonoBehaviour where TCell : PooledScrollCell<TData>
{
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected RectTransform viewport;
    [SerializeField] protected RectTransform content;
    [SerializeField] protected TCell cellPrefab;
    [SerializeField] protected int buffer = 2;
    [SerializeField] protected int prewarm;
    [SerializeField] protected bool updateEveryFrame = true;

    protected IList<TData> data = Array.Empty<TData>();

    private ObjectPool<TCell> pool;
    private readonly Dictionary<int, TCell> visible = new();
    private bool pendingRefresh;

    #region Unity

    protected virtual void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            viewport = scrollRect.viewport;
            content = scrollRect.content;
        }
        else
        {
            if (!viewport) viewport = CachedRectTransform;
            if (!content) content = viewport;
        }

        pool = new ObjectPool<TCell>(CreateCell, OnGetCell, OnReleaseCell, OnDestroyCell, collectionCheck: false);
        Prewarm();
    }

    protected virtual void OnEnable()
    {
        pendingRefresh = true;
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }
    }

    protected virtual void OnDisable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
    }

    protected virtual void Update()
    {
        if (updateEveryFrame)
        {
            UpdateVisible();
        }
        else if (pendingRefresh)
        {
            UpdateVisible();
            pendingRefresh = false;
        }
    }

    protected virtual void OnRectTransformDimensionsChange()
    {
        pendingRefresh = true;
    }

    protected virtual void OnValidate()
    {
        ConfigureScrollRect();
    }

    #endregion

    #region API

    public virtual void SetData(IList<TData> source)
    {
        data = source ?? Array.Empty<TData>();

        RebuildLayoutCaches();
        UpdateContentSize();
        ClearVisible();
        SetScrollPosition(ClampScrollPosition(GetScrollPosition()));
        pendingRefresh = true;
    }

    public void RefreshItem(int index)
    {
        if (!visible.TryGetValue(index, out var cell)) return;
        if (data == null || index < 0 || index >= data.Count) return;

        cell.BindData(data[index], index);
    }

    public void RefreshAll()
    {
        foreach (var kvp in visible)
        {
            RefreshItem(kvp.Key);
        }
    }

    public void ScrollTo(int index, bool center = false)
    {
        var target = GetScrollPositionForIndex(index, center);
        if (target < 0f) return;
        SetScrollPosition(ClampScrollPosition(target));
        UpdateVisible();
    }

    #endregion

    #region Pool

    private void Prewarm()
    {
        if (prewarm <= 0 || cellPrefab == null) return;

        var temp = new List<TCell>(prewarm);
        for (var i = 0; i < prewarm; i++)
        {
            var cell = pool.Get();
            temp.Add(cell);
        }

        foreach (var cell in temp)
        {
            pool.Release(cell);
        }
    }

    private TCell CreateCell()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("[PooledScrollView] Cell prefab is missing.");
            return null;
        }

        var cell = Instantiate(cellPrefab, content);
        cell.gameObject.SetActive(false);
        var rect = cell.CachedRectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        return cell;
    }

    private void OnGetCell(TCell cell)
    {
        if (cell == null) return;
        cell.gameObject.SetActive(true);
        cell.OnReused();
    }

    private void OnReleaseCell(TCell cell)
    {
        if (cell == null) return;
        cell.gameObject.SetActive(false);
    }

    private void OnDestroyCell(TCell cell)
    {
        if (cell != null)
        {
            Destroy(cell.gameObject);
        }
    }

    #endregion

    #region Internal

    private void OnScrollChanged(Vector2 _)
    {
        pendingRefresh = true;
    }

    private void ClearVisible()
    {
        foreach (var cell in visible.Values)
        {
            pool.Release(cell);
        }
        visible.Clear();
    }

    private void UpdateVisible()
    {
        if (viewport == null || content == null || data == null || data.Count == 0)
        {
            ClearVisible();
            return;
        }

        var viewportSize = GetViewportSize();
        if (viewportSize <= 0f) return;

        var currentPos = GetScrollPosition();
        var clampedPos = ClampScrollPosition(currentPos);

        GetViewBounds(clampedPos, viewportSize, out var viewStart, out var viewEnd);
        GetVisibleRange(viewStart, viewEnd, out var first, out var last);

        if (first < 0 || last < first)
        {
            ClearVisible();
            return;
        }

        first = Mathf.Max(0, first - buffer);
        last = Mathf.Min(data.Count - 1, last + buffer);

        RecycleOutside(first, last);
        RenderRange(first, last);
    }

    private void RecycleOutside(int first, int last)
    {
        EnsureListCapacity(s_tempIndices, visible.Count);
        s_tempIndices.Clear();
        foreach (var kvp in visible)
        {
            if (kvp.Key < first || kvp.Key > last)
            {
                s_tempIndices.Add(kvp.Key);
                pool.Release(kvp.Value);
            }
        }

        foreach (var idx in s_tempIndices)
        {
            visible.Remove(idx);
        }
    }

    private void RenderRange(int first, int last)
    {
        for (var i = first; i <= last; i++)
        {
            if (visible.TryGetValue(i, out var existing))
            {
                PositionCell(existing, i);
                existing.BindData(data[i], i);
                continue;
            }

            var cell = pool.Get();
            if (cell == null) continue;

            PositionCell(cell, i);
            cell.SetIndex(i);

            visible[i] = cell;

            cell.BindData(data[i], i);
        }
    }

    protected abstract void ConfigureScrollRect();
    protected abstract float GetViewportSize();
    protected abstract float GetScrollPosition();
    protected abstract void SetScrollPosition(float value);
    protected abstract float ClampScrollPosition(float value);
    protected abstract void GetViewBounds(float scrollPos, float viewportSize, out float viewStart, out float viewEnd);
    protected abstract void GetVisibleRange(float viewStart, float viewEnd, out int first, out int last);
    protected abstract void PositionCell(TCell cell, int index);
    protected abstract void RebuildLayoutCaches();
    protected abstract void UpdateContentSize();
    protected abstract float GetScrollPositionForIndex(int index, bool center);

    protected static void EnsureListCapacity<T>(List<T> list, int capacity)
    {
        if (list.Capacity < capacity) list.Capacity = capacity;
    }

    private static readonly List<int> s_tempIndices = new();

    #endregion
}
