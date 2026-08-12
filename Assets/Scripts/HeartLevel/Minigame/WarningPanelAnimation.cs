using System.Collections;
using UnityEngine;

public class WarningPanelAnimation : MonoBehaviour
{
    [Header("Pop Settings")]
    [SerializeField] private float startScale = 0.95f;
    [SerializeField] private float overshootScale = 1.025f;

    [SerializeField] private float growDuration = 0.10f;
    [SerializeField] private float settleDuration = 0.14f;

    private Vector3 baseScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine =
            StartCoroutine(PlayPop());
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        transform.localScale = baseScale;
    }

    private IEnumerator PlayPop()
    {
        Vector3 smallScale =
            baseScale * startScale;

        Vector3 bigScale =
            baseScale * overshootScale;

        transform.localScale = smallScale;

        // -----------------------------------------
        // 1. Леко уголемяване
        // 0.95 -> 1.025
        // -----------------------------------------

        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / growDuration
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

        transform.localScale = bigScale;

        // -----------------------------------------
        // 2. Плавно връщане
        // 1.025 -> 1.00
        // -----------------------------------------

        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / settleDuration
                );

            t = SmoothStep(t);

            transform.localScale =
                Vector3.Lerp(
                    bigScale,
                    baseScale,
                    t
                );

            yield return null;
        }

        transform.localScale = baseScale;
        animationCoroutine = null;
    }

    private float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);

        return t * t * (3f - 2f * t);
    }
}