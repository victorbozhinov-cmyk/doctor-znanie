using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Card Type")]
    [SerializeField] private CardType cardType;

    [Header("Drag Settings")]
    [SerializeField] private Transform dragLayer;

    public CardType CardType => cardType;
    public DropZone CurrentDropZone => currentDropZone;
    public bool IsPlaced => currentDropZone != null;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Transform originalParent;
    private Vector2 originalPosition;
    private int originalSiblingIndex;

    private DropZone currentDropZone;
    private DropZone hoveredDropZone;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        if (dragLayer == null && canvas != null)
        {
            dragLayer = canvas.transform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        hoveredDropZone = null;

        canvasGroup.alpha = 0.9f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
        {
            return;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (hoveredDropZone == null)
        {
            ReturnToCurrentPosition();
            return;
        }

        DropZone sourceZone = currentDropZone;
        DropZone targetZone = hoveredDropZone;

        hoveredDropZone = null;

        // Пускаме картата обратно върху същото поле.
        if (sourceZone == targetZone)
        {
            SnapToDropZone(sourceZone);
            return;
        }

        DraggableCard targetCard = targetZone.CurrentCard;

        // Размяна:
        // картата идва от поле и целевото поле вече има друга карта.
        if (sourceZone != null &&
            targetCard != null &&
            targetCard != this)
        {
            // Картата от целевото поле отива в старото поле.
            sourceZone.SetCurrentCard(targetCard);

            targetCard.currentDropZone = sourceZone;
            targetCard.hoveredDropZone = null;
            targetCard.SnapToDropZone(sourceZone);

            // Влачената карта заема целевото поле.
            targetZone.SetCurrentCard(this);

            currentDropZone = targetZone;
            SnapToDropZone(targetZone);

            return;
        }

        // Ако картата идва отляво и бъде сложена
        // върху заето поле, старата карта се връща у дома.
        if (targetCard != null && targetCard != this)
        {
            targetCard.ReturnHome();
        }

        // Освобождаваме старото поле на влачената карта.
        if (sourceZone != null)
        {
            sourceZone.Clear();
        }

        currentDropZone = targetZone;
        targetZone.SetCurrentCard(this);

        SnapToDropZone(targetZone);
    }

    public void SetHoveredDropZone(DropZone dropZone)
    {
        hoveredDropZone = dropZone;
    }

    private void SnapToDropZone(DropZone dropZone)
    {
        if (dropZone == null)
        {
            return;
        }

        transform.SetParent(dropZone.transform, false);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private void ReturnToCurrentPosition()
    {
        if (currentDropZone != null)
        {
            SnapToDropZone(currentDropZone);
        }
        else
        {
            ReturnHome();
        }
    }

    public void ReturnHome()
    {
        if (currentDropZone != null)
        {
            currentDropZone.Clear();
            currentDropZone = null;
        }

        hoveredDropZone = null;

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}