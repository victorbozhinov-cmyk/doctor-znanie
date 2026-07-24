using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderPercentage : MonoBehaviour
{
    [SerializeField] private TMP_Text percentageText;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        slider.onValueChanged.AddListener(UpdatePercentage);
        UpdatePercentage(slider.value);
    }

    private void UpdatePercentage(float value)
    {
        float normalizedValue = Mathf.InverseLerp(
            slider.minValue,
            slider.maxValue,
            value
        );

        int percentage = Mathf.RoundToInt(normalizedValue * 100f);
        percentageText.text = percentage + "%";
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(UpdatePercentage);
        }
    }
}