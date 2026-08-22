using System;
using System.Collections;
using UnityEngine;

public class StomachFoodTransitionAnimator : MonoBehaviour
{
    [Header("Outgoing Food - Shake")]
    [SerializeField] private float shakeDistance = 7f;
    [SerializeField] private float shakeStepDuration = 0.04f;
    [SerializeField] private int shakeCount = 3;

    [Header("Outgoing Food - Exit")]
    [SerializeField] private float exitMoveDown = 35f;
    [SerializeField] private float exitDuration = 0.25f;

    [Header("Incoming Food - Appear")]
    [SerializeField] private float incomingStartScale = 0.88f;
    [SerializeField] private float incomingPopScale = 1.06f;
    [SerializeField] private float fadeInDuration = 0.22f;
    [SerializeField] private float settleDuration = 0.14f;

    private Coroutine transitionCoroutine;

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayTransition(
        GameObject currentFood,
        GameObject nextFood,
        Action onComplete = null)
    {
        if (nextFood == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine =
            StartCoroutine(
                TransitionRoutine(
                    currentFood,
                    nextFood,
                    onComplete
                )
            );
    }

    // =========================================================
    // MAIN TRANSITION
    // =========================================================

    private IEnumerator TransitionRoutine(
        GameObject currentFood,
        GameObject nextFood,
        Action onComplete)
    {
        // ---------------------------------------------
        // CURRENT FOOD
        // ---------------------------------------------

        if (currentFood != null &&
            currentFood.activeInHierarchy)
        {
            RectTransform currentRect =
                currentFood.transform as RectTransform;

            CanvasGroup currentCanvas =
                GetOrCreateCanvasGroup(currentFood);

            if (currentRect != null)
            {
                Vector2 originalPosition =
                    currentRect.anchoredPosition;

                Vector3 originalScale =
                    currentRect.localScale;

                float originalAlpha =
                    currentCanvas.alpha;

                // 1. Кратко разклащане
                for (int i = 0; i < shakeCount; i++)
                {
                    yield return MoveTo(
                        currentRect,
                        originalPosition +
                        Vector2.left * shakeDistance,
                        shakeStepDuration
                    );

                    yield return MoveTo(
                        currentRect,
                        originalPosition +
                        Vector2.right * shakeDistance,
                        shakeStepDuration
                    );
                }

                yield return MoveTo(
                    currentRect,
                    originalPosition,
                    shakeStepDuration
                );

                // 2. Надолу + Fade Out
                Vector2 exitPosition =
                    originalPosition +
                    Vector2.down * exitMoveDown;

                yield return MoveAndFade(
                    currentRect,
                    currentCanvas,
                    exitPosition,
                    0f,
                    exitDuration
                );

                // Връщаме оригиналните стойности,
                // преди да го изключим.
                currentRect.anchoredPosition =
                    originalPosition;

                currentRect.localScale =
                    originalScale;

                currentCanvas.alpha =
                    originalAlpha;
            }

            currentFood.SetActive(false);
        }

        // ---------------------------------------------
        // NEXT FOOD
        // ---------------------------------------------

        nextFood.SetActive(true);

        RectTransform nextRect =
            nextFood.transform as RectTransform;

        CanvasGroup nextCanvas =
            GetOrCreateCanvasGroup(nextFood);

        if (nextRect != null)
        {
            Vector2 normalPosition =
                nextRect.anchoredPosition;

            Vector3 normalScale =
                nextRect.localScale;

            nextCanvas.alpha = 0f;

            nextRect.localScale =
                normalScale *
                incomingStartScale;

            // 3. Fade In + Pop
            yield return ScaleAndFade(
                nextRect,
                nextCanvas,
                normalScale * incomingPopScale,
                1f,
                fadeInDuration
            );

            // 4. Settle обратно към нормалния размер
            yield return ScaleTo(
                nextRect,
                normalScale,
                settleDuration
            );

            // Гарантираме точния финален state.
            nextRect.anchoredPosition =
                normalPosition;

            nextRect.localScale =
                normalScale;

            nextCanvas.alpha = 1f;
        }

        transitionCoroutine = null;

        onComplete?.Invoke();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private CanvasGroup GetOrCreateCanvasGroup(
        GameObject target)
    {
        CanvasGroup canvasGroup =
            target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private IEnumerator MoveTo(
        RectTransform target,
        Vector2 endPosition,
        float duration)
    {
        Vector2 startPosition =
            target.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            target.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            yield return null;
        }

        target.anchoredPosition =
            endPosition;
    }

    private IEnumerator MoveAndFade(
        RectTransform target,
        CanvasGroup canvasGroup,
        Vector2 endPosition,
        float endAlpha,
        float duration)
    {
        Vector2 startPosition =
            target.anchoredPosition;

        float startAlpha =
            canvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            target.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    t
                );

            yield return null;
        }

        target.anchoredPosition =
            endPosition;

        canvasGroup.alpha =
            endAlpha;
    }

    private IEnumerator ScaleAndFade(
        RectTransform target,
        CanvasGroup canvasGroup,
        Vector3 endScale,
        float endAlpha,
        float duration)
    {
        Vector3 startScale =
            target.localScale;

        float startAlpha =
            canvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    t
                );

            yield return null;
        }

        target.localScale =
            endScale;

        canvasGroup.alpha =
            endAlpha;
    }

    private IEnumerator ScaleTo(
        RectTransform target,
        Vector3 endScale,
        float duration)
    {
        Vector3 startScale =
            target.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );

            yield return null;
        }

        target.localScale =
            endScale;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}