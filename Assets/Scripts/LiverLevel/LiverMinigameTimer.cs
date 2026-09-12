using UnityEngine;
using TMPro;

public class LiverMinigameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Game References")]
    [SerializeField] private LiverHealthController liverHealthController;
    [SerializeField] private LiverMinigamePenaltyController penaltyController;
    [SerializeField] private GameObject minigameSuccessPanel;

    [Header("Time By Difficulty")]
    [SerializeField] private float easyTime = 60f;
    [SerializeField] private float mediumTime = 75f;
    [SerializeField] private float hardTime = 90f;

    private float remainingTime;
    private float startingTime;

    private bool timerRunning;
    private bool finished = false;

    public bool TimerRunning => timerRunning;
    public float RemainingTime => remainingTime;
    public float StartingTime => startingTime;
    public bool IsFinished => finished;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        if (minigameSuccessPanel != null)
        {
            minigameSuccessPanel.SetActive(false);
        }

        SetTimeFromDifficulty();

        if (penaltyController != null)
        {
            penaltyController.ResetPenaltyTracking();
        }

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

    // =========================================================
    // DIFFICULTY / START TIME
    // =========================================================

    private void SetTimeFromDifficulty()
    {
        string difficultyString =
            PlayerPrefs.GetString(
                "Difficulty",
                ""
            ).ToLower();

        if (difficultyString == "easy")
        {
            startingTime = easyTime;
            remainingTime = startingTime;
            return;
        }

        if (difficultyString == "medium")
        {
            startingTime = mediumTime;
            remainingTime = startingTime;
            return;
        }

        if (difficultyString == "hard")
        {
            startingTime = hardTime;
            remainingTime = startingTime;
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
                startingTime = easyTime;
                break;

            case 2:
                startingTime = hardTime;
                break;

            default:
                startingTime = mediumTime;
                break;
        }

        remainingTime = startingTime;
    }

    // =========================================================
    // UI
    // =========================================================

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

        SubmitMinigameScore();

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
    // SCORE
    // =========================================================

    private void SubmitMinigameScore()
    {
        if (LiverScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "LiverScoreManager.Instance липсва. " +
                "Резултатът от Liver Minigame не беше записан."
            );

            return;
        }

        if (liverHealthController == null)
        {
            Debug.LogWarning(
                "LiverHealthController не е свързан. " +
                "Резултатът от Liver Minigame не беше записан."
            );

            return;
        }

        float totalPenaltyTime = 0f;

        if (penaltyController != null)
        {
            totalPenaltyTime =
                penaltyController.TotalPenaltyTime;
        }
        else
        {
            Debug.LogWarning(
                "LiverMinigamePenaltyController не е свързан. " +
                "Penalty Time ще бъде отчетено като 0."
            );
        }

        LiverScoreManager.Instance.SubmitMinigameResult(
            liverHealthController.CurrentVisualStateIndex,
            totalPenaltyTime,
            startingTime
        );

        Debug.Log(
            $"Liver Minigame Score submitted | " +
            $"Liver State: " +
            $"{liverHealthController.CurrentVisualStateIndex}/4 | " +
            $"Penalty Time: {totalPenaltyTime:0.0}s | " +
            $"Starting Time: {startingTime:0.0}s"
        );
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