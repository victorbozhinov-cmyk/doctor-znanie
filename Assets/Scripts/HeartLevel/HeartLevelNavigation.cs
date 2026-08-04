using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartLevelNavigation : MonoBehaviour
{
    private const string LockDifficultyKey =
        "LockDifficultyInSettings";

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationOverlay;
    [SerializeField] private UIPopupAnimation exitConfirmationAnimation;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoOverlay;
    [SerializeField] private UIPopupAnimation infoPanelAnimation;

    public void BackToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    public void OpenSettings()
    {
        PlayerPrefs.SetInt(LockDifficultyKey, 1);
        PlayerPrefs.Save();

        SettingsNavigation.OpenSettings();
    }

    // =========================
    // Exit Panel
    // =========================

    public void OpenExitConfirmation()
    {
        if (exitConfirmationOverlay == null)
        {
            Debug.LogError(
                "Exit Confirmation Overlay не е свързан."
            );

            return;
        }

        exitConfirmationOverlay.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationOverlay == null)
        {
            return;
        }

        if (exitConfirmationAnimation != null)
        {
            exitConfirmationAnimation.PlayClose(
                () => exitConfirmationOverlay.SetActive(false)
            );
        }
        else
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
        if (infoOverlay == null)
        {
            Debug.LogError(
                "Info Overlay не е свързан."
            );

            return;
        }

        infoOverlay.SetActive(true);
    }

    public void CloseInfoPanel()
    {
        if (infoOverlay == null)
        {
            return;
        }

        if (infoPanelAnimation != null)
        {
            infoPanelAnimation.PlayClose(
                () => infoOverlay.SetActive(false)
            );
        }
        else
        {
            infoOverlay.SetActive(false);
        }
    }
}