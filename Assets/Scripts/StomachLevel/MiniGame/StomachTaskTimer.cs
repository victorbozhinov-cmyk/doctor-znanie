using System;
using UnityEngine;
using UnityEngine.UI;

public class StomachTaskTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image timerFill;

    [Header("Test Settings")]
    [SerializeField] private float testDuration = 5f;
    [SerializeField] private bool startOnPlayForTesting = false;

    private float duration;
    private float remainingTime;
    private bool isRunning;
    private bool isPaused;

    public event Action TimerExpired;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    private void Start()
    {
        if (startOnPlayForTesting)
            StartTimer(testDuration);
        else
            SetFill(1f);
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

            TimerExpired?.Invoke();
            return;
        }

        SetFill(remainingTime / duration);
    }

    public void StartTimer(float seconds)
    {
        duration = Mathf.Max(seconds, 0.01f);
        remainingTime = duration;

        isRunning = true;
        isPaused = false;

        SetFill(1f);
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
            isPaused = false;
    }

    public void ResetTimer()
    {
        isRunning = false;
        isPaused = false;

        remainingTime = duration;

        SetFill(1f);
    }

    private void SetFill(float amount)
    {
        if (timerFill != null)
            timerFill.fillAmount = Mathf.Clamp01(amount);
    }
}