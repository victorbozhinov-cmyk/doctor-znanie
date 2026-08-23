using System.Collections;
using UnityEngine;

public class JuiceSpaceIconPulse : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseScale = 1.08f;
    [SerializeField] private float scaleUpDuration = 0.22f;
    [SerializeField] private float scaleDownDuration = 0.25f;

    [Header("Timing")]
    [SerializeField] private float delayBetweenPulses = 1.25f;

    private RectTransform rectTransform;
    private Vector3 normalScale;

    private Coroutine pulseCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
        {
            normalScale =
                rectTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale =
            normalScale;

        pulseCoroutine =
            StartCoroutine(
                PulseLoop()
            );
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(
                pulseCoroutine
            );

            pulseCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                normalScale;
        }
    }

    // =========================================================
    // LOOP
    // =========================================================

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                delayBetweenPulses
            );

            yield return ScaleTo(
                normalScale * pulseScale,
                scaleUpDuration
            );

            yield return ScaleTo(
                normalScale,
                scaleDownDuration
            );
        }
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 startScale =
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
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;
    }
}