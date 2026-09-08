using UnityEngine;

public class BrainQuizWelcomeController : MonoBehaviour
{
    [Header("Quiz")]
    [SerializeField] private BrainQuizManager quizManager;

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
                FindFirstObjectByType<BrainQuizManager>();
        }

        // Куизът НЕ стартира,
        // докато Welcome панелът е активен.
        if (quizManager != null)
        {
            quizManager.enabled = false;
        }

        // Спираме Hint idle анимацията преди старта.
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
                "BrainQuizManager не е свързан " +
                "в BrainQuizWelcomeController."
            );

            return;
        }

        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        // Първо затваряме Welcome панела.
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

        // Отваряме Info и подаваме callback,
        // за да се върнем към Welcome след затваряне.
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

    // =========================================================
    // FINISH START
    // =========================================================

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

        // Чак сега разрешаваме BrainQuizManager.
        // Тук ще се изпълни неговият Start()
        // и реално ще започне куизът.
        if (quizManager != null)
        {
            quizManager.enabled = true;
        }
        else
        {
            Debug.LogError(
                "BrainQuizManager не е свързан " +
                "в BrainQuizWelcomeController."
            );
        }
    }
}