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
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField]
    private HeartMinigameAudio minigameAudio;

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

    private bool criticalImmediateLossUsed;

    private bool isGameOver;
    private bool isMinigameWon;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        isGameOver = false;
        isMinigameWon = false;
        isPaused = false;

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

        Debug.Log(
            "Difficulty: " +
            difficulty +
            " | Game duration: " +
            gameDuration +
            " sec." +
            " | Danger countdown: " +
            dangerDuration +
            " sec." +
            " | Drift: " +
            driftAmount +
            " BPM every " +
            driftInterval +
            " sec." +
            " | Event every " +
            eventInterval +
            " sec."
        );
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

            UpdateTimerUI();
            TriggerMinigameSuccess();

            return;
        }

        UpdateTimerUI();
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

    private void TriggerMinigameSuccess()
    {
        if (isGameOver || isMinigameWon)
            return;

        isMinigameWon = true;
        remainingTime = 0f;

        UpdateTimerUI();
        ResetDangerCountdown();
        HideAllEventCards();

        // Спираме ECG monitor звука.
        if (minigameAudio != null)
        {
            minigameAudio.StopECG();
        }

        // Heart Minigame музиката се заменя
        // с Lobby / Main Menu музиката.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayLobbyMusic();
        }

        // FeedbackPanelSound на този overlay
        // пуска Success звука.
        //
        // Той автоматично duck-ва Lobby музиката,
        // докато Success звукът приключи.
        if (minigameSuccessOverlay != null)
        {
            minigameSuccessOverlay.SetActive(true);
        }

        Debug.Log(
            "MINIGAME SUCCESS! Timer reached 00:00."
        );
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
                Random.Range(0, 10);
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
        if (isGameOver ||
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
            Random.value < 0.5f
                ? -1
                : 1;

        currentBPM +=
            driftAmount * direction;

        UpdatePulseUI();
        UpdatePulseState();

        Debug.Log(
            "Pulse drift: " +
            (direction > 0 ? "+" : "-") +
            driftAmount +
            " BPM | Current BPM: " +
            currentBPM
        );
    }

    // =====================================================
    // INPUT
    // =====================================================

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (
            Keyboard.current
                .downArrowKey
                .wasPressedThisFrame
            ||
            Keyboard.current
                .sKey
                .wasPressedThisFrame
        )
        {
            DecreasePulse();

            UISoundManager
                .Instance
                ?.PlayClick();

            if (decreaseButtonHitbox != null)
            {
                if (decreaseAnimation != null)
                {
                    StopCoroutine(
                        decreaseAnimation
                    );
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
            Keyboard.current
                .upArrowKey
                .wasPressedThisFrame
            ||
            Keyboard.current
                .wKey
                .wasPressedThisFrame
        )
        {
            IncreasePulse();

            UISoundManager
                .Instance
                ?.PlayClick();

            if (increaseButtonHitbox != null)
            {
                if (increaseAnimation != null)
                {
                    StopCoroutine(
                        increaseAnimation
                    );
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
            currentPulseState ==
                PulseState.DangerLow
            ||
            currentPulseState ==
                PulseState.DangerHigh
            ||
            (
                currentPulseState ==
                    PulseState.CriticalLow
                &&
                criticalImmediateLossUsed
            )
            ||
            (
                currentPulseState ==
                    PulseState.CriticalHigh
                &&
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
                dangerTimer /
                dangerDuration
            );

        bool isLow =
            currentPulseState ==
                PulseState.DangerLow
            ||
            currentPulseState ==
                PulseState.CriticalLow;

        bool isHigh =
            currentPulseState ==
                PulseState.DangerHigh
            ||
            currentPulseState ==
                PulseState.CriticalHigh;

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
        if (heartLives == null)
        {
            Debug.LogWarning(
                "HeartMinigameLives reference is missing!"
            );

            return;
        }

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

        Debug.Log(
            "Life lost. Remaining: " +
            heartLives.CurrentLives
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

        ResetDangerCountdown();
        HideAllEventCards();

        // Спираме ECG monitor звука.
        if (minigameAudio != null)
        {
            minigameAudio.StopECG();
        }

        // Heart Minigame музиката се заменя
        // с Lobby / Main Menu музиката.
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

        // FeedbackPanelSound на GameOverOverlay
        // пуска Game Over звука и автоматично
        // duck-ва Lobby музиката за неговата дължина.
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(true);
        }

        Debug.Log("GAME OVER!");
    }

    // =====================================================
    // KEYBOARD ANIMATION
    // =====================================================

    private IEnumerator PlayKeyboardPressAnimation(
        RectTransform button
    )
    {
        Vector3 originalScale =
            Vector3.one;

        Vector3 pressedScale =
            originalScale *
            keyboardPressedScale;

        float timer = 0f;

        while (timer < keyboardPressDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            button.localScale =
                Vector3.Lerp(
                    originalScale,
                    pressedScale,
                    timer /
                    keyboardPressDuration
                );

            yield return null;
        }

        timer = 0f;

        while (timer < keyboardPressDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            button.localScale =
                Vector3.Lerp(
                    pressedScale,
                    originalScale,
                    timer /
                    keyboardPressDuration
                );

            yield return null;
        }

        button.localScale =
            originalScale;
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
            ecgLine.SetBPM(
                currentBPM
            );
        }

        if (heartBeatAnimation != null)
        {
            heartBeatAnimation.SetBPM(
                currentBPM
            );
        }
    }

    // =====================================================
    // PULSE STATE
    // =====================================================

    private void UpdatePulseState()
    {
        PulseState previousState =
            currentPulseState;

        if (
            currentBPM >= 70 &&
            currentBPM <= 100
        )
        {
            currentPulseState =
                PulseState.Normal;
        }
        else if (
            currentBPM >= 40 &&
            currentBPM < 70
        )
        {
            currentPulseState =
                PulseState.DangerLow;
        }
        else if (
            currentBPM > 100 &&
            currentBPM <= 130
        )
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
                currentPulseState ==
                    PulseState.CriticalLow
                ||
                currentPulseState ==
                    PulseState.CriticalHigh
            );

        HandleCriticalState(
            previousState
        );

        UpdateHeartVisual();

        if (
            stateChanged &&
            !isGameOver &&
            !isMinigameWon &&
            heartStateTransitionAnimation != null
        )
        {
            heartStateTransitionAnimation
                .PlayStateChange();
        }

        if (
            enteredCritical &&
            !isGameOver &&
            !isMinigameWon &&
            criticalHeartShake != null
        )
        {
            criticalHeartShake.PlayShake();
        }
    }

    private void HandleCriticalState(
        PulseState previousState
    )
    {
        bool isCritical =
            currentPulseState ==
                PulseState.CriticalLow
            ||
            currentPulseState ==
                PulseState.CriticalHigh;

        bool wasCritical =
            previousState ==
                PulseState.CriticalLow
            ||
            previousState ==
                PulseState.CriticalHigh;

        if (!isCritical)
        {
            criticalImmediateLossUsed =
                false;

            return;
        }

        if (
            !wasCritical &&
            !criticalImmediateLossUsed
        )
        {
            criticalImmediateLossUsed =
                true;

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
        if (isGameOver ||
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
        if (isGameOver ||
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
        if (isGameOver || isMinigameWon)
            return;

        if (isPaused)
            return;

        isPaused = true;

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(true);

        Debug.Log("MINIGAME PAUSED");
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        isPaused = false;

        Debug.Log("MINIGAME RESUMED");
    }

    // =====================================================
    // SETTINGS FROM PAUSE
    // =====================================================

    public void OpenSettingsFromPause()
    {
        if (!isPaused)
            return;

        if (difficultySelector != null)
            difficultySelector.SetLocked(true);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(true);

        Debug.Log(
            "SETTINGS OPENED FROM PAUSE"
        );
    }

    public void CloseSettingsToPause()
    {
        if (!isPaused)
            return;

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(true);

        Debug.Log(
            "RETURNED TO PAUSE FROM SETTINGS"
        );
    }

    // =====================================================
    // INFO FROM PAUSE
    // =====================================================

    public void OpenInfoFromPause()
    {
        if (!isPaused)
            return;

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(true);

        Debug.Log(
            "INFO OPENED FROM PAUSE"
        );
    }

    public void CloseInfoToPause()
    {
        if (!isPaused)
            return;

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(true);

        Debug.Log(
            "RETURNED TO PAUSE FROM INFO"
        );
    }

    // =====================================================
    // EXIT FROM PAUSE
    // =====================================================

    public void OpenExitConfirmationFromPause()
    {
        if (!isPaused)
            return;

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (infoOverlay != null)
            infoOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(true);

        Debug.Log(
            "EXIT CONFIRMATION OPENED"
        );
    }

    public void CloseExitConfirmationToPause()
    {
        if (!isPaused)
            return;

        if (exitConfirmationOverlay != null)
            exitConfirmationOverlay.SetActive(false);

        if (pauseOverlay != null)
            pauseOverlay.SetActive(true);

        Debug.Log(
            "RETURNED TO PAUSE FROM EXIT CONFIRMATION"
        );
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene(
            "BodyMap"
        );
    }

    // =====================================================
    // RETRY
    // =====================================================

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .name
        );
    }
}