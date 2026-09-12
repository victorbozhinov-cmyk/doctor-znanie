using TMPro;
using UnityEngine;

public class StomachFinishPanelDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text vitaminsAmountText;
    [SerializeField] private TMP_Text antibodiesAmountText;
    [SerializeField] private TMP_Text recordAntibodiesAmountText;

    [Header("Vitamin Reward")]
    [SerializeField]
    private OrganVitaminRewardManager vitaminRewardManager;

    private void OnEnable()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        UpdateVitamins();
        UpdateAntibodies();
    }

    // =========================================================
    // VITAMINS
    // =========================================================

    private void UpdateVitamins()
    {
        if (vitaminsAmountText == null)
        {
            Debug.LogWarning(
                "VitaminsAmountText не е свързан."
            );

            return;
        }

        if (vitaminRewardManager == null)
        {
            Debug.LogWarning(
                "OrganVitaminRewardManager не е свързан " +
                "в StomachFinishPanelDisplay."
            );

            vitaminsAmountText.text = "0";

            return;
        }

        int availableReward =
            vitaminRewardManager.GetAvailableReward();

        vitaminsAmountText.text =
            availableReward.ToString();
    }

    // =========================================================
    // ANTIBODIES
    // =========================================================

    private void UpdateAntibodies()
    {
        if (StomachScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "StomachScoreManager.Instance липсва."
            );

            if (antibodiesAmountText != null)
            {
                antibodiesAmountText.text = "0";
            }

            if (recordAntibodiesAmountText != null)
            {
                recordAntibodiesAmountText.text = "0";
            }

            return;
        }

        int currentScore =
            StomachScoreManager.Instance
                .CalculateFinalScore();

        if (antibodiesAmountText != null)
        {
            antibodiesAmountText.text =
                currentScore.ToString();
        }

        if (recordAntibodiesAmountText != null)
        {
            int previousBest = 0;

            if (AntibodyManager.Instance != null)
            {
                previousBest =
                    AntibodyManager.Instance
                        .StomachBestScore;
            }

            int displayedBest =
                Mathf.Max(
                    previousBest,
                    currentScore
                );

            recordAntibodiesAmountText.text =
                displayedBest.ToString();
        }
    }
}
