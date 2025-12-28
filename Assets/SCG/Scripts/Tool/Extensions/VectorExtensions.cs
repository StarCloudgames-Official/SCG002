using UnityEngine;
using UnityEngine.EventSystems;

public static class VectorExtensions
{
    public static Vector2 EventPointerToVector2(PointerEventData eventData, Camera camera)
    {
        var screenPosition = eventData.position;
        var worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        return new Vector2(worldPosition.x, worldPosition.y);
    }
}