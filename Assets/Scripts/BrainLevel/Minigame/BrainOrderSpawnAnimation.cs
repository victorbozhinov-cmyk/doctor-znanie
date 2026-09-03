using System.Collections;
using UnityEngine;

public class BrainOrderSpawnAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform animationTarget;

    [Header("Pulse")]
    [SerializeField]
    private float growDuration = 0.12f;

    [SerializeField]
    private float returnDuration = 0.16f;

    [SerializeField]
    private float pulseScale = 1.12f;

    private Vector3 originalScale;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (animationTarget == null)
        {
            animationTarget = transform;
        }

        originalScale = animationTarget.localScale;
    }

    public void Play()
    {
        if (animationTarget == null)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationTarget.localScale =
            originalScale;

        animationCoroutine =
            StartCoroutine(
                PlayPulseRoutine());
    }

    private IEnumerator PlayPulseRoutine()
    {
        Vector3 enlargedScale =
            originalScale * pulseScale;

        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / growDuration);

            animationTarget.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    progress);

            yield return null;
        }

        animationTarget.localScale =
            enlargedScale;

        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / returnDuration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            animationTarget.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    smoothProgress);

            yield return null;
        }

        animationTarget.localScale =
            originalScale;

        animationCoroutine = null;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (animationTarget != null)
        {
            animationTarget.localScale =
                originalScale;
        }
    }
}
