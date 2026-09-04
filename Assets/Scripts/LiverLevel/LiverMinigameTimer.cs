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
    public bool IsFinished => finished;

    private void Start()
    {
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

        // Ако вече сме загубили,
        // SuccessPanel не трябва да се появява.
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
            PlayerPrefs.GetString(
                "Difficulty",
                ""
            ).ToLower();

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

        int difficultyInt =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

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

    // =========================================================
    // TIME PENALTY
    // =========================================================

    public void AddTime(float seconds)
    {
        if (finished)
            return;

        if (liverHealthController != null &&
            liverHealthController.IsGameOver)
        {
            return;
        }

        if (seconds <= 0f)
            return;

        remainingTime += seconds;

        UpdateTimerText();
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    private void TimerFinished()
    {
        if (finished)
            return;

        if (liverHealthController != null &&
            liverHealthController.IsGameOver)
        {
            return;
        }

        finished = true;
        timerRunning = false;

        Debug.Log("МИСИЯТА Е ИЗПЪЛНЕНА!");

        // Спираме ЦЯЛАТА миниигра.
        Time.timeScale = 0f;

        if (minigameSuccessPanel != null)
        {
            minigameSuccessPanel.SetActive(true);
            minigameSuccessPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError(
                "MinigameSuccessPanel не е зададен!"
            );
        }
    }

    // =========================================================
    // TIMER CONTROL
    // =========================================================

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        if (remainingTime > 0f &&
            !finished)
        {
            timerRunning = true;
        }
    }

    // =========================================================
    // CONTINUE AFTER SUCCESS
    // =========================================================

    public void ContinueAfterSuccess()
    {
        Time.timeScale = 1f;
    }

    // =========================================================
    // SAFETY
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}