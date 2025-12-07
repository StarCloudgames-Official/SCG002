using System;
using System.Collections.Generic;
using UnityEngine;

public class PooledGridScrollView<TData, TCell> : PooledScrollViewBase<TData, TCell> where TCell : PooledScrollCell<TData>
{
    [SerializeField] private int columns = 1;
    [SerializeField] private Vector2 spacing = new(4f, 4f);
    [SerializeField] private Vector2 cellSize;

    private Func<TData, int, Vector2> sizeProvider;
    private readonly List<Vector2> itemSizes = new();
    private readonly List<float> rowOffsets = new();
    private readonly List<float> rowHeights = new();

    private float totalHeight;
    private float contentWidth;

    public void SetData(IList<TData> source, Func<TData, int, Vector2> sizeFunc = null)
    {
        sizeProvider = sizeFunc;
        base.SetData(source);
    }

    protected override void ConfigureScrollRect()
    {
        if (scrollRect == null) return;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
    }

    protected override float GetViewportSize()
    {
        if (viewport == null) return 0f;
        return viewport.rect.height;
    }

    protected override float GetScrollPosition()
    {
        if (content == null) return 0f;
        return content.anchoredPosition.y;
    }

    protected override void SetScrollPosition(float value)
    {
        if (content == null) return;
        var pos = content.anchoredPosition;
        pos.y = value;
        content.anchoredPosition = pos;
    }

    protected override float ClampScrollPosition(float value)
    {
        var maxOffset = Mathf.Max(0f, totalHeight - GetViewportSize());
        return Mathf.Clamp(value, 0f, maxOffset);
    }

    protected override void GetViewBounds(float scrollPos, float viewportSize, out float viewStart, out float viewEnd)
    {
        viewStart = scrollPos;
        viewEnd = viewStart + viewportSize;
    }

    protected override void GetVisibleRange(float viewStart, float viewEnd, out int first, out int last)
    {
        if (data.Count == 0 || columns <= 0)
        {
            first = -1;
            last = -1;
            return;
        }

        var firstRow = BinaryFindFirstRow(viewStart);
        var lastRow = BinaryFindLastRow(viewEnd);

        if (firstRow < 0 || lastRow < firstRow)
        {
            first = -1;
            last = -1;
            return;
        }

        first = Mathf.Clamp(firstRow * columns, 0, data.Count - 1);
        last = Mathf.Clamp(((lastRow + 1) * columns) - 1, 0, data.Count - 1);
    }

    protected override void PositionCell(TCell cell, int index)
    {
        var size = GetSize(index);
        var (row, col) = GetRowCol(index);
        var x = col * (cellSize.x + spacing.x);
        var y = rowOffsets[row];

        var rect = cell.CachedRectTransform;
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = size;
    }

    protected override void RebuildLayoutCaches()
    {
        var prefabSize = GetPrefabSize();
        var count = Mathf.Max(0, data.Count);
        var safeColumns = Mathf.Max(1, columns);
        var rows = (count + safeColumns - 1) / safeColumns;
        if (cellSize.x <= 0f || cellSize.y <= 0f) cellSize = prefabSize;
        if (cellSize.x <= 0f) cellSize.x = 1f;
        if (cellSize.y <= 0f) cellSize.y = 1f;

        EnsureListCapacity(itemSizes, count);
        itemSizes.Clear();
        EnsureListCapacity(rowOffsets, rows);
        EnsureListCapacity(rowHeights, rows);
        rowOffsets.Clear();
        rowHeights.Clear();

        for (var i = 0; i < count; i++)
        {
            itemSizes.Add(GetSizeFromProvider(i, prefabSize));
        }

        float offset = 0f;
        for (var r = 0; r < rows; r++)
        {
            var height = GetRowHeight(r, safeColumns);
            rowOffsets.Add(offset);
            rowHeights.Add(height);
            offset += height + spacing.y;
        }

        totalHeight = rows > 0 ? offset - spacing.y : 0f;
        if (totalHeight < 0f) totalHeight = 0f;

        contentWidth = safeColumns * cellSize.x + (safeColumns - 1) * spacing.x;
    }

    protected override void UpdateContentSize()
    {
        if (content == null) return;
        var size = content.sizeDelta;
        size.y = totalHeight;
        size.x = contentWidth;
        content.sizeDelta = size;
    }

    protected override float GetScrollPositionForIndex(int index, bool center)
    {
        if (data == null || index < 0 || index >= data.Count) return -1f;
        var viewportSize = GetViewportSize();
        if (viewportSize <= 0f) return -1f;

        var (row, _) = GetRowCol(index);
        var start = rowOffsets[row];
        var height = rowHeights[row];

        var target = start;
        if (center)
        {
            target -= Mathf.Max(0f, (viewportSize - height) * 0.5f);
        }

        return target;
    }

    private Vector2 GetPrefabSize()
    {
        if (cellPrefab == null) return new Vector2(100f, 100f);
        var rect = cellPrefab.GetComponent<RectTransform>();
        if (rect == null) return new Vector2(100f, 100f);
        return rect.rect.size;
    }

    private Vector2 GetSizeFromProvider(int index, Vector2 fallback)
    {
        if (sizeProvider != null && data != null)
        {
            try
            {
                var size = sizeProvider.Invoke(data[index], index);
                if (size.x <= 0f) size.x = fallback.x;
                if (size.y <= 0f) size.y = fallback.y;
                return size;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        return cellSize;
    }

    private float GetRowHeight(int row, int safeColumns)
    {
        var start = row * safeColumns;
        var end = Mathf.Min(start + safeColumns, data.Count);

        float maxHeight = 0f;
        for (var i = start; i < end; i++)
        {
            maxHeight = Mathf.Max(maxHeight, itemSizes[i].y);
        }

        return Mathf.Max(0.0001f, maxHeight);
    }

    private (int row, int col) GetRowCol(int index)
    {
        var safeColumns = Mathf.Max(1, columns);
        var row = index / safeColumns;
        var col = index % safeColumns;
        return (row, col);
    }

    private Vector2 GetSize(int index)
    {
        if (index < 0 || index >= itemSizes.Count) return cellSize;
        return itemSizes[index];
    }

    private int BinaryFindFirstRow(float viewStart)
    {
        var low = 0;
        var high = rowOffsets.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var mid = (low + high) >> 1;
            var start = rowOffsets[mid];
            var end = start + rowHeights[mid];

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

    private int BinaryFindLastRow(float viewEnd)
    {
        var low = 0;
        var high = rowOffsets.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var mid = (low + high) >> 1;
            var start = rowOffsets[mid];
            var end = start + rowHeights[mid];

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
}
