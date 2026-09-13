using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsLevelManager : MonoBehaviour
{
    // =========================================================
    // PANELS
    // =========================================================

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Gameplay Panels")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject puzzleSuccessPanel;

    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject minigameWelcomePanel;

    [SerializeField] private GameObject quizPanel;

    [SerializeField] private GameObject finishPanel;

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "SettingsPanel не е зададен в LungsLevelManager."
            );
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // =========================================================
    // OPEN PUZZLE
    // =========================================================

    public void OpenPuzzle()
    {
        Time.timeScale = 1f;

        if (puzzleSuccessPanel != null)
        {
            puzzleSuccessPanel.SetActive(false);
        }

        if (minigameWelcomePanel != null)
        {
            minigameWelcomePanel.SetActive(false);
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
            puzzlePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "PuzzlePanel не е зададен в LungsLevelManager."
            );
        }
    }

    // =========================================================
    // OPEN MINIGAME
    // =========================================================

    public void OpenMinigame()
    {
        Time.timeScale = 1f;

        if (puzzleSuccessPanel != null)
        {
            puzzleSuccessPanel.SetActive(false);
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
        else
        {
            Debug.LogWarning(
                "MinigamePanel не е зададен в LungsLevelManager."
            );
        }

        if (minigameWelcomePanel != null)
        {
            minigameWelcomePanel.SetActive(true);
        }
    }

    // =========================================================
    // OLD PUZZLE -> MINIGAME METHOD
    // =========================================================

    public void GoToMinigame()
    {
        OpenMinigame();
    }

    // =========================================================
    // OPEN FINISH PANEL
    // =========================================================

    public void OpenFinishPanel()
    {
        Time.timeScale = 1f;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (puzzleSuccessPanel != null)
        {
            puzzleSuccessPanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (minigameWelcomePanel != null)
        {
            minigameWelcomePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "FinishPanel не е зададен в LungsLevelManager."
            );
        }
    }

    // =========================================================
    // BODY MAP
    // =========================================================

    public void BackToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "BodyMap"
        );
    }

    public void GoToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "BodyMap"
        );
    }
}