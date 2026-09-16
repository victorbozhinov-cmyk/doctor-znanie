using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UILifeHeartAnimation : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField]
    private float pulseDuration = 0.12f;

    [SerializeField]
    private float pulseScale = 1.2f;

    [Header("Shake")]
    [SerializeField]
    private float shakeDuration = 0.22f;

    [SerializeField]
    private float shakeStrength = 10f;

    [Header("Disappear")]
    [SerializeField]
    private float disappearDuration = 0.2f;

    [SerializeField]
    private float disappearScale = 0.4f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalScale;
    private Vector2 animationStartPosition;

    private Coroutine animationCoroutine;

    private bool originalScaleSaved;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        EnsureReferences();
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private void EnsureReferences()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (
            rectTransform != null &&
            !originalScaleSaved
        )
        {
            originalScale =
                rectTransform.localScale;

            originalScaleSaved = true;
        }
    }

    // =========================================================
    // LOSE LIFE
    // =========================================================

    public void PlayLoseAnimation()
    {
        PlayLoseAnimation(null);
    }

    public void PlayLoseAnimation(
        Action onFinished)
    {
        EnsureReferences();

        if (
            rectTransform == null ||
            canvasGroup == null
        )
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "UILifeHeartAnimation няма нужните UI компоненти."
            );

            onFinished?.Invoke();
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );
        }

        // Принуждаваме Layout Group
        // да приключи подреждането.
        Canvas.ForceUpdateCanvases();

        animationStartPosition =
            rectTransform.anchoredPosition;

        originalScale =
            rectTransform.localScale;

        originalScaleSaved =
            true;

        animationCoroutine =
            StartCoroutine(
                LoseLifeRoutine(
                    onFinished
                )
            );
    }

    // =========================================================
    // RESET HEART
    // =========================================================

    public void ResetHeart()
    {
        // Много важно:
        // методът може да бъде извикан и върху сърце,
        // което преди това е било inactive.
        EnsureReferences();

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        gameObject.SetActive(true);

        // След активиране проверяваме
        // референциите още веднъж.
        EnsureReferences();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                originalScale;
        }

        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // LOSE ROUTINE
    // =========================================================

    private IEnumerator LoseLifeRoutine(
        Action onFinished)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // =========================
        // Pulse
        // =========================

        yield return AnimateScale(
            originalScale,
            originalScale * pulseScale,
            pulseDuration
        );

        yield return AnimateScale(
            originalScale * pulseScale,
            originalScale,
            pulseDuration
        );

        // =========================
        // Shake
        // =========================

        float shakeTimer = 0f;

        while (shakeTimer < shakeDuration)
        {
            shakeTimer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    shakeTimer /
                    shakeDuration
                );

            float currentStrength =
                Mathf.Lerp(
                    shakeStrength,
                    0f,
                    progress
                );

            Vector2 shakeOffset =
                UnityEngine.Random
                    .insideUnitCircle *
                currentStrength;

            rectTransform.anchoredPosition =
                animationStartPosition +
                shakeOffset;

            yield return null;
        }

        rectTransform.anchoredPosition =
            animationStartPosition;

        // =========================
        // Disappear
        // =========================

        float timer = 0f;

        float startAlpha =
            canvasGroup.alpha;

        Vector3 startScale =
            rectTransform.localScale;

        while (timer < disappearDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    disappearDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    eased
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    originalScale *
                    disappearScale,
                    eased
                );

            yield return null;
        }

        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            originalScale *
            disappearScale;

        rectTransform.anchoredPosition =
            animationStartPosition;

        animationCoroutine = null;

        // Първо callback,
        // после скриваме сърцето.
        onFinished?.Invoke();

        gameObject.SetActive(false);

        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator AnimateScale(
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float timer = 0f;

        if (duration <= 0f)
        {
            rectTransform.localScale =
                to;

            yield break;
        }

        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    from,
                    to,
                    eased
                );

            yield return null;
        }

        rectTransform.localScale =
            to;
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                originalScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}