using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsLevelNavigation : MonoBehaviour
{
    [Header("Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    private static bool returnToStartAfterReload = false;

    private void Awake()
    {
        if (returnToStartAfterReload)
        {
            returnToStartAfterReload = false;

            ShowStartPanel();
        }
    }

    // =========================================================
    // BODY MAP
    // =========================================================

    public void BackToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        SettingsNavigation.OpenSettings();
    }

    // =========================================================
    // START -> VIDEO
    // =========================================================

    public void OpenVideoPanel()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
            videoPanel.transform.SetAsLastSibling();
        }
    }

    // =========================================================
    // GAME OVER -> START PANEL
    // =========================================================

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        returnToStartAfterReload = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // =========================================================
    // SHOW START PANEL
    // =========================================================

    private void ShowStartPanel()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (videoPanel != null)
            videoPanel.SetActive(false);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(false);

        if (startPanel != null)
            startPanel.transform.SetAsLastSibling();
    }
}