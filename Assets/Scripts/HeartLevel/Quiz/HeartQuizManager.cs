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

    private bool isChangingQuestion = false;
    private bool isProcessingWrongAnswer = false;
    private bool isGameOver = false;
    private bool isQuizCompleted = false;

    private void Start()
    {
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

        SetupLives();
        ShowQuestion();
    }

    private void SetupLives()
    {
        // Difficulty:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

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
                lifeHearts[i].gameObject.SetActive(i < currentLives);
            }
        }
    }

    private void ShowQuestion()
    {
        if (questions.Count == 0)
        {
            Debug.LogWarning("Няма добавени въпроси в Heart Quiz!");
            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= questions.Count)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            questions[currentQuestionIndex];

        questionText.text = currentQuestion.question;

        answerAText.text = currentQuestion.answerA;
        answerBText.text = currentQuestion.answerB;
        answerVText.text = currentQuestion.answerV;
        answerGText.text = currentQuestion.answerG;
    }

    public void SelectAnswer(
        int selectedIndex,
        QuizAnswerFeedback feedback)
    {
        if (isChangingQuestion ||
            isProcessingWrongAnswer ||
            isGameOver ||
            isQuizCompleted)
        {
            return;
        }

        HeartQuizQuestion currentQuestion =
            questions[currentQuestionIndex];

        if (selectedIndex == currentQuestion.correctAnswerIndex)
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

        StartCoroutine(GoToNextQuestionAfterDelay());
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
            StartCoroutine(UnlockAfterWrongAnswer());
        }
    }

    private void LoseLife()
    {
        if (currentLives <= 0)
            return;

        int heartIndex = currentLives - 1;

        currentLives--;

        if (heartIndex >= 0 &&
            heartIndex < lifeHearts.Length &&
            lifeHearts[heartIndex] != null)
        {
            lifeHearts[heartIndex].PlayLoseAnimation();
        }

        Debug.Log("Оставащи животи: " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private IEnumerator UnlockAfterWrongAnswer()
    {
        yield return new WaitForSeconds(wrongAnswerLockDuration);

        if (!isGameOver && !isQuizCompleted)
        {
            SetAnswerButtonsInteractable(true);
            isProcessingWrongAnswer = false;
        }
    }

    private IEnumerator GoToNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(nextQuestionDelay);

        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            QuizCompleted();
            yield break;
        }

        ShowQuestion();

        SetAnswerButtonsInteractable(true);

        isChangingQuestion = false;
    }

    private void QuizCompleted()
    {
        Debug.Log("Куизът приключи успешно!");

        isQuizCompleted = true;
        isChangingQuestion = false;

        SetAnswerButtonsInteractable(false);

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
        Debug.Log("Game Over - животите свършиха!");

        isGameOver = true;
        isProcessingWrongAnswer = false;

        SetAnswerButtonsInteractable(false);

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

    private void SetAnswerButtonsInteractable(bool value)
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