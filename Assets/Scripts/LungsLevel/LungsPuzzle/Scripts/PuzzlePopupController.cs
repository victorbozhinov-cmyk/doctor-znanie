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
    // SUCCESS
    // =====================================================

    public void OpenSuccess()
    {
        if (successPanel == null)
            return;

        successPanel.SetActive(true);
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    public void OpenGameOver()
    {
        if (gameOverPanel == null)
            return;

        gameOverPanel.SetActive(true);
    }

    // =====================================================
    // RETRY LEVEL
    // =====================================================

    public void RetryPuzzle()
    {
        // За всеки случай връщаме нормалното време.
        Time.timeScale = 1f;

        // Презареждаме ЦЯЛАТА текуща сцена.
        //
        // Това reset-ва:
        // - Puzzle
        // - Minigame
        // - Quiz
        // - Welcome Panels
        // - всички локални manager-и
        // - всички runtime флагове
        //
        // Така след загуба започваме
        // Lungs Level напълно начисто.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}