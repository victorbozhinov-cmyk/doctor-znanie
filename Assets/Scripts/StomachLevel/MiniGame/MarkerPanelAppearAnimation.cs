using System.Collections;
using UnityEngine;

public class MarkerPanelAppearAnimation : MonoBehaviour
{
    [Header("Appear")]
    [SerializeField] private float startScale = 0.88f;
    [SerializeField] private float popScale = 1.07f;
    [SerializeField] private float appearDuration = 0.18f;
    [SerializeField] private float settleDuration = 0.12f;

    [Header("Attention Pulse")]
    [SerializeField] private float pulseScale = 1.03f;
    [SerializeField] private float pulseUpDuration = 0.10f;
    [SerializeField] private float pulseDownDuration = 0.12f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 normalScale;

    private Coroutine animationCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

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

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );
        }

        animationCoroutine =
            StartCoroutine(
                PlayAppearRoutine()
            );
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                normalScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    // =========================================================
    // APPEAR
    // =========================================================

    private IEnumerator PlayAppearRoutine()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            normalScale * startScale;

        // 1. Fade In + Pop
        yield return ScaleAndFadeTo(
            normalScale * popScale,
            1f,
            appearDuration
        );

        // 2. Settle до нормалния размер
        yield return ScaleTo(
            normalScale,
            settleDuration
        );

        // 3. Малък допълнителен pulse
        yield return ScaleTo(
            normalScale * pulseScale,
            pulseUpDuration
        );

        yield return ScaleTo(
            normalScale,
            pulseDownDuration
        );

        rectTransform.localScale =
            normalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        animationCoroutine = null;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private IEnumerator ScaleAndFadeTo(
        Vector3 targetScale,
        float targetAlpha,
        float duration)
    {
        Vector3 startingScale =
            rectTransform.localScale;

        float startingAlpha =
            canvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startingScale,
                    targetScale,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startingAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;

        canvasGroup.alpha =
            targetAlpha;
    }

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 startingScale =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t = SmoothStep(t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startingScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}