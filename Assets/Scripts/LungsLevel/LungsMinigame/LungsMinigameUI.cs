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

    private void Update()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        UpdatePhasePanels();
        UpdateProgress();
    }

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

    private void UpdateProgress()
    {
        float oxygen =
            LungsMinigameManager.Instance.OxygenPercent;

        float co2 =
            LungsMinigameManager.Instance.CO2Percent;

        if (inhaleFill != null)
        {
            inhaleFill.fillAmount = oxygen / 100f;
        }

        if (inhalePercentText != null)
        {
            inhalePercentText.text =
                Mathf.RoundToInt(oxygen) + "%";
        }

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
}
