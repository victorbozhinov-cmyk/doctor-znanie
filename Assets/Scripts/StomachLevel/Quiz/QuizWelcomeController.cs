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

        if (quizWelcomeOverlay != null)
        {
            quizWelcomeOverlay.SetActive(true);
        }
    }

    // =========================================================
    // HOW TO PLAY
    // =========================================================

    public void OpenHowToPlay()
    {
        if (quizStarted)
            return;

        if (infoButtonPulse != null)
        {
            infoButtonPulse.SetPulseEnabled(false);
        }

        if (quizManager != null)
        {
            quizManager.OpenInfoPanel();
        }
    }

    // =========================================================
    // START QUIZ
    // =========================================================

    public void StartQuiz()
    {
        if (quizStarted)
            return;

        quizStarted = true;

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
                "StomachQuizManager не е зададен в QuizWelcomeController!"
            );
        }
    }
}