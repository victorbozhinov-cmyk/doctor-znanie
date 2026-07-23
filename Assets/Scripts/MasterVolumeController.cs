using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MasterVolumeController : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = savedVolume;

        ApplyVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
        }
    }
}