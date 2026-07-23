using System;
using UnityEngine;

public class ColorThemeManager : MonoBehaviour
{
    public static ColorThemeManager Instance { get; private set; }

    // 0 = стандартна тема
    // 1 = тема за далтонисти
    public int CurrentTheme { get; private set; }

    public event Action<int> ThemeChanged;

    private const string ThemeKey = "ColorTheme";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // При първо стартиране използва Тема 1.
        CurrentTheme = PlayerPrefs.GetInt(ThemeKey, 0);
        CurrentTheme = Mathf.Clamp(CurrentTheme, 0, 1);
    }

    public void SetDefaultTheme()
    {
        SetTheme(0);
    }

    public void SetColorblindTheme()
    {
        SetTheme(1);
    }

    public void SetTheme(int themeIndex)
    {
        themeIndex = Mathf.Clamp(themeIndex, 0, 1);

        if (CurrentTheme == themeIndex)
        {
            return;
        }

        CurrentTheme = themeIndex;

        PlayerPrefs.SetInt(ThemeKey, CurrentTheme);
        PlayerPrefs.Save();

        ThemeChanged?.Invoke(CurrentTheme);
    }
}
