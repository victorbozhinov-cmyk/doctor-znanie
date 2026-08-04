using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPopupController : MonoBehaviour
{
    [Header("Level Popups")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject exitConfirmPanel;
    [SerializeField] private GameObject levelSettingsPanel;

    [Header("Navigation")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

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
        HideImmediately(exitConfirmPanel);
        HideImmediately(levelSettingsPanel);

        OpenPanel(infoPanel, "InfoPanel");
    }

    public void CloseInfoPanel()
    {
        ClosePanel(infoPanel);
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

        bool wasAlreadyActive = panel.activeSelf;

        panel.SetActive(true);

        /*
         * При активиране на панела OnEnable() на
         * UIPopupAnimation автоматично пуска PlayOpen().
         *
         * Ако панелът вече е бил активен, пускаме
         * анимацията ръчно.
         */
        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindAnimation(panel);

            animation?.PlayOpen();
        }
    }

    private void ClosePanel(GameObject panel)
    {
        if (panel == null || !panel.activeSelf)
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