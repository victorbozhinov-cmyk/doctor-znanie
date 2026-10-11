
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // =====================================================
    // EXIT CONFIRMATION
    // =====================================================

    public void OpenExitConfirmation()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);
    }

    // =====================================================
    // SETTINGS
    // =====================================================

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // =====================================================
    // SUCCESS
    // =====================================================

    public void OpenSuccess()
    {
        if (successPanel != null)
            successPanel.SetActive(true);
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    public void OpenGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // =====================================================
    // RETRY WHOLE LEVEL
    // =====================================================

    public void RetryPuzzle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("LungsLevel");
    }
}
