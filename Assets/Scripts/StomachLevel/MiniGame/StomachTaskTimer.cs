using System;
using UnityEngine;
using UnityEngine.UI;

public class StomachTaskTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image timerFill;

    // Целият визуален контейнер на таймера.
    // Тук ще сложим TaskTimerBG.
    [SerializeField] private RectTransform pulseTarget;

    [Header("Warning Pulse")]
    [SerializeField, Range(0f, 1f)]
    private float warningThreshold = 0.25f;

    [SerializeField]
    private float warningPulseScale = 1.06f;

    [SerializeField]
    private float warningPulseSpeed = 4f;

    [Header("Test Settings")]
    [SerializeField] private float testDuration = 5f;
    [SerializeField] private bool startOnPlayForTesting = false;

    private float duration;
    private float remainingTime;

    private bool isRunning;
    private bool isPaused;

    private Vector3 normalPulseScale;

    public event Action TimerExpired;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    private void Awake()
    {
        // Ако случайно не е зададен Pulse Target,
        // използваме Fill-а като резервен вариант.
        if (pulseTarget == null && timerFill != null)
        {
            pulseTarget = timerFill.rectTransform;
        }

        if (pulseTarget != null)
        {
            normalPulseScale =
                pulseTarget.localScale;
        }
    }

    private void Start()
    {
        if (startOnPlayForTesting)
        {
            StartTimer(testDuration);
        }
        else
        {
            SetFill(1f);
            ResetPulse();
        }
    }

    private void Update()
    {
        if (!isRunning || isPaused)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            SetFill(0f);
            ResetPulse();

            TimerExpired?.Invoke();

            return;
        }

        float normalizedTime =
            remainingTime / duration;

        SetFill(normalizedTime);

        UpdateWarningPulse(
            normalizedTime
        );
    }

    // =========================================================
    // TIMER CONTROL
    // =========================================================

    public void StartTimer(float seconds)
    {
        duration =
            Mathf.Max(seconds, 0.01f);

        remainingTime = duration;

        isRunning = true;
        isPaused = false;

        SetFill(1f);
        ResetPulse();
    }

    public void StopTimer()
    {
        isRunning = false;

        ResetPulse();
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
        {
            isPaused = false;
        }
    }

    public void ResetTimer()
    {
        isRunning = false;
        isPaused = false;

        remainingTime = duration;

        SetFill(1f);
        ResetPulse();
    }

    // =========================================================
    // FILL
    // =========================================================

    private void SetFill(float amount)
    {
        if (timerFill != null)
        {
            timerFill.fillAmount =
                Mathf.Clamp01(amount);
        }
    }

    // =========================================================
    // WARNING PULSE
    // =========================================================

    private void UpdateWarningPulse(
        float normalizedTime)
    {
        if (pulseTarget == null)
            return;

        if (normalizedTime > warningThreshold)
        {
            ResetPulse();
            return;
        }

        float pulse =
            (Mathf.Sin(
                Time.time *
                warningPulseSpeed
            ) + 1f) * 0.5f;

        float scale =
            Mathf.Lerp(
                1f,
                warningPulseScale,
                pulse
            );

        pulseTarget.localScale =
            normalPulseScale * scale;
    }

    private void ResetPulse()
    {
        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                normalPulseScale;
        }
    }
}