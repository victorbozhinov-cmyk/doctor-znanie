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

    public BrainPartType AcceptedPart => acceptedPart;
    public bool IsSolved => solved;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectDistance = outlineDistance;

        // Важно:
        // не използваме alpha-та на самия slot,
        // за да остане feedback цветът ярък.
        outline.useGraphicAlpha = false;

        outline.enabled = false;

        SetupPlacedLabel();
    }

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
        }

        placedLabel.SetActive(false);
    }

    public bool Accepts(BrainPartType part)
    {
        return !solved &&
               part == acceptedPart;
    }

    public void ShowCorrectFeedback()
    {
        ShowFeedback(correctColor);
    }

    public void ShowWrongFeedback()
    {
        ShowFeedback(wrongColor);
    }

    public void MarkSolved()
    {
        solved = true;
    }

    public void ShowPlacedLabel()
    {
        if (placedLabel == null)
        {
            Debug.LogWarning(
                "Няма зададен Placed Label за " +
                acceptedPart,
                this
            );

            return;
        }

        solved = true;

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
        }
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

    public void ResetZone()
    {
        solved = false;

        if (placedLabel != null)
        {
            placedLabel.SetActive(false);
        }

        if (placedCanvasGroup != null)
        {
            placedCanvasGroup.alpha = 0f;
        }

        if (placedRect != null)
        {
            placedRect.localScale = Vector3.one;
        }

        if (outline != null)
        {
            outline.enabled = false;
        }
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