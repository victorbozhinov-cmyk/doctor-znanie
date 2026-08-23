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
    // NORMAL FOOD TRANSITION
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
            StopCoroutine(
                transitionCoroutine
            );
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
    // FINAL FOOD EXIT
    // =========================================================

    public void PlayExitOnly(
        GameObject currentFood,
        Action onComplete = null)
    {
        if (currentFood == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(
                transitionCoroutine
            );
        }

        transitionCoroutine =
            StartCoroutine(
                ExitOnlyRoutine(
                    currentFood,
                    onComplete
                )
            );
    }

    // =========================================================
    // NORMAL TRANSITION ROUTINE
    // =========================================================

    private IEnumerator TransitionRoutine(
        GameObject currentFood,
        GameObject nextFood,
        Action onComplete)
    {
        // ---------------------------------------------
        // CURRENT FOOD OUT
        // ---------------------------------------------

        if (currentFood != null &&
            currentFood.activeInHierarchy)
        {
            yield return AnimateFoodOut(
                currentFood
            );

            currentFood.SetActive(false);
        }

        // ---------------------------------------------
        // NEXT FOOD IN
        // ---------------------------------------------

        nextFood.SetActive(true);

        RectTransform nextRect =
            nextFood.transform as RectTransform;

        CanvasGroup nextCanvas =
            GetOrCreateCanvasGroup(
                nextFood
            );

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

            // Fade In + Pop
            yield return ScaleAndFade(
                nextRect,
                nextCanvas,
                normalScale *
                incomingPopScale,
                1f,
                fadeInDuration
            );

            // Settle
            yield return ScaleTo(
                nextRect,
                normalScale,
                settleDuration
            );

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
    // FINAL EXIT ROUTINE
    // =========================================================

    private IEnumerator ExitOnlyRoutine(
        GameObject currentFood,
        Action onComplete)
    {
        if (currentFood.activeInHierarchy)
        {
            // Използваме абсолютно същата
            // outgoing анимация като при
            // нормалните смени на храната.
            yield return AnimateFoodOut(
                currentFood
            );

            currentFood.SetActive(false);
        }

        transitionCoroutine = null;

        // Много важно:
        // callback-ът идва ЧАК след като
        // shake + move down + fade са приключили.
        onComplete?.Invoke();
    }

    // =========================================================
    // SHARED OUTGOING FOOD ANIMATION
    // =========================================================

    private IEnumerator AnimateFoodOut(
        GameObject food)
    {
        RectTransform foodRect =
            food.transform as RectTransform;

        CanvasGroup foodCanvas =
            GetOrCreateCanvasGroup(
                food
            );

        if (foodRect == null)
            yield break;

        Vector2 originalPosition =
            foodRect.anchoredPosition;

        Vector3 originalScale =
            foodRect.localScale;

        float originalAlpha =
            foodCanvas.alpha;

        // ---------------------------------------------
        // 1. SHAKE
        // ---------------------------------------------

        for (int i = 0;
             i < shakeCount;
             i++)
        {
            yield return MoveTo(
                foodRect,
                originalPosition +
                Vector2.left *
                shakeDistance,
                shakeStepDuration
            );

            yield return MoveTo(
                foodRect,
                originalPosition +
                Vector2.right *
                shakeDistance,
                shakeStepDuration
            );
        }

        yield return MoveTo(
            foodRect,
            originalPosition,
            shakeStepDuration
        );

        // ---------------------------------------------
        // 2. MOVE DOWN + FADE OUT
        // ---------------------------------------------

        Vector2 exitPosition =
            originalPosition +
            Vector2.down *
            exitMoveDown;

        yield return MoveAndFade(
            foodRect,
            foodCanvas,
            exitPosition,
            0f,
            exitDuration
        );

        // ---------------------------------------------
        // RESET VALUES
        // ---------------------------------------------
        //
        // Връщаме ги преди SetActive(false),
        // така че при евентуален restart
        // изображението да е нормално.

        foodRect.anchoredPosition =
            originalPosition;

        foodRect.localScale =
            originalScale;

        foodCanvas.alpha =
            originalAlpha;
    }

    // =========================================================
    // CANVAS GROUP
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

    // =========================================================
    // MOVE
    // =========================================================

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
            elapsed +=
                Time.deltaTime;

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

    // =========================================================
    // MOVE + FADE
    // =========================================================

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
            elapsed +=
                Time.deltaTime;

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

    // =========================================================
    // SCALE + FADE
    // =========================================================

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
            elapsed +=
                Time.deltaTime;

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

    // =========================================================
    // SCALE
    // =========================================================

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
            elapsed +=
                Time.deltaTime;

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

    // =========================================================
    // SMOOTH
    // =========================================================

    private float SmoothStep(
        float t)
    {
        return
            t * t *
            (3f - 2f * t);
    }
}