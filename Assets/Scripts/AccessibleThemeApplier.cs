using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccessibleThemeApplier : MonoBehaviour
{
    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Accessible Theme")]
    [SerializeField]
    private Material accessibleMaterial;

    // =========================================================
    // RUNTIME
    // =========================================================

    private readonly Dictionary<Graphic, Material> originalMaterials =
        new Dictionary<Graphic, Material>();

    private Coroutine refreshCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(
            SubscribeNextFrame()
        );
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ThemeChanged -= OnThemeChanged;
        }
    }

    // =========================================================
    // SUBSCRIBE
    // =========================================================

    private IEnumerator SubscribeNextFrame()
    {
        yield return null;

        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            ColorThemeManager.Instance.ThemeChanged += OnThemeChanged;

            ApplyCurrentTheme();
        }
        else
        {
            Debug.LogWarning(
                "AccessibleThemeApplier: ColorThemeManager не е намерен."
            );
        }
    }

    // =========================================================
    // SCENE
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StartRefresh();
    }

    private void StartRefresh()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }

        refreshCoroutine =
            StartCoroutine(
                RefreshNextFrame()
            );
    }

    private IEnumerator RefreshNextFrame()
    {
        // Даваме време UI обектите в новата сцена
        // да приключат Awake / OnEnable.
        yield return null;

        RemoveDestroyedReferences();

        ApplyCurrentTheme();

        refreshCoroutine = null;
    }

    // =========================================================
    // THEME CHANGE
    // =========================================================

    private void OnThemeChanged(int themeIndex)
    {
        ApplyCurrentTheme();
    }

    // =========================================================
    // APPLY
    // =========================================================

    public void ApplyCurrentTheme()
    {
        if (ColorThemeManager.Instance == null)
        {
            return;
        }

        if (ColorThemeManager.Instance.IsColorblindThemeActive())
        {
            ApplyAccessibleTheme();
        }
        else
        {
            RestoreDefaultTheme();
        }
    }

    // =========================================================
    // ACCESSIBLE THEME
    // =========================================================

    private void ApplyAccessibleTheme()
    {
        if (accessibleMaterial == null)
        {
            Debug.LogWarning(
                "AccessibleThemeApplier: Accessible Material не е зададен."
            );

            return;
        }

        Scene activeScene =
            SceneManager.GetActiveScene();

        GameObject[] rootObjects =
            activeScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            Image[] images =
                rootObject.GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
            {
                ApplyToGraphic(image);
            }

            RawImage[] rawImages =
                rootObject.GetComponentsInChildren<RawImage>(true);

            foreach (RawImage rawImage in rawImages)
            {
                ApplyToGraphic(rawImage);
            }
        }
    }

    // =========================================================
    // APPLY TO ONE GRAPHIC
    // =========================================================

    private void ApplyToGraphic(Graphic graphic)
    {
        if (graphic == null)
        {
            return;
        }

        if (ShouldIgnore(graphic.transform))
        {
            RestoreGraphic(graphic);
            return;
        }

        // Запазваме оригиналния материал само първия път.
        if (!originalMaterials.ContainsKey(graphic))
        {
            originalMaterials.Add(
                graphic,
                graphic.material
            );
        }

        graphic.material =
            accessibleMaterial;

        graphic.SetMaterialDirty();
    }

    // =========================================================
    // IGNORE
    // =========================================================

    private bool ShouldIgnore(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        Transform current =
            target;

        bool isTargetObject = true;

        while (current != null)
        {
            IgnoreAccessibleTheme ignore =
                current.GetComponent<IgnoreAccessibleTheme>();

            if (ignore != null)
            {
                // Marker върху самия обект винаги
                // изключва самия обект.
                if (isTargetObject)
                {
                    return true;
                }

                // Marker върху родител изключва детето
                // само ако Ignore Children е включено.
                if (ignore.IgnoreChildren)
                {
                    return true;
                }
            }

            isTargetObject = false;
            current = current.parent;
        }

        return false;
    }

    // =========================================================
    // RESTORE DEFAULT
    // =========================================================

    private void RestoreDefaultTheme()
    {
        List<Graphic> graphics =
            new List<Graphic>(
                originalMaterials.Keys
            );

        foreach (Graphic graphic in graphics)
        {
            RestoreGraphic(graphic);
        }
    }

    private void RestoreGraphic(Graphic graphic)
    {
        if (graphic == null)
        {
            return;
        }

        if (!originalMaterials.TryGetValue(
                graphic,
                out Material originalMaterial))
        {
            return;
        }

        graphic.material =
            originalMaterial;

        graphic.SetMaterialDirty();
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void RemoveDestroyedReferences()
    {
        List<Graphic> destroyed =
            new List<Graphic>();

        foreach (
            KeyValuePair<Graphic, Material> pair
            in originalMaterials
        )
        {
            if (pair.Key == null)
            {
                destroyed.Add(pair.Key);
            }
        }

        foreach (Graphic graphic in destroyed)
        {
            originalMaterials.Remove(graphic);
        }
    }

    // =========================================================
    // MANUAL REFRESH
    // =========================================================

    public void RefreshTheme()
    {
        ApplyCurrentTheme();
    }
}