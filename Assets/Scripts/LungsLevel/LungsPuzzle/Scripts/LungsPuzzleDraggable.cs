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

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        rootCanvas =
            GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            rootCanvas =
                rootCanvas.rootCanvas;
        }

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        SaveStartPosition();
    }

    // =====================================================
    // SAVE START POSITION
    // =====================================================

    private void SaveStartPosition()
    {
        if (rectTransform == null)
            return;

        startAnchoredPosition =
            rectTransform.anchoredPosition;

        startPositionSaved = true;
    }

    // =====================================================
    // BEGIN DRAG
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
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

    // =====================================================
    // DRAG
    // =====================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (rectTransform == null)
            return;

        float scaleFactor = 1f;

        if (rootCanvas != null &&
            rootCanvas.scaleFactor > 0f)
        {
            scaleFactor =
                rootCanvas.scaleFactor;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / scaleFactor;
    }

    // =====================================================
    // END DRAG
    // =====================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (puzzleManager == null)
        {
            ResetToStart();
            return;
        }

        Camera eventCamera =
            eventData.pressEventCamera;

        // ==========================================
        // 1. ПРАВИЛНИЯТ TARGET
        // ==========================================

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

        // ==========================================
        // 2. НЯКОЙ ДРУГ TARGET
        // ==========================================

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

        // ==========================================
        // 3. ПРАЗНО ПРОСТРАНСТВО
        // ==========================================

        puzzleManager.DroppedOnEmptySpace();

        ResetToStart();
    }

    // =====================================================
    // RESET
    // =====================================================

    public void ResetToStart()
    {
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