using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class PauseUIPopupAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float openDuration = 0.7f;
    [SerializeField] private float closeDuration = 0.35f;
    [SerializeField] private float startScale = 0.75f;
    [SerializeField] private float overshootScale = 1.03f;

    [Header("Fade")]
    [SerializeField] private bool useFade = true;

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

        float duration = opening
            ? openDuration
            : closeDuration;

        if (duration <= 0f)
        {
            FinishAnimation();
            return;
        }

        float progress =
            Mathf.Clamp01(timer / duration);

        if (opening)
        {
            PlayOpeningAnimation(progress);
        }
        else
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
        if (useFade)
        {
            canvasGroup.alpha = progress;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

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

        rectTransform.localScale =
            Vector3.one * scale;
    }

    private void PlayClosingAnimation(float progress)
    {
        if (useFade)
        {
            canvasGroup.alpha =
                1f - progress;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        float scale =
            Mathf.Lerp(
                1f,
                startScale,
                progress
            );

        rectTransform.localScale =
            Vector3.one * scale;
    }

    private void FinishAnimation()
    {
        if (opening)
        {
            opening = false;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            rectTransform.localScale =
                Vector3.one;

            return;
        }

        if (closing)
        {
            closing = false;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false;

            rectTransform.localScale =
                Vector3.one;

            Action callback =
                onCloseFinished;

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

        canvasGroup.alpha =
            useFade ? 0f : 1f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = false;

        rectTransform.localScale =
            Vector3.one * startScale;
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
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = false;

        rectTransform.localScale =
            Vector3.one;
    }

    private void OnDisable()
    {
        opening = false;
        closing = false;

        timer = 0f;

        onCloseFinished = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale =
                Vector3.one;
        }
    }
}