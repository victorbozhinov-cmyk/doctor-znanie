using System.Collections;
using UnityEngine;

public class UIButtonShowcaseAnimation : MonoBehaviour
{
    [Header("Showcase")]
    [SerializeField] private float liftDistance = 10f;
    [SerializeField] private float sideDistance = 5f;
    [SerializeField] private float tiltAngle = 2f;

    [Header("Timing")]
    [SerializeField] private float moveDuration = 0.10f;
    [SerializeField] private float returnDuration = 0.12f;
    [SerializeField] private float pauseBetweenPulses = 0.06f;
    [SerializeField] private int pulseCount = 2;

    private RectTransform rectTransform;

    private Vector2 originalPosition;
    private Quaternion originalRotation;

    private Coroutine showcaseCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        SaveOriginalTransform();
    }

    private void OnDisable()
    {
        StopShowcaseImmediate();
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayShowcase()
    {
        if (rectTransform == null)
            return;

        if (showcaseCoroutine != null)
        {
            StopCoroutine(
                showcaseCoroutine
            );
        }

        ResetTransform();

        showcaseCoroutine =
            StartCoroutine(
                ShowcaseRoutine()
            );
    }

    public void StopShowcaseImmediate()
    {
        if (showcaseCoroutine != null)
        {
            StopCoroutine(
                showcaseCoroutine
            );

            showcaseCoroutine = null;
        }

        ResetTransform();
    }

    // =========================================================
    // SHOWCASE
    // =========================================================

    private IEnumerator ShowcaseRoutine()
    {
        for (int i = 0;
             i < pulseCount;
             i++)
        {
            // ---------------------------------------------
            // UP + SLIGHT RIGHT
            // ---------------------------------------------

            Vector2 firstPosition =
                originalPosition +
                Vector2.up *
                liftDistance +
                Vector2.right *
                sideDistance;

            Quaternion firstRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    -tiltAngle
                );

            yield return AnimateTo(
                firstPosition,
                firstRotation,
                moveDuration
            );

            // ---------------------------------------------
            // UP + SLIGHT LEFT
            // ---------------------------------------------

            Vector2 secondPosition =
                originalPosition +
                Vector2.up *
                liftDistance +
                Vector2.left *
                sideDistance;

            Quaternion secondRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    tiltAngle
                );

            yield return AnimateTo(
                secondPosition,
                secondRotation,
                moveDuration
            );

            // ---------------------------------------------
            // RETURN
            // ---------------------------------------------

            yield return AnimateTo(
                originalPosition,
                originalRotation,
                returnDuration
            );

            if (i <
                pulseCount - 1)
            {
                yield return new WaitForSecondsRealtime(
                    pauseBetweenPulses
                );
            }
        }

        ResetTransform();

        showcaseCoroutine = null;
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator AnimateTo(
        Vector2 targetPosition,
        Quaternion targetRotation,
        float duration)
    {
        Vector2 startPosition =
            rectTransform.anchoredPosition;

        Quaternion startRotation =
            rectTransform.localRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            // SmoothStep
            t =
                t * t *
                (3f - 2f * t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            rectTransform.localRotation =
                Quaternion.Lerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        rectTransform.anchoredPosition =
            targetPosition;

        rectTransform.localRotation =
            targetRotation;
    }

    // =========================================================
    // ORIGINAL TRANSFORM
    // =========================================================

    private void SaveOriginalTransform()
    {
        if (rectTransform == null)
            return;

        originalPosition =
            rectTransform.anchoredPosition;

        originalRotation =
            rectTransform.localRotation;
    }

    private void ResetTransform()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition =
            originalPosition;

        rectTransform.localRotation =
            originalRotation;
    }
}