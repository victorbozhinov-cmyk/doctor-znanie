using System.Collections;
using UnityEngine;

public class LungsAirEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform airRect;
    [SerializeField] private CanvasGroup airCanvasGroup;

    // =========================================================
    // INHALE SETTINGS
    // =========================================================

    [Header("Inhale Movement")]
    [Tooltip("Колко пиксела по-нагоре започва въздухът.")]
    [SerializeField] private float inhaleStartYOffset = 35f;

    [Tooltip("Колко време се движи въздухът към крайната си позиция.")]
    [SerializeField] private float inhaleMoveDuration = 0.8f;

    [Header("Inhale Visibility")]
    [Range(0f, 1f)]
    [SerializeField] private float inhaleMaxAlpha = 0.5f;

    [Tooltip("Колко време отнема появяването на въздуха.")]
    [SerializeField] private float inhaleFadeInDuration = 0.45f;

    [Header("Inhale Timing")]
    [Tooltip("Забавяне след началото на разширяването на дробовете.")]
    [SerializeField] private float inhaleStartDelay = 0.15f;

    [SerializeField] private float inhaleHoldDuration = 0.2f;

    [SerializeField] private float inhaleFadeOutDuration = 0.4f;

    // =========================================================
    // EXHALE SETTINGS
    // =========================================================

    [Header("Exhale Movement")]
    [Tooltip(
        "Колко пиксела нагоре се премества въздухът при издишване."
    )]
    [SerializeField] private float exhaleEndYOffset = 50f;

    [Tooltip(
        "Колко време се движи въздухът нагоре при издишване."
    )]
    [SerializeField] private float exhaleMoveDuration = 0.9f;

    [Header("Exhale Visibility")]
    [Range(0f, 1f)]
    [SerializeField] private float exhaleMaxAlpha = 0.5f;

    [Tooltip(
        "Колко време отнема появяването на въздуха при издишване."
    )]
    [SerializeField] private float exhaleFadeInDuration = 0.3f;

    [Header("Exhale Timing")]
    [Tooltip(
        "Забавяне след началото на свиването на дробовете."
    )]
    [SerializeField] private float exhaleStartDelay = 0.15f;

    [SerializeField] private float exhaleHoldDuration = 0.15f;

    [SerializeField] private float exhaleFadeOutDuration = 0.65f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private Vector2 normalPosition;

    private Coroutine currentRoutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (airRect == null)
        {
            airRect = GetComponent<RectTransform>();
        }

        if (airCanvasGroup == null)
        {
            airCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (airRect != null)
        {
            normalPosition =
                airRect.anchoredPosition;
        }

        if (airCanvasGroup != null)
        {
            airCanvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }

    // =========================================================
    // INHALE
    // =========================================================

    public void PlayInhale()
    {
        StopCurrentEffect();

        gameObject.SetActive(true);

        currentRoutine =
            StartCoroutine(
                PlayInhaleRoutine()
            );
    }

    private IEnumerator PlayInhaleRoutine()
    {
        if (
            airRect == null ||
            airCanvasGroup == null
        )
        {
            yield break;
        }

        Vector2 startPosition =
            normalPosition +
            new Vector2(
                0f,
                inhaleStartYOffset
            );

        airRect.anchoredPosition =
            startPosition;

        airCanvasGroup.alpha = 0f;

        // -------------------------
        // START DELAY
        // -------------------------

        if (inhaleStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                inhaleStartDelay
            );
        }

        // -------------------------
        // MOVE DOWN + FADE IN
        // -------------------------

        float elapsed = 0f;

        while (elapsed < inhaleMoveDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    inhaleMoveDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            airRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    normalPosition,
                    t
                );

            float fadeT =
                Mathf.Clamp01(
                    elapsed /
                    inhaleFadeInDuration
                );

            fadeT = Mathf.SmoothStep(
                0f,
                1f,
                fadeT
            );

            airCanvasGroup.alpha =
                Mathf.Lerp(
                    0f,
                    inhaleMaxAlpha,
                    fadeT
                );

            yield return null;
        }

        airRect.anchoredPosition =
            normalPosition;

        airCanvasGroup.alpha =
            inhaleMaxAlpha;

        // -------------------------
        // HOLD
        // -------------------------

        if (inhaleHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                inhaleHoldDuration
            );
        }

        // -------------------------
        // FADE OUT
        // -------------------------

        elapsed = 0f;

        while (elapsed < inhaleFadeOutDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    inhaleFadeOutDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            airCanvasGroup.alpha =
                Mathf.Lerp(
                    inhaleMaxAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        FinishEffect();
    }

    // =========================================================
    // EXHALE
    // =========================================================

    public void PlayExhale()
    {
        StopCurrentEffect();

        gameObject.SetActive(true);

        currentRoutine =
            StartCoroutine(
                PlayExhaleRoutine()
            );
    }

    private IEnumerator PlayExhaleRoutine()
    {
        if (
            airRect == null ||
            airCanvasGroup == null
        )
        {
            yield break;
        }

        Vector2 endPosition =
            normalPosition +
            new Vector2(
                0f,
                exhaleEndYOffset
            );

        /*
         * При издишване въздухът започва
         * вътре в белите дробове.
         */
        airRect.anchoredPosition =
            normalPosition;

        airCanvasGroup.alpha = 0f;

        // -------------------------
        // START DELAY
        // -------------------------

        if (exhaleStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                exhaleStartDelay
            );
        }

        // -------------------------
        // FADE IN
        // -------------------------

        float elapsed = 0f;

        while (elapsed < exhaleFadeInDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    exhaleFadeInDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            airCanvasGroup.alpha =
                Mathf.Lerp(
                    0f,
                    exhaleMaxAlpha,
                    t
                );

            yield return null;
        }

        airCanvasGroup.alpha =
            exhaleMaxAlpha;

        // -------------------------
        // SHORT HOLD
        // -------------------------

        if (exhaleHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                exhaleHoldDuration
            );
        }

        // -------------------------
        // MOVE UP + FADE OUT
        // -------------------------

        elapsed = 0f;

        while (elapsed < exhaleMoveDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    exhaleMoveDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            airRect.anchoredPosition =
                Vector2.Lerp(
                    normalPosition,
                    endPosition,
                    t
                );

            /*
             * Fade-out може да приключи малко
             * преди или след самото движение,
             * според зададените стойности.
             */
            float fadeT =
                Mathf.Clamp01(
                    elapsed /
                    exhaleFadeOutDuration
                );

            fadeT = Mathf.SmoothStep(
                0f,
                1f,
                fadeT
            );

            airCanvasGroup.alpha =
                Mathf.Lerp(
                    exhaleMaxAlpha,
                    0f,
                    fadeT
                );

            yield return null;
        }

        FinishEffect();
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void FinishEffect()
    {
        if (airCanvasGroup != null)
        {
            airCanvasGroup.alpha = 0f;
        }

        if (airRect != null)
        {
            airRect.anchoredPosition =
                normalPosition;
        }

        gameObject.SetActive(false);

        currentRoutine = null;
    }

    // =========================================================
    // STOP
    // =========================================================

    private void StopCurrentEffect()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(
                currentRoutine
            );

            currentRoutine = null;
        }

        if (airCanvasGroup != null)
        {
            airCanvasGroup.alpha = 0f;
        }

        if (airRect != null)
        {
            airRect.anchoredPosition =
                normalPosition;
        }

        gameObject.SetActive(false);
    }
}