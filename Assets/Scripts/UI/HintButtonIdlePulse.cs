using System.Collections;
using UnityEngine;

public class HintButtonIdlePulse : MonoBehaviour
{
    [Header("Idle Pulse")]
    [SerializeField] private float pulseScale = 1.06f;
    [SerializeField] private float pulseDuration = 0.35f;
    [SerializeField] private float timeBetweenPulses = 3.5f;

    private Vector3 originalScale;
    private Coroutine idleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        idleCoroutine = StartCoroutine(IdlePulseLoop());
    }

    private void OnDisable()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator IdlePulseLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(timeBetweenPulses);

            yield return Pulse();
        }
    }

    private IEnumerator Pulse()
    {
        Vector3 enlargedScale = originalScale * pulseScale;
        float halfDuration = pulseDuration / 2f;

        // 1.00 -> 1.06
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / halfDuration);

            // По-меко движение
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(originalScale, enlargedScale, t);

            yield return null;
        }

        // 1.06 -> 1.00
        time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / halfDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(enlargedScale, originalScale, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void SetIdleAnimationEnabled(bool enabled)
    {
        if (enabled)
        {
            if (idleCoroutine == null && gameObject.activeInHierarchy)
                idleCoroutine = StartCoroutine(IdlePulseLoop());
        }
        else
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }

            transform.localScale = originalScale;
        }
    }
}
