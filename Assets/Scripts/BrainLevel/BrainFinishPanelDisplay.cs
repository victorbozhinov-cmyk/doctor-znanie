using TMPro;
using UnityEngine;

public class BrainFinishPanelDisplay : MonoBehaviour
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
                "в BrainFinishPanelDisplay."
            );

            vitaminsAmountText.text = "0";

            return;
        }

        int availableReward =
            vitaminRewardManager
                .GetAvailableReward();

        vitaminsAmountText.text =
            availableReward.ToString();
    }

    // =========================================================
    // ANTIBODIES
    // =========================================================

    private void UpdateAntibodies()
    {
        if (BrainScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "BrainScoreManager.Instance липсва."
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
            BrainScoreManager.Instance
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
                        .BrainBestScore;
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
