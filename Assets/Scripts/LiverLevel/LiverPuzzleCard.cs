using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class LiverPuzzleCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Transform startParent;
    private Vector2 startPosition;

    private bool placedCorrectly = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placedCorrectly)
            return;

        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;

        // Преместваме картата най-отгоре, докато я влачим.
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        // Позволява на DropSlot-а под картата да получи OnDrop.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placedCorrectly)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!placedCorrectly)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        transform.SetParent(startParent, false);
        rectTransform.anchoredPosition = startPosition;
    }

    public void PlaceCorrectly(Transform slot)
    {
        placedCorrectly = true;

        transform.SetParent(slot, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        canvasGroup.blocksRaycasts = true;
    }
}