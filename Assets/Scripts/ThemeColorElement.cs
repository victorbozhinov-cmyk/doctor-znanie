using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeColorElement : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;

    [Header("Theme 2 Colors")]
    [SerializeField] private Color themeTwoColor = Color.green;
    [SerializeField] private Color themeTwoOutlineColor = Color.black;

    private Color originalGraphicColor;

    private TMP_Text tmpText;
    private Color originalTextColor;
    private Color32 originalFaceColor;
    private Color32 originalOutlineColor;

    private bool isSubscribed;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetGraphic == null)
        {
            return;
        }

        // Запомняме оригиналния вид на Тема 1.
        originalGraphicColor = targetGraphic.color;

        tmpText = targetGraphic as TMP_Text;

        if (tmpText != null)
        {
            originalTextColor = tmpText.color;
            originalFaceColor = tmpText.faceColor;
            originalOutlineColor = tmpText.outlineColor;
        }
    }

    private void Start()
    {
        ConnectToThemeManager();
    }

    private void OnEnable()
    {
        if (ColorThemeManager.Instance != null)
        {
            ConnectToThemeManager();
        }
    }

    private void OnDisable()
    {
        if (isSubscribed && ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ThemeChanged -= ApplyTheme;
        }

        isSubscribed = false;
    }

    private void ConnectToThemeManager()
    {
        if (ColorThemeManager.Instance == null)
        {
            return;
        }

        if (!isSubscribed)
        {
            ColorThemeManager.Instance.ThemeChanged += ApplyTheme;
            isSubscribed = true;
        }

        ApplyTheme(ColorThemeManager.Instance.CurrentTheme);
    }

    private void ApplyTheme(int themeIndex)
    {
        if (targetGraphic == null)
        {
            return;
        }

        if (themeIndex == 0)
        {
            RestoreThemeOne();
        }
        else
        {
            ApplyThemeTwo();
        }
    }

    private void RestoreThemeOne()
    {
        // Връща абсолютно оригиналните настройки от сцената.
        targetGraphic.color = originalGraphicColor;

        if (tmpText != null)
        {
            tmpText.color = originalTextColor;
            tmpText.faceColor = originalFaceColor;
            tmpText.outlineColor = originalOutlineColor;
        }
    }

    private void ApplyThemeTwo()
    {
        if (tmpText != null)
        {
            // Бял vertex цвят, за да не се смесва синьото със зеления цвят.
            tmpText.color = Color.white;
            tmpText.faceColor = themeTwoColor;
            tmpText.outlineColor = themeTwoOutlineColor;
        }
        else
        {
            targetGraphic.color = themeTwoColor;
        }
    }
}