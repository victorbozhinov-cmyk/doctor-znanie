using System;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleWelcomeController : MonoBehaviour
{
    [Header("Welcome Overlay")]
    [SerializeField] private GameObject puzzleWelcomeOverlay;
    [SerializeField] private UIPopupAnimation welcomePanelAnimation;

    [Header("Welcome Start")]
    [SerializeField] private bool showWelcomeOnAwake = true;

    [Header("Info Button")]
    [SerializeField] private UIButtonIdlePulse infoButtonPulse;

    [Header("Puzzle Start Event")]
    [SerializeField] private UnityEvent onPuzzleStarted;

    private bool puzzleStarted;
    private bool switchingPanels;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        puzzleStarted = false;
        switchingPanels = false;

        if (showWelcomeOnAwake)
        {
            ShowWelcome();
        }
        else if (puzzleWelcomeOverlay != null)
        {
            puzzleWelcomeOverlay.SetActive(false);
        }
    }

    // =========================================================
    // SHOW WELCOME
    // =========================================================

    public void ShowWelcome()
    {
        puzzleStarted = false;
        switchingPanels = false;

        if (puzzleWelcomeOverlay == null)
        {
            Debug.LogWarning(
                "Puzzle Welcome Overlay не е зададен!"
            );

            return;
        }

        puzzleWelcomeOverlay.SetActive(true);
        puzzleWelcomeOverlay.transform.SetAsLastSibling();

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayOpen();
        }

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(true);
        }
    }

    // =========================================================
    // HIDE FOR INFO
    // =========================================================

    public void HideWelcomeForInfo(
        Action onWelcomeHidden)
    {
        if (puzzleStarted || switchingPanels)
        {
            return;
        }

        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                () =>
                {
                    FinishHidingForInfo(
                        onWelcomeHidden
                    );
                }
            );
        }
        else
        {
            FinishHidingForInfo(
                onWelcomeHidden
            );
        }
    }

    private void FinishHidingForInfo(
        Action onWelcomeHidden)
    {
        if (puzzleWelcomeOverlay != null)
        {
            puzzleWelcomeOverlay.SetActive(false);
        }

        switchingPanels = false;

        onWelcomeHidden?.Invoke();
    }

    // =========================================================
    // RETURN FROM INFO
    // =========================================================

    public void ShowWelcomeAfterInfo()
    {
        if (puzzleStarted)
        {
            return;
        }

        switchingPanels = false;

        if (puzzleWelcomeOverlay == null)
        {
            return;
        }

        puzzleWelcomeOverlay.SetActive(true);
        puzzleWelcomeOverlay.transform.SetAsLastSibling();

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayOpen();
        }

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(true);
        }
    }

    // =========================================================
    // START PUZZLE
    // =========================================================

    public void StartPuzzle()
    {
        if (puzzleStarted || switchingPanels)
        {
            return;
        }

        puzzleStarted = true;
        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                FinishStartingPuzzle
            );
        }
        else
        {
            FinishStartingPuzzle();
        }
    }

    // =========================================================
    // FINISH START
    // =========================================================

    private void FinishStartingPuzzle()
    {
        if (puzzleWelcomeOverlay != null)
        {
            puzzleWelcomeOverlay.SetActive(false);
        }

        switchingPanels = false;

        onPuzzleStarted?.Invoke();
    }
}