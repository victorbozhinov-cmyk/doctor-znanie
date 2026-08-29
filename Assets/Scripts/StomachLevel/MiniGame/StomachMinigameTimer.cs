using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StomachMinigameTimer : MonoBehaviour
{
    public event Action TotalTimerExpired;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;

    // =========================================================
    // DIFFICULTY TIME
    // =========================================================

    [Header("Difficulty - Total Game Time")]
    [SerializeField] private float easyStartingTime = 180f;
    [SerializeField] private float mediumStartingTime = 210f;
    [SerializeField] private float hardStartingTime = 270f;

    // =========================================================
    // WARNING PULSE
    // =========================================================

    [Header("Warning Pulse")]
    [SerializeField] private RectTransform pulseTarget;

    [SerializeField] private float warningTime = 10f;
    [SerializeField] private float criticalTime = 5f;

    [SerializeField] private float warningPulseScale = 1.05f;
    [SerializeField] private float warningPulseUpDuration = 0.28f;
    [SerializeField] private float warningPulseDownDuration = 0.28f;

    [SerializeField] private float criticalPulseScale = 1.08f;
    [SerializeField] private float criticalPulseUpDuration = 0.16f;
    [SerializeField] private float criticalPulseDownDuration = 0.16f;

    // =========================================================
    // PENALTY
    // =========================================================

    [Header("Penalty UI")]
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private CanvasGroup penaltyCanvasGroup;
    [SerializeField] private RectTransform penaltyRect;

    [Header("Penalty Animation")]
    [SerializeField] private float penaltyAnimationDuration = 0.9f;
    [SerializeField] private float penaltyMoveDistance = 25f;

    private float remainingTime;

    private bool isRunning;
    private bool hasExpired;

    private int difficulty;

    private Vector2 penaltyStartPosition;
    private Coroutine penaltyCoroutine;

    private Vector3 normalPulseScale;
    private Coroutine pulseCoroutine;

    // =========================================================
    // PUBLIC INFO
    // =========================================================

    public float RemainingTime =>
        remainingTime;

    public bool IsRunning =>
        isRunning;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (pulseTarget == null)
        {
            pulseTarget =
                transform as RectTransform;
        }

        if (pulseTarget != null)
        {
            normalPulseScale =
                pulseTarget.localScale;
        }
    }

    private void Start()
    {
        difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        remainingTime =
            GetStartingTimeForDifficulty();

        // ВАЖНО:
        // Таймерът вече НЕ тръгва автоматично.
        // StomachMinigameManager ще го стартира,
        // когато WelcomePanel се затвори.
        isRunning = false;

        hasExpired = false;

        if (penaltyRect != null)
        {
            penaltyStartPosition =
                penaltyRect.anchoredPosition;
        }

        if (penaltyCanvasGroup != null)
        {
            penaltyCanvasGroup.alpha = 0f;
        }

        ResetPulseVisual();

        UpdateTimerText();
        UpdateWarningPulseState();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -=
            Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerText();

            ExpireTimer();

            return;
        }

        UpdateTimerText();
        UpdateWarningPulseState();
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private float GetStartingTimeForDifficulty()
    {
        switch (difficulty)
        {
            case 0:
                return easyStartingTime;

            case 2:
                return hardStartingTime;

            default:
                return mediumStartingTime;
        }
    }

    // =========================================================
    // PENALTY
    // =========================================================

    public void ApplyPenalty(
        float seconds)
    {
        if (seconds <= 0f ||
            hasExpired)
        {
            return;
        }

        remainingTime -=
            seconds;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerText();

            ShowPenalty(seconds);

            ExpireTimer();

            return;
        }

        UpdateTimerText();
        UpdateWarningPulseState();

        ShowPenalty(seconds);
    }

    // =========================================================
    // EXPIRATION
    // =========================================================

    private void ExpireTimer()
    {
        if (hasExpired)
            return;

        hasExpired = true;
        isRunning = false;

        remainingTime = 0f;

        StopWarningPulse();
        ResetPulseVisual();

        UpdateTimerText();

        TotalTimerExpired?.Invoke();
    }

    // =========================================================
    // WARNING PULSE
    // =========================================================

    private void UpdateWarningPulseState()
    {
        if (pulseTarget == null ||
            hasExpired)
        {
            return;
        }

        bool shouldPulse =
            remainingTime > 0f &&
            remainingTime <= warningTime;

        if (shouldPulse)
        {
            if (pulseCoroutine == null)
            {
                pulseCoroutine =
                    StartCoroutine(
                        WarningPulseLoop()
                    );
            }
        }
        else
        {
            StopWarningPulse();
            ResetPulseVisual();
        }
    }

    private IEnumerator WarningPulseLoop()
    {
        while (!hasExpired &&
               remainingTime > 0f &&
               remainingTime <= warningTime)
        {
            bool isCritical =
                remainingTime <=
                criticalTime;

            float targetScale =
                isCritical
                    ? criticalPulseScale
                    : warningPulseScale;

            float upDuration =
                isCritical
                    ? criticalPulseUpDuration
                    : warningPulseUpDuration;

            float downDuration =
                isCritical
                    ? criticalPulseDownDuration
                    : warningPulseDownDuration;

            yield return ScalePulseTo(
                normalPulseScale *
                targetScale,

                upDuration
            );

            yield return ScalePulseTo(
                normalPulseScale,

                downDuration
            );
        }

        ResetPulseVisual();

        pulseCoroutine = null;
    }

    private IEnumerator ScalePulseTo(
        Vector3 targetScale,
        float duration)
    {
        if (pulseTarget == null)
            yield break;

        Vector3 startScale =
            pulseTarget.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            pulseTarget.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        pulseTarget.localScale =
            targetScale;
    }

    private void StopWarningPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(
                pulseCoroutine
            );

            pulseCoroutine = null;
        }
    }

    private void ResetPulseVisual()
    {
        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                normalPulseScale;
        }
    }

    // =========================================================
    // PENALTY VISUAL
    // =========================================================

    private void ShowPenalty(
        float seconds)
    {
        if (penaltyText == null ||
            penaltyCanvasGroup == null ||
            penaltyRect == null)
        {
            return;
        }

        if (penaltyCoroutine != null)
        {
            StopCoroutine(
                penaltyCoroutine
            );
        }

        penaltyCoroutine =
            StartCoroutine(
                AnimatePenalty(
                    seconds
                )
            );
    }

    private IEnumerator AnimatePenalty(
        float seconds)
    {
        penaltyText.text =
            $"-{Mathf.RoundToInt(seconds)} сек.";

        penaltyRect.anchoredPosition =
            penaltyStartPosition;

        penaltyCanvasGroup.alpha =
            1f;

        float elapsed = 0f;

        while (elapsed <
               penaltyAnimationDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    penaltyAnimationDuration
                );

            Vector2 position =
                penaltyStartPosition;

            position.y -=
                penaltyMoveDistance * t;

            penaltyRect.anchoredPosition =
                position;

            penaltyCanvasGroup.alpha =
                1f - t;

            yield return null;
        }

        penaltyCanvasGroup.alpha =
            0f;

        penaltyRect.anchoredPosition =
            penaltyStartPosition;

        penaltyCoroutine = null;
    }

    // =========================================================
    // TIMER CONTROL
    // =========================================================

    public void PauseTimer()
    {
        if (hasExpired)
            return;

        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (hasExpired)
            return;

        if (remainingTime > 0f)
        {
            isRunning = true;
        }
    }

    // =========================================================
    // TIMER TEXT
    // =========================================================

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(
                remainingTime
            );

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            $"{minutes}:{seconds:00}";
    }
}