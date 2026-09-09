using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainLevelNavigation : MonoBehaviour
{
    [Header("Brain Puzzle")]
    [SerializeField] private BrainPuzzleManager puzzleManager;

    [Header("Main Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
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

    // =========================================================
    // UNITY
    // =========================================================

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

    // =========================================================
    // START PANEL -> VIDEO
    // =========================================================

    public void StartLevel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
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

        Debug.Log(
            "Brain Level: StartPanel -> VideoPanel"
        );
    }

    // =========================================================
    // VIDEO -> PUZZLE WELCOME
    // =========================================================

    public void ContinueFromVideoToPuzzle()
    {
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
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

        if (puzzlePanel != null)
        {
            /*
             * Само активираме PuzzlePanel.
             *
             * BrainPuzzleManager ще е подготвен,
             * а PuzzleWelcomeController ще покаже
             * Welcome Overlay-а.
             *
             * Реалният пъзел НЕ стартира тук.
             */
            puzzlePanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "PuzzlePanel не е свързан " +
                "в BrainLevelNavigation."
            );
        }

        Debug.Log(
            "Brain Level: Video -> Puzzle Welcome"
        );
    }

    // =========================================================
    // GAME OVER
    // =========================================================

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

    // =========================================================
    // RETRY PUZZLE
    // =========================================================

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

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    // =========================================================
    // RETRY FROM QUIZ
    // =========================================================

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

    // =========================================================
    // SHOW START PANEL
    // =========================================================

    private void ShowStartPanelOnly()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
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

    // =========================================================
    // PUZZLE SUCCESS
    // =========================================================

    public void ShowPuzzleSuccess()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(true);
        }
    }

    // =========================================================
    // PUZZLE -> MINIGAME
    // =========================================================

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

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
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

    // =========================================================
    // QUIZ -> FINISH
    // =========================================================

    public void OpenFinishPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
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

    // =========================================================
    // EXIT
    // =========================================================

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene(
            bodyMapSceneName
        );
    }
}