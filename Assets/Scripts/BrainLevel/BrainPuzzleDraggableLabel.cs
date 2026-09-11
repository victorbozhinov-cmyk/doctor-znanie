using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrainPuzzleDraggableLabel : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Puzzle References")]
    [SerializeField] private BrainPuzzleManager puzzleManager;

    [Header("Hover")]
    [SerializeField] private float hoverMultiplier = 1.04f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("Hover Glow")]
    [SerializeField] private Color hoverGlowColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float hoverGlowAlpha = 0.25f;

    [Header("Placement Feedback")]
    [SerializeField] private Color correctColor =
        new Color(0.15f, 1f, 0.3f, 1f);

    [SerializeField] private Color wrongColor =
        new Color(1f, 0.22f, 0.28f, 1f);

    [SerializeField] private float feedbackDuration = 0.25f;

    [SerializeField] private Vector2 feedbackOutlineDistance =
        new Vector2(6f, -6f);

    [Header("Correct Placement Animation")]
    [SerializeField] private float correctPopScale = 1.08f;
    [SerializeField] private float correctPopDuration = 0.12f;
    [SerializeField] private float correctHoldDuration = 0.12f;
    [SerializeField] private float fadeOutDuration = 0.18f;
    [SerializeField] private float placedLabelDelay = 0.12f;
    [SerializeField] private float nextLabelFadeDuration = 0.22f;

    [Header("Glow Shape")]
    [SerializeField] private float glowSize = 2f;

    [Header("Return Animation")]
    [SerializeField] private UIDragReturnAnimation returnAnimation;

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

    private const string ThemeKey = "ColorTheme";

    private readonly List<Shadow> glowShadows =
        new List<Shadow>();

    private BrainPartType CurrentPart
    {
        get
        {
            if (puzzleManager == null)
            {
                return BrainPartType.Forebrain;
            }

            return puzzleManager.CurrentPart;
        }
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
                FindFirstObjectByType<BrainPuzzleManager>();
        }

        if (puzzleManager == null)
        {
            Debug.LogError(
                "BrainPuzzleManager не е свързан.",
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

        canvasGroup.alpha = 1f;

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

        BrainPuzzleDropZone dropZone =
            null;

        if (objectUnderPointer != null)
        {
            dropZone =
                objectUnderPointer
                    .GetComponentInParent<
                        BrainPuzzleDropZone
                    >();
        }

        // Извън зона.
        if (dropZone == null)
        {
            ReturnToStartAnimated();
            return;
        }

        // =====================================================
        // ВЕЧЕ РЕШЕНА / ЗАКЛЮЧЕНА ЗОНА
        // =====================================================
        // Не се брои за грешка.
        // Няма Wrong feedback.
        // Не се губи живот.
        // Етикетът просто се връща обратно.
        // =====================================================

        if (dropZone.IsSolved)
        {
            ReturnToStartAnimated();
            return;
        }

        // =====================================================
        // ПРАВИЛНА ЗОНА
        // =====================================================

        if (dropZone.Accepts(CurrentPart))
        {
            // Заключваме зоната веднага,
            // още преди placement анимацията.
            dropZone.MarkSolved();

            /*
             * Correct звукът се пуска
             * ВЕДНАГА при правилния drop,
             * преди всички placement анимации.
             */
            if (puzzleManager != null)
            {
                puzzleManager
                    .PlayCorrectDropSound();
            }

            dropZone.ShowCorrectFeedback();

            SnapToDropZone(
                dropZone
            );

            feedbackCoroutine =
                StartCoroutine(
                    CorrectPlacementRoutine(
                        dropZone
                    )
                );

            return;
        }

        // =====================================================
        // ГРЕШНА ЗОНА
        // =====================================================

        dropZone.ShowWrongFeedback();

        SnapToDropZone(
            dropZone
        );

        if (puzzleManager != null)
        {
            puzzleManager
                .HandleWrongPlacement();
        }

        feedbackCoroutine =
            StartCoroutine(
                WrongPlacementRoutine()
            );
    }

    private void SnapToDropZone(
        BrainPuzzleDropZone dropZone)
    {
        RectTransform dropRect =
            dropZone.GetComponent<RectTransform>();

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

        rectTransform.localRotation =
            Quaternion.identity;

        rectTransform.localScale =
            originalScale;

        targetScale =
            originalScale;

        isLocked = true;

        canvasGroup.blocksRaycasts =
            false;
    }

    private IEnumerator CorrectPlacementRoutine(
        BrainPuzzleDropZone dropZone)
    {
        if (feedbackOutline != null)
        {
            if (IsColorblindThemeActive())
            {
                feedbackOutline.enabled =
                    false;
            }
            else
            {
                feedbackOutline.effectColor =
                    correctColor;

                feedbackOutline.enabled =
                    true;
            }
        }

        yield return ScaleRoutine(
            originalScale,
            originalScale * correctPopScale,
            correctPopDuration
        );

        yield return ScaleRoutine(
            originalScale * correctPopScale,
            originalScale,
            correctPopDuration
        );

        yield return new WaitForSeconds(
            correctHoldDuration
        );

        yield return FadeRoutine(
            1f,
            0f,
            fadeOutDuration
        );

        if (feedbackOutline != null)
        {
            feedbackOutline.enabled =
                false;
        }

        dropZone.ShowPlacedLabel();

        yield return new WaitForSeconds(
            placedLabelDelay
        );

        ReturnToStartInstantInvisible();

        if (puzzleManager != null)
        {
            puzzleManager
                .HandleCorrectPlacement(
                    dropZone
                );
        }

        if (puzzleManager != null &&
            puzzleManager.IsPuzzleCompleted)
        {
            canvasGroup.alpha = 0f;

            puzzleManager.HideCurrentLabel();

            feedbackCoroutine = null;
            yield break;
        }

        yield return FadeRoutine(
            0f,
            1f,
            nextLabelFadeDuration
        );

        isLocked = false;
        isReturning = false;
        isDragging = false;

        canvasGroup.blocksRaycasts =
            true;

        feedbackCoroutine = null;
    }

    private IEnumerator WrongPlacementRoutine()
    {
        if (feedbackOutline != null)
        {
            if (IsColorblindThemeActive())
            {
                feedbackOutline.enabled =
                    false;
            }
            else
            {
                feedbackOutline.effectColor =
                    wrongColor;

                feedbackOutline.enabled =
                    true;
            }
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackOutline != null)
        {
            feedbackOutline.enabled =
                false;
        }

        feedbackCoroutine = null;

        ReturnToStartAnimated();
    }

    private IEnumerator ScaleRoutine(
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    from,
                    to,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            to;
    }

    private IEnumerator FadeRoutine(
        float from,
        float to,
        float duration)
    {
        float timer = 0f;

        canvasGroup.alpha =
            from;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            yield return null;
        }

        canvasGroup.alpha =
            to;
    }

    private void ReturnToStartInstantInvisible()
    {
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

        isReturning =
            false;

        isDragging =
            false;

        isLocked =
            true;

        canvasGroup.blocksRaycasts =
            false;
    }

    private void ReturnToStartAnimated()
    {
        isReturning =
            true;

        isLocked =
            true;

        isDragging =
            false;

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
            FinishAnimatedReturn();
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

        canvasGroup.alpha =
            1f;

        isReturning =
            false;

        isLocked =
            false;

        isDragging =
            false;

        canvasGroup.blocksRaycasts =
            true;

        SetHoverGlow(
            hoverGlowColor,
            0f
        );
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
            returnAnimation
                .CancelAnimation();
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

        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                1f;
        }

        isDragging =
            false;

        isLocked =
            false;

        isReturning =
            false;

        SetHoverGlow(
            Color.white,
            0f
        );
    }
}