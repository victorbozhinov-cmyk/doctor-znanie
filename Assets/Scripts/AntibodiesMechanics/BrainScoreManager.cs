using UnityEngine;

public class BrainScoreManager : MonoBehaviour
{
    public static BrainScoreManager Instance { get; private set; }

    [Header("Score Weights")]
    [SerializeField, Range(0f, 1f)] private float puzzleWeight = 0.30f;
    [SerializeField, Range(0f, 1f)] private float minigameWeight = 0.30f;
    [SerializeField, Range(0f, 1f)] private float quizWeight = 0.40f;

    [Header("Quiz Calculation")]
    [SerializeField, Range(0f, 1f)] private float quizLivesWeight = 0.80f;
    [SerializeField, Range(0f, 1f)] private float quizHintsWeight = 0.20f;

    private float puzzlePerformance;
    private float minigamePerformance;
    private float quizPerformance;

    private bool puzzleCompleted;
    private bool minigameCompleted;
    private bool quizCompleted;

    private int currentScore;
    private bool isNewBest;

    public float PuzzlePerformance => puzzlePerformance;
    public float MinigamePerformance => minigamePerformance;
    public float QuizPerformance => quizPerformance;

    public int CurrentScore => currentScore;
    public bool IsNewBest => isNewBest;

    private void Awake()
    {
        if (Instance != null && Instance != this)
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

        puzzlePerformance = Mathf.Clamp01(
            (float)remainingLives / startingLives
        );

        puzzleCompleted = true;
    }

    // =========================================================
    // MINIGAME
    // =========================================================

    public void SubmitMinigameResult(
        float currentNervousTension,
        float maxNervousTension)
    {
        if (maxNervousTension <= 0f)
        {
            minigamePerformance = 0f;
            minigameCompleted = true;
            return;
        }

        float tensionPerformance =
            Mathf.Clamp01(
                currentNervousTension /
                maxNervousTension
            );

        minigamePerformance =
            1f - tensionPerformance;

        minigamePerformance =
            Mathf.Clamp01(
                minigamePerformance
            );

        minigameCompleted = true;
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
            (livesPerformance * quizLivesWeight) +
            (hintsPerformance * quizHintsWeight);

        quizPerformance =
            Mathf.Clamp01(
                quizPerformance
            );

        quizCompleted = true;
    }

    // =========================================================
    // FINAL SCORE
    // =========================================================

    public int CalculateFinalScore()
    {
        float overallPerformance =
            (puzzlePerformance * puzzleWeight) +
            (minigamePerformance * minigameWeight) +
            (quizPerformance * quizWeight);

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

    public void FinishBrainLevel()
    {
        currentScore =
            CalculateFinalScore();

        if (AntibodyManager.Instance == null)
        {
            Debug.LogWarning(
                "AntibodyManager.Instance липсва. " +
                "Brain score не може да бъде записан."
            );

            return;
        }

        isNewBest =
            AntibodyManager.Instance.SubmitScore(
                AntibodyManager.OrganType.Brain,
                currentScore
            );

        Debug.Log(
            $"Brain Current Score: {currentScore} | " +
            $"Brain Best Score: " +
            $"{AntibodyManager.Instance.BrainBestScore} | " +
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
        return puzzleCompleted &&
               minigameCompleted &&
               quizCompleted;
    }
}