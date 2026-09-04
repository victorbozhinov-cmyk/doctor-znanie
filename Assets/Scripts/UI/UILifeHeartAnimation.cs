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

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        originalScale =
            rectTransform.localScale;
    }

    public void PlayLoseAnimation()
    {
        PlayLoseAnimation(null);
    }

    public void PlayLoseAnimation(
        Action onFinished)
    {
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

        animationCoroutine =
            StartCoroutine(
                LoseLifeRoutine(
                    onFinished
                )
            );
    }

    public void ResetHeart()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        rectTransform.localScale =
            originalScale;

        Canvas.ForceUpdateCanvases();
    }

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

        // ВАЖНО:
        // Първо казваме на HeartPuzzleLives,
        // че цялата анимация е приключила.
        //
        // Ако това е последният живот,
        // там ще се покаже Game Over.
        onFinished?.Invoke();

        // Чак след callback-а
        // скриваме самото сърце.
        gameObject.SetActive(false);

        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator AnimateScale(
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float timer = 0f;

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

        rectTransform.localScale = to;
    }

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