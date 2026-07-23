using UnityEngine;
using UnityEngine.UI;

public class ColorThemeSelectorUI : MonoBehaviour
{
    [SerializeField] private Toggle themeOneToggle;
    [SerializeField] private Toggle themeTwoToggle;

    private void Start()
    {
        if (ColorThemeManager.Instance == null)
        {
            Debug.LogError("ColorThemeManager липсва в сцената.");
            return;
        }

        // Показва запазената тема без да задейства събитията.
        int currentTheme = ColorThemeManager.Instance.CurrentTheme;

        themeOneToggle.SetIsOnWithoutNotify(currentTheme == 0);
        themeTwoToggle.SetIsOnWithoutNotify(currentTheme == 1);

        themeOneToggle.onValueChanged.AddListener(OnThemeOneChanged);
        themeTwoToggle.onValueChanged.AddListener(OnThemeTwoChanged);
    }

    private void OnThemeOneChanged(bool isOn)
    {
        if (isOn && ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.SetDefaultTheme();
        }
    }

    private void OnThemeTwoChanged(bool isOn)
    {
        if (isOn && ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.SetColorblindTheme();
        }
    }

    private void OnDestroy()
    {
        themeOneToggle.onValueChanged.RemoveListener(OnThemeOneChanged);
        themeTwoToggle.onValueChanged.RemoveListener(OnThemeTwoChanged);
    }
}