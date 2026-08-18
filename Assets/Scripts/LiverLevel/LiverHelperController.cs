using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LiverHelperController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private Image speechBubbleImage;
    [SerializeField] private CanvasGroup speechBubbleCanvasGroup;

    [Header("Bubble Images")]
    [SerializeField] private Sprite[] bubbleSprites;

    [Header("Timing")]
    [SerializeField] private float displayTime = 4f;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float startScale = 0.8f;

    private int lastBubbleIndex = -1;
    private Coroutine bubbleCoroutine;

    private RectTransform bubbleRectTransform;

    private void Awake()
    {
        if (speechBubble != null)
            bubbleRectTransform = speechBubble.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);

        if (exclamationMark != null)
            exclamationMark.SetActive(true);
    }

    public void ShowBubble()
    {
        if (bubbleCoroutine != null)
        {
            StopCoroutine(bubbleCoroutine);
            bubbleCoroutine = null;
        }

        if (exclamationMark != null)
            exclamationMark.SetActive(false);

        // Избира различна картинка от предишната.
        if (bubbleSprites != null &&
            bubbleSprites.Length > 0 &&
            speechBubbleImage != null)
        {
            int newIndex;

            if (bubbleSprites.Length > 1)
            {
                do
                {
                    newIndex = Random.Range(0, bubbleSprites.Length);
                }
                while (newIndex == lastBubbleIndex);
            }
            else
            {
                newIndex = 0;
            }

            lastBubbleIndex = newIndex;
            speechBubbleImage.sprite = bubbleSprites[newIndex];
        }

        bubbleCoroutine = StartCoroutine(BubbleSequence());
    }

    private IEnumerator BubbleSequence()
    {
        speechBubble.SetActive(true);

        // Начално състояние.
        speechBubbleCanvasGroup.alpha = 0f;
        bubbleRectTransform.localScale =
            Vector3.one * startScale;

        // POP + FADE IN
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeInDuration);

            // По-мека анимация.
            float smoothT =
                1f - Mathf.Pow(1f - t, 3f);

            speechBubbleCanvasGroup.alpha = t;

            bubbleRectTransform.localScale =
                Vector3.Lerp(
                    Vector3.one * startScale,
                    Vector3.one,
                    smoothT
                );

            yield return null;
        }

        speechBubbleCanvasGroup.alpha = 1f;
        bubbleRectTransform.localScale = Vector3.one;

        // Стои показано.
        yield return new WaitForSecondsRealtime(displayTime);

        // FADE OUT
        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeOutDuration);

            speechBubbleCanvasGroup.alpha = 1f - t;

            yield return null;
        }

        speechBubbleCanvasGroup.alpha = 0f;

        speechBubble.SetActive(false);

        // Връщаме удивителната.
        if (exclamationMark != null)
            exclamationMark.SetActive(true);

        bubbleCoroutine = null;
    }
}