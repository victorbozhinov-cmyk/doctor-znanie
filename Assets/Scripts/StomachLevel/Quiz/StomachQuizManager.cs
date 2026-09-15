using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StomachQuizManager : MonoBehaviour
{
    [Header("Quiz Data")]
    [SerializeField] private List<StomachQuizQuestion> questions =
        new List<StomachQuizQuestion>();

    private List<StomachQuizQuestion> selectedQuestions =
        new List<StomachQuizQuestion>();

    // =========================================================
    // QUESTION TYPE PANELS
    // =========================================================

    [Header("Question Type Panels")]
    [SerializeField] private GameObject multipleQuestionUI;
    [SerializeField] private GameObject questionImageUI;
    [SerializeField] private GameObject imageAnswerUI;
    [SerializeField] private GameObject writtenAnswerUI;

    [Header("Question Panel Pulse")]
    [SerializeField] private QuizQuestionPulse multipleQuestionPulse;
    [SerializeField] private QuizQuestionPulse questionImagePulse;
    [SerializeField] private QuizQuestionPulse imageAnswerPulse;
    [SerializeField] private QuizQuestionPulse writtenAnswerPulse;

    // =========================================================
    // MULTIPLE CHOICE UI
    // =========================================================

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

    // =========================================================
    // QUESTION IMAGE UI
    // =========================================================

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

    // =========================================================
    // IMAGE ANSWERS UI
    // =========================================================

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

    // =========================================================
    // WRITTEN ANSWER UI
    // =========================================================

    [Header("Written Answer UI")]
    [SerializeField] private TMP_Text writtenQuestionText;
    [SerializeField] private TMP_InputField writtenInputField;
    [SerializeField] private Button writtenCheckButton;
    [SerializeField] private QuizAnswerFeedback writtenCheckFeedback;

    // =========================================================
    // QUIZ STATUS
    // =========================================================

    [Header("Quiz Status")]
    [SerializeField] private TMP_Text questionCounterText;
    [SerializeField] private TMP_Text hintCounterText;

    [SerializeField] private UICounterPulse questionCounterPulse;
    [SerializeField] private UICounterPulse hintCounterPulse;

    // =========================================================
    // LIVES
    // =========================================================

    [Header("Lives")]
    [Tooltip("Подреди ги: LifeHeart1, LifeHeart2, LifeHeart3")]
    [SerializeField] private UILifeHeartAnimation[] lifeHearts;

    // =========================================================
    // HINT
    // =========================================================

    [Header("Hint System")]
    [SerializeField] private GameObject hintOverlay;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Button hintButton;

    [Header("Hint Button Visuals")]
    [SerializeField] private Image hintButtonImage;
    [SerializeField] private Sprite activeHintSprite;
    [SerializeField] private Sprite disabledHintSprite;
    [SerializeField] private UIButtonHoverEffect hintHoverEffect;
    [SerializeField] private HintButtonIdlePulse hintButtonIdlePulse;

    // =========================================================
    // INFO
    // =========================================================

    [Header("Info")]
    [SerializeField] private GameObject infoOverlay;
    [SerializeField] private UIPopupAnimation infoPanelAnimation;

    // Callback, използван само когато Info е
    // отворен от Quiz Welcome панела.
    private System.Action infoClosedCallback;

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private UIPopupAnimation settingsPanelAnimation;

    [Header("Difficulty")]
    [SerializeField] private DifficultySelector difficultySelector;

    // =========================================================
    // EXIT
    // =========================================================

    [Header("Exit")]
    [SerializeField] private GameObject exitConfirmationOverlay;
    [SerializeField] private UIPopupAnimation exitConfirmationAnimation;

    // =========================================================
    // GAME OVER
    // =========================================================

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    // =========================================================
    // SUCCESS
    // =========================================================

    [Header("Success")]
    [SerializeField] private GameObject quizSuccessOverlay;
    [SerializeField] private GameObject quizSuccessPanel;

    // =========================================================
    // FLOW
    // =========================================================

    [Header("Question Flow")]
    [SerializeField] private float nextQuestionDelay = 0.8f;
    [SerializeField] private float wrongAnswerLockDuration = 0.6f;

    private int currentQuestionIndex;
    private int currentLives;

    private int remainingHints;
    private int maxHints;

    private int lastShownQuestion = -1;
    private int lastShownHintCount = -1;

    private HashSet<int> usedHintQuestions =
        new HashSet<int>();

    private bool isChangingQuestion;
    private bool isProcessingWrongAnswer;

    private bool isGameOver;
    private bool isQuizCompleted;

    private bool isHintOpen;
    private bool isInfoOpen;
    private bool isSettingsOpen;
    private bool isExitConfirmationOpen;

    private Vector3 originalHintButtonScale;

    private Vector2 originalHintImageSize;
    private Vector2 originalHintImagePosition;
    private Vector3 originalHintImageScale;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SaveHintButtonOriginalValues();

        if (writtenInputField != null)
        {
            writtenInputField.onSubmit.AddListener(
                OnWrittenInputSubmit
            );
        }

        HideOverlays();

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }
        else
        {
            Debug.LogWarning(
                "DifficultySelector не е зададен в StomachQuizManager!"
            );
        }

        SetupLives();
        SetupHints();

        BuildQuizForDifficulty();

        currentQuestionIndex = 0;

        ShowQuestion();
    }

    private void OnDestroy()
    {
        if (writtenInputField != null)
        {
            writtenInputField.onSubmit.RemoveListener(
                OnWrittenInputSubmit
            );
        }
    }

    // =========================================================
    // WRITTEN INPUT SUBMIT
    // =========================================================

    private void OnWrittenInputSubmit(
        string submittedText)
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= selectedQuestions.Count)
        {
            return;
        }

        StomachQuizQuestion currentQuestion =
            selectedQuestions[currentQuestionIndex];

        if (currentQuestion.questionType !=
            StomachQuizQuestionType.Written)
        {
            return;
        }

        CheckWrittenAnswer();
    }

    // =========================================================
    // QUIZ GENERATION
    // =========================================================

    private void BuildQuizForDifficulty()
    {
        selectedQuestions.Clear();
        usedHintQuestions.Clear();

        List<StomachQuizQuestion> multiple =
            GetQuestionsOfType(
                StomachQuizQuestionType.MultipleChoice
            );

        List<StomachQuizQuestion> questionImages =
            GetQuestionsOfType(
                StomachQuizQuestionType.QuestionImage
            );

        List<StomachQuizQuestion> imageAnswers =
            GetQuestionsOfType(
                StomachQuizQuestionType.ImageAnswers
            );

        List<StomachQuizQuestion> written =
            GetQuestionsOfType(
                StomachQuizQuestionType.Written
            );

        Shuffle(multiple);
        Shuffle(questionImages);
        Shuffle(imageAnswers);
        Shuffle(written);

        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:

                AddQuestions(multiple, 3);
                AddQuestions(questionImages, 1);
                AddQuestions(imageAnswers, 1);
                AddQuestions(written, 1);

                break;

            case 2:

                AddQuestions(multiple, 4);
                AddQuestions(questionImages, 2);
                AddQuestions(imageAnswers, 2);
                AddQuestions(written, 2);

                break;

            default:

                AddQuestions(multiple, 3);
                AddQuestions(questionImages, 2);
                AddQuestions(imageAnswers, 2);
                AddQuestions(written, 1);

                break;
        }

        Shuffle(selectedQuestions);

        Debug.Log(
            "Избрани въпроси за Stomach Quiz: " +
            selectedQuestions.Count
        );
    }

    private List<StomachQuizQuestion> GetQuestionsOfType(
        StomachQuizQuestionType type)
    {
        List<StomachQuizQuestion> result =
            new List<StomachQuizQuestion>();

        foreach (StomachQuizQuestion question in questions)
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
        List<StomachQuizQuestion> source,
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

            T temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }

    // =========================================================
    // SHOW QUESTION
    // =========================================================

    private void ShowQuestion()
    {
        if (selectedQuestions.Count == 0)
        {
            Debug.LogWarning(
                "Няма избрани въпроси за Stomach Quiz!"
            );

            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= selectedQuestions.Count)
        {
            return;
        }

        StomachQuizQuestion currentQuestion =
            selectedQuestions[currentQuestionIndex];

        HideAllQuestionPanels();

        switch (currentQuestion.questionType)
        {
            case StomachQuizQuestionType.MultipleChoice:

                ShowMultipleChoiceQuestion(
                    currentQuestion
                );

                break;

            case StomachQuizQuestionType.QuestionImage:

                ShowQuestionImageQuestion(
                    currentQuestion
                );

                break;

            case StomachQuizQuestionType.ImageAnswers:

                ShowImageAnswersQuestion(
                    currentQuestion
                );

                break;

            case StomachQuizQuestionType.Written:

                ShowWrittenQuestion(
                    currentQuestion
                );

                break;
        }

        UpdateQuestionCounter();
        UpdateHintButton();
        UpdateHintCounter();
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

    // =========================================================
    // MULTIPLE CHOICE
    // =========================================================

    private void ShowMultipleChoiceQuestion(
        StomachQuizQuestion question)
    {
        if (multipleQuestionUI != null)
        {
            multipleQuestionUI.SetActive(true);
        }

        if (multipleQuestionPulse != null)
        {
            multipleQuestionPulse.Play();
        }

        if (questionText != null)
        {
            questionText.text =
                question.question;
        }

        if (answerAText != null)
            answerAText.text = question.answerA;

        if (answerBText != null)
            answerBText.text = question.answerB;

        if (answerVText != null)
            answerVText.text = question.answerV;

        if (answerGText != null)
            answerGText.text = question.answerG;
    }

    // =========================================================
    // QUESTION IMAGE
    // =========================================================

    private void ShowQuestionImageQuestion(
        StomachQuizQuestion question)
    {
        if (questionImageUI != null)
        {
            questionImageUI.SetActive(true);
        }

        if (questionImagePulse != null)
        {
            questionImagePulse.Play();
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

            questionImage.preserveAspect = true;
        }

        if (questionImageAnswerAText != null)
            questionImageAnswerAText.text = question.answerA;

        if (questionImageAnswerBText != null)
            questionImageAnswerBText.text = question.answerB;

        if (questionImageAnswerVText != null)
            questionImageAnswerVText.text = question.answerV;

        if (questionImageAnswerGText != null)
            questionImageAnswerGText.text = question.answerG;
    }

    // =========================================================
    // IMAGE ANSWERS
    // =========================================================

    private void ShowImageAnswersQuestion(
        StomachQuizQuestion question)
    {
        if (imageAnswerUI != null)
        {
            imageAnswerUI.SetActive(true);
        }

        if (imageAnswerPulse != null)
        {
            imageAnswerPulse.Play();
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

        targetImage.preserveAspect =
            true;
    }

    // =========================================================
    // WRITTEN
    // =========================================================

    private void ShowWrittenQuestion(
        StomachQuizQuestion question)
    {
        if (writtenAnswerUI != null)
        {
            writtenAnswerUI.SetActive(true);
        }

        if (writtenAnswerPulse != null)
        {
            writtenAnswerPulse.Play();
        }

        if (writtenQuestionText != null)
        {
            writtenQuestionText.text =
                question.question;
        }

        if (writtenInputField != null)
        {
            writtenInputField.text = "";
            writtenInputField.interactable = true;
        }

        if (writtenCheckButton != null)
        {
            writtenCheckButton.interactable = true;
        }

        if (writtenCheckFeedback != null)
        {
            writtenCheckFeedback.ResetFeedback();
        }
    }

    public void CheckWrittenAnswer()
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (writtenInputField == null)
        {
            Debug.LogWarning(
                "WrittenInputField не е зададен!"
            );

            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= selectedQuestions.Count)
        {
            return;
        }

        StomachQuizQuestion currentQuestion =
            selectedQuestions[currentQuestionIndex];

        if (currentQuestion.questionType !=
            StomachQuizQuestionType.Written)
        {
            return;
        }

        string playerAnswer =
            NormalizeWrittenAnswer(
                writtenInputField.text
            );

        if (string.IsNullOrEmpty(playerAnswer))
        {
            return;
        }

        bool isCorrect = false;

        string mainCorrectAnswer =
            NormalizeWrittenAnswer(
                currentQuestion.correctWrittenAnswer
            );

        if (!string.IsNullOrEmpty(mainCorrectAnswer) &&
            playerAnswer == mainCorrectAnswer)
        {
            isCorrect = true;
        }

        if (!isCorrect &&
            currentQuestion.alternativeWrittenAnswers != null)
        {
            foreach (string alternativeAnswer in
                     currentQuestion.alternativeWrittenAnswers)
            {
                string normalizedAlternative =
                    NormalizeWrittenAnswer(
                        alternativeAnswer
                    );

                if (string.IsNullOrEmpty(
                        normalizedAlternative))
                {
                    continue;
                }

                if (playerAnswer ==
                    normalizedAlternative)
                {
                    isCorrect = true;
                    break;
                }
            }
        }

        if (isCorrect)
        {
            HandleWrittenCorrectAnswer();
        }
        else
        {
            HandleWrittenWrongAnswer();
        }
    }

    private string NormalizeWrittenAnswer(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value
            .Trim()
            .ToLowerInvariant();
    }

    private void HandleWrittenCorrectAnswer()
    {
        isChangingQuestion = true;

        SetAnswerButtonsInteractable(false);

        if (writtenInputField != null)
        {
            writtenInputField.interactable =
                false;
        }

        if (writtenCheckButton != null)
        {
            writtenCheckButton.interactable =
                false;
        }

        if (writtenCheckFeedback != null)
        {
            writtenCheckFeedback.PlayCorrect();
        }

        PlayCorrectSound();

        StartCoroutine(
            GoToNextQuestionAfterDelay()
        );
    }

    private void HandleWrittenWrongAnswer()
    {
        isProcessingWrongAnswer = true;

        SetAnswerButtonsInteractable(false);

        if (writtenInputField != null)
        {
            writtenInputField.interactable =
                false;
        }

        if (writtenCheckButton != null)
        {
            writtenCheckButton.interactable =
                false;
        }

        if (writtenCheckFeedback != null)
        {
            writtenCheckFeedback.PlayWrong();
        }

        PlayWrongSoundIfNotGameOver();

        LoseLife();

        if (!isGameOver)
        {
            StartCoroutine(
                UnlockAfterWrongAnswer()
            );
        }
    }

    // =========================================================
    // ANSWERS
    // =========================================================

    public void SelectAnswer(
        int selectedIndex,
        QuizAnswerFeedback feedback)
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= selectedQuestions.Count)
        {
            return;
        }

        StomachQuizQuestion currentQuestion =
            selectedQuestions[currentQuestionIndex];

        if (selectedIndex ==
            currentQuestion.correctAnswerIndex)
        {
            HandleCorrectAnswer(
                feedback
            );
        }
        else
        {
            HandleWrongAnswer(
                feedback
            );
        }
    }

    private void HandleCorrectAnswer(
        QuizAnswerFeedback feedback)
    {
        isChangingQuestion = true;

        SetAnswerButtonsInteractable(false);

        if (feedback != null)
        {
            feedback.PlayCorrect();
        }

        PlayCorrectSound();

        StartCoroutine(
            GoToNextQuestionAfterDelay()
        );
    }

    private void HandleWrongAnswer(
        QuizAnswerFeedback feedback)
    {
        isProcessingWrongAnswer = true;

        SetAnswerButtonsInteractable(false);

        if (feedback != null)
        {
            feedback.PlayWrong();
        }

        PlayWrongSoundIfNotGameOver();

        LoseLife();

        if (!isGameOver)
        {
            StartCoroutine(
                UnlockAfterWrongAnswer()
            );
        }
    }

    // =========================================================
    // QUIZ AUDIO
    // =========================================================

    private void PlayCorrectSound()
    {
        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayCorrect();
        }
    }

    private void PlayWrongSoundIfNotGameOver()
    {
        if (currentLives <= 1)
        {
            return;
        }

        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayWrong();
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

            if (writtenInputField != null)
            {
                writtenInputField.interactable =
                    true;
            }

            if (writtenCheckButton != null)
            {
                writtenCheckButton.interactable =
                    true;
            }

            isProcessingWrongAnswer =
                false;
        }
    }

    // =========================================================
    // LIVES
    // =========================================================

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

        if (lifeHearts == null)
            return;

        for (int i = 0;
             i < lifeHearts.Length;
             i++)
        {
            if (lifeHearts[i] == null)
                continue;

            bool shouldBeVisible =
                i < currentLives;

            lifeHearts[i]
                .gameObject
                .SetActive(
                    shouldBeVisible
                );

            if (shouldBeVisible)
            {
                lifeHearts[i]
                    .ResetHeart();
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

        if (lifeHearts != null &&
            heartIndex >= 0 &&
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

    // =========================================================
    // HINT
    // =========================================================

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
                maxHints = 3;
                break;

            case 2:
                maxHints = 1;
                break;

            default:
                maxHints = 2;
                break;
        }

        remainingHints =
            maxHints;

        UpdateHintButton();
        UpdateHintCounter();
    }

    public void OpenHint()
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= selectedQuestions.Count)
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
            return;
        }

        StomachQuizQuestion currentQuestion =
            selectedQuestions[currentQuestionIndex];

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

            UpdateHintCounter();
        }

        isHintOpen = true;

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(true);
            hintOverlay.transform.SetAsLastSibling();
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

        if (hintButtonIdlePulse != null)
        {
            bool shouldIdlePulse =
                buttonEnabled &&
                remainingHints > 0 &&
                !isHintOpen;

            hintButtonIdlePulse
                .SetIdleAnimationEnabled(
                    shouldIdlePulse
                );
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

    // =========================================================
    // QUIZ STATUS
    // =========================================================

    private void UpdateQuestionCounter()
    {
        if (questionCounterText == null)
            return;

        if (selectedQuestions.Count <= 0)
        {
            questionCounterText.text =
                "0/0";

            lastShownQuestion =
                0;

            return;
        }

        int shownQuestion =
            Mathf.Clamp(
                currentQuestionIndex + 1,
                1,
                selectedQuestions.Count
            );

        bool valueChanged =
            lastShownQuestion >= 0 &&
            shownQuestion != lastShownQuestion;

        questionCounterText.text =
            shownQuestion +
            "/" +
            selectedQuestions.Count;

        if (valueChanged &&
            questionCounterPulse != null &&
            questionCounterPulse.gameObject.activeInHierarchy)
        {
            questionCounterPulse.Play();
        }

        lastShownQuestion =
            shownQuestion;
    }

    private void UpdateHintCounter()
    {
        if (hintCounterText == null)
            return;

        bool valueChanged =
            lastShownHintCount >= 0 &&
            remainingHints != lastShownHintCount;

        hintCounterText.text =
            remainingHints +
            "/" +
            maxHints;

        if (valueChanged &&
            hintCounterPulse != null &&
            hintCounterPulse.gameObject.activeInHierarchy)
        {
            hintCounterPulse.Play();
        }

        lastShownHintCount =
            remainingHints;
    }

    // =========================================================
    // INFO
    // =========================================================

    public void OpenInfoPanel()
    {
        OpenInfoPanelInternal(null);
    }

    public void OpenInfoPanelFromWelcome(
        System.Action onInfoClosed)
    {
        OpenInfoPanelInternal(
            onInfoClosed
        );
    }

    private void OpenInfoPanelInternal(
        System.Action onInfoClosed)
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (infoOverlay == null)
        {
            onInfoClosed?.Invoke();
            return;
        }

        infoClosedCallback =
            onInfoClosed;

        isInfoOpen = true;

        bool wasAlreadyActive =
            infoOverlay.activeSelf;

        infoOverlay.SetActive(true);

        infoOverlay
            .transform
            .SetAsLastSibling();

        // Ако Overlay-ят вече е бил активен,
        // OnEnable няма да се извика отново.
        if (wasAlreadyActive &&
            infoPanelAnimation != null &&
            infoPanelAnimation.isActiveAndEnabled)
        {
            infoPanelAnimation.PlayOpen();
        }
    }

    public void CloseInfoPanel()
    {
        if (infoOverlay == null)
        {
            FinishClosingInfoPanel();
            return;
        }

        if (!infoOverlay.activeSelf)
        {
            FinishClosingInfoPanel();
            return;
        }

        if (infoPanelAnimation != null &&
            infoPanelAnimation.isActiveAndEnabled)
        {
            infoPanelAnimation.PlayClose(
                FinishClosingInfoPanel
            );
        }
        else
        {
            FinishClosingInfoPanel();
        }
    }

    private void FinishClosingInfoPanel()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        isInfoOpen = false;

        System.Action callback =
            infoClosedCallback;

        infoClosedCallback = null;

        callback?.Invoke();
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettingsPanel()
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (settingsOverlay == null)
            return;

        isSettingsOpen = true;

        settingsOverlay.SetActive(true);
        settingsOverlay.transform.SetAsLastSibling();

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(
                true
            );
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsOverlay == null)
        {
            isSettingsOpen = false;
            return;
        }

        if (settingsPanelAnimation != null)
        {
            settingsPanelAnimation.PlayClose(
                () =>
                {
                    settingsOverlay.SetActive(false);
                    isSettingsOpen = false;
                }
            );
        }
        else
        {
            settingsOverlay.SetActive(false);
            isSettingsOpen = false;
        }
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (IsInteractionBlocked())
        {
            return;
        }

        if (exitConfirmationOverlay == null)
            return;

        isExitConfirmationOpen = true;

        exitConfirmationOverlay.SetActive(true);

        exitConfirmationOverlay
            .transform
            .SetAsLastSibling();
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationOverlay == null)
        {
            isExitConfirmationOpen = false;
            return;
        }

        if (exitConfirmationAnimation != null)
        {
            exitConfirmationAnimation.PlayClose(
                () =>
                {
                    exitConfirmationOverlay.SetActive(false);

                    isExitConfirmationOpen = false;
                }
            );
        }
        else
        {
            exitConfirmationOverlay.SetActive(false);

            isExitConfirmationOpen = false;
        }
    }

    // =========================================================
    // GAME END
    // =========================================================

    private void QuizCompleted()
    {
        isQuizCompleted = true;

        isChangingQuestion = false;
        isProcessingWrongAnswer = false;

        SetAnswerButtonsInteractable(false);

        if (StomachScoreManager.Instance == null)
        {
            Debug.LogError(
                "StomachScoreManager.Instance липсва. " +
                "Quiz score не може да бъде записан."
            );
        }
        else
        {
            int startingLives =
                StomachScoreManager.Instance
                    .GetStartingLivesForCurrentDifficulty();

            StomachScoreManager.Instance.SubmitQuizResult(
                currentLives,
                startingLives,
                remainingHints,
                maxHints
            );

            Debug.Log(
                $"Stomach Quiz Score записан. " +
                $"Животи: {currentLives}/{startingLives} | " +
                $"Хинтове: {remainingHints}/{maxHints} | " +
                $"Performance: " +
                $"{StomachScoreManager.Instance.QuizPerformance:P0}"
            );
        }

        UpdateHintButton();
        UpdateHintCounter();

        HideAllQuestionPanels();

        if (hintOverlay != null)
            hintOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        infoClosedCallback = null;

        isHintOpen = false;
        isInfoOpen = false;
        isSettingsOpen = false;
        isExitConfirmationOpen = false;

        if (quizSuccessOverlay != null)
        {
            quizSuccessOverlay.SetActive(true);
        }

        if (quizSuccessPanel != null)
        {
            quizSuccessPanel.SetActive(true);
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        isProcessingWrongAnswer = false;
        isChangingQuestion = false;

        SetAnswerButtonsInteractable(false);

        if (writtenInputField != null)
        {
            writtenInputField.interactable =
                false;
        }

        if (writtenCheckButton != null)
        {
            writtenCheckButton.interactable =
                false;
        }

        UpdateHintButton();
        UpdateHintCounter();

        if (hintOverlay != null)
        {
            hintOverlay.SetActive(false);
        }

        isHintOpen = false;

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(true);

            gameOverOverlay
                .transform
                .SetAsLastSibling();
        }
    }

    // =========================================================
    // BUTTON STATES
    // =========================================================

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

        if (questionImageAnswerAButton != null)
            questionImageAnswerAButton.interactable = value;

        if (questionImageAnswerBButton != null)
            questionImageAnswerBButton.interactable = value;

        if (questionImageAnswerVButton != null)
            questionImageAnswerVButton.interactable = value;

        if (questionImageAnswerGButton != null)
            questionImageAnswerGButton.interactable = value;

        if (imageAnswerAButton != null)
            imageAnswerAButton.interactable = value;

        if (imageAnswerBButton != null)
            imageAnswerBButton.interactable = value;

        if (imageAnswerVButton != null)
            imageAnswerVButton.interactable = value;

        if (imageAnswerGButton != null)
            imageAnswerGButton.interactable = value;
    }

    // =========================================================
    // BLOCK CHECK
    // =========================================================

    private bool IsInteractionBlocked()
    {
        return
            isChangingQuestion ||
            isProcessingWrongAnswer ||
            isGameOver ||
            isQuizCompleted ||
            isHintOpen ||
            isInfoOpen ||
            isSettingsOpen ||
            isExitConfirmationOpen;
    }

    // =========================================================
    // INITIAL SETUP
    // =========================================================

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

        infoClosedCallback = null;

        isHintOpen = false;
        isInfoOpen = false;
        isSettingsOpen = false;
        isExitConfirmationOpen = false;
    }
}