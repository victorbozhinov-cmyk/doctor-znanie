using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StomachIrritationAnimation : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private float shakeDistance = 9f;
    [SerializeField] private float shakeStepDuration = 0.045f;
    [SerializeField] private int shakeCount = 4;

    [Header("Pulse")]
    [SerializeField] private float firstPulseScale = 1.025f;
    [SerializeField] private float secondPulseScale = 1.015f;

    [SerializeField] private float pulseUpDuration = 0.10f;
    [SerializeField] private float pulseDownDuration = 0.11f;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = true;

    private RectTransform rectTransform;

    private Coroutine irritationCoroutine;

    private Vector2 basePosition;
    private Vector3 baseScale;

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
        {
            basePosition =
                rectTransform.anchoredPosition;

            baseScale =
                rectTransform.localScale;
        }
    }

    private void Update()
    {
        if (!enableTestKey ||
            Keyboard.current == null)
        {
            return;
        }

        // Временно само за тест.
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            PlayIrritation();
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayIrritation()
    {
        if (rectTransform == null)
            return;

        if (irritationCoroutine != null)
        {
            StopCoroutine(
                irritationCoroutine
            );
        }

        // Вземаме позицията точно преди анимацията,
        // за да няма постепенно изместване.
        basePosition =
            rectTransform.anchoredPosition;

        baseScale =
            rectTransform.localScale;

        irritationCoroutine =
            StartCoroutine(
                IrritationRoutine()
            );
    }

    // =========================================================
    // MAIN ANIMATION
    // =========================================================

    private IEnumerator IrritationRoutine()
    {
        // 1. SHAKE
        for (int i = 0; i < shakeCount; i++)
        {
            yield return MoveTo(
                basePosition +
                Vector2.left * shakeDistance,
                shakeStepDuration
            );

            yield return MoveTo(
                basePosition +
                Vector2.right * shakeDistance,
                shakeStepDuration
            );
        }

        yield return MoveTo(
            basePosition,
            shakeStepDuration
        );

        // 2. FIRST PULSE
        yield return ScaleTo(
            baseScale * firstPulseScale,
            pulseUpDuration
        );

        yield return ScaleTo(
            baseScale,
            pulseDownDuration
        );

        // 3. SECOND, SMALLER PULSE
        yield return ScaleTo(
            baseScale * secondPulseScale,
            pulseUpDuration
        );

        yield return ScaleTo(
            baseScale,
            pulseDownDuration
        );

        // Гарантираме абсолютно точно
        // връщане след анимацията.
        rectTransform.anchoredPosition =
            basePosition;

        rectTransform.localScale =
            baseScale;

        irritationCoroutine = null;
    }

    // =========================================================
    // MOVE
    // =========================================================

    private IEnumerator MoveTo(
        Vector2 target,
        float duration)
    {
        Vector2 start =
            rectTransform.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    start,
                    target,
                    t
                );

            yield return null;
        }

        rectTransform.anchoredPosition =
            target;
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScaleTo(
        Vector3 target,
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

            t = SmoothStep(t);

            rectTransform.localScale =
                Vector3.Lerp(
                    start,
                    target,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            target;
    }

    // =========================================================

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void OnDisable()
    {
        if (irritationCoroutine != null)
        {
            StopCoroutine(
                irritationCoroutine
            );

            irritationCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                basePosition;

            rectTransform.localScale =
                baseScale;
        }
    }
}