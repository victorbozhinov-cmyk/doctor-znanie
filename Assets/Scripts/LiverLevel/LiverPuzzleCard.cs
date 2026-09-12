using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum LiverFunctionType
{
    Produces,
    Stores,
    Processes,
    Detoxifies
}

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Outline))]
public class LiverPuzzleCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Card Settings")]
    [SerializeField] private LiverFunctionType correctFunction;

    [Header("Correct Feedback")]
    [SerializeField] private float correctGlowDuration = 0.45f;
    [SerializeField] private float correctScale = 1.08f;

    [Header("Wrong Feedback")]
    [SerializeField] private float wrongGlowDuration = 0.35f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeCount = 6;

    [Header("Glow")]
    [SerializeField] private float glowStrength = 6f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Outline outline;

    private Transform startParent;
    private Vector2 startPosition;
    private Vector3 normalScale;

    private bool placedCorrectly = false;
    private bool dropHandled = false;

    private Coroutine feedbackCoroutine;

    public LiverFunctionType CorrectFunction => correctFunction;
    public bool IsPlacedCorrectly => placedCorrectly;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        outline = GetComponent<Outline>();

        normalScale = rectTransform.localScale;

        // В началото няма сияние.
        outline.effectColor =
            new Color(0f, 0f, 0f, 0f);

        outline.effectDistance =
            new Vector2(glowStrength, -glowStrength);

        outline.useGraphicAlpha = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placedCorrectly)
            return;

        // Запомняме оригиналното място на картата.
        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;

        dropHandled = false;

        // Вадим картата най-отгоре по време на влачене.
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placedCorrectly)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Ако картата не е пусната върху слот,
        // просто се връща обратно.
        if (!placedCorrectly && !dropHandled)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        transform.SetParent(startParent, false);

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            startPosition;

        rectTransform.localScale =
            normalScale;

        canvasGroup.blocksRaycasts = true;
    }

    // =========================
    // ПРАВИЛНО ПОСТАВЯНЕ
    // =========================

    public void PlaceCorrectly(Transform slot)
    {
        if (placedCorrectly)
            return;

        dropHandled = true;
        placedCorrectly = true;

        transform.SetParent(slot, false);

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.localScale =
            normalScale;

        canvasGroup.blocksRaycasts = true;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine =
            StartCoroutine(PlayCorrectFeedback());
    }

    // =========================
    // ГРЕШНО ПОСТАВЯНЕ
    // =========================

    public void PlayWrongFeedback(Transform wrongSlot)
    {
        if (placedCorrectly)
            return;

        dropHandled = true;

        // Първо поставяме картата точно
        // в центъра на грешния слот.
        transform.SetParent(wrongSlot, false);

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.localScale =
            normalScale;

        canvasGroup.blocksRaycasts = true;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine =
            StartCoroutine(PlayWrongFeedbackCoroutine());
    }

    // =========================
    // CORRECT ANIMATION
    // =========================

    private IEnumerator PlayCorrectFeedback()
    {
        outline.effectColor =
            new Color(0.1f, 1f, 0.15f, 0.95f);

        float halfDuration =
            correctGlowDuration / 2f;

        float timer = 0f;

        // Уголемяване.
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / halfDuration
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    normalScale,
                    normalScale * correctScale,
                    t
                );

            yield return null;
        }

        timer = 0f;

        // Връщане към нормален размер
        // + изчезване на зеленото.
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / halfDuration
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    normalScale * correctScale,
                    normalScale,
                    t
                );

            Color glowColor =
                outline.effectColor;

            glowColor.a =
                Mathf.Lerp(
                    0.95f,
                    0f,
                    t
                );

            outline.effectColor =
                glowColor;

            yield return null;
        }

        rectTransform.localScale =
            normalScale;

        outline.effectColor =
            new Color(0f, 0f, 0f, 0f);

        feedbackCoroutine = null;
    }

    // =========================
    // WRONG ANIMATION
    // =========================

    private IEnumerator PlayWrongFeedbackCoroutine()
    {
        // Червено сияние.
        outline.effectColor =
            new Color(
                1f,
                0.05f,
                0.05f,
                0.95f
            );

        // Вече сме точно в центъра
        // на грешния слот.
        Vector2 wrongPosition =
            rectTransform.anchoredPosition;

        // Разклащане наляво-надясно.
        for (int i = 0; i < shakeCount; i++)
        {
            float direction =
                (i % 2 == 0)
                ? 1f
                : -1f;

            rectTransform.anchoredPosition =
                wrongPosition +
                new Vector2(
                    shakeStrength * direction,
                    0f
                );

            yield return
                new WaitForSeconds(0.04f);
        }

        // Връщаме я точно в центъра
        // след разклащането.
        rectTransform.anchoredPosition =
            wrongPosition;

        // Червеното постепенно изчезва.
        float timer = 0f;

        while (timer < wrongGlowDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / wrongGlowDuration
                );

            Color glowColor =
                outline.effectColor;

            glowColor.a =
                Mathf.Lerp(
                    0.95f,
                    0f,
                    t
                );

            outline.effectColor =
                glowColor;

            yield return null;
        }

        outline.effectColor =
            new Color(0f, 0f, 0f, 0f);

        // ЕДВА след цялата грешна анимация
        // връщаме картата обратно.
        ReturnToStart();

        dropHandled = false;
        feedbackCoroutine = null;
    }
}