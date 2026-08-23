using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CurrentTaskTextAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float startScale = 0.96f;
    [SerializeField] private float popScale = 1.04f;

    [SerializeField] private float popDuration = 0.14f;
    [SerializeField] private float settleDuration = 0.12f;

    [Header("Fade")]
    [SerializeField] private float startAlpha = 0.55f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        canvasGroup =
            GetComponent<CanvasGroup>();

        originalScale =
            rectTransform.localScale;
    }

    public void Play()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );
        }

        animationCoroutine =
            StartCoroutine(
                AnimationRoutine()
            );
    }

    private IEnumerator AnimationRoutine()
    {
        rectTransform.localScale =
            originalScale * startScale;

        canvasGroup.alpha =
            startAlpha;

        // =============================================
        // POP IN
        // =============================================

        yield return Animate(
            originalScale * startScale,
            originalScale * popScale,
            startAlpha,
            1f,
            popDuration
        );

        // =============================================
        // SETTLE
        // =============================================

        yield return Animate(
            originalScale * popScale,
            originalScale,
            1f,
            1f,
            settleDuration
        );

        rectTransform.localScale =
            originalScale;

        canvasGroup.alpha =
            1f;

        animationCoroutine = null;
    }

    private IEnumerator Animate(
        Vector3 startScaleValue,
        Vector3 endScaleValue,
        float startAlphaValue,
        float endAlphaValue,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScaleValue,
                    endScaleValue,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlphaValue,
                    endAlphaValue,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            endScaleValue;

        canvasGroup.alpha =
            endAlphaValue;
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
        }
    }
}