using UnityEngine;

public class LiverMinigamePauseController : MonoBehaviour
{
    [Header("Main Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Optional Sub Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;

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
    // SAFETY
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}