using System.Collections;
using UnityEngine;

public class LungsProgressFeedback : MonoBehaviour
{
    [Header("Correct Pop")]
    [SerializeField] private float popScaleMultiplier = 1.06f;
    [SerializeField] private float popUpDuration = 0.07f;
    [SerializeField] private float popReturnDuration = 0.10f;

    [Header("Wrong Shake")]
    [SerializeField] private float shakeAmount = 8f;
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeSpeed = 45f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector2 originalPosition;

    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            originalScale =
                rectTransform.localScale;

            originalPosition =
                rectTransform.anchoredPosition;
        }
    }

    // =========================================================
    // CORRECT
    // =========================================================

    public void PlayCorrect()
    {
        if (rectTransform == null)
        {
            return;
        }

        StopCurrentFeedback();

        feedbackCoroutine =
            StartCoroutine(
                CorrectRoutine()
            );
    }

    // =========================================================
    // WRONG
    // =========================================================

    public void PlayWrong()
    {
        if (rectTransform == null)
        {
            return;
        }

        StopCurrentFeedback();

        feedbackCoroutine =
            StartCoroutine(
                WrongRoutine()
            );
    }

    // =========================================================
    // CORRECT ROUTINE
    // =========================================================

    private IEnumerator CorrectRoutine()
    {
        Vector3 targetScale =
            originalScale *
            popScaleMultiplier;

        yield return ScaleTo(
            targetScale,
            popUpDuration
        );

        yield return ScaleTo(
            originalScale,
            popReturnDuration
        );

        feedbackCoroutine = null;
    }

    // =========================================================
    // WRONG ROUTINE
    // =========================================================

    private IEnumerator WrongRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float fade =
                1f -
                Mathf.Clamp01(
                    elapsed /
                    shakeDuration
                );

            float offset =
                Mathf.Sin(
                    elapsed *
                    shakeSpeed
                ) *
                shakeAmount *
                fade;

            rectTransform.anchoredPosition =
                originalPosition +
                new Vector2(
                    offset,
                    0f
                );

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalPosition;

        feedbackCoroutine = null;
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration
    )
    {
        Vector3 startScale =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;
    }

    // =========================================================
    // RESET / STOP
    // =========================================================

    private void StopCurrentFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(
                feedbackCoroutine
            );

            feedbackCoroutine = null;
        }

        rectTransform.localScale =
            originalScale;

        rectTransform.anchoredPosition =
            originalPosition;
    }
}
