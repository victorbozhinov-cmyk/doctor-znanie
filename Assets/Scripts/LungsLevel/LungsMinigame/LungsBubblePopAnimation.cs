using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LungsBubblePopAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image bubbleImage;

    [Header("Pop Scale")]
    [SerializeField] private float popScaleMultiplier = 1.18f;

    [Header("Timing")]
    [SerializeField] private float growDuration = 0.08f;
    [SerializeField] private float shrinkDuration = 0.12f;

    [Header("Correct / Wrong Colors")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    [Range(0f, 1f)]
    [SerializeField] private float colorStrength = 0.65f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Color originalColor;

    private Coroutine popCoroutine;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (bubbleImage == null)
        {
            bubbleImage =
                GetComponent<Image>();
        }

        if (rectTransform != null)
        {
            originalScale =
                rectTransform.localScale;
        }

        if (bubbleImage != null)
        {
            originalColor =
                bubbleImage.color;
        }
    }

    // =========================================================
    // PLAY POP
    // =========================================================

    public void PlayPop(bool isCorrect)
    {
        if (rectTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        if (popCoroutine != null)
        {
            return;
        }

        popCoroutine =
            StartCoroutine(
                PopRoutine(isCorrect)
            );
    }

    // =========================================================
    // POP ROUTINE
    // =========================================================

    private IEnumerator PopRoutine(bool isCorrect)
    {
        Vector3 enlargedScale =
            originalScale *
            popScaleMultiplier;

        Color feedbackColor =
            isCorrect
                ? correctColor
                : wrongColor;

        // =====================================================
        // COLOR FEEDBACK
        // =====================================================

        if (bubbleImage != null)
        {
            Color mixedColor =
                Color.Lerp(
                    originalColor,
                    feedbackColor,
                    colorStrength
                );

            // Правим feedback-а напълно непрозрачен,
            // за да не прихваща цвета на фона отзад.
            mixedColor.a = 1f;

            bubbleImage.color =
                mixedColor;
        }

        // =====================================================
        // GROW
        // =====================================================

        yield return ScaleTo(
            enlargedScale,
            growDuration
        );

        // =====================================================
        // SHRINK
        // =====================================================

        yield return ScaleTo(
            Vector3.zero,
            shrinkDuration
        );

        Destroy(gameObject);
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration
    )
    {
        Vector3 startScale =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;
    }
}