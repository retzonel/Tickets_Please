using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DraggableUI dragged = eventData.pointerDrag.GetComponent<DraggableUI>();

        if (dragged != null)
        {
            dragged.transform.SetParent(transform);
            dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
