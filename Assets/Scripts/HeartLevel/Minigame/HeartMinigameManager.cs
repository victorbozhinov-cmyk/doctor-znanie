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

    [Header("Pulse")]
    [SerializeField] private int currentBPM = 80;

    [Header("Panels")]
    [SerializeField] private GameObject minigamePanel;

    [Header("UI")]
    [SerializeField] private TMP_Text pulseValueText;

    [Header("ECG")]
    [SerializeField] private ECGLineGraphic ecgLine;

    [Header("Heart States")]
    [SerializeField] private GameObject heartNormal;
    [SerializeField] private GameObject heartLow;
    [SerializeField] private GameObject heartHigh;
    [SerializeField] private GameObject heartDead;

    [Header("Lives")]
    [SerializeField] private HeartMinigameLives heartLives;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Warning Panels")]
    [SerializeField] private GameObject highPulseWarningPanel;
    [SerializeField] private GameObject lowPulseWarningPanel;

    [SerializeField] private Image highWarningFill;
    [SerializeField] private Image lowWarningFill;

    [Header("Danger Countdown By Difficulty")]
    [SerializeField] private float easyDangerDuration = 4f;
    [SerializeField] private float mediumDangerDuration = 3f;
    [SerializeField] private float hardDangerDuration = 2f;

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

    // -1 = още няма предишно събитие.
    private int lastEventIndex = -1;

    // Пази от многократна моментална загуба
    // на живот в critical зона.
    private bool criticalImmediateLossUsed;

    private bool isGameOver;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        isGameOver = false;

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);

        SetDifficultyValues();

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
        if (isGameOver)
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

        // EVENT е първо нарочно.
        // Ако event и drift са готови едновременно,
        // event получава приоритет.
        UpdateEvents();

        UpdatePulseDrift();
    }

    // =====================================================
    // DIFFICULTY
    // =====================================================

    private void SetDifficultyValues()
    {
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        switch (difficulty)
        {
            case 0:
                dangerDuration =
                    easyDangerDuration;

                driftAmount =
                    easyDriftAmount;

                driftInterval =
                    easyDriftInterval;

                eventInterval =
                    easyEventInterval;

                break;

            case 2:
                dangerDuration =
                    hardDangerDuration;

                driftAmount =
                    hardDriftAmount;

                driftInterval =
                    hardDriftInterval;

                eventInterval =
                    hardEventInterval;

                break;

            default:
                dangerDuration =
                    mediumDangerDuration;

                driftAmount =
                    mediumDriftAmount;

                driftInterval =
                    mediumDriftInterval;

                eventInterval =
                    mediumEventInterval;

                break;
        }

        Debug.Log(
            "Difficulty: " +
            difficulty +
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
    // EVENT SYSTEM
    // =====================================================

    private void UpdateEvents()
    {
        eventTimer += Time.deltaTime;

        if (eventTimer < eventInterval)
            return;

        // Ако drift е станал преди по-малко
        // от minimumDriftEventGap секунди,
        // изчакваме още малко.
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

        // Запомняме момента на event-а.
        lastEventTime = Time.time;

        // Много важно:
        // drift започва да брои отначало.
        // Така няма да дойде веднага след event.
        driftTimer = 0f;

        switch (eventIndex)
        {
            // =============================================
            // УВЕЛИЧАВАНЕ
            // =============================================

            // Силен гняв +40
            case 0:
                ShowEventCard(angerCard);

                ApplyEventBPM(
                    40,
                    "СИЛЕН ГНЯВ"
                );

                break;

            // Кофеин +25
            case 1:
                ShowEventCard(caffeineCard);

                ApplyEventBPM(
                    25,
                    "КОФЕИН"
                );

                break;

            // Уплах +30
            case 2:
                ShowEventCard(fearCard);

                ApplyEventBPM(
                    30,
                    "УПЛАХ"
                );

                break;

            // Физическо натоварване +35
            case 3:
                ShowEventCard(
                    physicalStressCard
                );

                ApplyEventBPM(
                    35,
                    "ФИЗИЧЕСКО НАТОВАРВАНЕ"
                );

                break;

            // Стрес +25
            case 4:
                ShowEventCard(stressCard);

                ApplyEventBPM(
                    25,
                    "СТРЕС"
                );

                break;

            // =============================================
            // НАМАЛЯВАНЕ
            // =============================================

            // Силно охлаждане -30
            case 5:
                ShowEventCard(freezingCard);

                ApplyEventBPM(
                    -30,
                    "СИЛНО ОХЛАЖДАНЕ"
                );

                break;

            // Болест -30
            case 6:
                ShowEventCard(illnessCard);

                ApplyEventBPM(
                    -30,
                    "БОЛЕСТ"
                );

                break;

            // Продължителен покой -25
            case 7:
                ShowEventCard(lazyCard);

                ApplyEventBPM(
                    -25,
                    "ПРОДЪЛЖИТЕЛЕН ПОКОЙ"
                );

                break;

            // Медитация -15
            case 8:
                ShowEventCard(
                    meditationCard
                );

                ApplyEventBPM(
                    -15,
                    "МЕДИТАЦИЯ"
                );

                break;

            // Сънливост -20
            case 9:
                ShowEventCard(sleepyCard);

                ApplyEventBPM(
                    -20,
                    "СЪНЛИВОСТ"
                );

                break;
        }
    }

    private void ShowEventCard(
        GameObject card
    )
    {
        HideAllEventCards();

        if (card != null)
        {
            card.SetActive(true);
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

        // Допълнителна защита.
        // Ако event е бил съвсем скоро,
        // drift изчаква.
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

        // ↓ или S
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

        // ↑ или W
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
            {
                lowPulseWarningPanel
                    .SetActive(true);
            }

            if (highPulseWarningPanel != null)
            {
                highPulseWarningPanel
                    .SetActive(false);
            }

            if (lowWarningFill != null)
            {
                lowWarningFill.fillAmount =
                    progress;
            }

            if (highWarningFill != null)
            {
                highWarningFill.fillAmount =
                    0f;
            }
        }

        if (isHigh)
        {
            if (highPulseWarningPanel != null)
            {
                highPulseWarningPanel
                    .SetActive(true);
            }

            if (lowPulseWarningPanel != null)
            {
                lowPulseWarningPanel
                    .SetActive(false);
            }

            if (highWarningFill != null)
            {
                highWarningFill.fillAmount =
                    progress;
            }

            if (lowWarningFill != null)
            {
                lowWarningFill.fillAmount =
                    0f;
            }
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
        {
            highPulseWarningPanel
                .SetActive(false);
        }

        if (lowPulseWarningPanel != null)
        {
            lowPulseWarningPanel
                .SetActive(false);
        }

        if (highWarningFill != null)
        {
            highWarningFill.fillAmount = 0f;
        }

        if (lowWarningFill != null)
        {
            lowWarningFill.fillAmount = 0f;
        }
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
            isGameOver)
        {
            return;
        }

        heartLives.LoseLife(
            () =>
            {
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
        if (isGameOver)
            return;

        isGameOver = true;

        ResetDangerCountdown();

        HideAllEventCards();

        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(true);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(true);

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

        while (
            timer <
            keyboardPressDuration
        )
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

        while (
            timer <
            keyboardPressDuration
        )
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
    }

    // =====================================================
    // PULSE STATE
    // =====================================================

    private void UpdatePulseState()
    {
        PulseState previousState =
            currentPulseState;

        // NORMAL 70 - 100
        if (
            currentBPM >= 70 &&
            currentBPM <= 100
        )
        {
            currentPulseState =
                PulseState.Normal;
        }

        // DANGER LOW 40 - 69
        else if (
            currentBPM >= 40 &&
            currentBPM < 70
        )
        {
            currentPulseState =
                PulseState.DangerLow;
        }

        // DANGER HIGH 101 - 130
        else if (
            currentBPM > 100 &&
            currentBPM <= 130
        )
        {
            currentPulseState =
                PulseState.DangerHigh;
        }

        // CRITICAL LOW
        else if (currentBPM < 40)
        {
            currentPulseState =
                PulseState.CriticalLow;
        }

        // CRITICAL HIGH
        else
        {
            currentPulseState =
                PulseState.CriticalHigh;
        }

        HandleCriticalState(
            previousState
        );

        UpdateHeartVisual();
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
        if (isGameOver)
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
                {
                    heartNormal
                        .SetActive(true);
                }

                break;

            case PulseState.DangerLow:

                if (heartLow != null)
                {
                    heartLow
                        .SetActive(true);
                }

                break;

            case PulseState.DangerHigh:

                if (heartHigh != null)
                {
                    heartHigh
                        .SetActive(true);
                }

                break;

            case PulseState.CriticalLow:
            case PulseState.CriticalHigh:

                if (heartDead != null)
                {
                    heartDead
                        .SetActive(true);
                }

                break;
        }
    }

    // =====================================================
    // PLAYER CONTROLS
    // =====================================================

    public void DecreasePulse()
    {
        if (isGameOver)
            return;

        currentBPM -= 5;

        UpdatePulseUI();

        UpdatePulseState();
    }

    public void IncreasePulse()
    {
        if (isGameOver)
            return;

        currentBPM += 5;

        UpdatePulseUI();

        UpdatePulseState();
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .name
        );
    }
}