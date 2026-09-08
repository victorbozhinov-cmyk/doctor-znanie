using System;
using UnityEngine;
using UnityEngine.UI;

public class StomachTaskTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image timerFill;

    [SerializeField] private RectTransform pulseTarget;

    [Header("Warning Pulse")]
    [SerializeField, Range(0f, 1f)]
    private float warningThreshold = 0.25f;

    [SerializeField]
    private float warningPulseScale = 1.06f;

    [SerializeField]
    private float warningPulseSpeed = 4f;

    [Header("Audio")]
    [SerializeField] private StomachMinigameAudio minigameAudio;

    [Header("Test Settings")]
    [SerializeField] private float testDuration = 5f;
    [SerializeField] private bool startOnPlayForTesting = false;

    private float duration;
    private float remainingTime;

    private bool isRunning;
    private bool isPaused;

    private bool warningPulseActive;

    private Vector3 normalPulseScale;

    private bool isInitialized;

    public event Action TimerExpired;

    public float RemainingTime =>
        remainingTime;

    public bool IsRunning =>
        isRunning;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        EnsureInitialized();

        if (startOnPlayForTesting)
        {
            StartTimer(
                testDuration
            );
        }
        else
        {
            SetFill(1f);
            StopWarningPulse();
        }
    }

    private void OnDisable()
    {
        StopWarningPulse();
    }

    private void Update()
    {
        if (!isRunning ||
            isPaused)
        {
            return;
        }

        remainingTime -=
            Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            SetFill(0f);

            StopWarningPulse();

            TimerExpired?.Invoke();

            return;
        }

        float normalizedTime =
            remainingTime /
            duration;

        SetFill(
            normalizedTime
        );

        UpdateWarningPulse(
            normalizedTime
        );
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void EnsureInitialized()
    {
        if (isInitialized)
            return;

        if (pulseTarget == null &&
            timerFill != null)
        {
            pulseTarget =
                timerFill.rectTransform;
        }

        if (pulseTarget != null)
        {
            normalPulseScale =
                pulseTarget.localScale;
        }
        else
        {
            normalPulseScale =
                Vector3.one;
        }

        isInitialized = true;
    }

    // =========================================================
    // TIMER CONTROL
    // =========================================================

    public void StartTimer(
        float seconds)
    {
        EnsureInitialized();

        duration =
            Mathf.Max(
                seconds,
                0.01f
            );

        remainingTime =
            duration;

        isRunning = true;
        isPaused = false;

        SetFill(1f);

        StopWarningPulse();
    }

    public void StopTimer()
    {
        EnsureInitialized();

        isRunning = false;
        isPaused = false;

        StopWarningPulse();
    }

    public void PauseTimer()
    {
        EnsureInitialized();

        isPaused = true;

        StopWarningPulse();
    }

    public void ResumeTimer()
    {
        EnsureInitialized();

        if (remainingTime > 0f)
        {
            isPaused = false;
        }
    }

    public void ResetTimer()
    {
        EnsureInitialized();

        isRunning = false;
        isPaused = false;

        remainingTime =
            duration;

        SetFill(1f);

        StopWarningPulse();
    }

    // =========================================================
    // FILL
    // =========================================================

    private void SetFill(
        float amount)
    {
        if (timerFill != null)
        {
            timerFill.fillAmount =
                Mathf.Clamp01(
                    amount
                );
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

        if (normalizedTime >
            warningThreshold)
        {
            StopWarningPulse();

            return;
        }

        StartWarningPulse();

        float pulse =
            (
                Mathf.Sin(
                    Time.time *
                    warningPulseSpeed
                ) + 1f
            ) * 0.5f;

        float scale =
            Mathf.Lerp(
                1f,
                warningPulseScale,
                pulse
            );

        pulseTarget.localScale =
            normalPulseScale *
            scale;
    }

    private void StartWarningPulse()
    {
        if (warningPulseActive)
        {
            return;
        }

        warningPulseActive = true;

        if (minigameAudio != null)
        {
            minigameAudio
                .StartFastTicking();
        }
    }

    private void StopWarningPulse()
    {
        EnsureInitialized();

        warningPulseActive = false;

        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                normalPulseScale;
        }

        if (minigameAudio != null)
        {
            minigameAudio
                .StopFastTicking();
        }
    }
}