using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartLevelNavigation : MonoBehaviour
{
    private enum InfoReturnTarget
    {
        Gameplay,
        PuzzleWelcome
    }

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationOverlay;
    [SerializeField] private UIPopupAnimation exitConfirmationAnimation;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoOverlay;
    [SerializeField] private UIPopupAnimation infoPanelAnimation;

    [Header("Puzzle Welcome")]
    [SerializeField] private PuzzleWelcomeController puzzleWelcomeController;

    private InfoReturnTarget infoReturnTarget =
        InfoReturnTarget.Gameplay;

    // =========================================================
    // BODY MAP
    // =========================================================

    public void BackToBodyMap()
    {
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
    // EXIT PANEL
    // =========================================================

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
                () =>
                    exitConfirmationOverlay.SetActive(false)
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

    // =========================================================
    // NORMAL INFO
    // =========================================================

    public void OpenInfoPanel()
    {
        infoReturnTarget =
            InfoReturnTarget.Gameplay;

        ShowInfoPanel();
    }

    // =========================================================
    // INFO FROM PUZZLE WELCOME
    // =========================================================

    public void OpenInfoFromPuzzleWelcome()
    {
        if (puzzleWelcomeController == null)
        {
            Debug.LogError(
                "PuzzleWelcomeController не е свързан " +
                "в HeartLevelNavigation."
            );

            return;
        }

        infoReturnTarget =
            InfoReturnTarget.PuzzleWelcome;

        puzzleWelcomeController.HideWelcomeForInfo(
            ShowInfoPanel
        );
    }

    // =========================================================
    // SHOW INFO
    // =========================================================

    private void ShowInfoPanel()
    {
        if (infoOverlay == null)
        {
            Debug.LogError(
                "Info Overlay не е свързан."
            );

            return;
        }

        infoOverlay.SetActive(true);
        infoOverlay.transform.SetAsLastSibling();

        if (infoPanelAnimation != null)
        {
            infoPanelAnimation.PlayOpen();
        }
    }

    // =========================================================
    // CLOSE INFO
    // =========================================================

    public void CloseInfoPanel()
    {
        if (infoOverlay == null)
        {
            return;
        }

        if (infoPanelAnimation != null)
        {
            infoPanelAnimation.PlayClose(
                FinishClosingInfoPanel
            );
        }
        else
        {
            FinishClosingInfoPanel();
        }
    }

    private void FinishClosingInfoPanel()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (infoReturnTarget ==
            InfoReturnTarget.PuzzleWelcome)
        {
            if (puzzleWelcomeController != null)
            {
                puzzleWelcomeController
                    .ShowWelcomeAfterInfo();
            }
        }

        infoReturnTarget =
            InfoReturnTarget.Gameplay;
    }
}