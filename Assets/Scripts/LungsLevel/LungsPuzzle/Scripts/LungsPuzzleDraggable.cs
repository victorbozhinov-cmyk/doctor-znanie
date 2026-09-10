using UnityEngine;
using UnityEngine.EventSystems;

public class LungsPuzzleDraggable :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Puzzle")]
    [SerializeField]
    private LungsPuzzleManager puzzleManager;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPosition;

    private bool startPositionSaved;
    private bool dragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            rootCanvas = rootCanvas.rootCanvas;
        }

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SaveStartPosition();
    }

    private void SaveStartPosition()
    {
        if (rectTransform == null)
            return;

        startAnchoredPosition =
            rectTransform.anchoredPosition;

        startPositionSaved = true;
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (puzzleManager == null)
            return;

        if (!puzzleManager.PuzzleActive)
            return;

        dragging = true;

        if (!startPositionSaved)
        {
            SaveStartPosition();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        transform.SetAsLastSibling();
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!dragging)
            return;

        if (puzzleManager == null ||
            !puzzleManager.PuzzleActive)
        {
            return;
        }

        if (rectTransform == null)
            return;

        float scaleFactor = 1f;

        if (rootCanvas != null &&
            rootCanvas.scaleFactor > 0f)
        {
            scaleFactor = rootCanvas.scaleFactor;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / scaleFactor;
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (!dragging)
            return;

        dragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (puzzleManager == null ||
            !puzzleManager.PuzzleActive)
        {
            ResetToStart();
            return;
        }

        Camera eventCamera =
            eventData.pressEventCamera;

        bool correct =
            puzzleManager.IsCorrectTarget(
                eventData.position,
                eventCamera
            );

        if (correct)
        {
            ResetToStart();
            puzzleManager.CorrectPlacement();
            return;
        }

        bool overAnotherTarget =
            puzzleManager.IsOverAnyTarget(
                eventData.position,
                eventCamera
            );

        if (overAnotherTarget)
        {
            puzzleManager.WrongPlacement();
            ResetToStart();
            return;
        }

        puzzleManager.DroppedOnEmptySpace();

        ResetToStart();
    }

    public void ResetToStart()
    {
        dragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        if (!startPositionSaved)
        {
            SaveStartPosition();
        }

        rectTransform.anchoredPosition =
            startAnchoredPosition;
    }
}