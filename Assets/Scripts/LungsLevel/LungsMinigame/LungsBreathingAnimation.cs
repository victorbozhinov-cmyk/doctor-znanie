using System.Collections;
using UnityEngine;

public class LungsBreathingAnimation : MonoBehaviour
{
    [Header("Lungs Target")]
    [SerializeField] private RectTransform lungsRect;

    // =========================================================
    // INHALE
    // =========================================================

    [Header("Inhale Animation")]

    [Tooltip("Колко да се уголемят дробовете при Вдишване.")]
    [SerializeField] private float inhaleScale = 1.06f;

    [Tooltip("Колко време отнема разширяването.")]
    [SerializeField] private float inhaleExpandDuration = 0.65f;

    [Tooltip("Колко време остават разширени.")]
    [SerializeField] private float inhaleHoldDuration = 0.25f;

    [Tooltip("Колко време отнема връщането към нормален размер.")]
    [SerializeField] private float inhaleReturnDuration = 0.45f;

    // =========================================================
    // EXHALE
    // =========================================================

    [Header("Exhale Animation")]

    [Tooltip("Колко да се свият дробовете при Издишване.")]
    [SerializeField] private float exhaleScale = 0.95f;

    [Tooltip("Колко време отнема свиването.")]
    [SerializeField] private float exhaleContractDuration = 0.65f;

    [Tooltip("Колко време остават свити.")]
    [SerializeField] private float exhaleHoldDuration = 0.2f;

    [Tooltip("Колко време отнема връщането към нормален размер.")]
    [SerializeField] private float exhaleReturnDuration = 0.45f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private Vector3 normalScale;

    private Coroutine animationCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (lungsRect == null)
        {
            lungsRect = GetComponent<RectTransform>();
        }

        if (lungsRect != null)
        {
            normalScale = lungsRect.localScale;
        }
    }

    // =========================================================
    // INHALE COMPLETE
    // =========================================================

    public void PlayInhaleComplete()
    {
        if (lungsRect == null)
        {
            return;
        }

        StopCurrentAnimation();

        animationCoroutine =
            StartCoroutine(
                InhaleCompleteRoutine()
            );
    }

    private IEnumerator InhaleCompleteRoutine()
    {
        Vector3 expandedScale =
            normalScale * inhaleScale;

        // Плавно разширяване.
        yield return ScaleTo(
            expandedScale,
            inhaleExpandDuration
        );

        // Задържане на пълния дроб.
        yield return new WaitForSecondsRealtime(
            inhaleHoldDuration
        );

        // Плавно връщане.
        yield return ScaleTo(
            normalScale,
            inhaleReturnDuration
        );

        lungsRect.localScale =
            normalScale;

        animationCoroutine = null;
    }

    // =========================================================
    // EXHALE COMPLETE
    // =========================================================

    public void PlayExhaleComplete()
    {
        if (lungsRect == null)
        {
            return;
        }

        StopCurrentAnimation();

        animationCoroutine =
            StartCoroutine(
                ExhaleCompleteRoutine()
            );
    }

    private IEnumerator ExhaleCompleteRoutine()
    {
        Vector3 contractedScale =
            normalScale * exhaleScale;

        // Плавно свиване.
        yield return ScaleTo(
            contractedScale,
            exhaleContractDuration
        );

        // Задържане.
        yield return new WaitForSecondsRealtime(
            exhaleHoldDuration
        );

        // Плавно връщане.
        yield return ScaleTo(
            normalScale,
            exhaleReturnDuration
        );

        lungsRect.localScale =
            normalScale;

        animationCoroutine = null;
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
            lungsRect.localScale;

        if (duration <= 0f)
        {
            lungsRect.localScale =
                targetScale;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            /*
             * SmoothStep прави движението по-меко:
             * бавно начало -> плавно ускоряване ->
             * бавно спиране.
             */
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            lungsRect.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        lungsRect.localScale =
            targetScale;
    }

    // =========================================================
    // STOP
    // =========================================================

    private void StopCurrentAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (lungsRect != null)
        {
            lungsRect.localScale =
                normalScale;
        }
    }
}