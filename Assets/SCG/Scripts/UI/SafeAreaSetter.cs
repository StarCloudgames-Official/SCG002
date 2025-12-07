using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class SafeAreaSetter : CachedMonoBehaviour
{
    private RectTransform rectTransform;

    private void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;

        rectTransform ??= GetComponent<RectTransform>();

        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}