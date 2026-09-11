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

    // =========================================================
    // ACCESSIBLE COLOR THEME
    // =========================================================

    [Header("Accessible Color Theme")]
    [SerializeField]
    private Material accessibleColorMaterial;

    [SerializeField]
    [Range(0f, 1f)]
    private float accessibleThemeStrength = 0.65f;

    // =========================================================
    // CANVAS RENDERING
    // =========================================================

    [Header("Canvas Rendering")]
    [Tooltip(
        "Колко след Near Clip Plane да стои UI Canvas-ът."
    )]
    [SerializeField]
    private float canvasNearClipOffset = 0.01f;

    [Tooltip(
        "Sorting Order на UI Canvas-ите при активен цветен режим."
    )]
    [SerializeField]
    private int accessibleCanvasSortingOrder = 10000;

    // =========================================================
    // CANVAS STATE
    // =========================================================

    private struct CanvasState
    {
        public RenderMode renderMode;
        public Camera worldCamera;
        public float planeDistance;

        public bool overrideSorting;
        public int sortingOrder;
        public int sortingLayerID;
    }

    private readonly Dictionary<Canvas, CanvasState> canvasStates =
        new Dictionary<Canvas, CanvasState>();

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

    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ApplyTheme();
    }

    // =========================================================
    // SCENE LOADED
    // =========================================================

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

    // =========================================================
    // PUBLIC THEME METHODS
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

    // =========================================================
    // APPLY THEME
    // =========================================================

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

        // =====================================================
        // THEME 0 - NORMAL
        // =====================================================

        if (CurrentTheme == 0)
        {
            accessibleColorMaterial.SetFloat(
                "_Strength",
                0f
            );

            RestoreCanvases();

            return;
        }

        // =====================================================
        // THEME 1 - COLORBLIND
        // =====================================================

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

    // =========================================================
    // CONVERT OVERLAY CANVASES
    // =========================================================

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

            // Ако този Canvas вече е бил конвертиран,
            // не го записваме и променяме втори път.
            if (canvasStates.ContainsKey(canvas))
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

            // =================================================
            // SAVE ORIGINAL STATE
            // =================================================

            CanvasState state =
                new CanvasState
                {
                    renderMode =
                        canvas.renderMode,

                    worldCamera =
                        canvas.worldCamera,

                    planeDistance =
                        canvas.planeDistance,

                    overrideSorting =
                        canvas.overrideSorting,

                    sortingOrder =
                        canvas.sortingOrder,

                    sortingLayerID =
                        canvas.sortingLayerID
                };

            canvasStates.Add(
                canvas,
                state
            );

            // =================================================
            // CONVERT TO SCREEN SPACE CAMERA
            // =================================================

            canvas.renderMode =
                RenderMode.ScreenSpaceCamera;

            canvas.worldCamera =
                mainCamera;

            // Поставяме Canvas-а максимално близо
            // до камерата, но след Near Clip Plane.
            canvas.planeDistance =
                mainCamera.nearClipPlane +
                Mathf.Max(
                    0.001f,
                    canvasNearClipOffset
                );

            // =================================================
            // FORCE UI ABOVE WORLD ELEMENTS
            // =================================================

            canvas.overrideSorting = true;

            canvas.sortingOrder =
                accessibleCanvasSortingOrder +
                state.sortingOrder;
        }
    }

    // =========================================================
    // RESTORE CANVASES
    // =========================================================

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

            canvas.overrideSorting =
                state.overrideSorting;

            canvas.sortingOrder =
                state.sortingOrder;

            canvas.sortingLayerID =
                state.sortingLayerID;
        }

        canvasStates.Clear();
    }

    // =========================================================
    // CLEAR DESTROYED CANVASES
    // =========================================================

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