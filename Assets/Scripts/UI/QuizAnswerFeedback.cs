using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuizAnswerFeedback : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image targetImage;

    [Header("Correct Feedback")]
    [SerializeField] private Color correctColor =
        new Color(0.45f, 1f, 0.45f, 1f);

    [SerializeField] private float correctScale = 1.06f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float correctHoldDuration = 0.35f;

    [Header("Wrong Feedback")]
    [SerializeField] private Color wrongColor =
        new Color(1f, 0.45f, 0.45f, 1f);

    [SerializeField] private float shakeDistance = 12f;
    [SerializeField] private float shakeDuration = 0.30f;
    [SerializeField] private float wrongHoldDuration = 0.25f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Color originalColor;

    private Coroutine feedbackCoroutine;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;

        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
        else
        {
            Debug.LogWarning(
                $"Няма зададен Target Image на {gameObject.name}!"
            );
        }
    }

    public void PlayCorrect()
    {
        if (feedbackCoroutine != null)
            return;

        feedbackCoroutine = StartCoroutine(CorrectRoutine());
    }

    public void PlayWrong()
    {
        if (feedbackCoroutine != null)
            return;

        feedbackCoroutine = StartCoroutine(WrongRoutine());
    }

    public void ResetFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        rectTransform.localScale = originalScale;
        rectTransform.anchoredPosition = originalPosition;

        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }

        IsPlaying = false;
    }

    private IEnumerator CorrectRoutine()
    {
        IsPlaying = true;

        if (targetImage != null)
        {
            targetImage.color = correctColor;
        }

        Vector3 targetScale =
            originalScale * correctScale;

        yield return ScaleTo(targetScale, popDuration);
        yield return ScaleTo(originalScale, popDuration);

        yield return new WaitForSeconds(correctHoldDuration);

        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }

        rectTransform.localScale = originalScale;

        IsPlaying = false;
        feedbackCoroutine = null;
    }

    private IEnumerator WrongRoutine()
    {
        IsPlaying = true;

        if (targetImage != null)
        {
            targetImage.color = wrongColor;
        }

        Vector2 startPosition = originalPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                elapsed / shakeDuration;

            float strength =
                1f - progress;

            float offsetX =
                Mathf.Sin(progress * Mathf.PI * 8f)
                * shakeDistance
                * strength;

            rectTransform.anchoredPosition =
                startPosition +
                new Vector2(offsetX, 0f);

            yield return null;
        }

        rectTransform.anchoredPosition =
            startPosition;

        yield return new WaitForSeconds(wrongHoldDuration);

        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }

        IsPlaying = false;
        feedbackCoroutine = null;
    }

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 startScale =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            t = t * t * (3f - 2f * t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale = targetScale;
    }
}