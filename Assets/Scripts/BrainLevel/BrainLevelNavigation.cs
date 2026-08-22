using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainLevelNavigation : MonoBehaviour
{
    [Header("Brain Puzzle")]
    [SerializeField] private BrainPuzzleManager puzzleManager;

    [Header("Main Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject puzzlePanel;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Success")]
    [SerializeField] private GameObject puzzleSuccessOverlay;

    [Header("Scenes")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    private void Start()
    {
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

    // START PANEL -> PUZZLE
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

        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle();
        }
    }

    // GAME OVER
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

    // ОПИТАЙ ПАК
    // Не започва пъзела веднага.
    // Връща играча на StartPanel.
    public void RetryPuzzle()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ResetPuzzle();
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

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    public void ShowPuzzleSuccess()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(true);
        }
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene(bodyMapSceneName);
    }
}