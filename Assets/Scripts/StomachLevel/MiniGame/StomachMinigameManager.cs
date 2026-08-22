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

    [Header("Current Task")]
    [SerializeField] private TMP_Text currentTaskText;

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

    // =========================================================
    // UNITY
    // =========================================================

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
            StopCoroutine(progressWaitCoroutine);
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

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        StartCoroutine(
            InitializeNextFrame()
        );
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        BeginCurrentStage();
    }

    // =========================================================
    // CURRENT FOOD STAGE
    // =========================================================

    private void BeginCurrentStage()
    {
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
            juicesButton.interactable = true;
        }

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        SetCurrentTask(
            "Добави стомашни сокове."
        );

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

        // Не стартираме перисталтиката веднага.
        // Първо изчакваме ProgressBar.
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
            stateCardController.ShowChyme();
        }

        UpdatePeristalsisTask();

        // ВАЖНО:
        // TaskTimer се стартира чак тук,
        // т.е. след ProgressBar анимацията.
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

        // =====================================================
        // ЦЕЛИЯТ WAVE SET Е ЗАВЪРШЕН
        // =====================================================

        if (currentWaves >= requiredWaves)
        {
            if (taskTimer != null)
            {
                taskTimer.StopTimer();
            }

            // Оттук:
            // Food transition
            // ↓
            // FoodStageAdvanced event
            // ↓
            // Progress animation
            // ↓
            // следваща задача
            if (foodStageController != null)
            {
                foodStageController
                    .AdvanceAfterCompletedWaveSet();
            }

            yield break;
        }

        // =====================================================
        // ИМА ОЩЕ ВЪЛНИ В СЪЩИЯ ЕТАП
        // =====================================================

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

        // Тук няма ProgressBar анимация,
        // защото progress се дава само след
        // целия завършен wave stage.
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
            foodStageController.IsHardDifficulty)
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
            foodStageController.TotalStageCount - 2)
        {
            if (stateCardController != null)
            {
                stateCardController
                    .ShowChyme();
            }
        }

        // Food анимацията вече е приключила.
        // Сега ProgressBar започва неговата анимация.
        CurrentState =
            GameState.ProgressFeedback;

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (newStageIndex >=
            foodStageController.TotalStageCount - 1)
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
        // FoodStageAdvanced се извиква непосредствено
        // преди DigestionCompleted и вече е стартирал
        // чакането на ProgressBar.
        //
        // Затова НЕ влизаме веднага във финалното
        // състояние и не прескачаме progress анимацията.

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
        // ВАЖНО:
        // ProgressBar и Manager слушат един и същи event.
        // Изчакваме 1 frame, за да сме сигурни,
        // че ProgressBar вече е стартирал coroutine-а си.
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

        if (intestineButton != null)
        {
            intestineButton.interactable =
                false;
        }

        if (taskTimer != null)
        {
            taskTimer.StopTimer();
        }

        IntestineTransferRequested?.Invoke();
    }

    public void CompleteMinigameAfterIntestineTransfer()
    {
        if (CurrentState !=
            GameState.TransferringToIntestine)
        {
            return;
        }

        CurrentState =
            GameState.Finished;

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
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
        if (currentTaskText != null)
        {
            currentTaskText.text =
                taskText;
        }
    }
}