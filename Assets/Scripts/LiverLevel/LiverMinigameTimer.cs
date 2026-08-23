using UnityEngine;
using TMPro;

public class LiverMinigameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Game References")]
    [SerializeField] private LiverHealthController liverHealthController;
    [SerializeField] private GameObject minigameSuccessPanel;

    [Header("Time By Difficulty")]
    [SerializeField] private float easyTime = 60f;
    [SerializeField] private float mediumTime = 75f;
    [SerializeField] private float hardTime = 90f;

    private float remainingTime;
    private bool timerRunning;
    private bool finished = false;

    public bool TimerRunning => timerRunning;
    public float RemainingTime => remainingTime;

    private void Start()
    {
        // Success панелът винаги започва скрит
        if (minigameSuccessPanel != null)
        {
            minigameSuccessPanel.SetActive(false);
        }

        SetTimeFromDifficulty();

        timerRunning = true;
        finished = false;

        UpdateTimerText();
    }

    private void Update()
    {
        if (!timerRunning || finished)
            return;

        // Ако вече сме загубили, таймерът спира
        // и SuccessPanel НЯМА да се появи.
        if (liverHealthController != null &&
            liverHealthController.IsGameOver)
        {
            timerRunning = false;
            return;
        }

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
        if (finished)
            return;

        // Допълнителна проверка:
        // ако сме загубили, няма победа.
        if (liverHealthController != null &&
            liverHealthController.IsGameOver)
        {
            return;
        }

        finished = true;

        Debug.Log("МИСИЯТА Е ИЗПЪЛНЕНА!");

        if (minigameSuccessPanel != null)
        {
            minigameSuccessPanel.SetActive(true);
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f && !finished)
        {
            timerRunning = true;
        }
    }
}