using UnityEngine;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance { get; private set; }

    [SerializeField] private Image brightnessOverlay;

    [Range(0f, 1f)]
    [SerializeField] private float maximumDarkness = 0.75f;

    private const string BrightnessKey = "Brightness";

    public float CurrentBrightness { get; private set; } = 1f;

    private void Awake()
    {
        // Не допускаме две копия на системата.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Запазва системата при преминаване към друга сцена.
        DontDestroyOnLoad(gameObject);

        float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, 1f);
        ApplyBrightness(savedBrightness, false);
    }

    public void SetBrightness(float value)
    {
        ApplyBrightness(value, true);
    }

    private void ApplyBrightness(float value, bool saveValue)
    {
        CurrentBrightness = Mathf.Clamp01(value);

        Color overlayColor = brightnessOverlay.color;

        // 100% яркост = прозрачен слой.
        // 0% яркост = силно, но не напълно черно затъмнение.
        overlayColor.a = Mathf.Lerp(
            maximumDarkness,
            0f,
            CurrentBrightness
        );

        brightnessOverlay.color = overlayColor;

        if (saveValue)
        {
            PlayerPrefs.SetFloat(BrightnessKey, CurrentBrightness);
            PlayerPrefs.Save();
        }
    }
}