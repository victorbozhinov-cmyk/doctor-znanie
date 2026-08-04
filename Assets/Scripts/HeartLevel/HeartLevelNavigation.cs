using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartLevelNavigation : MonoBehaviour
{
    private const string LockDifficultyKey =
        "LockDifficultyInSettings";

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationOverlay;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoOverlay;

    public void BackToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    public void OpenSettings()
    {
        // Казваме на Settings менюто, че е отворено
        // от вече започнато ниво.
        PlayerPrefs.SetInt(LockDifficultyKey, 1);
        PlayerPrefs.Save();

        SettingsNavigation.OpenSettings();
    }

    // =========================
    // Exit Panel
    // =========================

    public void OpenExitConfirmation()
    {
        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(true);
        }
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }
    }

    public void ExitLevel()
    {
        SceneManager.LoadScene("BodyMap");
    }

    // =========================
    // Info Panel
    // =========================

    public void OpenInfoPanel()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(true);
        }
    }

    public void CloseInfoPanel()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }
    }
}