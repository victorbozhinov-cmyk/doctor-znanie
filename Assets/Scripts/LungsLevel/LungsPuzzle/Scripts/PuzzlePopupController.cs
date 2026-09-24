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
    // RETRY FLAG
    // =====================================================

    // Остава true през презареждането на сцената.
    private static bool openStartPanelAfterReload = false;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        // Ако сцената е заредена от бутона "Опитай пак",
        // насила връщаме играча в началния екран.
        if (openStartPanelAfterReload)
        {
            openStartPanelAfterReload = false;

            Time.timeScale = 1f;

            if (startPanel != null)
                startPanel.SetActive(true);

            if (puzzlePanel != null)
                puzzlePanel.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (successPanel != null)
                successPanel.SetActive(false);

            if (exitConfirmationPanel != null)
                exitConfirmationPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
    }

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
        Time.timeScale = 1f;

        // Казваме на новозаредената сцена,
        // че трябва да започне от StartPanel.
        openStartPanelAfterReload = true;

        // Reload на целия LungsLevel:
        // reset-ва Puzzle, Minigame, Quiz,
        // животи, позиции и runtime флагове.
        SceneManager.LoadScene("LungsLevel");
    }
}