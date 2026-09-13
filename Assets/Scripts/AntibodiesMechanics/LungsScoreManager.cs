using UnityEngine;

public class LungsScoreManager : MonoBehaviour
{
    public static LungsScoreManager Instance { get; private set; }

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
    // QUIZ CALCULATION
    // =========================================================

    [Header("Quiz Calculation")]
    [SerializeField, Range(0f, 1f)]
    private float quizLivesWeight = 0.80f;

    [SerializeField, Range(0f, 1f)]
    private float quizHintsWeight = 0.20f;

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
    // PUBLIC
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
    // UNITY
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
            $"Lungs Puzzle Performance: " +
            $"{puzzlePerformance:P0}"
        );
    }

    // =========================================================
    // MINIGAME
    // =========================================================

    public void SubmitMinigameCompleted()
    {
        // Минииграта е pass/fail.
        // При успешно завършване получава
        // пълните 100% за своята част.

        minigamePerformance = 1f;
        minigameCompleted = true;

        Debug.Log(
            "Lungs Minigame Performance: 100%"
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
            $"Lungs Quiz | " +
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

        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        int minScore;
        int maxScore;

        switch (difficulty)
        {
            case 0:
                minScore = 0;
                maxScore = 1000;
                break;

            case 1:
                minScore = 1001;
                maxScore = 1500;
                break;

            case 2:
                minScore = 1501;
                maxScore = 2000;
                break;

            default:
                minScore = 1001;
                maxScore = 1500;
                break;
        }

        currentScore =
            Mathf.RoundToInt(
                Mathf.Lerp(
                    minScore,
                    maxScore,
                    overallPerformance
                )
            );

        return currentScore;
    }

    // =========================================================
    // FINISH LEVEL
    // =========================================================

    public void FinishLungsLevel()
    {
        currentScore =
            CalculateFinalScore();

        if (AntibodyManager.Instance == null)
        {
            Debug.LogWarning(
                "AntibodyManager.Instance липсва. " +
                "Lungs score не може да бъде записан."
            );

            return;
        }

        isNewBest =
            AntibodyManager.Instance.SubmitScore(
                AntibodyManager.OrganType.Lungs,
                currentScore
            );

        Debug.Log(
            $"Lungs Current Score: {currentScore} | " +
            $"Lungs Best Score: " +
            $"{AntibodyManager.Instance.LungsBestScore} | " +
            $"New Best: {isNewBest}"
        );
    }

    // =========================================================
    // RESET
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
