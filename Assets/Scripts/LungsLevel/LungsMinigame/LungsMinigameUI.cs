using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LungsMinigameUI : MonoBehaviour
{
    [Header("Phase Panels")]
    [SerializeField] private GameObject inhalePanel;
    [SerializeField] private GameObject exhalePanel;

    [Header("Inhale UI")]
    [SerializeField] private Image inhaleFill;
    [SerializeField] private TMP_Text inhalePercentText;

    [Header("Exhale UI")]
    [SerializeField] private Image exhaleFill;
    [SerializeField] private TMP_Text exhalePercentText;

    [Header("Round UI")]
    [SerializeField] private TMP_Text currentRoundText;
    [SerializeField] private TMP_Text totalRoundsText;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timeText;

    private void Update()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        UpdatePhasePanels();
        UpdateProgress();
        UpdateRound();
        UpdateTimer();
    }

    // =========================================================
    // PHASE PANELS
    // =========================================================

    private void UpdatePhasePanels()
    {
        bool isInhale =
            LungsMinigameManager.Instance.CurrentPhase ==
            LungsBreathingPhase.Inhale;

        if (inhalePanel != null)
        {
            inhalePanel.SetActive(isInhale);
        }

        if (exhalePanel != null)
        {
            exhalePanel.SetActive(!isInhale);
        }
    }

    // =========================================================
    // PROGRESS
    // =========================================================

    private void UpdateProgress()
    {
        float oxygen =
            LungsMinigameManager.Instance.OxygenPercent;

        float co2 =
            LungsMinigameManager.Instance.CO2Percent;

        if (inhaleFill != null)
        {
            inhaleFill.fillAmount =
                oxygen / 100f;
        }

        if (inhalePercentText != null)
        {
            inhalePercentText.text =
                Mathf.RoundToInt(oxygen) + "%";
        }

        if (exhaleFill != null)
        {
            exhaleFill.fillAmount =
                co2 / 100f;
        }

        if (exhalePercentText != null)
        {
            exhalePercentText.text =
                Mathf.RoundToInt(co2) + "%";
        }
    }

    // =========================================================
    // ROUND
    // =========================================================

    private void UpdateRound()
    {
        int currentRound =
            LungsMinigameManager.Instance.CurrentRound;

        int totalRounds =
            LungsMinigameManager.Instance.TotalRounds;

        if (currentRoundText != null)
        {
            currentRoundText.text =
                currentRound.ToString();
        }

        if (totalRoundsText != null)
        {
            totalRoundsText.text =
                "/" + totalRounds;
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
            LungsMinigameManager.Instance.PhaseTimeRemaining;

        remainingTime =
            Mathf.Max(0f, remainingTime);

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timeText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }
}