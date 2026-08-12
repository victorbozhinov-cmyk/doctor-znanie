using System.Collections;
using UnityEngine;

public class HeartStateTransitionAnimation : MonoBehaviour
{
    [Header("Pop Settings")]
    [SerializeField] private float startScale = 0.90f;
    [SerializeField] private float overshootScale = 1.08f;

    [SerializeField] private float scaleUpDuration = 0.10f;
    [SerializeField] private float settleDuration = 0.12f;

    private Coroutine transitionCoroutine;

    public void PlayStateChange()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine =
            StartCoroutine(
                PlayTransition()
            );
    }

    private IEnumerator PlayTransition()
    {
        Vector3 originalScale =
            Vector3.one;

        Vector3 smallScale =
            originalScale *
            startScale;

        Vector3 bigScale =
            originalScale *
            overshootScale;

        transform.localScale =
            smallScale;

        float timer = 0f;

        // 0.90 -> 1.08
        while (timer < scaleUpDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    scaleUpDuration
                );

            t = SmoothStep(t);

            transform.localScale =
                Vector3.Lerp(
                    smallScale,
                    bigScale,
                    t
                );

            yield return null;
        }

        timer = 0f;

        // 1.08 -> 1.00
        while (timer < settleDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    settleDuration
                );

            t = SmoothStep(t);

            transform.localScale =
                Vector3.Lerp(
                    bigScale,
                    originalScale,
                    t
                );

            yield return null;
        }

        transform.localScale =
            originalScale;

        transitionCoroutine = null;
    }

    private float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);

        return t * t * (3f - 2f * t);
    }
}
