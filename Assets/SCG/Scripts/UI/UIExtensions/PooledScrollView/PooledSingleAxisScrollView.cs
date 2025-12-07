using System;
using System.Collections.Generic;
using UnityEngine;

public enum ScrollAxis
{
    Vertical,
    Horizontal
}

public abstract class PooledSingleAxisScrollView<TData, TCell> : PooledScrollViewBase<TData, TCell> where TCell : PooledScrollCell<TData>
{
    [SerializeField] private float spacing = 4f;
    [SerializeField] protected ScrollAxis axis = ScrollAxis.Vertical;
    [SerializeField] private bool reverse;

    private Func<TData, int, float> sizeProvider;
    private readonly List<float> itemSizes = new();
    private readonly List<float> itemOffsets = new();

    private float totalLength;
    private float baseItemSize;

    public void SetData(IList<TData> source, Func<TData, int, float> sizeFunc = null)
    {
        sizeProvider = sizeFunc;
        base.SetData(source);
    }

    public void SetItemSize(int index, float size)
    {
        if (data == null || index < 0 || index >= data.Count) return;
        if (itemSizes.Count != data.Count) return;

        size = Mathf.Max(0.0001f, size);
        itemSizes[index] = size;
        RecalculateOffsets(index);
        UpdateContentSize();
    }

    protected override void ConfigureScrollRect()
    {
        if (scrollRect == null) return;

        scrollRect.vertical = axis == ScrollAxis.Vertical;
        scrollRect.horizontal = axis == ScrollAxis.Horizontal;
    }

    protected override float GetViewportSize()
    {
        if (viewport == null) return 0f;
        return axis == ScrollAxis.Vertical ? viewport.rect.height : viewport.rect.width;
    }

    protected override float GetScrollPosition()
    {
        if (content == null) return 0f;
        var pos = content.anchoredPosition;
        return axis == ScrollAxis.Vertical ? pos.y : -pos.x;
    }

    protected override void SetScrollPosition(float value)
    {
        if (content == null) return;
        var pos = content.anchoredPosition;
        if (axis == ScrollAxis.Vertical)
            pos.y = value;
        else
            pos.x = -value;
        content.anchoredPosition = pos;
    }

    protected override float ClampScrollPosition(float value)
    {
        var maxOffset = Mathf.Max(0f, totalLength - GetViewportSize());
        return Mathf.Clamp(value, 0f, maxOffset);
    }

    protected override void GetViewBounds(float scrollPos, float viewportSize, out float viewStart, out float viewEnd)
    {
        viewStart = reverse ? Mathf.Max(0f, totalLength - viewportSize - scrollPos) : scrollPos;
        viewEnd = viewStart + viewportSize;
    }

    protected override void GetVisibleRange(float viewStart, float viewEnd, out int first, out int last)
    {
        if (reverse)
        {
            first = LinearFindFirst(viewStart, viewEnd);
            last = LinearFindLast(viewStart, viewEnd);
            return;
        }

        first = BinaryFindFirst(viewStart);
        last = BinaryFindLast(viewEnd);
    }

    protected override void PositionCell(TCell cell, int index)
    {
        var start = GetStartOffset(index);
        var size = GetSize(index);

        var rect = cell.CachedRectTransform;
        var anchored = rect.anchoredPosition;
        anchored = axis == ScrollAxis.Vertical
            ? new Vector2(anchored.x, -start)
            : new Vector2(start, anchored.y);
        rect.anchoredPosition = anchored;

        var delta = rect.sizeDelta;
        if (axis == ScrollAxis.Vertical)
            delta.y = size;
        else
            delta.x = size;
        rect.sizeDelta = delta;
    }

    protected override void RebuildLayoutCaches()
    {
        baseItemSize = GetBaseItemSize();

        var count = data.Count;
        EnsureListCapacity(itemSizes, count);
        EnsureListCapacity(itemOffsets, count);

        itemSizes.Clear();
        itemOffsets.Clear();

        float offset = 0f;
        for (var i = 0; i < count; i++)
        {
            var size = GetSizeFromProvider(i);
            itemSizes.Add(size);
            itemOffsets.Add(offset);
            offset += size + spacing;
        }

        totalLength = count > 0 ? offset - spacing : 0f;
        if (totalLength < 0f) totalLength = 0f;
    }

