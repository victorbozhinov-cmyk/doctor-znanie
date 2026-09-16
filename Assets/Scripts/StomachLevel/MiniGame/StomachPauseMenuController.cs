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
    private bool isTransitioning;

    // Пази откъде е отворена информацията:
    // от Pause менюто или от Welcome панела.
    private bool informationOpenedFromWelcome;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        isPaused = false;
        isTransitioning = false;
        informationOpenedFromWelcome = false;

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
            isTransitioning)
        {
            return;
        }

        isPaused = true;
        isTransitioning = false;

        informationOpenedFromWelcome =
            false;

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
    // CONTINUE GAME
    // =========================================================

    public void ContinueGame()
    {
        if (!isPaused ||
            isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        ClosePanel(
            pauseMenuPanel,
            () =>
            {
                isPaused = false;
                isTransitioning = false;

                Time.timeScale = 1f;
            }
        );
    }

    // =========================================================
    // INFORMATION FROM PAUSE
    // =========================================================

    public void OpenInformation()
    {
        if (!isPaused ||
            isTransitioning)
        {
            return;
        }

        informationOpenedFromWelcome =
            false;

        ClosePauseThenOpen(
            informationOverlay
        );
    }

    // =========================================================
    // INFORMATION FROM WELCOME
    // =========================================================

    public void OpenInformationFromWelcome()
    {
        if (isTransitioning)
            return;

        informationOpenedFromWelcome =
            true;

        OpenPanel(
            informationOverlay
        );
    }

    // =========================================================
    // CLOSE INFORMATION
    // =========================================================

    public void CloseInformation()
    {
        if (isTransitioning)
            return;

        // Ако Information е отворен от Welcome,
        // просто го затваряме.
        // Welcome панелът остава отдолу.
        if (informationOpenedFromWelcome)
        {
            isTransitioning = true;

            ClosePanel(
                informationOverlay,
                () =>
                {
                    informationOpenedFromWelcome =
                        false;

                    isTransitioning =
                        false;
                }
            );

            return;
        }

        // Ако е отворен от Pause:
        // Information се затваря
        // и после Pause се връща.
        CloseOverlayThenReturnToPause(
            informationOverlay
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isPaused ||
            isTransitioning)
        {
            return;
        }

        ClosePauseThenOpen(
            settingsOverlay
        );
    }

    public void CloseSettings()
    {
        if (isTransitioning)
            return;

        CloseOverlayThenReturnToPause(
            settingsOverlay
        );
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused ||
            isTransitioning)
        {
            return;
        }

        ClosePauseThenOpen(
            exitOverlay
        );
    }

    public void StayInLevel()
    {
        if (isTransitioning)
            return;

        CloseOverlayThenReturnToPause(
            exitOverlay
        );
    }

    // =========================================================
    // EXIT TO BODY MAP
    // =========================================================

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            bodyMapSceneName
        );
    }

    // =========================================================
    // PAUSE -> OVERLAY
    // =========================================================

    private void ClosePauseThenOpen(
        GameObject targetOverlay)
    {
        if (targetOverlay == null)
            return;

        isTransitioning = true;

        // Първо затваряме Pause панела.
        ClosePanel(
            pauseMenuPanel,
            () =>
            {
                // Чак след неговата close
                // анимация отваряме новия panel.
                OpenPanel(
                    targetOverlay
                );

                isTransitioning =
                    false;
            }
        );
    }

    // =========================================================
    // OVERLAY -> PAUSE
    // =========================================================

    private void CloseOverlayThenReturnToPause(
        GameObject overlay)
    {
        if (overlay == null)
            return;

        isTransitioning = true;

        // Първо overlay-ят се затваря.
        ClosePanel(
            overlay,
            () =>
            {
                // После Pause менюто
                // се появява отново.
                OpenPanel(
                    pauseMenuPanel
                );

                isTransitioning =
                    false;
            }
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

        // При SetActive(true)
        // UIPopupAnimation.OnEnable()
        // автоматично пуска PlayOpen().
        //
        // Ако обектът вече е бил активен,
        // стартираме я ръчно.
        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindAnimation(
                    panel
                );

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
            FindAnimation(
                panel
            );

        // Ако няма UIPopupAnimation,
        // затваряме веднага.
        if (animation == null)
        {
            panel.SetActive(false);

            onFinished?.Invoke();

            return;
        }

        // Изчакваме close анимацията.
        animation.PlayClose(
            () =>
            {
                panel.SetActive(false);

                onFinished?.Invoke();
            }
        );
    }

    // =========================================================
    // FIND POPUP ANIMATION
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