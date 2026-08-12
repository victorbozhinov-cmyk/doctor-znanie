using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
public class LiverCardFeedback : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] private float glowDuration = 0.45f;
    [SerializeField] private float glowStrength = 7f;

    [Header("Correct")]
    [SerializeField] private float correctScale = 1.08f;

    [Header("Wrong")]
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeCount = 6;

    private Outline outline;
    private RectTransform rectTransform;

    private Vector3 normalScale;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        rectTransform = GetComponent<RectTransform>();

        normalScale = rectTransform.localScale;

        outline.effectColor = new Color(0f, 0f, 0f, 0f);
        outline.effectDistance = new Vector2(glowStrength, -glowStrength);
    }

    public void PlayCorrectFeedback()
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(CorrectFeedback());
    }

    public void PlayWrongFeedback()
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(WrongFeedback());
    }

    private IEnumerator CorrectFeedback()
    {
        float halfDuration = glowDuration / 2f;

        // Ярко зелено
        outline.effectColor = new Color(0.1f, 1f, 0.15f, 0.9f);

        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = timer / halfDuration;

            rectTransform.localScale =
                Vector3.Lerp(normalScale, normalScale * correctScale, t);

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = timer / halfDuration;

            rectTransform.localScale =
                Vector3.Lerp(normalScale * correctScale, normalScale, t);

            Color color = outline.effectColor;
            color.a = Mathf.Lerp(0.9f, 0f, t);
            outline.effectColor = color;

            yield return null;
        }

        rectTransform.localScale = normalScale;
        outline.effectColor = new Color(0f, 0f, 0f, 0f);

        feedbackCoroutine = null;
    }

    private IEnumerator WrongFeedback()
    {
        outline.effectColor = new Color(1f, 0.05f, 0.05f, 0.9f);

        Vector2 originalPosition = rectTransform.anchoredPosition;

        for (int i = 0; i < shakeCount; i++)
        {
            float direction = i % 2 == 0 ? 1f : -1f;

            rectTransform.anchoredPosition =
                originalPosition + new Vector2(shakeStrength * direction, 0f);

            yield return new WaitForSeconds(0.04f);
        }

        rectTransform.anchoredPosition = originalPosition;

        float timer = 0f;

        while (timer < glowDuration)
        {
            timer += Time.deltaTime;

            Color color = outline.effectColor;
            color.a = Mathf.Lerp(0.9f, 0f, timer / glowDuration);
            outline.effectColor = color;

            yield return null;
        }

        outline.effectColor = new Color(0f, 0f, 0f, 0f);

        feedbackCoroutine = null;
    }
}