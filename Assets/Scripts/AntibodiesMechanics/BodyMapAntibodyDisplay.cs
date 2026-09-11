using TMPro;
using UnityEngine;

public class BodyMapAntibodyDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text antibodyAmountText;

    private void Start()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (antibodyAmountText == null)
        {
            Debug.LogWarning(
                "AntibodyAmountText не е свързан в BodyMapAntibodyDisplay."
            );

            return;
        }

        if (AntibodyManager.Instance == null)
        {
            Debug.LogWarning(
                "AntibodyManager.Instance липсва."
            );

            antibodyAmountText.text = "0";
            return;
        }

        int totalAntibodies =
            AntibodyManager.Instance.TotalBestScore;

        antibodyAmountText.text =
            totalAntibodies.ToString();
    }
}
