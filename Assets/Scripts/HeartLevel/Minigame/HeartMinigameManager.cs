using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeartMinigameManager : MonoBehaviour
{
    public enum PulseState
    {
        Normal,
        DangerLow,
        DangerHigh,
        CriticalLow,
        CriticalHigh
    }

    // =====================================================
    // PULSE
    // =====================================================

    [Header("Pulse")]
    [SerializeField] private int currentBPM = 80;

    // =====================================================
    // PANELS
    // =====================================================

    [Header("Panels")]
    [SerializeField] private GameObject minigamePanel;

    // =====================================================
    // WELCOME
    // =====================================================

    [Header("Welcome")]
    [SerializeField] private GameObject welcomeOverlay;
    [SerializeField] private UIPopupAnimation welcomePanelAnimation;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField] private HeartMinigameAudio minigameAudio;

    // =====================================================
    // PAUSE
    // =====================================================

    [Header("Pause")]
    [SerializeField] private GameObject pauseOverlay;

    // =====================================================
    // SETTINGS
    // =====================================================

    [Header("Settings")]
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private DifficultySelector difficultySelector;

    // =====================================================
    // INFO
    // =====================================================

    [Header("Info")]
    [SerializeField] private GameObject infoOverlay;

    // =====================================================
    // EXIT CONFIRMATION
    // =====================================================

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationOverlay;

    private bool isPaused;
    private bool menuTransitionInProgress;

    public bool IsPaused
    {
        get { return isPaused; }
    }

    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]
    [SerializeField] private TMP_Text pulseValueText;

    // =====================================================
    // GAME TIMER
    // =====================================================

    [Header("Game Timer")]
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private float easyGameDuration = 50f;
    [SerializeField] private float mediumGameDuration = 60f;
    [SerializeField] private float hardGameDuration = 70f;

    [Header("Warning Pulse")]
    [SerializeField] private RectTransform pulseTarget;

    [SerializeField] private float warningTime = 10f;
    [SerializeField] private float criticalTime = 5f;

    [SerializeField] private float warningPulseScale = 1.05f;
    [SerializeField] private float warningPulseUpDuration = 0.28f;
    [SerializeField] private float warningPulseDownDuration = 0.28f;

    [SerializeField] private float criticalPulseScale = 1.08f;
    [SerializeField] private float criticalPulseUpDuration = 0.16f;
    [SerializeField] private float criticalPulseDownDuration = 0.16f;

    [Header("Minigame Success")]
    [SerializeField] private GameObject minigameSuccessOverlay;

    // =====================================================
    // ECG
    // =====================================================

    [Header("ECG")]
    [SerializeField] private ECGLineGraphic ecgLine;

    // =====================================================
    // HEART STATES
    // =====================================================

    [Header("Heart States")]
    [SerializeField] private GameObject heartNormal;
    [SerializeField] private GameObject heartLow;
    [SerializeField] private GameObject heartHigh;
    [SerializeField] private GameObject heartDead;

    // =====================================================
    // HEART ANIMATION
    // =====================================================

    [Header("Heart Animation")]
    [SerializeField] private HeartBeatAnimation heartBeatAnimation;
    [SerializeField] private HeartStateTransitionAnimation heartStateTransitionAnimation;
    [SerializeField] private CriticalHeartShake criticalHeartShake;

    // =====================================================
    // LIVES
    // =====================================================

    [Header("Lives")]
    [SerializeField] private HeartMinigameLives heartLives;

    // =====================================================
    // GAME OVER
    // =====================================================

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    // =====================================================
    // WARNING PANELS
    // =====================================================

    [Header("Warning Panels")]
    [SerializeField] private GameObject highPulseWarningPanel;
    [SerializeField] private GameObject lowPulseWarningPanel;

    [SerializeField] private Image highWarningFill;
    [SerializeField] private Image lowWarningFill;

    // =====================================================
    // DANGER COUNTDOWN
    // =====================================================

    [Header("Danger Countdown By Difficulty")]
    [SerializeField] private float easyDangerDuration = 4f;
    [SerializeField] private float mediumDangerDuration = 3f;
    [SerializeField] private float hardDangerDuration = 2f;

    // =====================================================
    // DRIFT
    // =====================================================

    [Header("Drift By Difficulty")]
    [SerializeField] private int easyDriftAmount = 7;
    [SerializeField] private int mediumDriftAmount = 10;
    [SerializeField] private int hardDriftAmount = 15;

    [SerializeField] private float easyDriftInterval = 4f;
    [SerializeField] private float mediumDriftInterval = 3f;
    [SerializeField] private float hardDriftInterval = 2f;

    // =====================================================
    // EVENTS
    // =====================================================

    [Header("Pulse Increase Event Cards")]
    [SerializeField] private GameObject angerCard;
    [SerializeField] private GameObject caffeineCard;
    [SerializeField] private GameObject fearCard;
    [SerializeField] private GameObject physicalStressCard;
    [SerializeField] private GameObject stressCard;

    [Header("Pulse Decrease Event Cards")]
    [SerializeField] private GameObject freezingCard;
    [SerializeField] private GameObject illnessCard;
    [SerializeField] private GameObject lazyCard;
    [SerializeField] private GameObject meditationCard;
    [SerializeField] private GameObject sleepyCard;

    [Header("Event Interval By Difficulty")]
    [SerializeField] private float easyEventInterval = 6f;
    [SerializeField] private float mediumEventInterval = 5f;
    [SerializeField] private float hardEventInterval = 4f;

    [Header("Drift / Event Protection")]
    [Tooltip("Минимално време между drift и event.")]
    [SerializeField] private float minimumDriftEventGap = 1f;

    // =====================================================
    // KEYBOARD
    // =====================================================

    [Header("Keyboard Button Animation")]
    [SerializeField] private RectTransform decreaseButtonHitbox;
    [SerializeField] private RectTransform increaseButtonHitbox;

    [SerializeField] private float keyboardPressedScale = 0.96f;
    [SerializeField] private float keyboardPressDuration = 0.08f;

    private Coroutine decreaseAnimation;
    private Coroutine increaseAnimation;

    // =====================================================
    // INTERNAL VALUES
    // =====================================================

    private PulseState currentPulseState;

    private float dangerDuration;
    private float dangerTimer;
    private bool dangerCountdownActive;

    private int driftAmount;
    private float driftInterval;
    private float driftTimer;

    private float eventInterval;
    private float eventTimer;

    private float lastDriftTime = -999f;
    private float lastEventTime = -999f;

    private int lastEventIndex = -1;

    private float gameDuration;
    private float remainingTime;

    private Vector3 normalPulseScale;
    private Coroutine pulseCoroutine;

    private bool criticalImmediateLossUsed;

    private bool isGameOver;
    private bool isMinigameWon;

    private bool minigameStarted;
    private bool welcomeIsClosing;
    private bool infoOpenedFromWelcome;

    private int startingBPM;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        startingBPM = currentBPM;

        if (pulseTarget != null)
        {
            normalPulseScale =
                pulseTarget.localScale;
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        isGameOver = false;
        isMinigameWon = false;
        isPaused = false;

        minigameStarted = false;
        welcomeIsClosing = false;
        infoOpenedFromWelcome = false;
        menuTransitionInProgress = false;

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (difficultySelector != null)
            difficultySelector.SetLocked(true);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);

        if (minigameSuccessOverlay != null)
            minigameSuccessOverlay.SetActive(false);

        SetDifficultyValues();

        remainingTime = gameDuration;

        StopWarningPulse();
        ResetPulseVisual();

        UpdateTimerUI();

        if (highPulseWarningPanel != null)
            highPulseWarningPanel.SetActive(false);

        if (lowPulseWarningPanel != null)
            lowPulseWarningPanel.SetActive(false);

        if (highWarningFill != null)
            highWarningFill.fillAmount = 0f;

        if (lowWarningFill != null)
            lowWarningFill.fillAmount = 0f;

        driftTimer = 0f;
        eventTimer = 0f;

        lastDriftTime = -999f;
        lastEventTime = -999f;

        HideAllEventCards();

        UpdatePulseUI();
        UpdatePulseState();
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!minigameStarted)
            return;

        if (isGameOver || isMinigameWon)
            return;

        if (isPaused)
            return;

        if (minigamePanel == null ||
            !minigamePanel.activeInHierarchy)
        {
            driftTimer = 0f;
            eventTimer = 0f;
            return;
        }

        HandleKeyboardInput();

        UpdateDangerCountdown();

        if (heartLives != null &&
            !heartLives.HasLives)
        {
            return;
        }

        UpdateGameTimer();

        if (isMinigameWon)
            return;

        UpdateEvents();
        UpdatePulseDrift();
    }

    // =====================================================
    // OVERLAY HELPERS
    // =====================================================

    private void ShowOverlay(GameObject overlay)
    {
        if (overlay == null)
            return;

        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
    }

    private void CloseOverlay(
        GameObject overlay,
        Action onClosed = null
    )
    {
        if (overlay == null)
        {
            onClosed?.Invoke();
            return;
        }

        UIPopupAnimation animation =
            overlay.GetComponentInChildren<UIPopupAnimation>(true);

        if (animation != null)
        {
            animation.PlayClose(
                () =>
                {
                    overlay.SetActive(false);
                    onClosed?.Invoke();
                }
            );
        }
        else
        {
            overlay.SetActive(false);
            onClosed?.Invoke();
        }
    }

    // =====================================================
    // WELCOME
    // =====================================================

    public void PrepareForWelcome()
    {
        minigameStarted = false;
        welcomeIsClosing = false;
        infoOpenedFromWelcome = false;
        menuTransitionInProgress = false;

        isGameOver = false;
        isMinigameWon = false;
        isPaused = false;

        currentBPM = startingBPM;

        criticalImmediateLossUsed = false;

        SetDifficultyValues();

        remainingTime = gameDuration;

        StopWarningPulse();
        ResetPulseVisual();

        driftTimer = 0f;
        eventTimer = 0f;

        lastDriftTime = -999f;
        lastEventTime = -999f;
        lastEventIndex = -1;

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);

        if (minigameSuccessOverlay != null)
            minigameSuccessOverlay.SetActive(false);

        ResetDangerCountdown();
        HideAllEventCards();

        UpdateTimerUI();
        UpdatePulseUI();
        UpdatePulseState();

        if (welcomeOverlay != null)
        {
            ShowOverlay(welcomeOverlay);

            if (welcomePanelAnimation != null)
            {
                welcomePanelAnimation.PlayOpen();
            }
        }
    }

    // =====================================================
    // START FROM WELCOME
    // =====================================================

    public void StartMinigameFromWelcome()
    {
        if (minigameStarted ||
            welcomeIsClosing)
        {
            return;
        }

        welcomeIsClosing = true;

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                BeginMinigameAfterWelcome
            );
        }
        else
        {
            BeginMinigameAfterWelcome();
        }
    }

    private void BeginMinigameAfterWelcome()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.SetActive(false);
        }

        welcomeIsClosing = false;
        minigameStarted = true;
        infoOpenedFromWelcome = false;

        remainingTime = gameDuration;

        StopWarningPulse();
        ResetPulseVisual();

        driftTimer = 0f;
        eventTimer = 0f;

        lastDriftTime = -999f;
        lastEventTime = -999f;
        lastEventIndex = -1;

        criticalImmediateLossUsed = false;

        ResetDangerCountdown();
        HideAllEventCards();

        UpdateTimerUI();

        Debug.Log("HEART MINIGAME STARTED");
    }

    // =====================================================
    // INFO FROM WELCOME
    // =====================================================

    public void OpenInfoFromWelcome()
    {
        if (minigameStarted ||
            welcomeIsClosing ||
            menuTransitionInProgress)
        {
            return;
        }

        infoOpenedFromWelcome = true;
        welcomeIsClosing = true;

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                OpenInfoAfterWelcomeClosed
            );
        }
        else
        {
            OpenInfoAfterWelcomeClosed();
        }
    }

    private void OpenInfoAfterWelcomeClosed()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.SetActive(false);
        }

        welcomeIsClosing = false;

        ShowOverlay(infoOverlay);
    }

    public void ShowWelcomeAfterInfo()
    {
        if (minigameStarted)
            return;

        welcomeIsClosing = false;

        if (welcomeOverlay == null)
            return;

        ShowOverlay(welcomeOverlay);

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayOpen();
        }
    }

    // =====================================================
    // CLOSE INFO
    // =====================================================

    public void CloseInfo()
    {
        if (menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        CloseOverlay(
            infoOverlay,
            () =>
            {
                if (infoOpenedFromWelcome)
                {
                    infoOpenedFromWelcome = false;
                    menuTransitionInProgress = false;

                    ShowWelcomeAfterInfo();

                    return;
                }

                if (isPaused)
                {
                    ShowOverlay(pauseOverlay);
                }

                menuTransitionInProgress = false;

                Debug.Log(
                    "INFO CLOSED"
                );
            }
        );
    }

    // =====================================================
    // DIFFICULTY
    // =====================================================

    private void SetDifficultyValues()
    {
        int difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        switch (difficulty)
        {
            case 0:
                dangerDuration = easyDangerDuration;
                driftAmount = easyDriftAmount;
                driftInterval = easyDriftInterval;
                eventInterval = easyEventInterval;
                gameDuration = easyGameDuration;
                break;

            case 2:
                dangerDuration = hardDangerDuration;
                driftAmount = hardDriftAmount;
                driftInterval = hardDriftInterval;
                eventInterval = hardEventInterval;
                gameDuration = hardGameDuration;
                break;

            default:
                dangerDuration = mediumDangerDuration;
                driftAmount = mediumDriftAmount;
                driftInterval = mediumDriftInterval;
                eventInterval = mediumEventInterval;
                gameDuration = mediumGameDuration;
                break;
        }
    }

    // =====================================================
    // MAIN GAME TIMER
    // =====================================================

    private void UpdateGameTimer()
    {
        if (remainingTime <= 0f)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            StopWarningPulse();
            ResetPulseVisual();

            UpdateTimerUI();
            TriggerMinigameSuccess();

            return;
        }

        UpdateTimerUI();
        UpdateWarningPulseState();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }

    // =====================================================
    // WARNING PULSE
    // =====================================================

    private void UpdateWarningPulseState()
    {
        if (pulseTarget == null)
            return;

        bool shouldPulse =
            minigameStarted &&
            !isGameOver &&
            !isMinigameWon &&
            !isPaused &&
            remainingTime > 0f &&
            remainingTime <= warningTime;

        if (shouldPulse)
        {
            if (pulseCoroutine == null)
            {
                pulseCoroutine =
                    StartCoroutine(
                        WarningPulseLoop()
                    );
            }
        }
        else
        {
            StopWarningPulse();
            ResetPulseVisual();
        }
    }

    private IEnumerator WarningPulseLoop()
    {
        while (
            minigameStarted &&
            !isGameOver &&
            !isMinigameWon &&
            !isPaused &&
            remainingTime > 0f &&
            remainingTime <= warningTime
        )
        {
            bool isCritical =
                remainingTime <= criticalTime;

            float targetScale =
                isCritical
                    ? criticalPulseScale
                    : warningPulseScale;

            float upDuration =
                isCritical
                    ? criticalPulseUpDuration
                    : warningPulseUpDuration;

            float downDuration =
                isCritical
                    ? criticalPulseDownDuration
                    : warningPulseDownDuration;

            yield return ScalePulseTo(
                normalPulseScale *
                targetScale,
                upDuration
            );

            yield return ScalePulseTo(
                normalPulseScale,
                downDuration
            );
        }

        ResetPulseVisual();
        pulseCoroutine = null;
    }

    private IEnumerator ScalePulseTo(
        Vector3 targetScale,
        float duration
    )
    {
        if (pulseTarget == null)
            yield break;

        Vector3 startScale =
            pulseTarget.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (
                isPaused ||
                isGameOver ||
                isMinigameWon ||
                !minigameStarted
            )
            {
                yield break;
            }

            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            pulseTarget.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        pulseTarget.localScale =
            targetScale;
    }

    private void StopWarningPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(
                pulseCoroutine
            );

            pulseCoroutine = null;
        }
    }

    private void ResetPulseVisual()
    {
        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                normalPulseScale;
        }
    }

    private void TriggerMinigameSuccess()
    {
        if (isGameOver || isMinigameWon)
            return;

        isMinigameWon = true;
        remainingTime = 0f;

        StopWarningPulse();
        ResetPulseVisual();

        UpdateTimerUI();
        ResetDangerCountdown();
        HideAllEventCards();

        // =====================================================
        // SCORE
        // =====================================================

        if (heartLives == null)
        {
            Debug.LogError(
                "HeartMinigameLives не е свързан. " +
                "Minigame score не може да бъде записан."
            );
        }
        else if (HeartScoreManager.Instance == null)
        {
            Debug.LogError(
                "HeartScoreManager.Instance липсва. " +
                "Minigame score не може да бъде записан."
            );
        }
        else
        {
            int remainingLives =
                heartLives.CurrentLives;

            int startingLives =
                heartLives.MaxLives;

            HeartScoreManager.Instance.SubmitMinigameResult(
                remainingLives,
                startingLives
            );

            Debug.Log(
                $"Heart Minigame Score записан. " +
                $"Животи: {remainingLives}/{startingLives} | " +
                $"Performance: " +
                $"{HeartScoreManager.Instance.MinigamePerformance:P0}"
            );
        }

        // =====================================================
        // AUDIO
        // =====================================================

        if (minigameAudio != null)
        {
            minigameAudio.StopECG();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayLobbyMusic();
        }

        // =====================================================
        // SUCCESS
        // =====================================================

        if (minigameSuccessOverlay != null)
        {
            minigameSuccessOverlay.SetActive(true);
        }
    }

    // =====================================================
    // EVENT SYSTEM
    // =====================================================

    private void UpdateEvents()
    {
        eventTimer += Time.deltaTime;

        if (eventTimer < eventInterval)
            return;

        if (Time.time - lastDriftTime <
            minimumDriftEventGap)
        {
            return;
        }

        eventTimer = 0f;

        TriggerRandomEvent();
    }

    private void TriggerRandomEvent()
    {
        int eventIndex;

        do
        {
            eventIndex =
                UnityEngine.Random.Range(0, 10);
        }
        while (eventIndex == lastEventIndex);

        lastEventIndex = eventIndex;
        lastEventTime = Time.time;

        driftTimer = 0f;

        switch (eventIndex)
        {
            case 0:
                ShowEventCard(angerCard);
                ApplyEventBPM(40, "СИЛЕН ГНЯВ");
                break;

            case 1:
                ShowEventCard(caffeineCard);
                ApplyEventBPM(25, "КОФЕИН");
                break;

            case 2:
                ShowEventCard(fearCard);
                ApplyEventBPM(30, "УПЛАХ");
                break;

            case 3:
                ShowEventCard(physicalStressCard);
                ApplyEventBPM(
                    35,
                    "ФИЗИЧЕСКО НАТОВАРВАНЕ"
                );
                break;

            case 4:
                ShowEventCard(stressCard);
                ApplyEventBPM(25, "СТРЕС");
                break;

            case 5:
                ShowEventCard(freezingCard);
                ApplyEventBPM(
                    -30,
                    "СИЛНО ОХЛАЖДАНЕ"
                );
                break;

            case 6:
                ShowEventCard(illnessCard);
                ApplyEventBPM(-30, "БОЛЕСТ");
                break;

            case 7:
                ShowEventCard(lazyCard);
                ApplyEventBPM(
                    -25,
                    "ПРОДЪЛЖИТЕЛЕН ПОКОЙ"
                );
                break;

            case 8:
                ShowEventCard(meditationCard);
                ApplyEventBPM(-15, "МЕДИТАЦИЯ");
                break;

            case 9:
                ShowEventCard(sleepyCard);
                ApplyEventBPM(-20, "СЪНЛИВОСТ");
                break;
        }
    }

    private void ShowEventCard(GameObject card)
    {
        HideAllEventCards();

        if (card != null)
        {
            card.SetActive(true);

            if (minigameAudio != null)
            {
                minigameAudio.PlayEventPop();
            }
        }
    }

    private void HideAllEventCards()
    {
        if (angerCard != null)
            angerCard.SetActive(false);

        if (caffeineCard != null)
            caffeineCard.SetActive(false);

        if (fearCard != null)
            fearCard.SetActive(false);

        if (physicalStressCard != null)
            physicalStressCard.SetActive(false);

        if (stressCard != null)
            stressCard.SetActive(false);

        if (freezingCard != null)
            freezingCard.SetActive(false);

        if (illnessCard != null)
            illnessCard.SetActive(false);

        if (lazyCard != null)
            lazyCard.SetActive(false);

        if (meditationCard != null)
            meditationCard.SetActive(false);

        if (sleepyCard != null)
            sleepyCard.SetActive(false);
    }

    private void ApplyEventBPM(
        int bpmChange,
        string eventName
    )
    {
        if (!minigameStarted ||
            isGameOver ||
            isMinigameWon ||
            isPaused)
        {
            return;
        }

        currentBPM += bpmChange;

        UpdatePulseUI();
        UpdatePulseState();

        string sign =
            bpmChange > 0
                ? "+"
                : "";

        Debug.Log(
            "EVENT: " +
            eventName +
            " | " +
            sign +
            bpmChange +
            " BPM" +
            " | Current BPM: " +
            currentBPM
        );
    }

    // =====================================================
    // DRIFT
    // =====================================================

    private void UpdatePulseDrift()
    {
        driftTimer += Time.deltaTime;

        if (driftTimer < driftInterval)
            return;

        if (Time.time - lastEventTime <
            minimumDriftEventGap)
        {
            return;
        }

        driftTimer = 0f;
        lastDriftTime = Time.time;

        int direction =
            UnityEngine.Random.value < 0.5f
                ? -1
                : 1;

        currentBPM +=
            driftAmount * direction;

        UpdatePulseUI();
        UpdatePulseState();
    }

    // =====================================================
    // INPUT
    // =====================================================

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (
            Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame
        )
        {
            DecreasePulse();

            UISoundManager.Instance?.PlayClick();

            if (decreaseButtonHitbox != null)
            {
                if (decreaseAnimation != null)
                {
                    StopCoroutine(decreaseAnimation);
                }

                decreaseAnimation =
                    StartCoroutine(
                        PlayKeyboardPressAnimation(
                            decreaseButtonHitbox
                        )
                    );
            }
        }

        if (
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame
        )
        {
            IncreasePulse();

            UISoundManager.Instance?.PlayClick();

            if (increaseButtonHitbox != null)
            {
                if (increaseAnimation != null)
                {
                    StopCoroutine(increaseAnimation);
                }

                increaseAnimation =
                    StartCoroutine(
                        PlayKeyboardPressAnimation(
                            increaseButtonHitbox
                        )
                    );
            }
        }
    }

    // =====================================================
    // WARNING COUNTDOWN
    // =====================================================

    private void UpdateDangerCountdown()
    {
        bool needsCountdown =
            currentPulseState == PulseState.DangerLow ||
            currentPulseState == PulseState.DangerHigh ||
            (
                currentPulseState == PulseState.CriticalLow &&
                criticalImmediateLossUsed
            )
            ||
            (
                currentPulseState == PulseState.CriticalHigh &&
                criticalImmediateLossUsed
            );

        if (!needsCountdown)
        {
            ResetDangerCountdown();
            return;
        }

        if (!dangerCountdownActive)
        {
            dangerCountdownActive = true;
            dangerTimer = 0f;
        }

        dangerTimer += Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                dangerTimer / dangerDuration
            );

        bool isLow =
            currentPulseState == PulseState.DangerLow ||
            currentPulseState == PulseState.CriticalLow;

        bool isHigh =
            currentPulseState == PulseState.DangerHigh ||
            currentPulseState == PulseState.CriticalHigh;

        if (isLow)
        {
            if (lowPulseWarningPanel != null)
                lowPulseWarningPanel.SetActive(true);

            if (highPulseWarningPanel != null)
                highPulseWarningPanel.SetActive(false);

            if (lowWarningFill != null)
                lowWarningFill.fillAmount = progress;

            if (highWarningFill != null)
                highWarningFill.fillAmount = 0f;
        }

        if (isHigh)
        {
            if (highPulseWarningPanel != null)
                highPulseWarningPanel.SetActive(true);

            if (lowPulseWarningPanel != null)
                lowPulseWarningPanel.SetActive(false);

            if (highWarningFill != null)
                highWarningFill.fillAmount = progress;

            if (lowWarningFill != null)
                lowWarningFill.fillAmount = 0f;
        }

        if (progress >= 1f)
        {
            LoseLife();
            ResetDangerCountdown();
        }
    }

    private void ResetDangerCountdown()
    {
        dangerCountdownActive = false;
        dangerTimer = 0f;

        if (highPulseWarningPanel != null)
            highPulseWarningPanel.SetActive(false);

        if (lowPulseWarningPanel != null)
            lowPulseWarningPanel.SetActive(false);

        if (highWarningFill != null)
            highWarningFill.fillAmount = 0f;

        if (lowWarningFill != null)
            lowWarningFill.fillAmount = 0f;
    }

    // =====================================================
    // LIVES
    // =====================================================

    private void LoseLife()
    {
        if (!minigameStarted)
            return;

        if (heartLives == null)
            return;

        if (!heartLives.HasLives ||
            isGameOver ||
            isMinigameWon ||
            isPaused)
        {
            return;
        }

        heartLives.LoseLife(
            () =>
            {
                if (isMinigameWon)
                    return;

                if (!heartLives.HasLives)
                {
                    TriggerGameOver();
                }
            }
        );
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void TriggerGameOver()
    {
        if (isGameOver || isMinigameWon)
            return;

        isGameOver = true;

        StopWarningPulse();
        ResetPulseVisual();

        ResetDangerCountdown();
        HideAllEventCards();

        if (minigameAudio != null)
        {
            minigameAudio.StopECG();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayLobbyMusic();
        }

        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(true);

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(true);
        }
    }

    // =====================================================
    // KEYBOARD ANIMATION
    // =====================================================

    private IEnumerator PlayKeyboardPressAnimation(
        RectTransform button
    )
    {
        Vector3 originalScale = Vector3.one;

        Vector3 pressedScale =
            originalScale * keyboardPressedScale;

        float timer = 0f;

        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale =
                Vector3.Lerp(
                    originalScale,
                    pressedScale,
                    timer / keyboardPressDuration
                );

            yield return null;
        }

        timer = 0f;

        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale =
                Vector3.Lerp(
                    pressedScale,
                    originalScale,
                    timer / keyboardPressDuration
                );

            yield return null;
        }

        button.localScale = originalScale;
    }

    // =====================================================
    // UI
    // =====================================================

    private void UpdatePulseUI()
    {
        if (pulseValueText != null)
        {
            pulseValueText.text =
                currentBPM.ToString();
        }

        if (ecgLine != null)
        {
            ecgLine.SetBPM(currentBPM);
        }

        if (heartBeatAnimation != null)
        {
            heartBeatAnimation.SetBPM(currentBPM);
        }
    }

    // =====================================================
    // PULSE STATE
    // =====================================================

    private void UpdatePulseState()
    {
        PulseState previousState =
            currentPulseState;

        if (currentBPM >= 70 &&
            currentBPM <= 100)
        {
            currentPulseState =
                PulseState.Normal;
        }
        else if (currentBPM >= 40 &&
                 currentBPM < 70)
        {
            currentPulseState =
                PulseState.DangerLow;
        }
        else if (currentBPM > 100 &&
                 currentBPM <= 130)
        {
            currentPulseState =
                PulseState.DangerHigh;
        }
        else if (currentBPM < 40)
        {
            currentPulseState =
                PulseState.CriticalLow;
        }
        else
        {
            currentPulseState =
                PulseState.CriticalHigh;
        }

        bool stateChanged =
            previousState != currentPulseState;

        bool enteredCritical =
            stateChanged &&
            (
                currentPulseState == PulseState.CriticalLow ||
                currentPulseState == PulseState.CriticalHigh
            );

        HandleCriticalState(previousState);

        UpdateHeartVisual();

        if (stateChanged &&
            !isGameOver &&
            !isMinigameWon &&
            heartStateTransitionAnimation != null)
        {
            heartStateTransitionAnimation.PlayStateChange();
        }

        if (enteredCritical &&
            !isGameOver &&
            !isMinigameWon &&
            criticalHeartShake != null)
        {
            criticalHeartShake.PlayShake();
        }
    }

    private void HandleCriticalState(
        PulseState previousState
    )
    {
        bool isCritical =
            currentPulseState == PulseState.CriticalLow ||
            currentPulseState == PulseState.CriticalHigh;

        bool wasCritical =
            previousState == PulseState.CriticalLow ||
            previousState == PulseState.CriticalHigh;

        if (!isCritical)
        {
            criticalImmediateLossUsed = false;
            return;
        }

        if (!wasCritical &&
            !criticalImmediateLossUsed)
        {
            criticalImmediateLossUsed = true;

            ResetDangerCountdown();
            LoseLife();
        }
    }

    // =====================================================
    // HEART VISUAL
    // =====================================================

    private void UpdateHeartVisual()
    {
        if (isGameOver || isMinigameWon)
            return;

        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(false);

        switch (currentPulseState)
        {
            case PulseState.Normal:

                if (heartNormal != null)
                    heartNormal.SetActive(true);

                break;

            case PulseState.DangerLow:

                if (heartLow != null)
                    heartLow.SetActive(true);

                break;

            case PulseState.DangerHigh:

                if (heartHigh != null)
                    heartHigh.SetActive(true);

                break;

            case PulseState.CriticalLow:
            case PulseState.CriticalHigh:

                if (heartDead != null)
                    heartDead.SetActive(true);

                break;
        }
    }

    // =====================================================
    // PLAYER CONTROLS
    // =====================================================

    public void DecreasePulse()
    {
        if (!minigameStarted ||
            isGameOver ||
            isMinigameWon ||
            isPaused)
        {
            return;
        }

        currentBPM -= 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    public void IncreasePulse()
    {
        if (!minigameStarted ||
            isGameOver ||
            isMinigameWon ||
            isPaused)
        {
            return;
        }

        currentBPM += 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    // =====================================================
    // PAUSE
    // =====================================================

    public void OpenPauseMenu()
    {
        if (!minigameStarted ||
            isGameOver ||
            isMinigameWon ||
            isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        isPaused = true;
        infoOpenedFromWelcome = false;

        StopWarningPulse();
        ResetPulseVisual();

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        ShowOverlay(pauseOverlay);
    }

    public void ResumeGame()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlay(
            pauseOverlay,
            () =>
            {
                isPaused = false;
                menuTransitionInProgress = false;

                UpdateWarningPulseState();

                Debug.Log(
                    "MINIGAME RESUMED"
                );
            }
        );
    }

    // =====================================================
    // SETTINGS FROM PAUSE
    // =====================================================

    public void OpenSettingsFromPause()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }

        CloseOverlay(
            pauseOverlay,
            () =>
            {
                ShowOverlay(settingsOverlay);
                menuTransitionInProgress = false;
            }
        );
    }

    public void CloseSettingsToPause()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlay(
            settingsOverlay,
            () =>
            {
                ShowOverlay(pauseOverlay);
                menuTransitionInProgress = false;

                Debug.Log(
                    "RETURNED TO PAUSE FROM SETTINGS"
                );
            }
        );
    }

    // =====================================================
    // INFO FROM PAUSE
    // =====================================================

    public void OpenInfoFromPause()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = true;

        CloseOverlay(
            pauseOverlay,
            () =>
            {
                ShowOverlay(infoOverlay);
                menuTransitionInProgress = false;

                Debug.Log(
                    "INFO OPENED FROM PAUSE"
                );
            }
        );
    }

    public void CloseInfoToPause()
    {
        CloseInfo();
    }

    public void ReturnToPauseAfterInfo()
    {
        CloseInfo();
    }

    // =====================================================
    // EXIT FROM PAUSE
    // =====================================================

    public void OpenExitConfirmationFromPause()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlay(
            pauseOverlay,
            () =>
            {
                ShowOverlay(exitConfirmationOverlay);
                menuTransitionInProgress = false;

                Debug.Log(
                    "EXIT CONFIRMATION OPENED"
                );
            }
        );
    }

    public void CloseExitConfirmationToPause()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlay(
            exitConfirmationOverlay,
            () =>
            {
                ShowOverlay(pauseOverlay);
                menuTransitionInProgress = false;

                Debug.Log(
                    "RETURNED TO PAUSE FROM EXIT CONFIRMATION"
                );
            }
        );
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    // =====================================================
    // RETRY
    // =====================================================

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}