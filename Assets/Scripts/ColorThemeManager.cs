using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ColorThemeManager : MonoBehaviour
{
    public static ColorThemeManager Instance { get; private set; }

    // 0 = стандартна тема
    // 1 = достъпен цветен режим
    public int CurrentTheme { get; private set; }

    public event Action<int> ThemeChanged;

    private const string ThemeKey = "ColorTheme";

    [Header("Accessible Color Theme")]
    [SerializeField]
    private Material accessibleColorMaterial;

    [SerializeField]
    [Range(0f, 1f)]
    private float accessibleThemeStrength = 0.65f;

    [SerializeField]
    private float canvasPlaneDistance = 1f;

    private struct CanvasState
    {
        public RenderMode renderMode;
        public Camera worldCamera;
        public float planeDistance;
    }

    private readonly Dictionary<Canvas, CanvasState> canvasStates =
        new Dictionary<Canvas, CanvasState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CurrentTheme =
            PlayerPrefs.GetInt(
                ThemeKey,
                0
            );

        CurrentTheme =
            Mathf.Clamp(
                CurrentTheme,
                0,
                1
            );
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplyTheme();
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StartCoroutine(
            ApplyThemeNextFrame()
        );
    }

    private IEnumerator ApplyThemeNextFrame()
    {
        yield return null;

        ClearDestroyedCanvases();

        ApplyTheme();
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
        themeIndex =
            Mathf.Clamp(
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

        ApplyTheme();

        ThemeChanged?.Invoke(
            CurrentTheme
        );
    }

    private void ApplyTheme()
    {
        if (accessibleColorMaterial == null)
        {
            Debug.LogError(
                "ColorThemeManager: " +
                "Accessible Color Material не е зададен."
            );

            return;
        }

        // =========================
        // ТЕМА 1 - НОРМАЛНА
        // =========================

        if (CurrentTheme == 0)
        {
            accessibleColorMaterial.SetFloat(
                "_Strength",
                0f
            );

            RestoreCanvases();

            return;
        }

        // =========================
        // ТЕМА 2 - COLORBLIND
        // =========================

        accessibleColorMaterial.SetFloat(
            "_Strength",
            accessibleThemeStrength
        );

        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning(
                "ColorThemeManager: " +
                "Не е намерена Main Camera."
            );

            return;
        }

        ConvertOverlayCanvases(
            mainCamera
        );
    }

    private void ConvertOverlayCanvases(
        Camera mainCamera
    )
    {
        Canvas[] canvases =
            FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null)
            {
                continue;
            }

            if (!canvas.isRootCanvas)
            {
                continue;
            }

            if (
                canvas.renderMode !=
                RenderMode.ScreenSpaceOverlay
            )
            {
                continue;
            }

            if (
                !canvasStates.ContainsKey(
                    canvas
                )
            )
            {
                CanvasState state =
                    new CanvasState
                    {
                        renderMode =
                            canvas.renderMode,

                        worldCamera =
                            canvas.worldCamera,

                        planeDistance =
                            canvas.planeDistance
                    };

                canvasStates.Add(
                    canvas,
                    state
                );
            }

            canvas.renderMode =
                RenderMode.ScreenSpaceCamera;

            canvas.worldCamera =
                mainCamera;

            canvas.planeDistance =
                Mathf.Max(
                    canvasPlaneDistance,
                    mainCamera.nearClipPlane
                    + 0.01f
                );
        }
    }

    private void RestoreCanvases()
    {
        foreach (
            KeyValuePair<Canvas, CanvasState> pair
            in canvasStates
        )
        {
            Canvas canvas =
                pair.Key;

            if (canvas == null)
            {
                continue;
            }

            CanvasState state =
                pair.Value;

            canvas.renderMode =
                state.renderMode;

            canvas.worldCamera =
                state.worldCamera;

            canvas.planeDistance =
                state.planeDistance;
        }

        canvasStates.Clear();
    }

    private void ClearDestroyedCanvases()
    {
        List<Canvas> destroyedCanvases =
            new List<Canvas>();

        foreach (
            KeyValuePair<Canvas, CanvasState> pair
            in canvasStates
        )
        {
            if (pair.Key == null)
            {
                destroyedCanvases.Add(
                    pair.Key
                );
            }
        }

        foreach (
            Canvas canvas
            in destroyedCanvases
        )
        {
            canvasStates.Remove(
                canvas
            );
        }
    }
}