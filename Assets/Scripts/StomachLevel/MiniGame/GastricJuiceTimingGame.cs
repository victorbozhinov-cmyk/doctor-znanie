using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GastricJuiceTimingGame : MonoBehaviour
{
    public enum JuiceZone
    {
        None,
        Yellow,
        OrangeLow,
        Green,
        OrangeHigh,
        Red
    }

    public event Action JuiceSucceeded;

    [Header("Movement References")]
    [SerializeField] private RectTransform indicatorTrack;
    [SerializeField] private RectTransform indicator;

    [Header("Juice Zones")]
    [SerializeField] private RectTransform yellowZone;
    [SerializeField] private RectTransform orangeLowZone;
    [SerializeField] private RectTransform greenZone;
    [SerializeField] private RectTransform orangeHighZone;
    [SerializeField] private RectTransform redZone;

    [Header("Panels")]
    [SerializeField] private CanvasGroup juiceMiniGameCanvasGroup;
    [SerializeField] private GameObject feedbackPanel;

    [Header("Feedback")]
    [SerializeField] private GameObject failBackground;
    [SerializeField] private GameObject successBackground;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackDuration = 1.5f;

    [Header("Success Sequence")]
    [SerializeField] private float delayBeforeJuiceDrops = 0.20f;

    [Header("Task Timer")]
    [SerializeField] private StomachTaskTimer taskTimer;

    [Header("Total Timer")]
    [SerializeField] private StomachMinigameTimer totalTimer;

    [Header("Stomach State Cards")]
    [SerializeField] private StomachStateCardController stateCardController;

    [Header("Juice Drop Effect")]
    [SerializeField] private StomachJuiceDropEffect juiceDropEffect;

    [Header("Audio")]
    [SerializeField] private StomachMinigameAudio minigameAudio;

    // =========================================================
    // DIFFICULTY SETTINGS
    // =========================================================

    [Header("Difficulty - Green Zone Size")]
    [SerializeField] private float easyGreenZoneMultiplier = 1.35f;
    [SerializeField] private float mediumGreenZoneMultiplier = 1.00f;
    [SerializeField] private float hardGreenZoneMultiplier = 0.70f;

    [Header("Difficulty - Indicator Speed")]
    [SerializeField] private float easyMoveSpeed = 240f;
    [SerializeField] private float mediumMoveSpeed = 300f;
    [SerializeField] private float hardMoveSpeed = 380f;

    [Header("Difficulty - Task Duration")]
    [SerializeField] private float easyTaskDuration = 7f;
    [SerializeField] private float mediumTaskDuration = 5f;
    [SerializeField] private float hardTaskDuration = 3.5f;

    [Header("Time Penalties")]
    [SerializeField] private float yellowPenalty = 5f;
    [SerializeField] private float orangeLowPenalty = 3f;
    [SerializeField] private float orangeHighPenalty = 3f;
    [SerializeField] private float redPenalty = 5f;
    [SerializeField] private float timeoutPenalty = 5f;

    private float direction = 1f;
    private bool isRunning;

    private int difficulty;

    private float currentMoveSpeed;
    private float currentTaskDuration;

    private Vector3 originalGreenZoneScale;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (greenZone != null)
        {
            originalGreenZoneScale =
                greenZone.localScale;
        }
    }

    private void Start()
    {
        difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        ApplyDifficultySettings();

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        StopAndHide();
    }

    private void OnEnable()
    {
        if (taskTimer != null)
        {
            taskTimer.TimerExpired +=
                OnTaskTimerExpired;
        }
    }

    private void OnDisable()
    {
        if (taskTimer != null)
        {
            taskTimer.TimerExpired -=
                OnTaskTimerExpired;
        }
    }

    private void Update()
    {
        if (!isRunning)
            return;

        MoveIndicator();

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StopIndicator();
        }
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void ApplyDifficultySettings()
    {
        float greenMultiplier;

        switch (difficulty)
        {
            case 0:
                greenMultiplier =
                    easyGreenZoneMultiplier;

                currentMoveSpeed =
                    easyMoveSpeed;

                currentTaskDuration =
                    easyTaskDuration;
                break;

            case 2:
                greenMultiplier =
                    hardGreenZoneMultiplier;

                currentMoveSpeed =
                    hardMoveSpeed;

                currentTaskDuration =
                    hardTaskDuration;
                break;

            default:
                greenMultiplier =
                    mediumGreenZoneMultiplier;

                currentMoveSpeed =
                    mediumMoveSpeed;

                currentTaskDuration =
                    mediumTaskDuration;
                break;
        }

        if (greenZone != null)
        {
            greenZone.localScale =
                new Vector3(
                    originalGreenZoneScale.x *
                    greenMultiplier,

                    originalGreenZoneScale.y,
                    originalGreenZoneScale.z
                );
        }
    }

    // =========================================================
    // INDICATOR MOVEMENT
    // =========================================================

    private void MoveIndicator()
    {
        if (indicatorTrack == null ||
            indicator == null)
        {
            return;
        }

        float halfTrackWidth =
            indicatorTrack.rect.width * 0.5f;

        float halfIndicatorWidth =
            indicator.rect.width * 0.5f;

        float leftLimit =
            -halfTrackWidth +
            halfIndicatorWidth;

        float rightLimit =
            halfTrackWidth -
            halfIndicatorWidth;

        Vector2 pos =
            indicator.anchoredPosition;

        pos.x +=
            direction *
            currentMoveSpeed *
            Time.deltaTime;

        if (pos.x >= rightLimit)
        {
            pos.x = rightLimit;
            direction = -1f;
        }
        else if (pos.x <= leftLimit)
        {
            pos.x = leftLimit;
            direction = 1f;
        }

        indicator.anchoredPosition =
            pos;
    }

    // =========================================================
    // SPACE
    // =========================================================

    private void StopIndicator()
    {
        if (!isRunning)
            return;

        isRunning = false;

        if (taskTimer != null)
        {
            taskTimer.PauseTimer();
        }

        JuiceZone result =
            GetCurrentZone();

        switch (result)
        {
            case JuiceZone.Yellow:

                StartCoroutine(
                    ShowFailFeedback(
                        "Твърде малко стомашни сокове!",
                        yellowPenalty,
                        false
                    )
                );
                break;

            case JuiceZone.OrangeLow:

                StartCoroutine(
                    ShowFailFeedback(
                        "Недостатъчно стомашни сокове!",
                        orangeLowPenalty,
                        false
                    )
                );
                break;

            case JuiceZone.Green:

                if (minigameAudio != null)
                {
                    minigameAudio
                        .PlayGreenZoneHit();
                }

                StartCoroutine(
                    ShowSuccessFeedback(
                        "Точното количество стомашни сокове!"
                    )
                );
                break;

            case JuiceZone.OrangeHigh:

                StartCoroutine(
                    ShowFailFeedback(
                        "Повече от необходимото количество!",
                        orangeHighPenalty,
                        false
                    )
                );
                break;

            case JuiceZone.Red:

                StartCoroutine(
                    ShowFailFeedback(
                        "Твърде много стомашни сокове!",
                        redPenalty,
                        false
                    )
                );
                break;

            default:

                ResumeTimingWithRemainingTime();
                break;
        }
    }

    // =========================================================
    // TIMER EXPIRED
    // =========================================================

    private void OnTaskTimerExpired()
    {
        if (!isRunning)
            return;

        isRunning = false;

        StartCoroutine(
            ShowFailFeedback(
                "Времето за задачата изтече!",
                timeoutPenalty,
                true
            )
        );
    }

    // =========================================================
    // FAIL
    // =========================================================

    private IEnumerator ShowFailFeedback(
        string message,
        float penalty,
        bool restartWithFullTime)
    {
        HideJuicePanel();

        if (stateCardController != null)
        {
            stateCardController
                .ShowIrritated();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();

            totalTimer.ApplyPenalty(
                penalty
            );
        }

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
                message;
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

        ShowJuicePanel();

        if (totalTimer != null)
        {
            totalTimer.ResumeTimer();
        }

        if (restartWithFullTime)
        {
            StartTiming();
        }
        else
        {
            ResumeTimingWithRemainingTime();
        }
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    private IEnumerator ShowSuccessFeedback(
        string message)
    {
        HideJuicePanel();

        if (taskTimer != null)
        {
            taskTimer.StopTimer();
        }

        if (totalTimer != null)
        {
            totalTimer.PauseTimer();
        }

        if (stateCardController != null)
        {
            stateCardController
                .ShowJuicesMixed();
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
                message;
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        // 1. SUCCESS FEEDBACK

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        // 2. КРАТКА ПАУЗА

        yield return new WaitForSeconds(
            delayBeforeJuiceDrops
        );

        // 3. КАПКИ

        if (juiceDropEffect != null)
        {
            if (minigameAudio != null)
            {
                minigameAudio
                    .PlayStomachAcids();
            }

            juiceDropEffect
                .PlayJuiceDropEffect();

            yield return null;

            while (juiceDropEffect.IsPlaying)
            {
                yield return null;
            }
        }

        // 4. PROGRESS

        JuiceSucceeded?.Invoke();
    }

    // =========================================================
    // PUBLIC CONTROL
    // =========================================================

    public void BeginJuiceTask()
    {
        if (isRunning)
            return;

        ShowJuicePanel();

        StartTiming();
    }

    public void StopAndHide()
    {
        isRunning = false;

        if (taskTimer != null)
        {
            taskTimer.StopTimer();
        }

        HideJuicePanel();
    }

    // =========================================================
    // FULL NEW ATTEMPT
    // =========================================================

    public void StartTiming()
    {
        SetIndicatorToLeft();

        if (taskTimer != null)
        {
            taskTimer.StartTimer(
                currentTaskDuration
            );
        }

        isRunning = true;
    }

    // =========================================================
    // RESUME AFTER WRONG ZONE
    // =========================================================

    private void ResumeTimingWithRemainingTime()
    {
        SetIndicatorToLeft();

        if (taskTimer != null)
        {
            taskTimer.ResumeTimer();
        }

        isRunning = true;
    }

    // =========================================================
    // PANEL
    // =========================================================

    private void HideJuicePanel()
    {
        if (juiceMiniGameCanvasGroup == null)
            return;

        juiceMiniGameCanvasGroup.alpha = 0f;
        juiceMiniGameCanvasGroup.interactable = false;
        juiceMiniGameCanvasGroup.blocksRaycasts = false;
    }

    private void ShowJuicePanel()
    {
        if (juiceMiniGameCanvasGroup == null)
            return;

        juiceMiniGameCanvasGroup.alpha = 1f;
        juiceMiniGameCanvasGroup.interactable = true;
        juiceMiniGameCanvasGroup.blocksRaycasts = true;
    }

    // =========================================================
    // ZONES
    // =========================================================

    private JuiceZone GetCurrentZone()
    {
        if (indicator == null)
            return JuiceZone.None;

        float indicatorX =
            indicator.position.x;

        if (IsInsideZone(
            indicatorX,
            greenZone))
        {
            return JuiceZone.Green;
        }

        if (IsInsideZone(
            indicatorX,
            yellowZone))
        {
            return JuiceZone.Yellow;
        }

        if (IsInsideZone(
            indicatorX,
            orangeLowZone))
        {
            return JuiceZone.OrangeLow;
        }

        if (IsInsideZone(
            indicatorX,
            orangeHighZone))
        {
            return JuiceZone.OrangeHigh;
        }

        if (IsInsideZone(
            indicatorX,
            redZone))
        {
            return JuiceZone.Red;
        }

        return JuiceZone.None;
    }

    private bool IsInsideZone(
        float indicatorX,
        RectTransform zone)
    {
        if (zone == null)
            return false;

        Vector3[] corners =
            new Vector3[4];

        zone.GetWorldCorners(
            corners
        );

        float left =
            corners[0].x;

        float right =
            corners[2].x;

        return
            indicatorX >= left &&
            indicatorX <= right;
    }

    // =========================================================
    // INDICATOR RESET
    // =========================================================

    private void SetIndicatorToLeft()
    {
        if (indicatorTrack == null ||
            indicator == null)
        {
            return;
        }

        float halfTrackWidth =
            indicatorTrack.rect.width * 0.5f;

        float halfIndicatorWidth =
            indicator.rect.width * 0.5f;

        Vector2 pos =
            indicator.anchoredPosition;

        pos.x =
            -halfTrackWidth +
            halfIndicatorWidth;

        indicator.anchoredPosition =
            pos;

        direction = 1f;
    }
}