using UnityEngine;

public class QuizWelcomeController : MonoBehaviour
{
    [Header("Quiz")]
    [SerializeField] private StomachQuizManager quizManager;

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
                FindFirstObjectByType<StomachQuizManager>();
        }

        // Спираме Quiz Manager-а,
        // докато Welcome панелът е активен.
        if (quizManager != null)
        {
            quizManager.enabled = false;
        }

        // Спираме САМО idle pulse анимацията
        // на Hint бутона.
        if (hintButtonIdlePulse != null)
        {
            hintButtonIdlePulse.enabled = false;
        }

        quizStarted = false;
        switchingPanels = false;

        OpenWelcomeOverlay();
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

        switchingPanels = true;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        CloseWelcomeOverlay(
            FinishOpeningHowToPlay
        );
    }

    private void FinishOpeningHowToPlay()
    {
        if (quizManager == null)
        {
            Debug.LogError(
                "StomachQuizManager не е зададен " +
                "в QuizWelcomeController!"
            );

            switchingPanels = false;

            OpenWelcomeOverlay();

            if (infoButtonPulse != null)
            {
                infoButtonPulse.SetPulseEnabled(true);
            }

            return;
        }

        quizManager.OpenInfoPanelFromWelcome(
            ReturnFromHowToPlay
        );

        switchingPanels = false;
    }

    // =========================================================
    // RETURN FROM INFO
    // =========================================================

    private void ReturnFromHowToPlay()
    {
        if (quizStarted)
        {
            return;
        }

        switchingPanels = true;

        OpenWelcomeOverlay();

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(true);
        }

        switchingPanels = false;
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

        CloseWelcomeOverlay(
            FinishStartingQuiz
        );
    }

    // =========================================================
    // FINISH START
    // =========================================================

    private void FinishStartingQuiz()
    {
        switchingPanels = false;

        // Връщаме idle pulse анимацията
        // на Hint бутона.
        if (hintButtonIdlePulse != null)
        {
            hintButtonIdlePulse.enabled = true;
        }

        // Стартираме куиза.
        if (quizManager != null)
        {
            quizManager.enabled = true;
        }
        else
        {
            Debug.LogError(
                "StomachQuizManager не е зададен " +
                "в QuizWelcomeController!"
            );
        }
    }

    // =========================================================
    // WELCOME HELPERS
    // =========================================================

    private void OpenWelcomeOverlay()
    {
        if (quizWelcomeOverlay == null)
        {
            Debug.LogWarning(
                "Quiz Welcome Overlay не е зададен!"
            );

            return;
        }

        bool wasAlreadyActive =
            quizWelcomeOverlay.activeSelf;

        quizWelcomeOverlay.SetActive(true);

        quizWelcomeOverlay
            .transform
            .SetAsLastSibling();

        // Ако Overlay-ят току-що е бил активиран,
        // UIPopupAnimation.OnEnable() сам пуска
        // open анимацията.
        //
        // Ръчно PlayOpen() е нужен само ако
        // Overlay-ят вече е бил активен.
        if (wasAlreadyActive &&
            welcomePanelAnimation != null &&
            welcomePanelAnimation.isActiveAndEnabled)
        {
            welcomePanelAnimation.PlayOpen();
        }
    }

    private void CloseWelcomeOverlay(
        System.Action onFinished)
    {
        if (quizWelcomeOverlay == null ||
            !quizWelcomeOverlay.activeSelf)
        {
            onFinished?.Invoke();
            return;
        }

        if (welcomePanelAnimation != null &&
            welcomePanelAnimation.isActiveAndEnabled)
        {
            welcomePanelAnimation.PlayClose(
                () =>
                {
                    if (quizWelcomeOverlay != null)
                    {
                        quizWelcomeOverlay.SetActive(false);
                    }

                    onFinished?.Invoke();
                }
            );
        }
        else
        {
            quizWelcomeOverlay.SetActive(false);

            onFinished?.Invoke();
        }
    }
}