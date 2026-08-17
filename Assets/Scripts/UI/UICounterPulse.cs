using System.Collections;
using UnityEngine;

public class UICounterPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseScale = 1.12f;
    [SerializeField] private float pulseDuration = 0.20f;

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Play()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        transform.localScale = originalScale;

        pulseCoroutine =
            StartCoroutine(PlayPulse());
    }

    private IEnumerator PlayPulse()
    {
        float halfDuration =
            pulseDuration / 2f;

        Vector3 enlargedScale =
            originalScale * pulseScale;

        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    time / halfDuration
                );

            transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    t
                );

            yield return null;
        }

        time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    time / halfDuration
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

        pulseCoroutine = null;
    }

    private void OnDisable()
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
