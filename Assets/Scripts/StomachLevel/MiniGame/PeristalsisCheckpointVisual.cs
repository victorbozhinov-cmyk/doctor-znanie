using System;
using System.Collections;
using UnityEngine;

public class PeristalsisCheckpointVisual : MonoBehaviour
{
    [Header("Difficulty Size")]
    [SerializeField] private float easySizeMultiplier = 1.00f;
    [SerializeField] private float mediumSizeMultiplier = 0.85f;
    [SerializeField] private float hardSizeMultiplier = 0.85f;

    [Header("Medium Blink")]
    [SerializeField] private float mediumMinAlpha = 0.15f;
    [SerializeField] private float mediumFadeDuration = 0.45f;
    [SerializeField] private float mediumVisibleHold = 0.25f;
    [SerializeField] private float mediumFadedHold = 0.10f;

    [Header("Hard Blink")]
    [SerializeField] private float hardMinAlpha = 0f;
    [SerializeField] private float hardFadeDuration = 0.45f;
    [SerializeField] private float hardVisibleHold = 0.25f;
    [SerializeField] private float hardFadedHold = 0.10f;

    [Header("Pop Animation")]
    [SerializeField] private float startScale = 0.70f;
    [SerializeField] private float popScale = 1.10f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.10f;

    [Header("Pulse Animation")]
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseDuration = 0.55f;

    [Header("Success Animation")]
    [SerializeField] private float successScale = 1.18f;
    [SerializeField] private float successDuration = 0.18f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalScale;
    private Vector3 difficultyScale;

    private int difficulty;

    private Coroutine animationCoroutine;
    private Coroutine blinkCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        InitializeReferences();
    }

    private void OnEnable()
    {
        InitializeReferences();

        difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        CalculateDifficultyScale();

        rectTransform.localScale =
            difficultyScale;

        StopAllVisualCoroutines();

        // Начална прозрачност според трудността.
        switch (difficulty)
        {
            // EASY
            case 0:
                canvasGroup.alpha = 1f;
                break;

            // HARD
            case 2:
                canvasGroup.alpha = hardMinAlpha;
                break;

            // MEDIUM
            default:
                canvasGroup.alpha = mediumMinAlpha;
                break;
        }

        animationCoroutine =
            StartCoroutine(
                PlayAppearAndPulse()
            );

        // Medium и Hard премигват.
        if (difficulty == 1)
        {
            blinkCoroutine =
                StartCoroutine(
                    PlayBlink(
                        mediumMinAlpha,
                        mediumFadeDuration,
                        mediumVisibleHold,
                        mediumFadedHold
                    )
                );
        }
        else if (difficulty == 2)
        {
            blinkCoroutine =
                StartCoroutine(
                    PlayBlink(
                        hardMinAlpha,
                        hardFadeDuration,
                        hardVisibleHold,
                        hardFadedHold
                    )
                );
        }
    }

    private void OnDisable()
    {
        StopAllVisualCoroutines();

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

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void InitializeReferences()
    {
        if (rectTransform == null)
        {
            rectTransform =
                transform as RectTransform;

            if (rectTransform != null)
            {
                originalScale =
                    rectTransform.localScale;
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    // =========================================================
    // DIFFICULTY SIZE
    // =========================================================

    private void CalculateDifficultyScale()
    {
        float multiplier;

        switch (difficulty)
        {
            // EASY
            case 0:
                multiplier =
                    easySizeMultiplier;
                break;

            // HARD
            case 2:
                multiplier =
                    hardSizeMultiplier;
                break;

            // MEDIUM
            default:
                multiplier =
                    mediumSizeMultiplier;
                break;
        }

        difficultyScale =
            originalScale * multiplier;
    }

    // =========================================================
    // APPEAR + PULSE
    // =========================================================

    private IEnumerator PlayAppearAndPulse()
    {
        rectTransform.localScale =
            difficultyScale * startScale;

        yield return ScaleTo(
            difficultyScale * popScale,
            popDuration
        );

        yield return ScaleTo(
            difficultyScale,
            settleDuration
        );

        while (true)
        {
            yield return ScaleTo(
                difficultyScale * pulseScale,
                pulseDuration
            );

            yield return ScaleTo(
                difficultyScale,
                pulseDuration
            );
        }
    }

    // =========================================================
    // MEDIUM / HARD BLINK
    // =========================================================

    private IEnumerator PlayBlink(
        float minAlpha,
        float fadeDuration,
        float visibleHold,
        float fadedHold)
    {
        // Докато трае първоначалният pop,
        // checkpoint-ът остава в минималната
        // си прозрачност.
        yield return new WaitForSeconds(
            popDuration +
            settleDuration
        );

        while (true)
        {
            // Започва от най-прозрачното
            // състояние.
            canvasGroup.alpha =
                minAlpha;

            yield return new WaitForSeconds(
                fadedHold
            );

            // Плавно става напълно видим.
            yield return FadeTo(
                1f,
                fadeDuration
            );

            // Стои видим.
            yield return new WaitForSeconds(
                visibleHold
            );

            // Плавно избледнява обратно.
            yield return FadeTo(
                minAlpha,
                fadeDuration
            );
        }
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    public void PlaySuccess(
        Action onComplete = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(
                blinkCoroutine
            );

            blinkCoroutine = null;
        }

        animationCoroutine =
            StartCoroutine(
                PlaySuccessRoutine(
                    onComplete
                )
            );
    }

    private IEnumerator PlaySuccessRoutine(
        Action onComplete)
    {
        Vector3 startScaleValue =
            rectTransform.localScale;

        float startAlpha =
            canvasGroup.alpha;

        Vector3 targetScale =
            difficultyScale *
            successScale;

        float elapsed = 0f;

        while (elapsed < successDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    successDuration
                );

            t =
                t * t *
                (3f - 2f * t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScaleValue,
                    targetScale,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        onComplete?.Invoke();

        gameObject.SetActive(false);
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 start =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            rectTransform.localScale =
                Vector3.Lerp(
                    start,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;
    }

    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator FadeTo(
        float targetAlpha,
        float duration)
    {
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

            t =
                t * t *
                (3f - 2f * t);

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        canvasGroup.alpha =
            targetAlpha;
    }

    // =========================================================
    // STOP
    // =========================================================

    private void StopAllVisualCoroutines()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(
                blinkCoroutine
            );

            blinkCoroutine = null;
        }
    }
}