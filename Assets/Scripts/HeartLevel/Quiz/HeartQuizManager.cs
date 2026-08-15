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

    private List<HeartQuizQuestion> selectedQuestions =
        new List<HeartQuizQuestion>();

    // =========================
    // QUESTION TYPE PANELS
    // =========================

    [Header("Question Type Panels")]
    [SerializeField] private GameObject multipleQuestionUI;
    [SerializeField] private GameObject questionImageUI;
    [SerializeField] private GameObject imageAnswerUI;
    [SerializeField] private GameObject writtenAnswerUI;

    // =========================
    // MULTIPLE CHOICE UI
    // =========================

    [Header("Multiple Choice UI")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private TMP_Text answerAText;
    [SerializeField] private TMP_Text answerBText;
    [SerializeField] private TMP_Text answerVText;
    [SerializeField] private TMP_Text answerGText;

    [SerializeField] private Button answerAButton;
    [SerializeField] private Button answerBButton;
    [SerializeField] private Button answerVButton;
    [SerializeField] private Button answerGButton;

    // =========================
    // QUESTION IMAGE UI
    // =========================

    [Header("Question Image UI")]
    [SerializeField] private TMP_Text questionImageText;
    [SerializeField] private Image questionImage;

    [SerializeField] private TMP_Text questionImageAnswerAText;
    [SerializeField] private TMP_Text questionImageAnswerBText;
    [SerializeField] private TMP_Text questionImageAnswerVText;
    [SerializeField] private TMP_Text questionImageAnswerGText;

    [SerializeField] private Button questionImageAnswerAButton;
    [SerializeField] private Button questionImageAnswerBButton;
    [SerializeField] private Button questionImageAnswerVButton;
    [SerializeField] private Button questionImageAnswerGButton;

    // =========================
    // IMAGE ANSWERS UI
    // =========================

    [Header("Image Answers UI")]
    [SerializeField] private TMP_Text imageAnswersQuestionText;

    [SerializeField] private Image imageAnswerAImage;
    [SerializeField] private Image imageAnswerBImage;
    [SerializeField] private Image imageAnswerVImage;
    [SerializeField] private Image imageAnswerGImage;

    [SerializeField] private Button imageAnswerAButton;
    [SerializeField] private Button imageAnswerBButton;
    [SerializeField] private Button imageAnswerVButton;
    [SerializeField] private Button imageAnswerGButton;

    // =========================
    // LIVES
    // =========================

    [Header("Lives")]
    [Tooltip("Подреди ги: LifeHeart1, LifeHeart2, LifeHeart3")]
    [SerializeField] private UILifeHeartAnimation[] lifeHearts;

    // =========================
    // HINT
    // =========================

    [Header("Hint System")]
    [SerializeField] private GameObject hintOverlay;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Button hintButton;

    [Header("Hint Button Visuals")]
    [SerializeField] private Image hintButtonImage;
    [SerializeField] private Sprite activeHintSprite;
    [SerializeField] private Sprite disabledHintSprite;
    [SerializeField] private UIButtonHoverEffect hintHoverEffect;

    // =========================
    // PANELS
    // =========================

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

    // =========================
    // FLOW
    // =========================

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

    private Vector3 originalHintButtonScale;

    private Vector2 originalHintImageSize;
    private Vector2 originalHintImagePosition;
    private Vector3 originalHintImageScale;

    private void Start()
    {
        SaveHintButtonOriginalValues();

        HideOverlays();

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

        BuildQuizForDifficulty();

        currentQuestionIndex = 0;

        ShowQuestion();
    }

    // =========================
    // QUIZ GENERATION
    // =========================

    private void BuildQuizForDifficulty()
    {
        selectedQuestions.Clear();
        usedHintQuestions.Clear();

        List<HeartQuizQuestion> multiple =
            GetQuestionsOfType(
                HeartQuizQuestionType.MultipleChoice
            );

        List<HeartQuizQuestion> questionImages =
            GetQuestionsOfType(
                HeartQuizQuestionType.QuestionImage
            );

        List<HeartQuizQuestion> imageAnswers =
            GetQuestionsOfType(
                HeartQuizQuestionType.ImageAnswers
            );

        List<HeartQuizQuestion> written =
            GetQuestionsOfType(
                HeartQuizQuestionType.Written
            );

        Shuffle(multiple);
        Shuffle(questionImages);
        Shuffle(imageAnswers);
        Shuffle(written);

        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            // =========================
            // EASY = 6
            // 3 Multiple Choice
            // 1 Question Image
            // 1 Image Answers
            // 1 Written
            // =========================
            case 0:

                AddQuestions(multiple, 3);
                AddQuestions(questionImages, 1);
                AddQuestions(imageAnswers, 1);
                AddQuestions(written, 1);

                break;

            // =========================
            // HARD = 10
            // 4 Multiple Choice
            // 2 Question Image
            // 2 Image Answers
            // 2 Written
            // =========================
            case 2:

                AddQuestions(multiple, 4);
                AddQuestions(questionImages, 2);
                AddQuestions(imageAnswers, 2);
                AddQuestions(written, 2);

                break;

            // =========================
            // MEDIUM = 8
            // 3 Multiple Choice
            // 2 Question Image
            // 2 Image Answers
            // 1 Written
            // =========================
            default:

                AddQuestions(multiple, 3);
                AddQuestions(questionImages, 2);
                AddQuestions(imageAnswers, 2);
                AddQuestions(written, 1);

                break;
        }

        Shuffle(selectedQuestions);

        Debug.Log(
            "Избрани въпроси за този куиз: " +
            selectedQuestions.Count
        );
    }

    private List<HeartQuizQuestion> GetQuestionsOfType(
        HeartQuizQuestionType type)
    {
        List<HeartQuizQuestion> result =
            new List<HeartQuizQuestion>();

        foreach (HeartQuizQuestion question in questions)
        {
            if (question != null &&
                question.questionType == type)
            {
                result.Add(question);
            }
        }

        return result;
    }

    private void AddQuestions(
        List<HeartQuizQuestion> source,
        int amount)
    {
        int count =
            Mathf.Min(
                amount,
                source.Count
            );

        for (int i = 0; i < count; i++)
        {
            selectedQuestions.Add(
                source[i]
            );
        }

        if (count < amount)
        {
            Debug.LogWarning(
                "Няма достатъчно въпроси от дадения тип. " +
                "Искани: " +
                amount +
                ", налични: " +
                source.Count
            );
        }
    }

    private void Shuffle<T>(
        List<T> list)
    {
        for (int i = list.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            T temp = list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }

    // =========================
    // SHOW QUESTION
    // =========================

    private void ShowQuestion()
    {
        if (selectedQuestions.Count == 0)
        {
            Debug.LogWarning(
                "Няма избрани въпроси за Heart Quiz!"
            );

            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >=
            selectedQuestions.Count)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            selectedQuestions[
                currentQuestionIndex
            ];

        HideAllQuestionPanels();

        switch (currentQuestion.questionType)
        {
            case HeartQuizQuestionType.MultipleChoice:

                ShowMultipleChoiceQuestion(
                    currentQuestion
                );

                break;

            case HeartQuizQuestionType.QuestionImage:

                ShowQuestionImageQuestion(
                    currentQuestion
                );

                break;

            case HeartQuizQuestionType.ImageAnswers:

                ShowImageAnswersQuestion(
                    currentQuestion
                );

                break;

            case HeartQuizQuestionType.Written:

                ShowWrittenQuestion(
                    currentQuestion
                );

                break;
        }

        UpdateHintButton();
    }

    private void HideAllQuestionPanels()
    {
        if (multipleQuestionUI != null)
            multipleQuestionUI.SetActive(false);

        if (questionImageUI != null)
            questionImageUI.SetActive(false);

        if (imageAnswerUI != null)
            imageAnswerUI.SetActive(false);

        if (writtenAnswerUI != null)
            writtenAnswerUI.SetActive(false);
    }

    private void ShowMultipleChoiceQuestion(
        HeartQuizQuestion question)
    {
        if (multipleQuestionUI != null)
        {
            multipleQuestionUI.SetActive(true);
        }

        if (questionText != null)
            questionText.text = question.question;

        if (answerAText != null)
            answerAText.text = question.answerA;

        if (answerBText != null)
            answerBText.text = question.answerB;

        if (answerVText != null)
            answerVText.text = question.answerV;

        if (answerGText != null)
            answerGText.text = question.answerG;
    }

    private void ShowQuestionImageQuestion(
        HeartQuizQuestion question)
    {
        if (questionImageUI != null)
        {
            questionImageUI.SetActive(true);
        }

        if (questionImageText != null)
        {
            questionImageText.text =
                question.question;
        }

        if (questionImage != null)
        {
            questionImage.sprite =
                question.questionImage;

            questionImage.enabled =
                question.questionImage != null;
        }

        if (questionImageAnswerAText != null)
        {
            questionImageAnswerAText.text =
                question.answerA;
        }

        if (questionImageAnswerBText != null)
        {
            questionImageAnswerBText.text =
                question.answerB;
        }

        if (questionImageAnswerVText != null)
        {
            questionImageAnswerVText.text =
                question.answerV;
        }

        if (questionImageAnswerGText != null)
        {
            questionImageAnswerGText.text =
                question.answerG;
        }
    }

    // =========================
    // IMAGE ANSWERS
    // =========================

    private void ShowImageAnswersQuestion(
        HeartQuizQuestion question)
    {
        if (imageAnswerUI != null)
        {
            imageAnswerUI.SetActive(true);
        }

        if (imageAnswersQuestionText != null)
        {
            imageAnswersQuestionText.text =
                question.question;
        }

        SetAnswerImage(
            imageAnswerAImage,
            question.answerAImage
        );

        SetAnswerImage(
            imageAnswerBImage,
            question.answerBImage
        );

        SetAnswerImage(
            imageAnswerVImage,
            question.answerVImage
        );

        SetAnswerImage(
            imageAnswerGImage,
            question.answerGImage
        );
    }

    private void SetAnswerImage(
        Image targetImage,
        Sprite sprite)
    {
        if (targetImage == null)
            return;

        targetImage.sprite =
            sprite;

        targetImage.enabled =
            sprite != null;

        targetImage.preserveAspect = true;
    }

    // =========================
    // WRITTEN
    // =========================

    private void ShowWrittenQuestion(
        HeartQuizQuestion question)
    {
        if (writtenAnswerUI != null)
        {
            writtenAnswerUI.SetActive(true);
        }

        // Ще го направим в следващата стъпка.
    }

    // =========================
    // ANSWERS
    // =========================

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

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >=
            selectedQuestions.Count)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            selectedQuestions[
                currentQuestionIndex
            ];

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

    private IEnumerator GoToNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(
            nextQuestionDelay
        );

        currentQuestionIndex++;

        if (currentQuestionIndex >=
            selectedQuestions.Count)
        {
            QuizCompleted();
            yield break;
        }

        ShowQuestion();

        SetAnswerButtonsInteractable(true);

        isChangingQuestion = false;
    }

    // =========================
    // LIVES
    // =========================

    private void SetupLives()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

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

        for (int i = 0;
             i < lifeHearts.Length;
             i++)
        {
            if (lifeHearts[i] != null)
            {
                lifeHearts[i]
                    .gameObject
                    .SetActive(
                        i < currentLives
                    );
            }
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
            SetAnswerButtonsInteractable(
                true
            );

            isProcessingWrongAnswer =
                false;
        }
    }

    // =========================
    // HINT
    // =========================

    private void SetupHints()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

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
            currentQuestionIndex >=
            selectedQuestions.Count)
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
            selectedQuestions[
                currentQuestionIndex
            ];

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

            if (difficultySelector != null)
            {
                difficultySelector.SetLocked(
                    true
                );
            }
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
            exitConfirmationOverlay.SetActive(
                true
            );
        }
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(
                false
            );
        }
    }

    // =========================
    // GAME END
    // =========================

    private void QuizCompleted()
    {
        Debug.Log(
            "Куизът приключи успешно!"
        );

        isQuizCompleted = true;
        isChangingQuestion = false;

        SetAnswerButtonsInteractable(false);

        UpdateHintButton();

        HideAllQuestionPanels();

        if (hintOverlay != null)
            hintOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (quizSuccessOverlay != null)
            quizSuccessOverlay.SetActive(true);

        if (quizSuccessPanel != null)
            quizSuccessPanel.SetActive(true);
    }

    private void GameOver()
    {
        Debug.Log(
            "Game Over - животите свършиха!"
        );

        isGameOver = true;
        isProcessingWrongAnswer = false;

        SetAnswerButtonsInteractable(false);

        UpdateHintButton();

        if (hintOverlay != null)
            hintOverlay.SetActive(false);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(true);
    }

    // =========================
    // BUTTON STATE
    // =========================

    private void SetAnswerButtonsInteractable(
        bool value)
    {
        // Multiple Choice
        if (answerAButton != null)
            answerAButton.interactable = value;

        if (answerBButton != null)
            answerBButton.interactable = value;

        if (answerVButton != null)
            answerVButton.interactable = value;

        if (answerGButton != null)
            answerGButton.interactable = value;

        // Question Image
        if (questionImageAnswerAButton != null)
            questionImageAnswerAButton.interactable = value;

        if (questionImageAnswerBButton != null)
            questionImageAnswerBButton.interactable = value;

        if (questionImageAnswerVButton != null)
            questionImageAnswerVButton.interactable = value;

        if (questionImageAnswerGButton != null)
            questionImageAnswerGButton.interactable = value;

        // Image Answers
        if (imageAnswerAButton != null)
            imageAnswerAButton.interactable = value;

        if (imageAnswerBButton != null)
            imageAnswerBButton.interactable = value;

        if (imageAnswerVButton != null)
            imageAnswerVButton.interactable = value;

        if (imageAnswerGButton != null)
            imageAnswerGButton.interactable = value;
    }

    // =========================
    // INITIAL SETUP
    // =========================

    private void SaveHintButtonOriginalValues()
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
    }

    private void HideOverlays()
    {
        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);

        if (quizSuccessOverlay != null)
            quizSuccessOverlay.SetActive(false);

        if (quizSuccessPanel != null)
            quizSuccessPanel.SetActive(false);

        if (hintOverlay != null)
            hintOverlay.SetActive(false);
    }
}