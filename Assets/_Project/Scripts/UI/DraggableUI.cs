using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool debugMode;
    [SerializeField] private bool interactable;
    [SerializeField] private RectTransform validDropRegion;

    [Header("---------- Events ----------")]
    public UnityEvent onBeginDrag;
    public UnityEvent onEndDrag;
    public UnityEvent onValidDrop;
    public UnityEvent onInvalidDrop;

    public bool Interactable
    {
        get { return interactable; }
        set { interactable = value; }
    }

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        transform.SetAsLastSibling();
        onBeginDrag?.Invoke();

        if (debugMode) Debug.Log("Drag started");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (!IsInsideDropRegion())
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        onEndDrag?.Invoke();

        bool isValid = IsInsideDropRegion();

        if (isValid)
        {
            onValidDrop?.Invoke();
            if (debugMode) Debug.Log("Dropped inside VALID region");
        }
        else
        {
            onInvalidDrop?.Invoke();
            ResetPosition();
            if (debugMode) Debug.Log("Dropped OUTSIDE valid region, returning to original position");
        }
    }

    private bool IsInsideDropRegion()
    {
        Vector3[] worldCorners = new Vector3[4];
        validDropRegion.GetWorldCorners(worldCorners);

        Vector3 panelCenter = rectTransform.position;

        bool inside =
            panelCenter.x >= worldCorners[0].x &&
            panelCenter.x <= worldCorners[2].x &&
            panelCenter.y >= worldCorners[0].y &&
            panelCenter.y <= worldCorners[2].y;

        if(debugMode) Debug.Log($"Inside check: {inside} | PanelPos: {panelCenter}");

        return inside;
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
}