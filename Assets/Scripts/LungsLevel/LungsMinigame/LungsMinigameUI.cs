using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LungsMinigameUI : MonoBehaviour
{
    // =========================================================
    // PHASE PANELS
    // =========================================================

    [Header("Phase Panels")]
    [SerializeField] private GameObject inhalePanel;
    [SerializeField] private GameObject exhalePanel;

    [Header("Phase Panel Transitions")]
    [SerializeField]
    private LungsPhasePanelTransition inhalePanelTransition;

    [SerializeField]
    private LungsPhasePanelTransition exhalePanelTransition;

    // =========================================================
    // INHALE UI
    // =========================================================

    [Header("Inhale UI")]
    [SerializeField] private Image inhaleFill;
    [SerializeField] private TMP_Text inhalePercentText;
    [SerializeField] private LungsProgressFeedback inhaleFeedback;

    // =========================================================
    // EXHALE UI
    // =========================================================

    [Header("Exhale UI")]
    [SerializeField] private Image exhaleFill;
    [SerializeField] private TMP_Text exhalePercentText;
    [SerializeField] private LungsProgressFeedback exhaleFeedback;

    [Header("Exhale Fill Visual")]
    [SerializeField]
    private Color exhaleFillColor =
        new Color(
            0.55f,
            0.55f,
            0.55f,
            1f
        );

    // =========================================================
    // PROGRESS
    // =========================================================

    [Header("Progress Smoothness")]
    [SerializeField] private float fillSmoothSpeed = 8f;

    // =========================================================
    // ROUND
    // =========================================================

    [Header("Round UI")]
    [SerializeField] private TMP_Text currentRoundText;
    [SerializeField] private TMP_Text totalRoundsText;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timeText;

    // =========================================================
    // RUNTIME
    // =========================================================

    private float displayedOxygen;
    private float displayedCO2;

    private float previousOxygen;
    private float previousCO2;

    private bool initialized;

    private bool previousTransitionState;

    private LungsBreathingPhase displayedPhase;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        SetupExhaleFill();
    }

    private void Update()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        if (!initialized)
        {
            InitializeValues();
        }

        UpdatePhasePanelAnimations();

        UpdateProgress();
        UpdateRound();
        UpdateTimer();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeValues()
    {
        float oxygen =
            LungsMinigameManager.Instance
                .OxygenPercent;

        float co2 =
            LungsMinigameManager.Instance
                .CO2Percent;

        displayedOxygen = oxygen;
        displayedCO2 = co2;

        previousOxygen = oxygen;
        previousCO2 = co2;

        displayedPhase =
            LungsMinigameManager.Instance
                .CurrentPhase;

        previousTransitionState =
            LungsMinigameManager.Instance
                .IsTransitioning;

        // =====================================================
        // INITIAL PANEL STATE
        // =====================================================

        if (
            displayedPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            if (inhalePanelTransition != null)
            {
                inhalePanelTransition
                    .ShowImmediate();
            }
            else if (inhalePanel != null)
            {
                inhalePanel.SetActive(true);
            }

            if (exhalePanelTransition != null)
            {
                exhalePanelTransition
                    .HideImmediate();
            }
            else if (exhalePanel != null)
            {
                exhalePanel.SetActive(false);
            }
        }
        else
        {
            if (exhalePanelTransition != null)
            {
                exhalePanelTransition
                    .ShowImmediate();
            }
            else if (exhalePanel != null)
            {
                exhalePanel.SetActive(true);
            }

            if (inhalePanelTransition != null)
            {
                inhalePanelTransition
                    .HideImmediate();
            }
            else if (inhalePanel != null)
            {
                inhalePanel.SetActive(false);
            }
        }

        initialized = true;
    }

    // =========================================================
    // PHASE PANEL ANIMATIONS
    // =========================================================

    private void UpdatePhasePanelAnimations()
    {
        LungsMinigameManager manager =
            LungsMinigameManager.Instance;

        bool currentlyTransitioning =
            manager.IsTransitioning;

        LungsBreathingPhase actualPhase =
            manager.CurrentPhase;

        // =====================================================
        // TRANSITION JUST STARTED
        // =====================================================

        if (
            currentlyTransitioning &&
            !previousTransitionState
        )
        {
            HideCurrentPhasePanel();
        }

        // =====================================================
        // PHASE CHANGED
        // =====================================================

        if (
            actualPhase !=
            displayedPhase
        )
        {
            displayedPhase =
                actualPhase;

            ShowCurrentPhasePanel();
        }

        previousTransitionState =
            currentlyTransitioning;
    }

    // =========================================================
    // HIDE CURRENT PANEL
    // =========================================================

    private void HideCurrentPhasePanel()
    {
        if (
            displayedPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            if (inhalePanelTransition != null)
            {
                inhalePanelTransition
                    .PlayHide();
            }
        }
        else
        {
            if (exhalePanelTransition != null)
            {
                exhalePanelTransition
                    .PlayHide();
            }
        }
    }

    // =========================================================
    // SHOW CURRENT PANEL
    // =========================================================

    private void ShowCurrentPhasePanel()
    {
        if (
            displayedPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            if (inhalePanelTransition != null)
            {
                inhalePanelTransition
                    .PlayShow();
            }
            else if (inhalePanel != null)
            {
                inhalePanel.SetActive(true);
            }

            if (exhalePanelTransition != null)
            {
                exhalePanelTransition
                    .HideImmediate();
            }
        }
        else
        {
            if (exhalePanelTransition != null)
            {
                exhalePanelTransition
                    .PlayShow();
            }
            else if (exhalePanel != null)
            {
                exhalePanel.SetActive(true);
            }

            if (inhalePanelTransition != null)
            {
                inhalePanelTransition
                    .HideImmediate();
            }
        }
    }

    // =========================================================
    // EXHALE FILL SETUP
    // =========================================================

    private void SetupExhaleFill()
    {
        if (exhaleFill == null)
        {
            return;
        }

        exhaleFill.type =
            Image.Type.Filled;

        exhaleFill.fillMethod =
            Image.FillMethod.Horizontal;

        exhaleFill.fillOrigin =
            (int)Image.OriginHorizontal.Left;

        exhaleFill.fillClockwise = true;

        exhaleFill.color =
            exhaleFillColor;
    }

    // =========================================================
    // PROGRESS
    // =========================================================

    private void UpdateProgress()
    {
        float oxygen =
            LungsMinigameManager.Instance
                .OxygenPercent;

        float co2 =
            LungsMinigameManager.Instance
                .CO2Percent;

        CheckForProgressFeedback(
            oxygen,
            co2
        );

        displayedOxygen =
            Mathf.Lerp(
                displayedOxygen,
                oxygen,
                fillSmoothSpeed *
                Time.deltaTime
            );

        displayedCO2 =
            Mathf.Lerp(
                displayedCO2,
                co2,
                fillSmoothSpeed *
                Time.deltaTime
            );

        // =====================================================
        // INHALE
        // =====================================================

        if (inhaleFill != null)
        {
            inhaleFill.fillAmount =
                displayedOxygen /
                100f;
        }

        if (inhalePercentText != null)
        {
            inhalePercentText.text =
                Mathf.RoundToInt(
                    displayedOxygen
                ) +
                "%";
        }

        // =====================================================
        // EXHALE
        // =====================================================

        if (exhaleFill != null)
        {
            exhaleFill.fillAmount =
                displayedCO2 /
                100f;
        }

        if (exhalePercentText != null)
        {
            exhalePercentText.text =
                Mathf.RoundToInt(
                    displayedCO2
                ) +
                "%";
        }

        previousOxygen = oxygen;
        previousCO2 = co2;
    }

    // =========================================================
    // FEEDBACK
    // =========================================================

    private void CheckForProgressFeedback(
        float oxygen,
        float co2
    )
    {
        LungsBreathingPhase phase =
            LungsMinigameManager.Instance
                .CurrentPhase;

        // =====================================================
        // INHALE
        // =====================================================

        if (
            phase ==
            LungsBreathingPhase.Inhale
        )
        {
            if (
                Mathf.Approximately(
                    oxygen,
                    previousOxygen
                )
            )
            {
                return;
            }

            if (oxygen > previousOxygen)
            {
                if (inhaleFeedback != null)
                {
                    inhaleFeedback
                        .PlayCorrect();
                }
            }
            else
            {
                if (inhaleFeedback != null)
                {
                    inhaleFeedback
                        .PlayWrong();
                }
            }
        }

        // =====================================================
        // EXHALE
        // =====================================================

        else
        {
            if (
                Mathf.Approximately(
                    co2,
                    previousCO2
                )
            )
            {
                return;
            }

            if (co2 < previousCO2)
            {
                if (exhaleFeedback != null)
                {
                    exhaleFeedback
                        .PlayCorrect();
                }
            }
            else
            {
                if (exhaleFeedback != null)
                {
                    exhaleFeedback
                        .PlayWrong();
                }
            }
        }
    }

    // =========================================================
    // ROUND
    // =========================================================

    private void UpdateRound()
    {
        int currentRound =
            LungsMinigameManager.Instance
                .CurrentRound;

        int totalRounds =
            LungsMinigameManager.Instance
                .TotalRounds;

        if (currentRoundText != null)
        {
            currentRoundText.text =
                currentRound.ToString();
        }

        if (totalRoundsText != null)
        {
            totalRoundsText.text =
                "/" +
                totalRounds;
        }
    }

    // =========================================================
    // TIMER
    // =========================================================

    private void UpdateTimer()
    {
        if (timeText == null)
        {
            return;
        }

        float remainingTime =
            LungsMinigameManager.Instance
                .PhaseTimeRemaining;

        remainingTime =
            Mathf.Max(
                0f,
                remainingTime
            );

        int totalSeconds =
            Mathf.CeilToInt(
                remainingTime
            );

        int minutes =
            totalSeconds /
            60;

        int seconds =
            totalSeconds %
            60;

        timeText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }
}