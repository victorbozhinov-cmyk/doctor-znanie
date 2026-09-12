using UnityEngine;

public class LiverScoreManager : MonoBehaviour
{
    public static LiverScoreManager Instance { get; private set; }

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
    // MINIGAME CALCULATION
    // =========================================================

    [Header("Minigame Calculation")]
    [SerializeField, Range(0f, 1f)]
    private float liverConditionWeight = 0.60f;

    [SerializeField, Range(0f, 1f)]
    private float penaltyTimeWeight = 0.40f;

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
            $"Liver Puzzle Performance: " +
            $"{puzzlePerformance:P0}"
        );
    }

    // =========================================================
    // MINIGAME
    // =========================================================

    public void SubmitMinigameResult(
        int finalLiverVisualState,
        float totalPenaltyTime,
        float startingTime)
    {
        // -----------------------------------------
        // LIVER CONDITION
        //
        // 0 = Very Sick     = 0%
        // 1 = Sick          = 25%
        // 2 = Neutral       = 50%
        // 3 = Healthy       = 75%
        // 4 = Very Healthy  = 100%
        // -----------------------------------------

        int clampedLiverState =
            Mathf.Clamp(
                finalLiverVisualState,
                0,
                4
            );

        float liverConditionPerformance =
            clampedLiverState / 4f;

        // -----------------------------------------
        // PENALTY TIME
        // -----------------------------------------

        float penaltyPerformance;

        if (startingTime <= 0f)
        {
            penaltyPerformance = 0f;
        }
        else
        {
            penaltyPerformance =
                1f -
                (
                    Mathf.Max(
                        0f,
                        totalPenaltyTime
                    )
                    /
                    startingTime
                );

            penaltyPerformance =
                Mathf.Clamp01(
                    penaltyPerformance
                );
        }

        // -----------------------------------------
        // FINAL MINIGAME PERFORMANCE
        // -----------------------------------------

        minigamePerformance =
            (liverConditionPerformance *
             liverConditionWeight)
            +
            (penaltyPerformance *
             penaltyTimeWeight);

        minigamePerformance =
            Mathf.Clamp01(
                minigamePerformance
            );

        minigameCompleted = true;

        Debug.Log(
            $"Liver Minigame | " +
            $"Liver State: {clampedLiverState}/4 | " +
            $"Condition: {liverConditionPerformance:P0} | " +
            $"Penalty Time: {totalPenaltyTime:0.0}s | " +
            $"Penalty Performance: {penaltyPerformance:P0} | " +
            $"Final Performance: {minigamePerformance:P0}"
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
            $"Liver Quiz | " +
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
            // EASY
            case 0:
                minScore = 0;
                maxScore = 1000;
                break;

            // MEDIUM
            case 1:
                minScore = 1001;
                maxScore = 1500;
                break;

            // HARD
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

    public void FinishLiverLevel()
    {
        currentScore =
            CalculateFinalScore();

        if (AntibodyManager.Instance == null)
        {
            Debug.LogWarning(
                "AntibodyManager.Instance липсва. " +
                "Liver score не може да бъде записан."
            );

            return;
        }

        isNewBest =
            AntibodyManager.Instance.SubmitScore(
                AntibodyManager.OrganType.Liver,
                currentScore
            );

        Debug.Log(
            $"Liver Current Score: {currentScore} | " +
            $"Liver Best Score: " +
            $"{AntibodyManager.Instance.LiverBestScore} | " +
            $"New Best: {isNewBest}"
        );
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

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