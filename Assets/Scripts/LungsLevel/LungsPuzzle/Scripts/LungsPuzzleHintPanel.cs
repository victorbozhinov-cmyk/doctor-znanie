using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LungsPuzzleHintPanel : MonoBehaviour
{
    [Header("Hint")]
    [SerializeField] private Image hintContentImage;
    [SerializeField] private Sprite[] hintSprites;

    [Header("Hint Animation")]
    [SerializeField] private float fadeOutDuration = 0.08f;
    [SerializeField] private float popDuration = 0.16f;
    [SerializeField] private float settleDuration = 0.10f;

    [SerializeField] private float startScale = 0.82f;
    [SerializeField] private float popScale = 1.08f;

    [Header("Attention Icon")]
    [SerializeField] private RectTransform exclamationTransform;
    [SerializeField] private CanvasGroup exclamationCanvasGroup;

    [SerializeField] private float exclamationPulseDuration = 0.65f;
    [SerializeField] private float exclamationMinAlpha = 0.35f;
    [SerializeField] private float exclamationMinScale = 0.92f;
    [SerializeField] private float exclamationMaxScale = 1.08f;

    private RectTransform hintRectTransform;
    private CanvasGroup hintCanvasGroup;

    private int currentHintIndex;

    private Coroutine hintAnimationCoroutine;
    private Coroutine exclamationCoroutine;

    private bool hintAnimating;

    private void Awake()
    {
        if (hintContentImage != null)
        {
            hintRectTransform =
                hintContentImage.rectTransform;

            hintCanvasGroup =
                hintContentImage.GetComponent<CanvasGroup>();

            if (hintCanvasGroup == null)
            {
                hintCanvasGroup =
                    hintContentImage.gameObject
                        .AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        currentHintIndex = 0;

        if (hintSprites != null &&
            hintSprites.Length > 0 &&
            hintContentImage != null)
        {
            hintContentImage.sprite =
                hintSprites[currentHintIndex];
        }

        ResetHintVisual();

        if (exclamationTransform != null &&
            exclamationCanvasGroup != null)
        {
            exclamationCoroutine =
                StartCoroutine(
                    ExclamationPulseRoutine()
                );
        }
    }

    // =====================================================
    // NEXT HINT
    // =====================================================

    public void NextHint()
    {
        if (hintAnimating)
            return;

        if (hintSprites == null ||
            hintSprites.Length == 0)
        {
            return;
        }

        hintAnimationCoroutine =
            StartCoroutine(
                ChangeHintRoutine()
            );
    }

    // =====================================================
    // HINT CHANGE ANIMATION
    // =====================================================

    private IEnumerator ChangeHintRoutine()
    {
        hintAnimating = true;

        // -------------------------
        // FADE OUT
        // -------------------------

        float timer = 0f;

        Vector3 originalScale =
            Vector3.one;

        Vector3 fadeTargetScale =
            Vector3.one * 0.94f;

        while (timer < fadeOutDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    fadeOutDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            hintCanvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    eased
                );

            hintRectTransform.localScale =
                Vector3.Lerp(
                    originalScale,
                    fadeTargetScale,
                    eased
                );

            yield return null;
        }

        // -------------------------
        // CHANGE SPRITE
        // -------------------------

        currentHintIndex++;

        if (currentHintIndex >=
            hintSprites.Length)
        {
            currentHintIndex = 0;
        }

        hintContentImage.sprite =
            hintSprites[currentHintIndex];

        hintCanvasGroup.alpha = 0f;

        hintRectTransform.localScale =
            Vector3.one * startScale;

        // -------------------------
        // POP IN
        // -------------------------

        timer = 0f;

        Vector3 popStart =
            Vector3.one * startScale;

        Vector3 popTarget =
            Vector3.one * popScale;

        while (timer < popDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    popDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            hintCanvasGroup.alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    eased
                );

            hintRectTransform.localScale =
                Vector3.Lerp(
                    popStart,
                    popTarget,
                    eased
                );

            yield return null;
        }

        // -------------------------
        // SETTLE
        // -------------------------

        timer = 0f;

        while (timer < settleDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    settleDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            hintRectTransform.localScale =
                Vector3.Lerp(
                    popTarget,
                    Vector3.one,
                    eased
                );

            yield return null;
        }

        hintCanvasGroup.alpha = 1f;

        hintRectTransform.localScale =
            Vector3.one;

        hintAnimating = false;

        hintAnimationCoroutine = null;
    }

    // =====================================================
    // EXCLAMATION PULSE
    // =====================================================

    private IEnumerator ExclamationPulseRoutine()
    {
        Vector3 originalScale =
            exclamationTransform.localScale;

        while (true)
        {
            // Fade / scale up
            float timer = 0f;

            while (timer <
                   exclamationPulseDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer /
                        exclamationPulseDuration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        progress
                    );

                exclamationCanvasGroup.alpha =
                    Mathf.Lerp(
                        exclamationMinAlpha,
                        1f,
                        eased
                    );

                float scale =
                    Mathf.Lerp(
                        exclamationMinScale,
                        exclamationMaxScale,
                        eased
                    );

                exclamationTransform.localScale =
                    originalScale * scale;

                yield return null;
            }

            // Fade / scale down
            timer = 0f;

            while (timer <
                   exclamationPulseDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer /
                        exclamationPulseDuration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        progress
                    );

                exclamationCanvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        exclamationMinAlpha,
                        eased
                    );

                float scale =
                    Mathf.Lerp(
                        exclamationMaxScale,
                        exclamationMinScale,
                        eased
                    );

                exclamationTransform.localScale =
                    originalScale * scale;

                yield return null;
            }
        }
    }

    // =====================================================
    // RESET
    // =====================================================

    private void ResetHintVisual()
    {
        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 1f;
        }

        if (hintRectTransform != null)
        {
            hintRectTransform.localScale =
                Vector3.one;
        }
    }

    private void OnDisable()
    {
        if (hintAnimationCoroutine != null)
        {
            StopCoroutine(
                hintAnimationCoroutine
            );

            hintAnimationCoroutine = null;
        }

        if (exclamationCoroutine != null)
        {
            StopCoroutine(
                exclamationCoroutine
            );

            exclamationCoroutine = null;
        }

        hintAnimating = false;

        ResetHintVisual();

        if (exclamationCanvasGroup != null)
        {
            exclamationCanvasGroup.alpha = 1f;
        }

        if (exclamationTransform != null)
        {
            exclamationTransform.localScale =
                Vector3.one;
        }
    }
}