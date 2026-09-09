using UnityEngine;

public class HeartQuizWelcomeController : MonoBehaviour
{
    [Header("Quiz")]
    [SerializeField] private HeartQuizManager quizManager;

    [Header("Welcome Overlay")]
    [SerializeField] private GameObject quizWelcomeOverlay;
    [SerializeField] private UIPopupAnimation welcomePanelAnimation;

    [Header("Info Button")]
    [SerializeField] private UIButtonIdlePulse infoButtonPulse;

    [Header("Hint Button")]
    [SerializeField] private HintButtonIdlePulse hintButtonIdlePulse;

    private bool quizStarted;
    private bool switchingPanels;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (quizManager == null)
        {
            quizManager =
                FindFirstObjectByType<HeartQuizManager>();
        }

        // Куизът не трябва да стартира,
        // докато Welcome панелът е активен.
        if (quizManager != null)
        {
            quizManager.enabled = false;
        }

        // Hint pulse не работи преди старта.
        if (hintButtonIdlePulse != null)
        {
            hintButtonIdlePulse.enabled = false;
        }

        quizStarted = false;
        switchingPanels = false;

        if (quizWelcomeOverlay != null)
        {
            quizWelcomeOverlay.SetActive(true);
            quizWelcomeOverlay.transform.SetAsLastSibling();
        }
    }

    // =========================================================
    // HOW TO PLAY
    // =========================================================

    public void OpenHowToPlay()
    {
        if (quizStarted ||
            switchingPanels)
        {
            return;
        }

        if (quizManager == null)
        {
            Debug.LogError(
                "HeartQuizManager не е свързан " +
                "в HeartQuizWelcomeController."
            );

            return;
        }

        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        // Първо затваряме Welcome.
        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                OpenInfoAfterWelcomeClosed
            );
        }
        else
        {
            OpenInfoAfterWelcomeClosed();
        }
    }

    private void OpenInfoAfterWelcomeClosed()
    {
        if (quizWelcomeOverlay != null)
        {
            quizWelcomeOverlay.SetActive(false);
        }

        switchingPanels = false;

        // HeartQuizManager отваря своя InfoOverlay.
        // Подаваме му callback, за да знае,
        // че след затваряне трябва да върне Welcome.
        quizManager.OpenInfoPanelFromWelcome(
            ShowWelcomeAfterInfo
        );
    }

    // =========================================================
    // RETURN FROM INFO
    // =========================================================

    public void ShowWelcomeAfterInfo()
    {
        if (quizStarted)
        {
            return;
        }

        switchingPanels = false;

        if (quizWelcomeOverlay == null)
        {
            return;
        }

        quizWelcomeOverlay.SetActive(true);
        quizWelcomeOverlay.transform.SetAsLastSibling();

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
    // START QUIZ
    // =========================================================

    public void StartQuiz()
    {
        if (quizStarted ||
            switchingPanels)
        {
            return;
        }

        quizStarted = true;
        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                FinishStartingQuiz
            );
        }
        else
        {
            FinishStartingQuiz();
        }
    }

    private void FinishStartingQuiz()
    {
        if (quizWelcomeOverlay != null)
        {
            quizWelcomeOverlay.SetActive(false);
        }

        switchingPanels = false;

        if (hintButtonIdlePulse != null)
        {
            hintButtonIdlePulse.enabled = true;
        }

        if (quizManager != null)
        {
            quizManager.enabled = true;
        }
        else
        {
            Debug.LogError(
                "HeartQuizManager не е зададен " +
                "в HeartQuizWelcomeController!"
            );
        }
    }
}