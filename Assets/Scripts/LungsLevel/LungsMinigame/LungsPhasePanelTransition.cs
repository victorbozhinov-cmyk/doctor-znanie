using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LungsPhasePanelTransition : MonoBehaviour
{
    [Header("Show Animation")]
    [Range(0.5f, 1f)]
    [SerializeField] private float showStartScale = 0.88f;

    [Min(0.01f)]
    [SerializeField] private float showDuration = 0.22f;

    [Header("Hide Animation")]
    [Range(0.5f, 1f)]
    [SerializeField] private float hideEndScale = 0.92f;

    [Min(0.01f)]
    [SerializeField] private float hideDuration = 0.18f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalScale;

    private Coroutine animationCoroutine;

    public bool IsVisible =>
        gameObject.activeSelf &&
        canvasGroup != null &&
        canvasGroup.alpha > 0.01f;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        originalScale =
            rectTransform.localScale;
    }

    // =========================================================
    // SHOW IMMEDIATELY
    // =========================================================

    public void ShowImmediate()
    {
        StopCurrentAnimation();

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        rectTransform.localScale =
            originalScale;
    }

    // =========================================================
    // HIDE IMMEDIATELY
    // =========================================================

    public void HideImmediate()
    {
        StopCurrentAnimation();

        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            originalScale;

        gameObject.SetActive(false);
    }

    // =========================================================
    // PLAY SHOW
    // =========================================================

    public void PlayShow()
    {
        StopCurrentAnimation();

        gameObject.SetActive(true);

        animationCoroutine =
            StartCoroutine(
                ShowRoutine()
            );
    }

    // =========================================================
    // PLAY HIDE
    // =========================================================

    public void PlayHide()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        StopCurrentAnimation();

        animationCoroutine =
            StartCoroutine(
                HideRoutine()
            );
    }

    // =========================================================
    // SHOW ROUTINE
    // =========================================================

    private IEnumerator ShowRoutine()
    {
        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            originalScale *
            showStartScale;

        float elapsed = 0f;

        while (elapsed < showDuration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    showDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            canvasGroup.alpha =
                smoothT;

            rectTransform.localScale =
                Vector3.Lerp(
                    originalScale *
                    showStartScale,
                    originalScale,
                    smoothT
                );

            yield return null;
        }

        canvasGroup.alpha = 1f;

        rectTransform.localScale =
            originalScale;

        animationCoroutine = null;
    }

    // =========================================================
    // HIDE ROUTINE
    // =========================================================

    private IEnumerator HideRoutine()
    {
        float startAlpha =
            canvasGroup.alpha;

        Vector3 startScale =
            rectTransform.localScale;

        Vector3 targetScale =
            originalScale *
            hideEndScale;

        float elapsed = 0f;

        while (elapsed < hideDuration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    hideDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    smoothT
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    smoothT
                );

            yield return null;
        }

        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            originalScale;

        animationCoroutine = null;

        gameObject.SetActive(false);
    }

    // =========================================================
    // STOP
    // =========================================================

    private void StopCurrentAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }
    }
}
