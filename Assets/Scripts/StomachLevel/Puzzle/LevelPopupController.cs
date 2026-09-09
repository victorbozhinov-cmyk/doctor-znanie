using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPopupController : MonoBehaviour
{
    [Header("Level Popups")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject exitConfirmPanel;
    [SerializeField] private GameObject levelSettingsPanel;

    [Header("Puzzle Welcome")]
    [SerializeField] private PuzzleWelcomeController puzzleWelcomeController;

    [Header("Navigation")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    private bool infoOpenedFromPuzzleWelcome = false;

    private void Start()
    {
        HideImmediately(infoPanel);
        HideImmediately(exitConfirmPanel);
        HideImmediately(levelSettingsPanel);
    }

    // =========================
    // INFO PANEL
    // =========================

    public void OpenInfoPanel()
    {
        infoOpenedFromPuzzleWelcome = false;

        HideImmediately(exitConfirmPanel);
        HideImmediately(levelSettingsPanel);

        OpenPanel(infoPanel, "InfoPanel");
    }

    public void OpenInfoFromPuzzleWelcome()
    {
        if (puzzleWelcomeController == null)
        {
            Debug.LogWarning(
                "PuzzleWelcomeController не е свързан " +
                "в LevelPopupController."
            );

            return;
        }

        infoOpenedFromPuzzleWelcome = true;

        puzzleWelcomeController.HideWelcomeForInfo(
            () =>
            {
                HideImmediately(exitConfirmPanel);
                HideImmediately(levelSettingsPanel);

                OpenPanel(
                    infoPanel,
                    "InfoPanel"
                );
            }
        );
    }

    public void CloseInfoPanel()
    {
        if (infoPanel == null ||
            !infoPanel.activeSelf)
        {
            return;
        }

        UIPopupAnimation animation =
            FindAnimation(infoPanel);

        if (animation != null)
        {
            animation.PlayClose(
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
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (infoOpenedFromPuzzleWelcome)
        {
            infoOpenedFromPuzzleWelcome = false;

            if (puzzleWelcomeController != null)
            {
                puzzleWelcomeController
                    .ShowWelcomeAfterInfo();
            }
        }
    }

    // =========================
    // EXIT CONFIRM PANEL
    // =========================

    public void OpenExitConfirmPanel()
    {
        HideImmediately(infoPanel);
        HideImmediately(levelSettingsPanel);

        OpenPanel(
            exitConfirmPanel,
            "ExitConfirmPanel"
        );
    }

    public void CloseExitConfirmPanel()
    {
        ClosePanel(exitConfirmPanel);
    }

    public void ConfirmExitToBodyMap()
    {
        if (string.IsNullOrWhiteSpace(bodyMapSceneName))
        {
            Debug.LogError(
                "Името на BodyMap сцената не е зададено."
            );

            return;
        }

        SceneManager.LoadScene(bodyMapSceneName);
    }

    // =========================
    // SETTINGS PANEL
    // =========================

    public void OpenSettingsPanel()
    {
        HideImmediately(infoPanel);
        HideImmediately(exitConfirmPanel);

        OpenPanel(
            levelSettingsPanel,
            "LevelSettingsPanel"
        );
    }

    public void CloseSettingsPanel()
    {
        ClosePanel(levelSettingsPanel);
    }

    // =========================
    // SHARED
    // =========================

    private void OpenPanel(
        GameObject panel,
        string panelName
    )
    {
        if (panel == null)
        {
            Debug.LogWarning(
                panelName +
                " не е свързан в LevelPopupController."
            );

            return;
        }

        bool wasAlreadyActive =
            panel.activeSelf;

        panel.SetActive(true);

        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindAnimation(panel);

            animation?.PlayOpen();
        }
    }

    private void ClosePanel(GameObject panel)
    {
        if (panel == null ||
            !panel.activeSelf)
        {
            return;
        }

        UIPopupAnimation animation =
            FindAnimation(panel);

        if (animation == null)
        {
            panel.SetActive(false);
            return;
        }

        animation.PlayClose(
            () => panel.SetActive(false)
        );
    }

    private void HideImmediately(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private UIPopupAnimation FindAnimation(
        GameObject panel
    )
    {
        return panel.GetComponentInChildren
            <UIPopupAnimation>(true);
    }
}