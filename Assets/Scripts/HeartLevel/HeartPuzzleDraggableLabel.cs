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
    [SerializeField]
    private HeartPartType partType =
        HeartPartType.RightAtrium;

    [Header("Puzzle References")]
    [SerializeField]
    private HeartPuzzleManager puzzleManager;

    [Header("Hover")]
    [SerializeField]
    private float hoverMultiplier = 1.06f;

    [SerializeField]
    private float scaleSpeed = 10f;

    [Header("Hover Glow")]
    [SerializeField]
    private Color hoverGlowColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float hoverGlowAlpha = 0.5f;

    [Header("Normal Theme Feedback")]
    [SerializeField]
    private Color correctColor =
        new Color(0.15f, 1f, 0.3f, 1f);

    [SerializeField]
    private Color wrongColor =
        new Color(1f, 0.08f, 0.08f, 1f);

    [Header("Colorblind Theme Feedback")]
    [SerializeField]
    private Color colorblindCorrectColor =
        new Color32(0, 114, 178, 255); // #0072B2

    [SerializeField]
    private Color colorblindWrongColor =
        new Color32(213, 94, 0, 255); // #D55E00

    [Header("Feedback Settings")]
    [SerializeField]
    private float feedbackDuration = 0.5f;

    [SerializeField]
    private Vector2 feedbackOutlineDistance =
        new Vector2(8f, -8f);

    [Header("Glow Shape")]
    [SerializeField]
    private float glowSize = 3f;

    [Header("Return Animation")]
    [SerializeField]
    private UIDragReturnAnimation returnAnimation;

    private const string ThemeKey = "ColorTheme";

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Outline feedbackOutline;

    private Transform originalParent;
    private Vector2 originalPosition;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isDragging;
    private bool isLocked;
    private bool isReturning;

    private Coroutine feedbackCoroutine;

    private readonly List<Shadow> glowShadows =
        new List<Shadow>();

    public HeartPartType PartType => partType;

    public void SetPartType(
        HeartPartType newPartType)
    {
        partType = newPartType;
    }

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvas =
            GetComponentInParent<Canvas>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        if (returnAnimation == null)
        {
            returnAnimation =
                GetComponent<UIDragReturnAnimation>();
        }

        if (puzzleManager == null)
        {
            puzzleManager =
                FindFirstObjectByType<
                    HeartPuzzleManager
                >();
        }

        if (puzzleManager == null)
        {
            Debug.LogError(
                "HeartPuzzleManager не е свързан " +
                "към HeartPuzzleDraggableLabel.",
                this
            );
        }

        if (canvas == null)
        {
            Debug.LogError(
                "Не е намерен Canvas за draggable етикета.",
                this
            );
        }

        originalParent =
            rectTransform.parent;

        originalPosition =
            rectTransform.anchoredPosition;

        originalScale =
            rectTransform.localScale;

        targetScale =
            originalScale;

        CreateHoverGlow();
        CreateFeedbackOutline();

        SetHoverGlow(
            hoverGlowColor,
            0f
        );
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
            Shadow shadow =
                gameObject.AddComponent<Shadow>();

            shadow.effectDistance =
                direction * glowSize;

            shadow.useGraphicAlpha = true;

            glowShadows.Add(shadow);
        }
    }

    private void CreateFeedbackOutline()
    {
        feedbackOutline =
            GetComponent<Outline>();

        if (feedbackOutline == null)
        {
            feedbackOutline =
                gameObject.AddComponent<Outline>();
        }

        feedbackOutline.effectDistance =
            feedbackOutlineDistance;

        feedbackOutline.useGraphicAlpha =
            false;

        feedbackOutline.enabled =
            false;
    }

    private void Update()
    {
        if (rectTransform == null ||
            isReturning)
        {
            return;
        }

        rectTransform.localScale =
            Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                scaleSpeed * Time.deltaTime
            );
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (isLocked ||
            isDragging ||
            isReturning)
        {
            return;
        }

        targetScale =
            originalScale * hoverMultiplier;

        SetHoverGlow(
            hoverGlowColor,
            hoverGlowAlpha
        );
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (isLocked ||
            isDragging ||
            isReturning)
        {
            return;
        }

        targetScale =
            originalScale;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (isLocked || isReturning)
        {
            return;
        }

        if (canvas == null)
        {
            Debug.LogError(
                "Drag не може да започне, " +
                "защото Canvas липсва.",
                this
            );

            return;
        }

        isDragging = true;

        targetScale =
            originalScale;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );

        canvasGroup.blocksRaycasts =
            false;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (isLocked ||
            isReturning ||
            !isDragging)
        {
            return;
        }

        if (canvas == null)
        {
            return;
        }

        rectTransform.anchoredPosition +=
            eventData.delta /
            canvas.scaleFactor;
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (isLocked ||
            isReturning ||
            !isDragging)
        {
            return;
        }

        isDragging = false;

        canvasGroup.blocksRaycasts =
            true;

        GameObject objectUnderPointer =
            eventData.pointerCurrentRaycast.gameObject;

        HeartPuzzleDropZone dropZone =
            null;

        if (objectUnderPointer != null)
        {
            dropZone =
                objectUnderPointer
                    .GetComponentInParent<
                        HeartPuzzleDropZone
                    >();
        }

        // Пуснат извън зона:
        // няма Correct/Wrong звук
        // и етикетът просто се връща.
        if (dropZone == null)
        {
            ReturnToStartAnimated();
            return;
        }

        // =========================
        // ПРАВИЛЕН ОТГОВОР
        // =========================

        if (dropZone.Accepts(partType))
        {
            dropZone.ShowCorrectFeedback();

            if (GameFeedbackSoundManager.Instance != null)
            {
                GameFeedbackSoundManager
                    .Instance
                    .PlayCorrect();
            }

            SnapToDropZone(
                dropZone,
                true
            );

            StartFeedback(
                GetCorrectFeedbackColor(),
                false,
                dropZone
            );

            return;
        }

        // =========================
        // ГРЕШЕН ОТГОВОР
        // =========================

        dropZone.ShowWrongFeedback();

        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayWrong();
        }

        SnapToDropZone(
            dropZone,
            true
        );

        if (puzzleManager != null)
        {
            puzzleManager
                .HandleWrongPlacement();
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleManager не е свързан. " +
                "Животът не може да бъде отнет.",
                this
            );
        }

        StartFeedback(
            GetWrongFeedbackColor(),
            true,
            null
        );
    }

    private Color GetCorrectFeedbackColor()
    {
        if (IsColorblindThemeActive())
        {
            return colorblindCorrectColor;
        }

        return correctColor;
    }

    private Color GetWrongFeedbackColor()
    {
        if (IsColorblindThemeActive())
        {
            return colorblindWrongColor;
        }

        return wrongColor;
    }

    private bool IsColorblindThemeActive()
    {
        if (ColorThemeManager.Instance != null)
        {
            return ColorThemeManager.Instance.CurrentTheme == 1;
        }

        return PlayerPrefs.GetInt(
            ThemeKey,
            0
        ) == 1;
    }

    private void SnapToDropZone(
        HeartPuzzleDropZone dropZone,
        bool lockLabel)
    {
        if (dropZone == null)
        {
            ReturnToStart();
            return;
        }

        RectTransform dropRect =
            dropZone.GetComponent<
                RectTransform
            >();

        if (dropRect == null)
        {
            Debug.LogError(
                "Drop зоната няма RectTransform.",
                dropZone
            );

            ReturnToStart();
            return;
        }

        rectTransform.SetParent(
            dropRect,
            false
        );

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

        rectTransform.localRotation =
            Quaternion.identity;

        targetScale =
            originalScale;

        isLocked =
            lockLabel;

        canvasGroup.blocksRaycasts =
            false;
    }

    private void StartFeedback(
        Color color,
        bool returnAfter,
        HeartPuzzleDropZone correctZone)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(
                feedbackCoroutine
            );
        }

        feedbackCoroutine =
            StartCoroutine(
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
        if (feedbackOutline != null)
        {
            feedbackOutline.effectColor =
                color;

            feedbackOutline.enabled =
                true;
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackOutline != null)
        {
            feedbackOutline.enabled =
                false;
        }

        feedbackCoroutine =
            null;

        if (returnAfter)
        {
            ReturnToStartAnimated();
            yield break;
        }

        if (puzzleManager != null)
        {
            puzzleManager
                .HandleCorrectPlacement(
                    correctZone
                );
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleManager не е свързан.",
                this
            );
        }
    }

    private void ReturnToStartAnimated()
    {
        if (rectTransform == null ||
            originalParent == null)
        {
            return;
        }

        isReturning = true;
        isLocked = true;
        isDragging = false;

        targetScale =
            originalScale;

        canvasGroup.blocksRaycasts =
            false;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );

        if (returnAnimation == null)
        {
            ReturnToStart();
            return;
        }

        returnAnimation.PlayReturn(
            originalParent,
            originalPosition,
            originalScale,
            FinishAnimatedReturn
        );
    }

    private void FinishAnimatedReturn()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.SetParent(
            originalParent,
            false
        );

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            originalPosition;

        rectTransform.localScale =
            originalScale;

        rectTransform.localRotation =
            Quaternion.identity;

        targetScale =
            originalScale;

        isReturning = false;
        isLocked = false;
        isDragging = false;

        canvasGroup.blocksRaycasts =
            true;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );
    }

    private void ReturnToStart()
    {
        if (rectTransform == null ||
            originalParent == null)
        {
            return;
        }

        if (returnAnimation != null)
        {
            returnAnimation.CancelAnimation();
        }

        rectTransform.SetParent(
            originalParent,
            false
        );

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            originalPosition;

        rectTransform.localScale =
            originalScale;

        rectTransform.localRotation =
            Quaternion.identity;

        targetScale =
            originalScale;

        isLocked = false;
        isDragging = false;
        isReturning = false;

        canvasGroup.blocksRaycasts =
            true;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );
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

            Color finalColor =
                color;

            finalColor.a =
                alpha;

            shadow.effectColor =
                finalColor;
        }
    }

    private void OnDisable()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(
                feedbackCoroutine
            );

            feedbackCoroutine =
                null;
        }

        if (returnAnimation != null)
        {
            returnAnimation.CancelAnimation();
        }

        if (feedbackOutline != null)
        {
            feedbackOutline.enabled =
                false;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                originalScale;
        }

        isDragging = false;
        isLocked = false;
        isReturning = false;

        SetHoverGlow(
            Color.white,
            0f
        );
    }
}