    protected override void UpdateContentSize()
    {
        if (content == null) return;

        var size = content.sizeDelta;
        if (axis == ScrollAxis.Vertical)
            size.y = totalLength;
        else
            size.x = totalLength;
        content.sizeDelta = size;
    }

    protected override float GetScrollPositionForIndex(int index, bool center)
    {
        if (data == null || index < 0 || index >= data.Count) return -1f;

        var viewportSize = GetViewportSize();
        if (viewportSize <= 0f) return -1f;

        var startOffset = GetStartOffset(index);
        var size = GetSize(index);

        var target = reverse
            ? Mathf.Max(0f, totalLength - viewportSize - startOffset)
            : startOffset;

        if (center)
        {
            target -= Mathf.Max(0f, (viewportSize - size) * 0.5f);
        }

        return target;
    }

    private float GetBaseItemSize()
    {
        if (cellPrefab == null) return 0f;
        var rect = cellPrefab.GetComponent<RectTransform>();
        if (rect == null) return 0f;
        var size = axis == ScrollAxis.Vertical ? rect.rect.height : rect.rect.width;
        return Mathf.Max(0.0001f, size);
    }

    private float GetSizeFromProvider(int index)
    {
        if (sizeProvider != null && data != null)
        {
            try
            {
                var size = sizeProvider.Invoke(data[index], index);
                return Mathf.Max(0.0001f, size);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        return baseItemSize > 0f ? baseItemSize : 0.0001f;
    }

    private float GetStartOffset(int index)
    {
        if (index < 0 || index >= itemOffsets.Count) return 0f;
        var start = itemOffsets[index];
        if (reverse)
        {
            start = Mathf.Max(0f, totalLength - itemSizes[index] - start);
        }

        return start;
    }

    private float GetSize(int index)
    {
        if (index < 0 || index >= itemSizes.Count) return 0f;
        return itemSizes[index];
    }

    private void RecalculateOffsets(int startIndex)
    {
        if (startIndex < 0) startIndex = 0;
        if (startIndex >= itemSizes.Count) return;

        float offset = startIndex == 0 ? 0f : itemOffsets[startIndex - 1] + itemSizes[startIndex - 1] + spacing;

        for (var i = startIndex; i < itemSizes.Count; i++)
        {
            itemOffsets[i] = offset;
            offset += itemSizes[i] + spacing;
        }

        totalLength = itemSizes.Count > 0 ? offset - spacing : 0f;
        if (totalLength < 0f) totalLength = 0f;
    }

    private int BinaryFindFirst(float viewStart)
    {
        var low = 0;
        var high = data.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var mid = (low + high) >> 1;
            var start = itemOffsets[mid];
            var end = start + itemSizes[mid];

            if (end >= viewStart)
            {
                result = mid;
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return result;
    }

    private int BinaryFindLast(float viewEnd)
    {
        var low = 0;
        var high = data.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var mid = (low + high) >> 1;
            var start = itemOffsets[mid];
            var end = start + itemSizes[mid];

            if (start <= viewEnd)
            {
                result = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return result;
    }

    private int LinearFindFirst(float viewStart, float viewEnd)
    {
        for (var i = 0; i < data.Count; i++)
        {
            var start = GetStartOffset(i);
            var end = start + itemSizes[i];
            if (end >= viewStart && start <= viewEnd) return i;
        }

        return -1;
    }

    private int LinearFindLast(float viewStart, float viewEnd)
    {
        for (var i = data.Count - 1; i >= 0; i--)
        {
            var start = GetStartOffset(i);
            var end = start + itemSizes[i];
            if (start <= viewEnd && end >= viewStart) return i;
        }

        return -1;
    }
}
