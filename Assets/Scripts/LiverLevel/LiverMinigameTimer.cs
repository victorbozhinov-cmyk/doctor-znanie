using UnityEngine;
using TMPro;

public class LiverMinigameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Time By Difficulty")]
    [SerializeField] private float easyTime = 60f;
    [SerializeField] private float mediumTime = 75f;
    [SerializeField] private float hardTime = 90f;

    private float remainingTime;
    private bool timerRunning;

    public bool TimerRunning => timerRunning;
    public float RemainingTime => remainingTime;

    private void Start()
    {
        SetTimeFromDifficulty();

        timerRunning = true;

        UpdateTimerText();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            timerRunning = false;

            UpdateTimerText();

            TimerFinished();

            return;
        }

        UpdateTimerText();
    }

    private void SetTimeFromDifficulty()
    {
        string difficultyString =
            PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            remainingTime = easyTime;
            return;
        }

        if (difficultyString == "medium")
        {
            remainingTime = mediumTime;
            return;
        }

        if (difficultyString == "hard")
        {
            remainingTime = hardTime;
            return;
        }

        // Ако Difficulty се пази като int:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficultyInt =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficultyInt)
        {
            case 0:
                remainingTime = easyTime;
                break;

            case 2:
                remainingTime = hardTime;
                break;

            default:
                remainingTime = mediumTime;
                break;
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    private void TimerFinished()
    {
        Debug.Log("Таймерът свърши!");
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
            timerRunning = true;
    }
}