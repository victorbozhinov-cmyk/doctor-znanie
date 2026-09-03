using System.Collections;
using UnityEngine;

public class BrainTensionStateAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform animationTarget;

    [Header("Pop")]
    [SerializeField]
    private float popScale = 1.15f;

    [SerializeField]
    private float popDuration = 0.12f;

    [Header("Shake")]
    [SerializeField]
    private float shakeDuration = 0.25f;

    [SerializeField]
    private float shakeStrength = 0.06f;

    [SerializeField]
    private float shakeSpeed = 40f;

    [Header("Return")]
    [SerializeField]
    private float returnDuration = 0.14f;

    private Vector3 originalScale;
    private Vector3 originalLocalPosition;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (animationTarget == null)
        {
            animationTarget = transform;
        }

        originalScale =
            animationTarget.localScale;

        originalLocalPosition =
            animationTarget.localPosition;
    }

    public void Play()
    {
        if (animationTarget == null)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine);
        }

        animationTarget.localScale =
            originalScale;

        animationTarget.localPosition =
            originalLocalPosition;

        animationCoroutine =
            StartCoroutine(
                PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        Vector3 enlargedScale =
            originalScale * popScale;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / popDuration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            animationTarget.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    smoothProgress);

            yield return null;
        }

        animationTarget.localScale =
            enlargedScale;

        timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / shakeDuration);

            float strength =
                shakeStrength *
                (1f - progress);

            float offsetX =
                Mathf.Sin(
                    timer * shakeSpeed) *
                strength;

            animationTarget.localPosition =
                originalLocalPosition +
                new Vector3(
                    offsetX,
                    0f,
                    0f);

            yield return null;
        }

        animationTarget.localPosition =
            originalLocalPosition;

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

        animationTarget.localPosition =
            originalLocalPosition;

        animationCoroutine = null;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine);

            animationCoroutine = null;
        }

        if (animationTarget != null)
        {
            animationTarget.localScale =
                originalScale;

            animationTarget.localPosition =
                originalLocalPosition;
        }
    }
}
