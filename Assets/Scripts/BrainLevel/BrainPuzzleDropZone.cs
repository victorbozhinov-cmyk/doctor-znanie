using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BrainPuzzleDropZone : MonoBehaviour
{
    [Header("Correct Answer")]
    [SerializeField] private BrainPartType acceptedPart;

    [Header("Solved / Placed Label")]
    [SerializeField] private GameObject placedLabel;

    [Header("Visual Feedback")]
    [SerializeField] private Color correctColor =
        new Color(0.15f, 1f, 0.3f, 1f);

    [SerializeField] private Color wrongColor =
        new Color(1f, 0.12f, 0.18f, 1f);

    [SerializeField] private float feedbackDuration = 0.4f;

    [SerializeField] private Vector2 outlineDistance =
        new Vector2(4f, -4f);

    private Outline outline;
    private Coroutine feedbackCoroutine;

    private CanvasGroup placedCanvasGroup;
    private RectTransform placedRect;

    private bool solved;

    private const string ThemeKey = "ColorTheme";

    public BrainPartType AcceptedPart => acceptedPart;
    public bool IsSolved => solved;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectDistance = outlineDistance;

        // Не използваме alpha-та на самия slot,
        // за да остане feedback цветът ярък.
        outline.useGraphicAlpha = false;
        outline.enabled = false;

        SetupPlacedLabel();
    }

    // =========================================================
    // PLACED LABEL SETUP
    // =========================================================

    private void SetupPlacedLabel()
    {
        if (placedLabel == null)
        {
            return;
        }

        placedRect =
            placedLabel.GetComponent<RectTransform>();

        placedCanvasGroup =
            placedLabel.GetComponent<CanvasGroup>();

        if (placedCanvasGroup == null)
        {
            placedCanvasGroup =
                placedLabel.AddComponent<CanvasGroup>();
        }

        placedCanvasGroup.alpha = 0f;
        placedCanvasGroup.blocksRaycasts = false;
        placedCanvasGroup.interactable = false;

        if (placedRect != null)
        {
            placedRect.localScale = Vector3.one;
            placedRect.localRotation = Quaternion.identity;
        }

        placedLabel.SetActive(false);
    }

    // =========================================================
    // ACCEPT CHECK
    // =========================================================

    public bool Accepts(BrainPartType part)
    {
        if (solved)
        {
            return false;
        }

        return part == acceptedPart;
    }

    // =========================================================
    // SOLVED / LOCKED STATE
    // =========================================================

    public void MarkSolved()
    {
        solved = true;
    }

    public void ShowPlacedLabel()
    {
        // Зоната остава заключена.
        solved = true;

        if (placedLabel == null)
        {
            Debug.LogWarning(
                "Няма зададен Placed Label за " +
                acceptedPart,
                this
            );

            return;
        }

        placedLabel.SetActive(true);

        if (placedCanvasGroup == null)
        {
            placedCanvasGroup =
                placedLabel.GetComponent<CanvasGroup>();

            if (placedCanvasGroup == null)
            {
                placedCanvasGroup =
                    placedLabel.AddComponent<CanvasGroup>();
            }
        }

        placedCanvasGroup.alpha = 1f;
        placedCanvasGroup.blocksRaycasts = false;
        placedCanvasGroup.interactable = false;

        if (placedRect == null)
        {
            placedRect =
                placedLabel.GetComponent<RectTransform>();
        }

        if (placedRect != null)
        {
            placedRect.localScale = Vector3.one;
            placedRect.localRotation = Quaternion.identity;
        }
    }

    // =========================================================
    // FEEDBACK
    // =========================================================

    public void ShowCorrectFeedback()
    {
        // При достъпния цветен режим не показваме
        // placement Outline feedback, защото може да
        // създава визуални артефакти върху drop зоните.
        if (IsColorblindThemeActive())
        {
            if (outline != null)
            {
                outline.enabled = false;
            }

            return;
        }

        ShowFeedback(correctColor);
    }

    public void ShowWrongFeedback()
    {
        // Решена зона не трябва да дава Wrong feedback.
        if (solved)
        {
            return;
        }

        // При достъпния цветен режим не показваме
        // placement Outline feedback.
        if (IsColorblindThemeActive())
        {
            if (outline != null)
            {
                outline.enabled = false;
            }

            return;
        }

        ShowFeedback(wrongColor);
    }

    private bool IsColorblindThemeActive()
    {
        if (ColorThemeManager.Instance != null)
        {
            return ColorThemeManager.Instance.CurrentTheme == 1;
        }

        return PlayerPrefs.GetInt(
            ThemeKey,
            0
        ) == 1;
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

    private IEnumerator FeedbackRoutine(
        Color color)
    {
        if (outline != null)
        {
            outline.effectColor = color;
            outline.enabled = true;
        }

        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (outline != null)
        {
            outline.enabled = false;
        }

        feedbackCoroutine = null;
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetZone()
    {
        solved = false;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        if (placedLabel != null)
        {
            placedLabel.SetActive(false);
        }

        if (placedCanvasGroup != null)
        {
            placedCanvasGroup.alpha = 0f;
            placedCanvasGroup.blocksRaycasts = false;
            placedCanvasGroup.interactable = false;
        }

        if (placedRect != null)
        {
            placedRect.localScale = Vector3.one;
            placedRect.localRotation = Quaternion.identity;
        }

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    // =========================================================
    // DISABLE
    // =========================================================

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
