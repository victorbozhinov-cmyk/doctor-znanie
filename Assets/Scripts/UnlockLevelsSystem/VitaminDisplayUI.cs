using TMPro;
using UnityEngine;

public class VitaminDisplayUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text vitaminAmountText;

    private void Start()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (vitaminAmountText == null)
        {
            return;
        }

        if (VitaminManager.Instance == null)
        {
            vitaminAmountText.text = "0";
            return;
        }

        vitaminAmountText.text =
            VitaminManager.Instance.CurrentVitamins.ToString();
    }
}