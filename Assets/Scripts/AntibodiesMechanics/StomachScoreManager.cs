using UnityEngine;

public class StomachScoreManager : MonoBehaviour
{
    public static StomachScoreManager Instance { get; private set; }

    // =========================================================
    // SCORE WEIGHTS
    // =========================================================

    [Header("Score Weights")]
    [SerializeField, Range(0f, 1f)]
    private float puzzleWeight = 0.30f;

    [SerializeField, Range(0f, 1f)]
    private float minigameWeight = 0.30f;

    [SerializeField, Range(0f, 1f)]
    private float quizWeight = 0.40f;

    // =========================================================
    // QUIZ
    // =========================================================

    [Header("Quiz Calculation")]
    [SerializeField, Range(0f, 1f)]
    private float quizLivesWeight = 0.80f;

    [SerializeField, Range(0f, 1f)]
    private float quizHintsWeight = 0.20f;

    // =========================================================
    // MINIGAME
    // =========================================================

    [Header("Minigame Time Calculation")]

    [Tooltip(
        "Минималната част от Minigame точките, " +
        "която се получава само за успешно завършване."
    )]
    [SerializeField, Range(0f, 1f)]
    private float minigameBasePerformance = 0.50f;

    [Tooltip(
        "Ако играчът има поне този процент от времето, " +
        "получава 100% Minigame резултат."
    )]
    [SerializeField, Range(0.01f, 1f)]
    private float perfectTimeThreshold = 0.50f;

    // =========================================================
    // CURRENT RUN
    // =========================================================

    private float puzzlePerformance;
    private float minigamePerformance;
    private float quizPerformance;

    private bool puzzleCompleted;
    private bool minigameCompleted;
    private bool quizCompleted;

    private int currentScore;
    private bool isNewBest;

    // =========================================================
    // PUBLIC VALUES
    // =========================================================

    public float PuzzlePerformance =>
        puzzlePerformance;

    public float MinigamePerformance =>
        minigamePerformance;

    public float QuizPerformance =>
        quizPerformance;

    public int CurrentScore =>
        currentScore;

    public bool IsNewBest =>
        isNewBest;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================================================
    // PUZZLE
    // =========================================================

    public void SubmitPuzzleResult(
        int remainingLives,
        int startingLives)
    {
        if (startingLives <= 0)
        {
            puzzlePerformance = 0f;
            puzzleCompleted = true;

            return;
        }

        puzzlePerformance =
            Mathf.Clamp01(
                (float)remainingLives /
                startingLives
            );

        puzzleCompleted = true;

        Debug.Log(
            $"Stomach Puzzle Performance: " +
            $"{puzzlePerformance:P0}"
        );
    }

    // =========================================================
    // MINIGAME
    // =========================================================

    public void SubmitMinigameResult(
        float remainingTime,
        float startingTime)
    {
        if (startingTime <= 0f)
        {
            minigamePerformance =
                minigameBasePerformance;

            minigameCompleted = true;

            return;
        }

        float remainingTimePercent =
            Mathf.Clamp01(
                remainingTime /
                startingTime
            );

        // Ако са останали поне 50% от началното време,
        // играчът получава максимален Minigame резултат.
        if (remainingTimePercent >=
            perfectTimeThreshold)
        {
            minigamePerformance = 1f;
        }
        else
        {
            // Преобразуваме оставащото време
            // от диапазона 0% - 50%
            // в стойност 0 - 1.
            float normalizedTime =
                Mathf.Clamp01(
                    remainingTimePercent /
                    perfectTimeThreshold
                );

            // 50% базов резултат +
            // до още 50% според времето.
            minigamePerformance =
                Mathf.Lerp(
                    minigameBasePerformance,
                    1f,
                    normalizedTime
                );
        }

        minigamePerformance =
            Mathf.Clamp01(
                minigamePerformance
            );

        minigameCompleted = true;

        Debug.Log(
            $"Stomach Minigame | " +
            $"Remaining Time: " +
            $"{remainingTime:0.0}/{startingTime:0.0} | " +
            $"Performance: " +
            $"{minigamePerformance:P0}"
        );
    }

    // =========================================================
    // QUIZ
    // =========================================================

    public void SubmitQuizResult(
        int remainingLives,
        int startingLives,
        int remainingHints,
        int startingHints)
    {
        float livesPerformance = 0f;
        float hintsPerformance = 0f;

        if (startingLives > 0)
        {
            livesPerformance =
                Mathf.Clamp01(
                    (float)remainingLives /
                    startingLives
                );
        }

        if (startingHints > 0)
        {
            hintsPerformance =
                Mathf.Clamp01(
                    (float)remainingHints /
                    startingHints
                );
        }

        quizPerformance =
            (livesPerformance *
             quizLivesWeight)
            +
            (hintsPerformance *
             quizHintsWeight);

        quizPerformance =
            Mathf.Clamp01(
                quizPerformance
            );

        quizCompleted = true;

        Debug.Log(
            $"Stomach Quiz | " +
            $"Lives: {remainingLives}/{startingLives} | " +
            $"Hints: {remainingHints}/{startingHints} | " +
            $"Performance: {quizPerformance:P0}"
        );
    }

    // =========================================================
    // FINAL SCORE
    // =========================================================

    public int CalculateFinalScore()
    {
        float overallPerformance =
            (puzzlePerformance *
             puzzleWeight)
            +
            (minigamePerformance *
             minigameWeight)
            +
            (quizPerformance *
             quizWeight);

        overallPerformance =
            Mathf.Clamp01(
                overallPerformance
            );

        int maxScore =
            GetMaxScoreForCurrentDifficulty();

        currentScore =
            Mathf.RoundToInt(
                maxScore *
                overallPerformance
            );

        return currentScore;
    }

    public void FinishStomachLevel()
    {
        currentScore =
            CalculateFinalScore();

        if (AntibodyManager.Instance == null)
        {
            Debug.LogWarning(
                "AntibodyManager.Instance липсва. " +
                "Stomach score не може да бъде записан."
            );

            return;
        }

        isNewBest =
            AntibodyManager.Instance.SubmitScore(
                AntibodyManager.OrganType.Stomach,
                currentScore
            );

        Debug.Log(
            $"Stomach Current Score: {currentScore} | " +
            $"Stomach Best Score: " +
            $"{AntibodyManager.Instance.StomachBestScore} | " +
            $"New Best: {isNewBest}"
        );
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private int GetMaxScoreForCurrentDifficulty()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                return 1000;

            case 1:
                return 1500;

            case 2:
                return 2000;

            default:
                return 1500;
        }
    }

    public int GetStartingLivesForCurrentDifficulty()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                return 3;

            case 1:
                return 2;

            case 2:
                return 1;

            default:
                return 2;
        }
    }

    public int GetStartingHintsForCurrentDifficulty()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                return 3;

            case 1:
                return 2;

            case 2:
                return 1;

            default:
                return 2;
        }
    }

    // =========================================================
    // TESTING / RESET
    // =========================================================

    public void ResetRunScore()
    {
        puzzlePerformance = 0f;
        minigamePerformance = 0f;
        quizPerformance = 0f;

        puzzleCompleted = false;
        minigameCompleted = false;
        quizCompleted = false;

        currentScore = 0;
        isNewBest = false;
    }

    public bool AreAllSectionsCompleted()
    {
        return
            puzzleCompleted &&
            minigameCompleted &&
            quizCompleted;
    }
}
