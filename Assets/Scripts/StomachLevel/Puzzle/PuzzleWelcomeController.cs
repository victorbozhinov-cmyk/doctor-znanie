using UnityEngine;

public class PuzzleWelcomeController : MonoBehaviour
{
    [Header("Welcome Overlay")]
    [SerializeField] private GameObject puzzleWelcomeOverlay;
    [SerializeField] private UIPopupAnimation welcomePanelAnimation;

    [Header("Info Button")]
    [SerializeField] private UIButtonIdlePulse infoButtonPulse;

    [Header("How To Play Panel")]
    [SerializeField] private GameObject infoPanel;

    private bool puzzleStarted;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        puzzleStarted = false;

        if (puzzleWelcomeOverlay != null)
        {
            puzzleWelcomeOverlay.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    // =========================================================
    // HOW TO PLAY
    // =========================================================

    public void OpenHowToPlay()
    {
        if (puzzleStarted)
            return;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        if (infoPanel == null)
        {
            Debug.LogWarning(
                "Info Panel не е зададен в PuzzleWelcomeController!"
            );

            return;
        }

        // Поставяме InfoPanel над Welcome Overlay.
        infoPanel.transform.SetAsLastSibling();

        infoPanel.SetActive(true);

        UIPopupAnimation animation =
            infoPanel.GetComponentInChildren<UIPopupAnimation>(true);

        if (animation != null)
        {
            animation.PlayOpen();
        }
    }

    public void CloseHowToPlay()
    {
        if (infoPanel == null)
            return;

        UIPopupAnimation animation =
            infoPanel.GetComponentInChildren<UIPopupAnimation>(true);

        if (animation != null)
        {
            animation.PlayClose(
                () =>
                {
                    infoPanel.SetActive(false);
                }
            );
        }
        else
        {
            infoPanel.SetActive(false);
        }
    }

    // =========================================================
    // START PUZZLE
    // =========================================================

    public void StartPuzzle()
    {
        if (puzzleStarted)
            return;

        puzzleStarted = true;

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

    private void FinishStartingPuzzle()
    {
        if (puzzleWelcomeOverlay != null)
        {
            puzzleWelcomeOverlay.SetActive(false);
        }

        Debug.Log("Пъзелът започна.");
    }
}