using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeartPuzzleDraggableLabel : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Label Type")]
    [SerializeField] private HeartPartType partType =
        HeartPartType.RightAtrium;

    [Header("Hover")]
    [SerializeField] private float hoverMultiplier = 1.06f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("Hover Glow")]
    [SerializeField] private Color hoverGlowColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float hoverGlowAlpha = 0.5f;

    [Header("Placement Feedback")]
    [SerializeField] private Color correctColor =
        new Color(0.15f, 1f, 0.3f, 1f);

    [SerializeField] private Color wrongColor =
        new Color(1f, 0.08f, 0.08f, 1f);

    [SerializeField] private float feedbackDuration = 0.5f;

    [SerializeField] private Vector2 feedbackOutlineDistance =
        new Vector2(8f, -8f);

    [Header("Glow Shape")]
    [SerializeField] private float glowSize = 3f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private HeartPuzzleManager puzzleManager;
    private HeartPuzzleLives puzzleLives;

    private Outline feedbackOutline;

    private Transform originalParent;
    private Vector2 originalPosition;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerOver;
    private bool isDragging;
    private bool isLocked;

    private Coroutine feedbackCoroutine;

    private readonly List<Shadow> glowShadows =
        new List<Shadow>();

    public HeartPartType PartType => partType;

    public void SetPartType(HeartPartType newPartType)
    {
        partType = newPartType;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        puzzleManager =
            FindFirstObjectByType<HeartPuzzleManager>();

        puzzleLives =
            FindFirstObjectByType<HeartPuzzleLives>();

        originalParent = rectTransform.parent;
        originalPosition = rectTransform.anchoredPosition;

        originalScale = rectTransform.localScale;
        targetScale = originalScale;

        CreateHoverGlow();
        CreateFeedbackOutline();

        SetHoverGlow(hoverGlowColor, 0f);
    }

    private void CreateHoverGlow()
    {
        Vector2[] directions =
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f),

            new Vector2(-0.7f, 0.7f),
            new Vector2(0.7f, 0.7f),
            new Vector2(-0.7f, -0.7f),
            new Vector2(0.7f, -0.7f)
        };

        foreach (Vector2 direction in directions)
        {
            Shadow shadow = gameObject.AddComponent<Shadow>();

            shadow.effectDistance = direction * glowSize;
            shadow.useGraphicAlpha = true;

            glowShadows.Add(shadow);
        }
    }

    private void CreateFeedbackOutline()
    {
        feedbackOutline = GetComponent<Outline>();

        if (feedbackOutline == null)
        {
            feedbackOutline = gameObject.AddComponent<Outline>();
        }

        feedbackOutline.effectDistance = feedbackOutlineDistance;
        feedbackOutline.useGraphicAlpha = false;
        feedbackOutline.enabled = false;
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            scaleSpeed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked || isDragging)
        {
            return;
        }

        isPointerOver = true;
        targetScale = originalScale * hoverMultiplier;

        SetHoverGlow(
            hoverGlowColor,
            hoverGlowAlpha
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked || isDragging)
        {
            return;
        }

        isPointerOver = false;
        targetScale = originalScale;

        SetHoverGlow(hoverGlowColor, 0f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            return;
        }

        isDragging = true;
        isPointerOver = false;

        targetScale = originalScale;
        SetHoverGlow(hoverGlowColor, 0f);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            return;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            return;
        }

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        GameObject objectUnderPointer =
            eventData.pointerCurrentRaycast.gameObject;

        HeartPuzzleDropZone dropZone = null;

        if (objectUnderPointer != null)
        {
            dropZone = objectUnderPointer
                .GetComponentInParent<HeartPuzzleDropZone>();
        }

        if (dropZone == null)
        {
            ReturnToStart();
            return;
        }

        if (dropZone.Accepts(partType))
        {
            dropZone.ShowCorrectFeedback();

            SnapTemporarilyToDropZone(dropZone);

            StartFeedback(
                correctColor,
                false,
                dropZone
            );

            return;
        }

        dropZone.ShowWrongFeedback();

        if (puzzleLives != null)
        {
            puzzleLives.LoseLife();
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleLives не е намерен."
            );
        }

        StartFeedback(
            wrongColor,
            true,
            null
        );
    }

    private void SnapTemporarilyToDropZone(
        HeartPuzzleDropZone dropZone)
    {
        RectTransform dropRect =
            dropZone.GetComponent<RectTransform>();

        rectTransform.SetParent(dropRect, false);

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.localScale =
            originalScale;

        targetScale = originalScale;

        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    private void StartFeedback(
        Color color,
        bool returnAfter,
        HeartPuzzleDropZone correctZone)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(
            FeedbackRoutine(
                color,
                returnAfter,
                correctZone
            )
        );
    }

    private IEnumerator FeedbackRoutine(
        Color color,
        bool returnAfter,
        HeartPuzzleDropZone correctZone)
    {
        feedbackOutline.effectColor = color;
        feedbackOutline.enabled = true;

        yield return new WaitForSeconds(
            feedbackDuration
        );

        feedbackOutline.enabled = false;
        feedbackCoroutine = null;

        if (returnAfter)
        {
            ReturnToStart();
            yield break;
        }

        if (puzzleManager != null)
        {
            puzzleManager.HandleCorrectPlacement(
                correctZone
            );
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleManager не е намерен."
            );
        }
    }

    private void ReturnToStart()
    {
        rectTransform.SetParent(
            originalParent,
            false
        );

        rectTransform.anchoredPosition =
            originalPosition;

        rectTransform.localScale =
            originalScale;

        targetScale = originalScale;

        isLocked = false;
        isDragging = false;
        isPointerOver = false;

        canvasGroup.blocksRaycasts = true;

        SetHoverGlow(hoverGlowColor, 0f);
    }

    public void ResetForNextLabel()
    {
        ReturnToStart();
        gameObject.SetActive(true);
    }

    private void SetHoverGlow(
        Color color,
        float alpha)
    {
        foreach (Shadow shadow in glowShadows)
        {
            if (shadow == null)
            {
                continue;
            }

            Color finalColor = color;
            finalColor.a = alpha;

            shadow.effectColor = finalColor;
        }
    }

    private void OnDisable()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        if (feedbackOutline != null)
        {
            feedbackOutline.enabled = false;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }

        SetHoverGlow(Color.white, 0f);
    }
}