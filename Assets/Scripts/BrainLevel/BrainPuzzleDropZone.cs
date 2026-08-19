using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BrainPuzzleDropZone : MonoBehaviour
{
    [Header("Correct Answer")]
    [SerializeField] private BrainPartType acceptedPart;

    [Header("Visual Feedback")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private float feedbackDuration = 0.4f;
    [SerializeField] private Vector2 outlineDistance = new Vector2(4f, -4f);

    private Outline outline;
    private Coroutine feedbackCoroutine;

    public BrainPartType AcceptedPart => acceptedPart;

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

    public bool Accepts(BrainPartType part)
    {
        return part == acceptedPart;
    }

    public void ShowCorrectFeedback()
    {
        ShowFeedback(correctColor);
    }

    public void ShowWrongFeedback()
    {
        ShowFeedback(wrongColor);
    }

    private void ShowFeedback(Color color)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(FeedbackRoutine(color));
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
