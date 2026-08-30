using System.Collections;
using UnityEngine;

public class UIButtonIdlePulse : MonoBehaviour
{
    [Header("Idle Pulse")]
    [SerializeField] private float pulseScale = 1.04f;
    [SerializeField] private float pulseDuration = 0.45f;
    [SerializeField] private float timeBetweenPulses = 2.5f;

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        pulseCoroutine =
            StartCoroutine(PulseLoop());
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        transform.localScale = originalScale;
    }

    // =========================================================
    // PULSE LOOP
    // =========================================================

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                timeBetweenPulses
            );

            yield return PlayPulse();
        }
    }

    // =========================================================
    // PULSE
    // =========================================================

    private IEnumerator PlayPulse()
    {
        Vector3 enlargedScale =
            originalScale * pulseScale;

        float halfDuration =
            pulseDuration / 2f;

        // 1.00 -> 1.04
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    time / halfDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    t
                );

            yield return null;
        }

        // 1.04 -> 1.00
        time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    time / halfDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            transform.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    t
                );

            yield return null;
        }

        transform.localScale =
            originalScale;
    }

    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    public void SetPulseEnabled(bool enabled)
    {
        if (enabled)
        {
            if (pulseCoroutine == null &&
                gameObject.activeInHierarchy)
            {
                pulseCoroutine =
                    StartCoroutine(PulseLoop());
            }
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            transform.localScale =
                originalScale;
        }
    }
}