using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPopupAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float openDuration = 0.7f;
    [SerializeField] private float closeDuration = 0.35f;
    [SerializeField] private float startScale = 0.75f;
    [SerializeField] private float overshootScale = 1.03f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private float timer;
    private bool opening;
    private bool closing;

    private Action onCloseFinished;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        PlayOpen();
    }

    private void Update()
    {
        if (!opening && !closing)
            return;

        timer += Time.unscaledDeltaTime;

        float duration = opening ? openDuration : closeDuration;
        float progress = Mathf.Clamp01(timer / duration);

        if (opening)
        {
            PlayOpeningAnimation(progress);
        }
        else if (closing)
        {
            PlayClosingAnimation(progress);
        }

        if (progress >= 1f)
        {
            FinishAnimation();
        }
    }

    private void PlayOpeningAnimation(float progress)
    {
        canvasGroup.alpha = progress;

        float scale;

        if (progress < 0.8f)
        {
            scale = Mathf.Lerp(
                startScale,
                overshootScale,
                progress / 0.8f
            );
        }
        else
        {
            scale = Mathf.Lerp(
                overshootScale,
                1f,
                (progress - 0.8f) / 0.2f
            );
        }

        rectTransform.localScale = Vector3.one * scale;
    }

    private void PlayClosingAnimation(float progress)
    {
        float reversedProgress = 1f - progress;

        canvasGroup.alpha = reversedProgress;

        float scale = Mathf.Lerp(
            1f,
            startScale,
            progress
        );

        rectTransform.localScale = Vector3.one * scale;
    }

    private void FinishAnimation()
    {
        if (opening)
        {
            opening = false;

            canvasGroup.alpha = 1f;
            rectTransform.localScale = Vector3.one;
        }
        else if (closing)
        {
            closing = false;

            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.one * startScale;

            Action callback = onCloseFinished;
            onCloseFinished = null;
            callback?.Invoke();
        }
    }

    public void PlayOpen()
    {
        timer = 0f;

        opening = true;
        closing = false;

        onCloseFinished = null;

        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * startScale;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void PlayClose(Action onFinished)
    {
        if (closing)
            return;

        timer = 0f;

        opening = false;
        closing = true;

        onCloseFinished = onFinished;

        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}