using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachPauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Overlays")]
    [SerializeField] private GameObject informationOverlay;
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private GameObject exitOverlay;

    [Header("Scenes")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    private bool isPaused;
    private bool isClosingPause;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        isPaused = false;
        isClosingPause = false;

        HideImmediately(
            pauseMenuPanel
        );

        HideImmediately(
            informationOverlay
        );

        HideImmediately(
            settingsOverlay
        );

        HideImmediately(
            exitOverlay
        );
    }

    // =========================================================
    // OPEN PAUSE
    // =========================================================

    public void OpenPauseMenu()
    {
        if (isPaused ||
            isClosingPause)
        {
            return;
        }

        isPaused = true;

        HideImmediately(
            informationOverlay
        );

        HideImmediately(
            settingsOverlay
        );

        HideImmediately(
            exitOverlay
        );

        Time.timeScale = 0f;

        OpenPanel(
            pauseMenuPanel
        );
    }

    // =========================================================
    // CONTINUE
    // =========================================================

    public void ContinueGame()
    {
        if (!isPaused ||
            isClosingPause)
        {
            return;
        }

        isClosingPause = true;

        HideImmediately(
            informationOverlay
        );

        HideImmediately(
            settingsOverlay
        );

        HideImmediately(
            exitOverlay
        );

        ClosePanel(
            pauseMenuPanel,
            () =>
            {
                isPaused = false;
                isClosingPause = false;

                Time.timeScale = 1f;
            }
        );
    }

    // =========================================================
    // INFORMATION - FROM PAUSE
    // =========================================================

    public void OpenInformation()
    {
        if (!isPaused)
            return;

        OpenPanel(
            informationOverlay
        );
    }

    // =========================================================
    // INFORMATION - FROM WELCOME
    // =========================================================

    public void OpenInformationFromWelcome()
    {
        OpenPanel(
            informationOverlay
        );
    }

    // =========================================================
    // CLOSE INFORMATION
    // =========================================================

    public void CloseInformation()
    {
        ClosePanel(
            informationOverlay
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isPaused)
            return;

        OpenPanel(
            settingsOverlay
        );
    }

    public void CloseSettings()
    {
        ClosePanel(
            settingsOverlay
        );
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused)
            return;

        OpenPanel(
            exitOverlay
        );
    }

    public void StayInLevel()
    {
        ClosePanel(
            exitOverlay
        );
    }

    // =========================================================
    // EXIT LEVEL
    // =========================================================

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            bodyMapSceneName
        );
    }

    // =========================================================
    // OPEN PANEL
    // =========================================================

    private void OpenPanel(
        GameObject panel)
    {
        if (panel == null)
            return;

        bool wasAlreadyActive =
            panel.activeSelf;

        panel.SetActive(true);

        panel.transform
            .SetAsLastSibling();

        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindAnimation(panel);

            if (animation != null)
            {
                animation.PlayOpen();
            }
        }
    }

    // =========================================================
    // CLOSE PANEL
    // =========================================================

    private void ClosePanel(
        GameObject panel,
        Action onFinished = null)
    {
        if (panel == null)
        {
            onFinished?.Invoke();
            return;
        }

        if (!panel.activeSelf)
        {
            onFinished?.Invoke();
            return;
        }

        UIPopupAnimation animation =
            FindAnimation(panel);

        if (animation == null)
        {
            panel.SetActive(false);

            onFinished?.Invoke();

            return;
        }

        animation.PlayClose(
            () =>
            {
                panel.SetActive(false);

                onFinished?.Invoke();
            }
        );
    }

    // =========================================================
    // FIND ANIMATION
    // =========================================================

    private UIPopupAnimation FindAnimation(
        GameObject panel)
    {
        if (panel == null)
            return null;

        return panel
            .GetComponentInChildren
            <UIPopupAnimation>(true);
    }

    // =========================================================
    // HIDE IMMEDIATELY
    // =========================================================

    private void HideImmediately(
        GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}