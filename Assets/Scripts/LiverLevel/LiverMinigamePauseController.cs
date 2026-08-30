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
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    // =========================================================
    // OPEN PAUSE
    // =========================================================

    public void OpenPause()
    {
        if (isPaused)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    // =========================================================
    // CONTINUE
    // =========================================================

    public void ContinueGame()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        isPaused = false;

        Time.timeScale = 1f;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    // =========================================================
    // INFO
    // =========================================================

    public void OpenInfo()
    {
        if (!isPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    // =========================================================
    // SAFETY
    // =========================================================

    private void OnDestroy()
    {
        // Ако напуснем сцената, никога не оставяме играта
        // глобално замръзнала.
        Time.timeScale = 1f;
    }
}