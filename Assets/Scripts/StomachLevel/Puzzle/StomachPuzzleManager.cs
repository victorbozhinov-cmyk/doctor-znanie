using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StomachPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Elements")]
    [SerializeField] private DropZone[] dropZones;
    [SerializeField] private DraggableCard[] cards;

    [Header("Level Panels")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject minigamePanel;

    [Header("Attempts UI")]
    [SerializeField] private TMP_Text attemptsText;

    [Tooltip("Heart1, Heart2 и Heart3")]
    [SerializeField] private GameObject[] hearts;

    [SerializeField] private string attemptsPrefix = "Опити: ";

    [Header("Feedback")]
    [SerializeField] private GameObject incompleteFeedback;
    [SerializeField] private GameObject wrongFeedback;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Колко време стои краткият feedback преди да се затвори.")]
    [SerializeField] private float feedbackDuration = 1f;

    [Header("Puzzle Completed")]
    [SerializeField] private UnityEvent onPuzzleCompleted;

    private int maximumAttempts;
    private int remainingAttempts;

    private bool isChecking;
    private bool puzzleFinished;

    private Coroutine feedbackCoroutine;

    private void Start()
    {
        LoadAttemptsFromDifficulty();
        HideAllFeedbackImmediately();
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

        if (remainingAttempts < 0)
        {
            remainingAttempts = 0;
        }

        UpdateAttemptsUI();

        Debug.Log(
            "Грешен ред. Оставащи опити: " +
            remainingAttempts +
            "/" +
            maximumAttempts
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

        OpenAnimatedPanel(
            wrongFeedback,
            "Wrong Feedback"
        );

        yield return new WaitForSecondsRealtime(
            feedbackDuration
        );

        yield return CloseAnimatedPanel(wrongFeedback);

        ReturnAllCardsHome();
        UnlockCards();

        isChecking = false;
        feedbackCoroutine = null;
    }

    private IEnumerator ShowIncompleteFeedback()
    {
        isChecking = true;

        Debug.Log("Първо постави всички карти.");

        OpenAnimatedPanel(
            incompleteFeedback,
            "Incomplete Feedback"
        );

        yield return new WaitForSecondsRealtime(
            feedbackDuration
        );

        yield return CloseAnimatedPanel(
            incompleteFeedback
        );

        isChecking = false;
        feedbackCoroutine = null;
    }

    private void CompletePuzzle()
    {
        puzzleFinished = true;
        isChecking = false;

        LockCards();
        HideAllFeedbackImmediately();

        OpenAnimatedPanel(
            successPanel,
            "Success Panel"
        );

        Debug.Log("Пъзелът е преминат успешно!");

        onPuzzleCompleted?.Invoke();
    }

    public void ContinueToMinigame()
    {
        if (!puzzleFinished || isChecking)
        {
            return;
        }

        feedbackCoroutine =
            StartCoroutine(ContinueToMinigameSequence());
    }

    private IEnumerator ContinueToMinigameSequence()
    {
        isChecking = true;

        yield return CloseAnimatedPanel(successPanel);

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Puzzle Panel не е свързан в Inspector."
            );
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Minigame Panel не е свързан в Inspector."
            );
        }

        isChecking = false;
        feedbackCoroutine = null;

        Debug.Log("Преминаване към минииграта.");
    }

    private void ShowGameOver()
    {
        puzzleFinished = true;
        isChecking = false;

        LockCards();
        HideAllFeedbackImmediately();

        OpenAnimatedPanel(
            gameOverPanel,
            "Game Over Panel"
        );

        Debug.Log("Няма останали опити. Game Over!");
    }

    public void RestartPuzzle()
    {
        if (isChecking)
        {
            return;
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        feedbackCoroutine =
            StartCoroutine(RestartPuzzleSequence());
    }

    private IEnumerator RestartPuzzleSequence()
    {
        isChecking = true;

        yield return CloseAnimatedPanel(gameOverPanel);

        puzzleFinished = false;

        HideAllFeedbackImmediately();
        ReturnAllCardsHome();
        UnlockCards();

        LoadAttemptsFromDifficulty();
        UpdateAttemptsUI();

        isChecking = false;
        feedbackCoroutine = null;

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
                maximumAttempts = 3;
                break;

            case 1: // Средно
                maximumAttempts = 2;
                break;

            case 2: // Трудно
                maximumAttempts = 1;
                break;

            default:
                maximumAttempts = 2;
                break;
        }

        remainingAttempts = maximumAttempts;
    }

    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
        {
            attemptsText.text =
                attemptsPrefix +
                remainingAttempts +
                "/" +
                maximumAttempts;
        }

        if (hearts == null)
        {
            return;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].SetActive(
                    i < remainingAttempts
                );
            }
        }
    }

    // ==========================================
    // PANEL ANIMATIONS
    // ==========================================

    private void OpenAnimatedPanel(
        GameObject panel,
        string panelName
    )
    {
        if (panel == null)
        {
            Debug.LogWarning(
                panelName +
                " не е свързан в Inspector."
            );

            return;
        }

        bool wasAlreadyActive = panel.activeSelf;

        panel.SetActive(true);

        /*
         * Когато родителят се активира,
         * UIPopupAnimation.OnEnable() автоматично
         * стартира отварящата анимация.
         */
        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindPopupAnimation(panel);

            animation?.PlayOpen();
        }
    }

    private IEnumerator CloseAnimatedPanel(
        GameObject panel
    )
    {
        if (panel == null || !panel.activeSelf)
        {
            yield break;
        }

        UIPopupAnimation animation =
            FindPopupAnimation(panel);

        if (
            animation == null ||
            !animation.isActiveAndEnabled
        )
        {
            panel.SetActive(false);
            yield break;
        }

        bool animationFinished = false;

        animation.PlayClose(
            () =>
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }

                animationFinished = true;
            }
        );

        while (!animationFinished)
        {
            yield return null;
        }
    }

    private UIPopupAnimation FindPopupAnimation(
        GameObject panel
    )
    {
        if (panel == null)
        {
            return null;
        }

        return panel.GetComponentInChildren
            <UIPopupAnimation>(true);
    }

    private void HideAllFeedbackImmediately()
    {
        HideImmediately(incompleteFeedback);
        HideImmediately(wrongFeedback);
        HideImmediately(successPanel);
        HideImmediately(gameOverPanel);
    }

    private void HideImmediately(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}