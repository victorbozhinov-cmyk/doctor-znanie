using UnityEngine;

public class LungsPuzzlePopupController : MonoBehaviour
{
    [Header("Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject puzzlePanel;

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationPanel;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [Header("Success")]
    [SerializeField] private GameObject successPanel;
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private LungsPuzzleManager puzzleManager;

    // =====================================================
    // EXIT CONFIRMATION
    // =====================================================

    public void OpenExitConfirmation()
    {
        if (exitConfirmationPanel == null)
            return;

        exitConfirmationPanel.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel == null)
            return;

        exitConfirmationPanel.SetActive(false);
    }

    // =====================================================
    // SETTINGS
    // =====================================================

    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);
    }

    // =====================================================
    // GAME OVER
    // =====================================================
    public void OpenSuccess()
    {
        if (successPanel == null)
            return;

        successPanel.SetActive(true);
    }
    public void OpenGameOver()
    {
        if (gameOverPanel == null)
            return;

        gameOverPanel.SetActive(true);
    }

    // =====================================================
    // RETRY
    // =====================================================

    public void RetryPuzzle()
    {
        // Скриваме Game Over панела.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Нулираме пъзела, за да бъде готов,
        // когато играчът стигне отново до него.
        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle();
        }

        // Скриваме самия Puzzle Panel.
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Връщаме играча в началото на нивото.
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }
}