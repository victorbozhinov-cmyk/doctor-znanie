using System;
using UnityEngine;

public class ColorThemeManager : MonoBehaviour
{
    public static ColorThemeManager Instance { get; private set; }

    // 0 = стандартна тема
    // 1 = достъпен цветен режим
    public int CurrentTheme { get; private set; }

    public event Action<int> ThemeChanged;

    private const string ThemeKey = "ColorTheme";

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CurrentTheme = PlayerPrefs.GetInt(
            ThemeKey,
            0
        );

        CurrentTheme = Mathf.Clamp(
            CurrentTheme,
            0,
            1
        );
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Казваме на всички системи коя тема е активна
        // при първоначалното стартиране.
        ThemeChanged?.Invoke(CurrentTheme);
    }

    // =========================================================
    // PUBLIC METHODS
    // =========================================================

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
        themeIndex = Mathf.Clamp(
            themeIndex,
            0,
            1
        );

        CurrentTheme = themeIndex;

        PlayerPrefs.SetInt(
            ThemeKey,
            CurrentTheme
        );

        PlayerPrefs.Save();

        ThemeChanged?.Invoke(
            CurrentTheme
        );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    public bool IsColorblindThemeActive()
    {
        return CurrentTheme == 1;
    }

    public bool IsDefaultThemeActive()
    {
        return CurrentTheme == 0;
    }
}