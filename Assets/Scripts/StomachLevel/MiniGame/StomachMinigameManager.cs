using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StomachMinigameManager : MonoBehaviour
{
    public enum GameState
    {
        Initializing,
        WaitingForStart,
        JuiceReady,
        JuicePlaying,
        Peristalsis,
        WaveFeedback,
        ProgressFeedback,
        ReadyForIntestine,
        TransferringToIntestine,
        Finished
    }

    public event Action IntestineTransferRequested;

    [Header("Main Systems")]
    [SerializeField] private GastricJuiceTimingGame juiceGame;
    [SerializeField] private StomachFoodStageController foodStageController;
    [SerializeField] private PeristalsisDragController peristalsisController;
    [SerializeField] private StomachStateCardController stateCardController;
    [SerializeField] private StomachProgressBar progressBar;

    [Header("Gameplay UI")]
    [SerializeField] private Button juicesButton;
    [SerializeField] private GameObject markerPanel;
    [SerializeField] private Button intestineButton;

    [Header("Welcome")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject welcomeInfoPanel;

    [Header("Button Showcase")]
    [SerializeField] private UIButtonShowcaseAnimation juicesButtonShowcase;
    [SerializeField] private UIButtonShowcaseAnimation intestineButtonShowcase;

    [Header("Success")]
    [SerializeField] private GameObject successPanel;

    [Header("Current Task")]
    [SerializeField] private TMP_Text currentTaskText;
    [SerializeField] private CurrentTaskTextAnimation currentTaskTextAnimation;

    [Header("Task Timer")]
    [SerializeField] private StomachTaskTimer taskTimer;

    [SerializeField] private float easyPeristalsisDuration = 15f;
    [SerializeField] private float mediumPeristalsisDuration = 11f;
    [SerializeField] private float hardPeristalsisDuration = 8f;

    [Header("Early Release Penalty")]
    [SerializeField] private float easyEarlyReleasePenalty = 3f;
    [SerializeField] private float mediumEarlyReleasePenalty = 4f;
    [SerializeField] private float hardEarlyReleasePenalty = 5f;

    [Header("Peristalsis Timeout")]
    [SerializeField] private float peristalsisTimeoutPenalty = 5f;

    [Header("Feedback")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject failBackground;
    [SerializeField] private GameObject successBackground;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackDuration = 1.5f;

    [Header("Total Timer")]
    [SerializeField] private StomachMinigameTimer totalTimer;

    public GameState CurrentState
    {
        get;
        private set;
    }

    private int difficulty;

    private Coroutine progressWaitCoroutine;

    private bool minigameStarted;
    private bool welcomeIsClosing;
    private bool welcomeInfoOpen;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (currentTaskTextAnimation == null &&
            currentTaskText != null)
        {
            currentTaskTextAnimation =
                currentTaskText
                    .GetComponent<CurrentTaskTextAnimation>();
        }
    }

    private void OnEnable()
    {
        if (juiceGame != null)
        {
            juiceGame.JuiceSucceeded +=
                OnJuiceSucceeded;
        }

        if (foodStageController != null)
        {
            foodStageController.SuccessfulWaveRegistered +=
                OnSuccessfulWaveRegistered;

            foodStageController.FoodStageAdvanced +=
                OnFoodStageAdvanced;

            foodStageController.DigestionCompleted +=
                OnDigestionCompleted;
        }

        if (peristalsisController != null)
        {
            peristalsisController.WaveFailedEarlyRelease +=
                OnWaveFailedEarlyRelease;
        }

        if (taskTimer != null)
        {
            taskTimer.TimerExpired +=
                OnTaskTimerExpired;
        }
    }

    private void OnDisable()
    {
        if (juiceGame != null)
        {
            juiceGame.JuiceSucceeded -=
                OnJuiceSucceeded;
        }

        if (foodStageController != null)
        {
            foodStageController.SuccessfulWaveRegistered -=
                OnSuccessfulWaveRegistered;

            foodStageController.FoodStageAdvanced -=
                OnFoodStageAdvanced;

            foodStageController.DigestionCompleted -=
                OnDigestionCompleted;
        }

        if (peristalsisController != null)
        {
            peristalsisController.WaveFailedEarlyRelease -=
                OnWaveFailedEarlyRelease;
        }

        if (taskTimer != null)
        {
            taskTimer.TimerExpired -=
                OnTaskTimerExpired;
        }

        if (progressWaitCoroutine != null)
        {
            StopCoroutine(
                progressWaitCoroutine
            );

            progressWaitCoroutine = null;
        }
    }

    private void Start()
    {
        CurrentState =
            GameState.Initializing;

        difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        minigameStarted = false;
        welcomeIsClosing = false;
        welcomeInfoOpen = false;

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        PrepareForWelcome();
    }

    // =========================================================
    // WELCOME
    // =========================================================

    private void PrepareForWelcome()
    {
        CurrentState =
            GameState.WaitingForStart;

        if (juiceGame != null)
        {
            juiceGame.StopAndHide();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .StopShowcaseImmediate();
        }

        if (intestineButtonShowcase != null)
        {
            intestineButtonShowcase
                .StopShowcaseImmediate();
        }

        if (taskTimer != null)
        {
            taskTimer.ResetTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (welcomeInfoPanel != null)
        {
            welcomeInfoPanel.SetActive(false);
        }

        OpenAnimatedPanel(
            welcomePanel
        );
    }

    public void OpenInfoFromWelcome()
    {
        if (minigameStarted ||
            welcomeIsClosing ||
            welcomeInfoOpen)
        {
            return;
        }

        welcomeIsClosing = true;

        CloseAnimatedPanel(
            welcomePanel,
            () =>
            {
                if (welcomeInfoPanel == null)
                {
                    Debug.LogWarning(
                        "Welcome Info Panel не е свързан " +
                        "в StomachMinigameManager."
                    );

                    OpenAnimatedPanel(
                        welcomePanel
                    );

                    welcomeIsClosing = false;
                    return;
                }

                welcomeInfoOpen = true;

                OpenAnimatedPanel(
                    welcomeInfoPanel
                );

                welcomeIsClosing = false;
            }
        );
    }

    public void CloseInfoToWelcome()
    {
        // Този бутон трябва винаги да може да върне играча
        // от Info панела към Welcome панела.
        // Не използваме close animation callback тук, защото
        // ако callback-ът не се изпълни, бутонът изглежда сякаш не работи.

        Debug.Log("[Stomach] CloseInfoToWelcome called.");

        if (minigameStarted)
        {
            return;
        }

        welcomeIsClosing = false;
        welcomeInfoOpen = false;

        if (welcomeInfoPanel != null)
        {
            welcomeInfoPanel.SetActive(false);
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
            welcomePanel.transform.SetAsLastSibling();
        }
    }

    public void StartMinigameFromWelcome()
    {
        if (minigameStarted ||
            welcomeIsClosing ||
            welcomeInfoOpen)
        {
            return;
        }

        welcomeIsClosing = true;

        CloseAnimatedPanel(
            welcomePanel,
            BeginMinigameAfterWelcome
        );
    }

    private void OpenAnimatedPanel(
        GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        bool wasAlreadyActive =
            panel.activeSelf;

        panel.SetActive(true);

        panel.transform
            .SetAsLastSibling();

        if (wasAlreadyActive)
        {
            UIPopupAnimation animation =
                FindPopupAnimation(
                    panel
                );

            if (animation != null &&
                animation.isActiveAndEnabled)
            {
                animation.PlayOpen();
            }
        }
    }

    private void CloseAnimatedPanel(
        GameObject panel,
        Action onFinished)
    {
        if (panel == null ||
            !panel.activeSelf)
        {
            onFinished?.Invoke();
            return;
        }

        UIPopupAnimation animation =
            FindPopupAnimation(
                panel
            );

        if (animation == null ||
            !animation.isActiveAndEnabled)
        {
            panel.SetActive(false);

            onFinished?.Invoke();
            return;
        }

        animation.PlayClose(
            () =>
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }

                onFinished?.Invoke();
            }
        );
    }

    private UIPopupAnimation FindPopupAnimation(
        GameObject panel)
    {
        if (panel == null)
        {
            return null;
        }

        return panel.GetComponentInChildren
            <UIPopupAnimation>(true);
    }

    private void BeginMinigameAfterWelcome()
    {
        minigameStarted = true;
        welcomeIsClosing = false;
        welcomeInfoOpen = false;

        if (welcomeInfoPanel != null)
        {
            welcomeInfoPanel.SetActive(false);
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowStartingFoodCard();
        }

        BeginCurrentStage();
    }

    // =========================================================
    // CURRENT FOOD STAGE
    // =========================================================

    private void BeginCurrentStage()
    {
        if (!minigameStarted)
            return;

        if (foodStageController == null)
            return;

        if (foodStageController.IsDigestionComplete)
        {
            EnterReadyForIntestineState();
            return;
        }

        int stageIndex =
            foodStageController.CurrentStageIndex;

        if (stageIndex >=
            foodStageController.TotalStageCount - 1)
        {
            EnterReadyForIntestineState();
            return;
        }

        if (StageNeedsJuices(stageIndex))
        {
            EnterJuiceReadyState();
        }
        else
        {
            EnterPeristalsisState();
        }
    }

    // =========================================================
    // JUICE STAGES
    // =========================================================

    private bool StageNeedsJuices(
        int stageIndex)
    {
        switch (difficulty)
        {
            case 0:
                return
                    stageIndex == 0 ||
                    stageIndex == 2;

            case 2:
                return
                    stageIndex == 0 ||
                    stageIndex == 2 ||
                    stageIndex == 4;

            default:
                return
                    stageIndex == 0 ||
                    stageIndex == 2;
        }
    }

    // =========================================================
    // LAST STAGE BEFORE HIMUS
    // =========================================================

    private bool IsLastStageBeforeHimus()
    {
        if (foodStageController == null)
            return false;

        return
            foodStageController.CurrentStageIndex ==
            foodStageController.TotalStageCount - 2;
    }

    // =========================================================
    // JUICE READY
    // =========================================================

    private void EnterJuiceReadyState()
    {
        CurrentState =
            GameState.JuiceReady;

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (juiceGame != null)
        {
            juiceGame.StopAndHide();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                true;
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        SetCurrentTask(
            "Добави стомашни сокове."
        );

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .PlayShowcase();
        }

        if (taskTimer != null)
        {
            taskTimer.ResetTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    // =========================================================
    // JUICE BUTTON
    // =========================================================

    public void OnJuicesButtonPressed()
    {
        if (CurrentState !=
            GameState.JuiceReady)
        {
            return;
        }

        CurrentState =
            GameState.JuicePlaying;

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .StopShowcaseImmediate();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (juiceGame != null)
        {
            juiceGame.BeginJuiceTask();
        }
    }

    // =========================================================
    // JUICE SUCCESS
    // =========================================================

    private void OnJuiceSucceeded()
    {
        if (CurrentState !=
            GameState.JuicePlaying)
        {
            return;
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowJuicesMixed();
        }

        CurrentState =
            GameState.ProgressFeedback;

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        StartProgressWait(
            BeginPeristalsisAfterProgress
        );
    }

    private void BeginPeristalsisAfterProgress()
    {
        EnterPeristalsisState();
    }

    // =========================================================
    // PERISTALSIS
    // =========================================================

    private void EnterPeristalsisState()
    {
        CurrentState =
            GameState.Peristalsis;

        if (juiceGame != null)
        {
            juiceGame.StopAndHide();
        }

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .StopShowcaseImmediate();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(true);
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        if (peristalsisController != null)
        {
            peristalsisController
                .ResetPeristalsis();
        }

        if (IsLastStageBeforeHimus() &&
            stateCardController != null)
        {
            stateCardController
                .ShowChyme();
        }

        UpdatePeristalsisTask();

        if (taskTimer != null)
        {
            taskTimer.StartTimer(
                GetPeristalsisDuration()
            );
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    private float GetPeristalsisDuration()
    {
        switch (difficulty)
        {
            case 0:
                return easyPeristalsisDuration;

            case 2:
                return hardPeristalsisDuration;

            default:
                return mediumPeristalsisDuration;
        }
    }

    // =========================================================
    // SUCCESSFUL WAVE
    // =========================================================

    private void OnSuccessfulWaveRegistered(
        int currentWaves,
        int requiredWaves)
    {
        if (CurrentState !=
            GameState.Peristalsis)
        {
            return;
        }

        if (currentWaves == 1 &&
            !IsLastStageBeforeHimus() &&
            stateCardController != null)
        {
            stateCardController
                .ShowBreakingDown();
        }

        CurrentState =
            GameState.WaveFeedback;

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        UpdatePeristalsisTask(
            currentWaves,
            requiredWaves
        );

        if (taskTimer != null)
        {
            taskTimer.PauseTimer();
        }

        StartCoroutine(
            ShowWaveSuccessFeedback(
                currentWaves,
                requiredWaves
            )
        );
    }

    private IEnumerator ShowWaveSuccessFeedback(
        int currentWaves,
        int requiredWaves)
    {
        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (failBackground != null)
        {
            failBackground.SetActive(false);
        }

        if (successBackground != null)
        {
            successBackground.SetActive(true);
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                $"Успешна перисталтична вълна! " +
                $"{currentWaves}/{requiredWaves}";
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        if (currentWaves >= requiredWaves)
        {
            if (taskTimer != null)
            {
                taskTimer.StopTimer();
            }

            if (foodStageController != null)
            {
                foodStageController
                    .AdvanceAfterCompletedWaveSet();
            }

            yield break;
        }

        CurrentState =
            GameState.Peristalsis;

        if (markerPanel != null)
        {
            markerPanel.SetActive(true);
        }

        if (peristalsisController != null)
        {
            peristalsisController
                .ResetPeristalsis();
        }

        if (taskTimer != null)
        {
            taskTimer.ResumeTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    // =========================================================
    // EARLY RELEASE
    // =========================================================

    private void OnWaveFailedEarlyRelease()
    {
        if (CurrentState !=
            GameState.Peristalsis)
        {
            return;
        }

        CurrentState =
            GameState.WaveFeedback;

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (taskTimer != null)
        {
            taskTimer.PauseTimer();
        }

        if (foodStageController != null &&
            foodStageController
                .IsHardDifficulty)
        {
            foodStageController
                .ResetCurrentWaveProgress();

            UpdatePeristalsisTask();
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowIrritated();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();

            totalTimer.ApplyPenalty(
                GetEarlyReleasePenalty()
            );
        }

        StartCoroutine(
            ShowEarlyReleaseFeedback()
        );
    }

    private float GetEarlyReleasePenalty()
    {
        switch (difficulty)
        {
            case 0:
                return easyEarlyReleasePenalty;

            case 2:
                return hardEarlyReleasePenalty;

            default:
                return mediumEarlyReleasePenalty;
        }
    }

    private IEnumerator ShowEarlyReleaseFeedback()
    {
        if (failBackground != null)
        {
            failBackground.SetActive(true);
        }

        if (successBackground != null)
        {
            successBackground.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "Пусна маркера твърде рано. " +
                "Опитай отново!";
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        CurrentState =
            GameState.Peristalsis;

        if (markerPanel != null)
        {
            markerPanel.SetActive(true);
        }

        if (peristalsisController != null)
        {
            peristalsisController
                .ResetPeristalsis();
        }

        if (taskTimer != null)
        {
            taskTimer.ResumeTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    // =========================================================
    // TASK TIMER EXPIRED
    // =========================================================

    private void OnTaskTimerExpired()
    {
        if (CurrentState !=
            GameState.Peristalsis)
        {
            return;
        }

        CurrentState =
            GameState.WaveFeedback;

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (peristalsisController != null)
        {
            peristalsisController
                .ResetPeristalsis();
        }

        if (foodStageController != null)
        {
            foodStageController
                .ResetCurrentWaveProgress();

            UpdatePeristalsisTask();
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowIrritated();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();

            totalTimer.ApplyPenalty(
                peristalsisTimeoutPenalty
            );
        }

        StartCoroutine(
            ShowPeristalsisTimeoutFeedback()
        );
    }

    private IEnumerator ShowPeristalsisTimeoutFeedback()
    {
        if (failBackground != null)
        {
            failBackground.SetActive(true);
        }

        if (successBackground != null)
        {
            successBackground.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "Времето за задачата изтече!";
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        CurrentState =
            GameState.Peristalsis;

        if (markerPanel != null)
        {
            markerPanel.SetActive(true);
        }

        if (peristalsisController != null)
        {
            peristalsisController
                .ResetPeristalsis();
        }

        if (taskTimer != null)
        {
            taskTimer.StartTimer(
                GetPeristalsisDuration()
            );
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    // =========================================================
    // FOOD STAGE CHANGED
    // =========================================================

    private void OnFoodStageAdvanced(
        int newStageIndex)
    {
        if (foodStageController == null)
            return;

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (newStageIndex ==
            foodStageController
                .TotalStageCount - 2)
        {
            if (stateCardController != null)
            {
                stateCardController
                    .ShowChyme();
            }
        }

        CurrentState =
            GameState.ProgressFeedback;

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (newStageIndex >=
            foodStageController
                .TotalStageCount - 1)
        {
            StartProgressWait(
                EnterReadyForIntestineState
            );

            return;
        }

        StartProgressWait(
            BeginCurrentStage
        );
    }

    // =========================================================
    // DIGESTION COMPLETED
    // =========================================================

    private void OnDigestionCompleted()
    {
        if (progressWaitCoroutine != null)
        {
            return;
        }

        CurrentState =
            GameState.ProgressFeedback;

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        StartProgressWait(
            EnterReadyForIntestineState
        );
    }

    // =========================================================
    // PROGRESS WAIT
    // =========================================================

    private void StartProgressWait(
        Action actionAfterProgress)
    {
        if (progressWaitCoroutine != null)
        {
            StopCoroutine(
                progressWaitCoroutine
            );
        }

        progressWaitCoroutine =
            StartCoroutine(
                WaitForProgressAnimation(
                    actionAfterProgress
                )
            );
    }

    private IEnumerator WaitForProgressAnimation(
        Action actionAfterProgress)
    {
        yield return null;

        if (progressBar != null)
        {
            while (progressBar.IsAnimating)
            {
                yield return null;
            }
        }

        progressWaitCoroutine = null;

        actionAfterProgress?.Invoke();
    }

    // =========================================================
    // HIMUS READY
    // =========================================================

    private void EnterReadyForIntestineState()
    {
        if (CurrentState ==
            GameState.ReadyForIntestine ||
            CurrentState ==
            GameState.TransferringToIntestine ||
            CurrentState ==
            GameState.Finished)
        {
            return;
        }

        CurrentState =
            GameState.ReadyForIntestine;

        if (juiceGame != null)
        {
            juiceGame.StopAndHide();
        }

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .StopShowcaseImmediate();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowReadyForIntestine();
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                true;
        }

        SetCurrentTask(
            "Изпрати химуса към тънкото черво."
        );

        if (intestineButtonShowcase != null)
        {
            intestineButtonShowcase
                .PlayShowcase();
        }

        if (taskTimer != null)
        {
            taskTimer.ResetTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }
    }

    // =========================================================
    // INTESTINE BUTTON
    // =========================================================

    public void OnIntestineButtonPressed()
    {
        if (CurrentState !=
            GameState.ReadyForIntestine)
        {
            return;
        }

        CurrentState =
            GameState.TransferringToIntestine;

        if (intestineButtonShowcase != null)
        {
            intestineButtonShowcase
                .StopShowcaseImmediate();
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (taskTimer != null)
        {
            taskTimer.StopTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        IntestineTransferRequested?.Invoke();

        if (foodStageController != null)
        {
            foodStageController
                .PlayFinalHimusExit(
                    CompleteMinigameAfterIntestineTransfer
                );
        }
        else
        {
            CompleteMinigameAfterIntestineTransfer();
        }
    }

    // =========================================================
    // MINIGAME COMPLETE
    // =========================================================

    public void CompleteMinigameAfterIntestineTransfer()
    {
        if (CurrentState !=
            GameState.TransferringToIntestine)
        {
            return;
        }

        CurrentState =
            GameState.Finished;

        if (juicesButtonShowcase != null)
        {
            juicesButtonShowcase
                .StopShowcaseImmediate();
        }

        if (intestineButtonShowcase != null)
        {
            intestineButtonShowcase
                .StopShowcaseImmediate();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (taskTimer != null)
        {
            taskTimer.StopTimer();
        }

        if (markerPanel != null)
        {
            markerPanel.SetActive(false);
        }

        if (juiceGame != null)
        {
            juiceGame.StopAndHide();
        }

        if (juicesButton != null)
        {
            juicesButton.interactable =
                false;
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        // =====================================================
        // SCORE
        // =====================================================

        if (totalTimer == null)
        {
            Debug.LogError(
                "StomachMinigameTimer не е свързан. " +
                "Minigame score не може да бъде записан."
            );
        }
        else if (StomachScoreManager.Instance == null)
        {
            Debug.LogError(
                "StomachScoreManager.Instance липсва. " +
                "Minigame score не може да бъде записан."
            );
        }
        else
        {
            float remainingTime =
                totalTimer.RemainingTime;

            float startingTime =
                totalTimer.StartingTime;

            StomachScoreManager.Instance.SubmitMinigameResult(
                remainingTime,
                startingTime
            );

            Debug.Log(
                $"Stomach Minigame Score записан. " +
                $"Време: {remainingTime:0.0}/{startingTime:0.0} | " +
                $"Performance: " +
                $"{StomachScoreManager.Instance.MinigamePerformance:P0}"
            );
        }

        // =====================================================
        // SUCCESS
        // =====================================================

        if (successPanel != null)
        {
            successPanel.SetActive(true);

            successPanel
                .transform
                .SetAsLastSibling();
        }
    }

    // =========================================================
    // CURRENT TASK
    // =========================================================

    private void UpdatePeristalsisTask()
    {
        if (foodStageController == null)
            return;

        UpdatePeristalsisTask(
            foodStageController
                .SuccessfulWavesInCurrentStage,

            foodStageController
                .WavesPerStage
        );
    }

    private void UpdatePeristalsisTask(
        int currentWaves,
        int requiredWaves)
    {
        SetCurrentTask(
            "Влачейки маркера през обозначените " +
            "полета, извърши перисталтични вълни. " +
            $"{currentWaves}/{requiredWaves}"
        );
    }

    private void SetCurrentTask(
        string taskText)
    {
        if (currentTaskText == null)
            return;

        bool textChanged =
            currentTaskText.text != taskText;

        currentTaskText.text =
            taskText;

        if (textChanged &&
            currentTaskTextAnimation != null)
        {
            currentTaskTextAnimation
                .Play();
        }
    }
}