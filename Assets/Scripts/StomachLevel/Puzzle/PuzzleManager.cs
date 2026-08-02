using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StomachPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Elements")]
    [SerializeField] private DropZone[] dropZones;
    [SerializeField] private DraggableCard[] cards;

    [Header("Attempts")]
    [SerializeField] private TMP_Text attemptsText;
    [SerializeField] private string attemptsPrefix = "Опити: ";

    [Header("Feedback")]
    [SerializeField] private GameObject incompleteFeedback;
    [SerializeField] private GameObject wrongFeedback;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private float feedbackDuration = 1f;

    [Header("Puzzle Completed")]
    [SerializeField] private UnityEvent onPuzzleCompleted;

    private int remainingAttempts;
    private bool isChecking;
    private bool puzzleFinished;
    private Coroutine feedbackCoroutine;

    private void Start()
    {
        LoadAttemptsFromDifficulty();
        HideAllFeedback();
        UpdateAttemptsUI();
    }

    public void CheckPuzzle()
    {
        if (isChecking || puzzleFinished)
        {
            return;
        }

        if (!AreAllZonesFilled())
        {
            feedbackCoroutine =
                StartCoroutine(ShowIncompleteFeedback());

            return;
        }

        if (IsPuzzleCorrect())
        {
            CompletePuzzle();
            return;
        }

        HandleWrongAnswer();
    }

    private bool AreAllZonesFilled()
    {
        if (dropZones == null || dropZones.Length == 0)
        {
            Debug.LogError(
                "Няма добавени DropZone полета в StomachPuzzleManager."
            );

            return false;
        }

        foreach (DropZone zone in dropZones)
        {
            if (zone == null || !zone.IsOccupied)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPuzzleCorrect()
    {
        foreach (DropZone zone in dropZones)
        {
            if (zone == null || !zone.IsCorrect())
            {
                return false;
            }
        }

        return true;
    }

    private void HandleWrongAnswer()
    {
        remainingAttempts--;
        UpdateAttemptsUI();

        Debug.Log(
            "Грешен ред. Оставащи опити: " + remainingAttempts
        );

        if (remainingAttempts <= 0)
        {
            ShowGameOver();
            return;
        }

        feedbackCoroutine =
            StartCoroutine(WrongAnswerSequence());
    }

    private IEnumerator WrongAnswerSequence()
    {
        isChecking = true;
        LockCards();

        if (wrongFeedback != null)
        {
            wrongFeedback.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(feedbackDuration);

        if (wrongFeedback != null)
        {
            wrongFeedback.SetActive(false);
        }

        ReturnAllCardsHome();
        UnlockCards();

        isChecking = false;
        feedbackCoroutine = null;
    }

    private IEnumerator ShowIncompleteFeedback()
    {
        isChecking = true;

        Debug.Log("Първо постави всички карти.");

        if (incompleteFeedback != null)
        {
            incompleteFeedback.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(feedbackDuration);

        if (incompleteFeedback != null)
        {
            incompleteFeedback.SetActive(false);
        }

        isChecking = false;
        feedbackCoroutine = null;
    }

    private void CompletePuzzle()
    {
        puzzleFinished = true;
        LockCards();
        HideAllFeedback();

        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }

        Debug.Log("Пъзелът е подреден правилно!");

        onPuzzleCompleted?.Invoke();
    }

    private void ShowGameOver()
    {
        puzzleFinished = true;
        LockCards();
        HideAllFeedback();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("Няма останали опити. Game Over!");
    }

    public void RestartPuzzle()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        puzzleFinished = false;
        isChecking = false;

        HideAllFeedback();
        ReturnAllCardsHome();
        UnlockCards();

        LoadAttemptsFromDifficulty();
        UpdateAttemptsUI();

        Debug.Log("Пъзелът е рестартиран.");
    }

    private void ReturnAllCardsHome()
    {
        if (cards == null)
        {
            return;
        }

        foreach (DraggableCard card in cards)
        {
            if (card != null)
            {
                card.ReturnHome();
            }
        }
    }

    private void LockCards()
    {
        if (cards == null)
        {
            return;
        }

        foreach (DraggableCard card in cards)
        {
            if (card != null)
            {
                card.enabled = false;
            }
        }
    }

    private void UnlockCards()
    {
        if (cards == null)
        {
            return;
        }

        foreach (DraggableCard card in cards)
        {
            if (card != null)
            {
                card.enabled = true;
            }
        }
    }

    private void LoadAttemptsFromDifficulty()
    {
        int difficulty =
            DifficultySelector.GetSavedDifficulty();

        switch (difficulty)
        {
            case 0: // Лесно
                remainingAttempts = 5;
                break;

            case 1: // Средно
                remainingAttempts = 3;
                break;

            case 2: // Трудно
                remainingAttempts = 2;
                break;

            default:
                remainingAttempts = 3;
                break;
        }
    }

    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
        {
            attemptsText.text =
                attemptsPrefix + remainingAttempts;
        }
    }

    private void HideAllFeedback()
    {
        if (incompleteFeedback != null)
        {
            incompleteFeedback.SetActive(false);
        }

        if (wrongFeedback != null)
        {
            wrongFeedback.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}