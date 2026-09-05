using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverMinigamePauseController : MonoBehaviour
{
    [Header("Main Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Optional Sub Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationPanel;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        /*
         * НЕ задаваме Time.timeScale = 1 тук.
         *
         * Причината е, че минииграта може да бъде
         * активирана зад Welcome или Info панел,
         * докато Time.timeScale трябва да остане 0.
         */

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }
    }

    // =========================================================
    // OPEN PAUSE
    // =========================================================

    public void OpenPause()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================================================
    // CONTINUE
    // =========================================================

    public void ContinueGame()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;

        Time.timeScale = 1f;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isPaused)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================================================
    // INFO
    // =========================================================

    public void OpenInfo()
    {
        if (!isPaused)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public void CloseInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
        }
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Не променяме timeScale.
        // Играта продължава да е паузирана.
    }

    public void ConfirmExitToBodyMap()
    {
        // Връщаме времето преди смяна на сцената,
        // за да не остане BodyMap замразен.
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // SAFETY
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}