using System;
using UnityEngine;

public class LungsMinigameWelcomeController : MonoBehaviour
{
    // =========================================================
    // WELCOME FLOW
    // =========================================================

    [Header("Welcome Flow")]
    [SerializeField] private GameObject welcomeFlowRoot;
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject infoOverlay;

    // =========================================================
    // MINIGAME
    // =========================================================

    [Header("Minigame")]
    [SerializeField] private LungsMinigameManager minigameManager;

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool hasStarted;
    private bool isTransitioning;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        ResetWelcomeFlow();
    }

    // =========================================================
    // RESET / SHOW WELCOME
    // =========================================================

    public void ResetWelcomeFlow()
    {
        hasStarted = false;
        isTransitioning = false;

        if (welcomeFlowRoot != null)
        {
            welcomeFlowRoot.SetActive(true);
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }
    }

    // =========================================================
    // OPEN INFO
    // =========================================================

    public void OpenInfo()
    {
        if (hasStarted || isTransitioning)
        {
            return;
        }

        if (infoOverlay == null)
        {
            Debug.LogWarning(
                "Welcome Info Overlay не е свързан."
            );

            return;
        }

        isTransitioning = true;

        CloseWelcomePanel(
            () =>
            {
                infoOverlay.SetActive(true);
                infoOverlay.transform.SetAsLastSibling();

                isTransitioning = false;
            }
        );
    }

    // =========================================================
    // CLOSE INFO
    // =========================================================

    public void CloseInfo()
    {
        if (hasStarted || isTransitioning)
        {
            return;
        }

        if (infoOverlay == null ||
            !infoOverlay.activeSelf)
        {
            return;
        }

        isTransitioning = true;

        CloseInfoPanel(
            () =>
            {
                if (welcomePanel != null)
                {
                    welcomePanel.SetActive(true);
                }

                isTransitioning = false;
            }
        );
    }

    // =========================================================
    // START MINIGAME
    // =========================================================

    public void StartMinigame()
    {
        if (hasStarted || isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        CloseWelcomePanel(
            () =>
            {
                hasStarted = true;

                if (minigameManager != null)
                {
                    minigameManager.StartMinigame();
                }
                else
                {
                    Debug.LogWarning(
                        "LungsMinigameManager не е свързан."
                    );
                }

                if (welcomeFlowRoot != null)
                {
                    welcomeFlowRoot.SetActive(false);
                }
                else
                {
                    Debug.LogWarning(
                        "Welcome Flow Root не е свързан."
                    );

                    if (infoOverlay != null)
                    {
                        infoOverlay.SetActive(false);
                    }

                    if (welcomePanel != null)
                    {
                        welcomePanel.SetActive(false);
                    }
                }

                isTransitioning = false;
            }
        );
    }

    // =========================================================
    // CLOSE WELCOME PANEL
    // =========================================================

    private void CloseWelcomePanel(
        Action onFinished)
    {
        if (welcomePanel == null ||
            !welcomePanel.activeSelf)
        {
            onFinished?.Invoke();
            return;
        }

        UIPopupAnimation popupAnimation =
            welcomePanel.GetComponentInChildren
                <UIPopupAnimation>(true);

        if (popupAnimation == null ||
            !popupAnimation.isActiveAndEnabled)
        {
            welcomePanel.SetActive(false);

            onFinished?.Invoke();
            return;
        }

        popupAnimation.PlayClose(
            () =>
            {
                if (welcomePanel != null)
                {
                    welcomePanel.SetActive(false);
                }

                onFinished?.Invoke();
            }
        );
    }

    // =========================================================
    // CLOSE INFO PANEL
    // =========================================================

    private void CloseInfoPanel(
        Action onFinished)
    {
        if (infoOverlay == null ||
            !infoOverlay.activeSelf)
        {
            onFinished?.Invoke();
            return;
        }

        UIPopupAnimation popupAnimation =
            infoOverlay.GetComponentInChildren
                <UIPopupAnimation>(true);

        if (popupAnimation == null ||
            !popupAnimation.isActiveAndEnabled)
        {
            infoOverlay.SetActive(false);

            onFinished?.Invoke();
            return;
        }

        popupAnimation.PlayClose(
            () =>
            {
                if (infoOverlay != null)
                {
                    infoOverlay.SetActive(false);
                }

                onFinished?.Invoke();
            }
        );
    }
}