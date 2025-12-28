using UnityEngine;
using UnityEngine.EventSystems;

public interface ITouchable : IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler
{
}