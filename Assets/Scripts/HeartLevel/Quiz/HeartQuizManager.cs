using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartQuizManager : MonoBehaviour
{
    [Header("Quiz Data")]
    [SerializeField] private List<HeartQuizQuestion> questions =
        new List<HeartQuizQuestion>();

    [Header("UI")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private TMP_Text answerAText;
    [SerializeField] private TMP_Text answerBText;
    [SerializeField] private TMP_Text answerVText;
    [SerializeField] private TMP_Text answerGText;

    [Header("Answer Buttons")]
    [SerializeField] private Button answerAButton;
    [SerializeField] private Button answerBButton;
    [SerializeField] private Button answerVButton;
    [SerializeField] private Button answerGButton;

    [Header("Lives")]
    [Tooltip("Подреди ги: LifeHeart1, LifeHeart2, LifeHeart3")]
    [SerializeField] private UILifeHeartAnimation[] lifeHearts;

    [Header("Hint System")]
    [SerializeField] private GameObject hintOverlay;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Button hintButton;

    [Header("Hint Button Visuals")]
    [SerializeField] private Image hintButtonImage;
    [SerializeField] private Sprite activeHintSprite;
    [SerializeField] private Sprite disabledHintSprite;
    [SerializeField] private UIButtonHoverEffect hintHoverEffect;

    [Header("Info")]
    [SerializeField] private GameObject infoOverlay;

    [Header("Settings")]
    [SerializeField] private GameObject settingsOverlay;

    [Header("Difficulty")]
    [SerializeField] private DifficultySelector difficultySelector;

    [Header("Exit")]
    [SerializeField] private GameObject exitConfirmationOverlay;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Success")]
    [SerializeField] private GameObject quizSuccessOverlay;
    [SerializeField] private GameObject quizSuccessPanel;

    [Header("Question Flow")]
    [SerializeField] private float nextQuestionDelay = 0.8f;
    [SerializeField] private float wrongAnswerLockDuration = 0.6f;

    private int currentQuestionIndex = 0;
    private int currentLives;
    private int remainingHints;

    private HashSet<int> usedHintQuestions =
        new HashSet<int>();

    private bool isChangingQuestion = false;
    private bool isProcessingWrongAnswer = false;
    private bool isGameOver = false;
    private bool isQuizCompleted = false;

    private bool isHintOpen = false;
    private bool isInfoOpen = false;
    private bool isSettingsOpen = false;

    // Оригинални настройки на Hint бутона.
    private Vector3 originalHintButtonScale;

    private Vector2 originalHintImageSize;
    private Vector2 originalHintImagePosition;
    private Vector3 originalHintImageScale;

    private void Start()
    {
        if (hintButton != null)
        {
            originalHintButtonScale =
                hintButton.transform.localScale;
        }

        if (hintButtonImage != null)
        {
            RectTransform hintRect =
                hintButtonImage.rectTransform;

            originalHintImageSize =
                hintRect.sizeDelta;

            originalHintImagePosition =
                hintRect.anchoredPosition;

            originalHintImageScale =
                hintRect.localScale;
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        if (quizSuccessOverlay != null)
        {
            quizSuccessOverlay.SetActive(false);
        }

        if (quizSuccessPanel != null)
        {
            quizSuccessPanel.SetActive(false);
        }

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(false);
        }

        // Заключваме трудността за целия куиз.
        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }
        else
        {
            Debug.LogWarning(
                "DifficultySelector не е зададен в HeartQuizManager!"
            );
        }

        SetupLives();
        SetupHints();
        ShowQuestion();
    }

    private void SetupLives()
    {
        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                currentLives = 3;
                break;

            case 2:
                currentLives = 1;
                break;

            default:
                currentLives = 2;
                break;
        }

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] != null)
            {
                lifeHearts[i].gameObject.SetActive(
                    i < currentLives
                );
            }
        }
    }

    private void SetupHints()
    {
        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                remainingHints = 3;
                break;

            case 2:
                remainingHints = 1;
                break;

            default:
                remainingHints = 2;
                break;
        }

        UpdateHintButton();
    }

    private void ShowQuestion()
    {
        if (questions.Count == 0)
        {
            Debug.LogWarning(
                "Няма добавени въпроси в Heart Quiz!"
            );

            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= questions.Count)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            questions[currentQuestionIndex];

        questionText.text =
            currentQuestion.question;

        answerAText.text =
            currentQuestion.answerA;

        answerBText.text =
            currentQuestion.answerB;

        answerVText.text =
            currentQuestion.answerV;

        answerGText.text =
            currentQuestion.answerG;

        UpdateHintButton();
    }

    public void SelectAnswer(
        int selectedIndex,
        QuizAnswerFeedback feedback)
    {
        if (isChangingQuestion ||
            isProcessingWrongAnswer ||
            isGameOver ||
            isQuizCompleted ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            questions[currentQuestionIndex];

        if (selectedIndex ==
            currentQuestion.correctAnswerIndex)
        {
            HandleCorrectAnswer(feedback);
        }
        else
        {
            HandleWrongAnswer(feedback);
        }
    }

    private void HandleCorrectAnswer(
        QuizAnswerFeedback feedback)
    {
        Debug.Log("Верен отговор!");

        isChangingQuestion = true;

        SetAnswerButtonsInteractable(false);

        if (feedback != null)
        {
            feedback.PlayCorrect();
        }

        StartCoroutine(
            GoToNextQuestionAfterDelay()
        );
    }

    private void HandleWrongAnswer(
        QuizAnswerFeedback feedback)
    {
        Debug.Log("Грешен отговор!");

        isProcessingWrongAnswer = true;

        SetAnswerButtonsInteractable(false);

        if (feedback != null)
        {
            feedback.PlayWrong();
        }

        LoseLife();

        if (!isGameOver)
        {
            StartCoroutine(
                UnlockAfterWrongAnswer()
            );
        }
    }

    private void LoseLife()
    {
        if (currentLives <= 0)
            return;

        int heartIndex =
            currentLives - 1;

        currentLives--;

        if (heartIndex >= 0 &&
            heartIndex < lifeHearts.Length &&
            lifeHearts[heartIndex] != null)
        {
            lifeHearts[heartIndex]
                .PlayLoseAnimation();
        }

        Debug.Log(
            "Оставащи животи: " +
            currentLives
        );

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private IEnumerator UnlockAfterWrongAnswer()
    {
        yield return new WaitForSeconds(
            wrongAnswerLockDuration
        );

        if (!isGameOver &&
            !isQuizCompleted)
        {
            SetAnswerButtonsInteractable(true);

            isProcessingWrongAnswer = false;
        }
    }

    private IEnumerator GoToNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(
            nextQuestionDelay
        );

        currentQuestionIndex++;

        if (currentQuestionIndex >=
            questions.Count)
        {
            QuizCompleted();
            yield break;
        }

        ShowQuestion();

        SetAnswerButtonsInteractable(true);

        isChangingQuestion = false;
    }

    // =========================
    // HINT SYSTEM
    // =========================

    public void OpenHint()
    {
        if (isGameOver ||
            isQuizCompleted ||
            isChangingQuestion ||
            isProcessingWrongAnswer ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen)
        {
            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= questions.Count)
        {
            return;
        }

        bool hintAlreadyUsed =
            usedHintQuestions.Contains(
                currentQuestionIndex
            );

        if (!hintAlreadyUsed &&
            remainingHints <= 0)
        {
            Debug.Log(
                "Нямаш останали хинтове!"
            );

            return;
        }

        HeartQuizQuestion currentQuestion =
            questions[currentQuestionIndex];

        if (hintText != null)
        {
            hintText.text =
                currentQuestion.hint;
        }

        if (!hintAlreadyUsed)
        {
            remainingHints--;

            usedHintQuestions.Add(
                currentQuestionIndex
            );

            Debug.Log(
                "Използван хинт. Остават: " +
                remainingHints
            );
        }

        isHintOpen = true;

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(true);
        }

        UpdateHintButton();
    }

    public void CloseHint()
    {
        if (!isHintOpen)
            return;

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(false);
        }

        isHintOpen = false;

        UpdateHintButton();
    }

    private void UpdateHintButton()
    {
        if (hintButton == null)
            return;

        bool hintAlreadyUsed =
            usedHintQuestions.Contains(
                currentQuestionIndex
            );

        bool canUseHint =
            hintAlreadyUsed ||
            remainingHints > 0;

        bool buttonEnabled =
            canUseHint &&
            !isGameOver &&
            !isQuizCompleted;

        hintButton.interactable =
            buttonEnabled;

        if (hintHoverEffect != null)
        {
            hintHoverEffect.enabled =
                buttonEnabled;
        }

        if (!buttonEnabled)
        {
            hintButton.transform.localScale =
                originalHintButtonScale;
        }

        if (hintButtonImage != null)
        {
            if (buttonEnabled)
            {
                if (activeHintSprite != null)
                {
                    hintButtonImage.sprite =
                        activeHintSprite;
                }
            }
            else
            {
                if (disabledHintSprite != null)
                {
                    hintButtonImage.sprite =
                        disabledHintSprite;
                }
            }

            RectTransform hintRect =
                hintButtonImage.rectTransform;

            hintRect.sizeDelta =
                originalHintImageSize;

            hintRect.anchoredPosition =
                originalHintImagePosition;

            hintRect.localScale =
                originalHintImageScale;
        }
    }

    // =========================
    // INFO
    // =========================

    public void OpenInfoPanel()
    {
        if (isGameOver ||
            isQuizCompleted ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen)
        {
            return;
        }

        if (infoOverlay != null)
        {
            isInfoOpen = true;
            infoOverlay.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "InfoOverlay не е зададен в HeartQuizManager!"
            );
        }
    }

    public void CloseInfoPanel()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        isInfoOpen = false;
    }

    // =========================
    // SETTINGS
    // =========================

    public void OpenSettingsPanel()
    {
        if (isGameOver ||
            isQuizCompleted ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen)
        {
            return;
        }

        if (settingsOverlay != null)
        {
            isSettingsOpen = true;
            settingsOverlay.SetActive(true);

            // Допълнително подсигуряване:
            // ако Settings е бил inactive при Start,
            // заключваме трудността и при самото отваряне.
            if (difficultySelector != null)
            {
                difficultySelector.SetLocked(true);
            }
        }
        else
        {
            Debug.LogWarning(
                "SettingsOverlay не е зададен в HeartQuizManager!"
            );
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        isSettingsOpen = false;
    }

    // =========================
    // EXIT
    // =========================

    public void OpenExitConfirmation()
    {
        if (isGameOver ||
            isQuizCompleted ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen)
        {
            return;
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "ExitConfirmationOverlay не е зададен в HeartQuizManager!"
            );
        }
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }
    }

    private void QuizCompleted()
    {
        Debug.Log(
            "Куизът приключи успешно!"
        );

        isQuizCompleted = true;
        isChangingQuestion = false;
        isHintOpen = false;
        isInfoOpen = false;
        isSettingsOpen = false;

        SetAnswerButtonsInteractable(false);
        UpdateHintButton();

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (quizSuccessOverlay != null)
        {
            quizSuccessOverlay.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "QuizSuccessOverlay не е зададен в HeartQuizManager!"
            );
        }

        if (quizSuccessPanel != null)
        {
            quizSuccessPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "QuizSuccessPanel не е зададен в HeartQuizManager!"
            );
        }
    }

    private void GameOver()
    {
        Debug.Log(
            "Game Over - животите свършиха!"
        );

        isGameOver = true;
        isProcessingWrongAnswer = false;
        isHintOpen = false;
        isInfoOpen = false;
        isSettingsOpen = false;

        SetAnswerButtonsInteractable(false);
        UpdateHintButton();

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "GameOverOverlay не е зададен в HeartQuizManager!"
            );
        }
    }

    private void SetAnswerButtonsInteractable(
        bool value)
    {
        if (answerAButton != null)
            answerAButton.interactable = value;

        if (answerBButton != null)
            answerBButton.interactable = value;

        if (answerVButton != null)
            answerVButton.interactable = value;

        if (answerGButton != null)
            answerGButton.interactable = value;
    }
}