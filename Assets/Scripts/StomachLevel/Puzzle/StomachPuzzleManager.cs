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

    [Tooltip(
        "Подредба: Element 0 = Heart1, " +
        "Element 1 = Heart2, Element 2 = Heart3"
    )]
    [SerializeField] private GameObject[] hearts;

    [SerializeField] private string attemptsPrefix = "Опити: ";

    [Header("Feedback")]
    [SerializeField] private GameObject incompleteFeedback;
    [SerializeField] private GameObject wrongFeedback;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip(
        "Колко време стои краткият feedback, " +
        "преди да се затвори."
    )]
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

        ResetHeartsForCurrentDifficulty();
        UpdateAttemptsText();
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
                "Няма добавени DropZone полета " +
                "в StomachPuzzleManager."
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
        feedbackCoroutine =
            StartCoroutine(WrongAnswerSequence());
    }

    private IEnumerator WrongAnswerSequence()
    {
        isChecking = true;
        LockCards();

        remainingAttempts--;

        if (remainingAttempts < 0)
        {
            remainingAttempts = 0;
        }

        UpdateAttemptsText();

        /*
        * След намаляването индексът на изгубеното
        * сърце е равен на remainingAttempts.
        *
        * 3 → 2: Heart3, индекс 2
        * 2 → 1: Heart2, индекс 1
        * 1 → 0: Heart1, индекс 0
        */
        int lostHeartIndex = remainingAttempts;

        bool heartAnimationFinished = true;

        if (
            hearts != null &&
            lostHeartIndex >= 0 &&
            lostHeartIndex < hearts.Length &&
            hearts[lostHeartIndex] != null
        )
        {
            GameObject lostHeart =
                hearts[lostHeartIndex];

            UILifeHeartAnimation heartAnimation =
                lostHeart.GetComponent<UILifeHeartAnimation>();

            if (
                heartAnimation != null &&
                lostHeart.activeSelf
            )
            {
                heartAnimationFinished = false;

                heartAnimation.PlayLoseAnimation(
                    () => heartAnimationFinished = true
                );
            }
            else
            {
                if (heartAnimation == null)
                {
                    Debug.LogWarning(
                        lostHeart.name +
                        " няма UILifeHeartAnimation."
                    );
                }

                lostHeart.SetActive(false);
            }
        }

        Debug.Log(
            "Грешен ред. Оставащи опити: " +
            remainingAttempts +
            "/" +
            maximumAttempts
        );

        // ======================================
        // НЯМА ПОВЕЧЕ ОПИТИ
        // ======================================

        if (remainingAttempts <= 0)
        {
            /*
            * Изчакваме последното сърце да завърши
            * анимацията си и директно показваме
            * Game Over, без WrongFeedback.
            */
            while (!heartAnimationFinished)
            {
                yield return null;
            }

            ShowGameOverAfterHeartAnimation();
            yield break;
        }

        // ======================================
        // ИМА ОЩЕ ОПИТИ
        // ======================================

        OpenAnimatedPanel(
            wrongFeedback,
            "Wrong Feedback"
        );

        yield return new WaitForSecondsRealtime(
            feedbackDuration
        );

        yield return CloseAnimatedPanel(
            wrongFeedback
        );

        /*
        * Изчакваме и анимацията на сърцето,
        * ако все още не е приключила.
        */
        while (!heartAnimationFinished)
        {
            yield return null;
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
            StartCoroutine(
                ContinueToMinigameSequence()
            );
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
                "Puzzle Panel не е свързан " +
                "в Inspector."
            );
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Minigame Panel не е свързан " +
                "в Inspector."
            );
        }

        isChecking = false;
        feedbackCoroutine = null;

        Debug.Log("Преминаване към минииграта.");
    }

    private void ShowGameOverAfterHeartAnimation()
    {
        puzzleFinished = true;

        HideAllFeedbackImmediately();

        OpenAnimatedPanel(
            gameOverPanel,
            "Game Over Panel"
        );

        isChecking = false;
        feedbackCoroutine = null;

        Debug.Log(
            "Няма останали опити. Game Over!"
        );
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

        LoadAttemptsFromDifficulty();
        ResetHeartsForCurrentDifficulty();
        UpdateAttemptsText();

        UnlockCards();

        isChecking = false;
        feedbackCoroutine = null;

        Debug.Log("Пъзелът е рестартиран.");
    }

    private void ResetHeartsForCurrentDifficulty()
    {
        if (hearts == null)
        {
            return;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            GameObject heart = hearts[i];

            if (heart == null)
            {
                continue;
            }

            UILifeHeartAnimation heartAnimation =
                heart.GetComponent
                    <UILifeHeartAnimation>();

            if (heartAnimation != null)
            {
                heartAnimation.ResetHeart();
            }
            else
            {
                heart.SetActive(true);

                Debug.LogWarning(
                    heart.name +
                    " няма UILifeHeartAnimation."
                );
            }

            /*
             * Лесно: показваме Heart1, 2 и 3.
             * Средно: показваме Heart1 и Heart2.
             * Трудно: показваме само Heart1.
             */
            heart.SetActive(i < maximumAttempts);
        }

        Canvas.ForceUpdateCanvases();
    }

    private void UpdateAttemptsText()
    {
        if (attemptsText == null)
        {
            return;
        }

        attemptsText.text =
            attemptsPrefix +
            remainingAttempts +
            "/" +
            maximumAttempts;
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
         * При първо активиране OnEnable()
         * стартира UIPopupAnimation автоматично.
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