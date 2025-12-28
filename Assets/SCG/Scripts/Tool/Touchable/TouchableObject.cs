using UnityEngine.EventSystems;

public class TouchableObject : CachedMonoBehaviour, ITouchable
{
    private bool isDragging;
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            OnDragEnd(eventData);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public virtual void OnDragEnd(PointerEventData eventData)
    {
        isDragging = false;
    }
}