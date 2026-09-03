using System.Collections;
using UnityEngine;

public class BrainCommandReadyAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform animationTarget;

    [Header("Pop")]
    [SerializeField]
    private float startScale = 0.75f;

    [SerializeField]
    private float overshootScale = 1.15f;

    [SerializeField]
    private float growDuration = 0.14f;

    [SerializeField]
    private float returnDuration = 0.12f;

    private Vector3 originalScale;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (animationTarget == null)
        {
            animationTarget = transform;
        }

        originalScale =
            animationTarget.localScale;
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
            originalScale * startScale;

        animationCoroutine =
            StartCoroutine(
                PlayPopRoutine());
    }

    private IEnumerator PlayPopRoutine()
    {
        Vector3 start =
            originalScale * startScale;

        Vector3 overshoot =
            originalScale * overshootScale;

        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / growDuration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            animationTarget.localScale =
                Vector3.Lerp(
                    start,
                    overshoot,
                    smoothProgress);

            yield return null;
        }

        animationTarget.localScale =
            overshoot;

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
                    overshoot,
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
            StopCoroutine(
                animationCoroutine);

            animationCoroutine = null;
        }

        if (animationTarget != null)
        {
            animationTarget.localScale =
                originalScale;
        }
    }
}