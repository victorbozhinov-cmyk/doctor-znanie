using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainLevelNavigation : MonoBehaviour
{
    [Header("Brain Puzzle")]
    [SerializeField] private BrainPuzzleManager puzzleManager;

    [Header("Main Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Success")]
    [SerializeField] private GameObject puzzleSuccessOverlay;

    [Header("Scenes")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    private const string BrainRetryFromQuizKey =
        "BrainRetryFromQuiz";

    private void Start()
    {
        bool retryFromQuiz =
            PlayerPrefs.GetInt(
                BrainRetryFromQuizKey,
                0
            ) == 1;

        if (retryFromQuiz)
        {
            PlayerPrefs.SetInt(
                BrainRetryFromQuizKey,
                0
            );

            PlayerPrefs.Save();

            ShowStartPanelOnly();
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }
    }

    // -------------------------
    // START PANEL -> PUZZLE
    // -------------------------

    public void StartBrainPuzzle()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle();
        }
    }

    // -------------------------
    // GAME OVER
    // -------------------------

    public void ShowGameOver()
    {
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // -------------------------
    // RETRY PUZZLE
    // -------------------------

    public void RetryPuzzle()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ResetPuzzle();
            puzzleManager.UnlockDifficulty();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    // -------------------------
    // RETRY FROM QUIZ
    // -------------------------

    public void RetryBrainLevel()
    {
        PlayerPrefs.SetInt(
            BrainRetryFromQuizKey,
            1
        );

        PlayerPrefs.Save();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // -------------------------
    // SHOW START PANEL
    // -------------------------

    private void ShowStartPanelOnly()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }
    }

    // -------------------------
    // PUZZLE SUCCESS
    // -------------------------

    public void ShowPuzzleSuccess()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(true);
        }
    }

    // -------------------------
    // PUZZLE -> MINIGAME
    // -------------------------

    public void OpenMinigame()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }
    }

    // -------------------------
    // QUIZ -> FINISH PANEL
    // -------------------------

    public void OpenFinishPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // -------------------------
    // EXIT
    // -------------------------

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene(
            bodyMapSceneName
        );
    }
}