using System.Collections;
using TMPro;
using UnityEngine;

public class StomachMinigameTimer : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Penalty UI")]
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private CanvasGroup penaltyCanvasGroup;
    [SerializeField] private RectTransform penaltyRect;

    [Header("Timer")]
    [SerializeField] private float startingTime = 195f;

    [Header("Penalty Animation")]
    [SerializeField] private float penaltyAnimationDuration = 0.9f;
    [SerializeField] private float penaltyMoveDistance = 25f;

    private float remainingTime;
    private bool isRunning;

    private Vector2 penaltyStartPosition;
    private Coroutine penaltyCoroutine;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    private void Start()
    {
        remainingTime = startingTime;
        isRunning = true;

        if (penaltyRect != null)
            penaltyStartPosition = penaltyRect.anchoredPosition;

        if (penaltyCanvasGroup != null)
            penaltyCanvasGroup.alpha = 0f;

        UpdateTimerText();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            Debug.Log("Времето изтече!");
        }

        UpdateTimerText();
    }

    public void ApplyPenalty(float seconds)
    {
        if (seconds <= 0f)
            return;

        remainingTime -= seconds;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
        }

        UpdateTimerText();

        ShowPenalty(seconds);
    }

    private void ShowPenalty(float seconds)
    {
        if (penaltyText == null ||
            penaltyCanvasGroup == null ||
            penaltyRect == null)
        {
            return;
        }

        if (penaltyCoroutine != null)
            StopCoroutine(penaltyCoroutine);

        penaltyCoroutine = StartCoroutine(
            AnimatePenalty(seconds)
        );
    }

    private IEnumerator AnimatePenalty(float seconds)
    {
        penaltyText.text = $"-{Mathf.RoundToInt(seconds)} сек.";

        penaltyRect.anchoredPosition = penaltyStartPosition;
        penaltyCanvasGroup.alpha = 1f;

        float elapsed = 0f;

        while (elapsed < penaltyAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / penaltyAnimationDuration
            );

            Vector2 position = penaltyStartPosition;

            position.y -= penaltyMoveDistance * t;

            penaltyRect.anchoredPosition = position;

            penaltyCanvasGroup.alpha = 1f - t;

            yield return null;
        }

        penaltyCanvasGroup.alpha = 0f;
        penaltyRect.anchoredPosition = penaltyStartPosition;

        penaltyCoroutine = null;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
            isRunning = true;
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes}:{seconds:00}";
    }
}