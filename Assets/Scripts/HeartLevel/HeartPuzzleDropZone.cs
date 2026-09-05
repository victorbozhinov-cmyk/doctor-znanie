using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartPuzzleDropZone : MonoBehaviour
{
    [Header("Correct Answer")]
    [SerializeField] private HeartPartType acceptedPart;

    [Header("Normal Theme Feedback")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    [Header("Colorblind Theme Feedback")]
    [SerializeField]
    private Color colorblindCorrectColor =
        new Color32(0, 114, 178, 255); // #0072B2

    [SerializeField]
    private Color colorblindWrongColor =
        new Color32(213, 94, 0, 255); // #D55E00

    [Header("Feedback Settings")]
    [SerializeField] private float feedbackDuration = 0.4f;

    [SerializeField]
    private Vector2 outlineDistance =
        new Vector2(4f, -4f);

    private Outline outline;
    private Coroutine feedbackCoroutine;

    private const string ThemeKey = "ColorTheme";

    public HeartPartType AcceptedPart => acceptedPart;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectDistance = outlineDistance;
        outline.useGraphicAlpha = true;
        outline.enabled = false;
    }

    public bool Accepts(HeartPartType part)
    {
        return part == acceptedPart;
    }

    public void ShowCorrectFeedback()
    {
        Color selectedColor;

        if (IsColorblindThemeActive())
        {
            selectedColor = colorblindCorrectColor;
        }
        else
        {
            selectedColor = correctColor;
        }

        ShowFeedback(selectedColor);
    }

    public void ShowWrongFeedback()
    {
        Color selectedColor;

        if (IsColorblindThemeActive())
        {
            selectedColor = colorblindWrongColor;
        }
        else
        {
            selectedColor = wrongColor;
        }

        ShowFeedback(selectedColor);
    }

    private bool IsColorblindThemeActive()
    {
        // Нормалният вариант:
        // използваме глобалния manager.
        if (ColorThemeManager.Instance != null)
        {
            return ColorThemeManager.Instance.CurrentTheme == 1;
        }

        // Резервен вариант:
        // ако сцената е пусната директно без Bootstrap,
        // четем запазената тема.
        return PlayerPrefs.GetInt(ThemeKey, 0) == 1;
    }

    private void ShowFeedback(Color color)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine =
            StartCoroutine(
                FeedbackRoutine(color)
            );
    }

    private IEnumerator FeedbackRoutine(Color color)
    {
        outline.effectColor = color;
        outline.enabled = true;

        yield return new WaitForSeconds(feedbackDuration);

        outline.enabled = false;
        feedbackCoroutine = null;
    }

    private void OnDisable()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}