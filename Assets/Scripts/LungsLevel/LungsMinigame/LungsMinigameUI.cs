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

    private void Update()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        UpdatePhasePanels();
        UpdateProgress();
        UpdateRound();
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

        // -------------------------
        // INHALE / O2
        // -------------------------

        if (inhaleFill != null)
        {
            inhaleFill.fillAmount = oxygen / 100f;
        }

        if (inhalePercentText != null)
        {
            inhalePercentText.text =
                Mathf.RoundToInt(oxygen) + "%";
        }

        // -------------------------
        // EXHALE / CO2
        // -------------------------

        if (exhaleFill != null)
        {
            exhaleFill.fillAmount = co2 / 100f;
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
}