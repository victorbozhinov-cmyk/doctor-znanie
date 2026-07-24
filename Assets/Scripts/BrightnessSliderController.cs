using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BrightnessSliderController : MonoBehaviour
{
    private Slider brightnessSlider;

    private void Start()
    {
        brightnessSlider = GetComponent<Slider>();

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 1f;

        brightnessSlider.onValueChanged.AddListener(ChangeBrightness);

        if (BrightnessManager.Instance != null)
        {
            brightnessSlider.value =
                BrightnessManager.Instance.CurrentBrightness;

            ChangeBrightness(brightnessSlider.value);
        }
    }

    private void ChangeBrightness(float value)
    {
        if (BrightnessManager.Instance != null)
        {
            BrightnessManager.Instance.SetBrightness(value);
        }
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(ChangeBrightness);
        }
    }
}