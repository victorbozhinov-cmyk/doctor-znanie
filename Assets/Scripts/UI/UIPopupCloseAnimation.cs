using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPopupCloseAnimation : MonoBehaviour
{
    [Header("Close Animation")]
    [SerializeField] private float closeDuration = 0.35f;

    [SerializeField] private float endScale = 0.75f;

    [SerializeField] private float overshootScale = 1.03f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private float timer;
    private bool closing;

    private Action onCloseFinished;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!closing)
        {
            return;
        }

        timer += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(
            timer / closeDuration
        );

        PlayClosingAnimation(progress);

        if (progress >= 1f)
        {
            FinishClose();
        }
    }

    private void PlayClosingAnimation(float progress)
    {
        // Fade Out
        canvasGroup.alpha = 1f - progress;

        float scale;

        // Точно обратното на open анимацията:
        // 1.00 -> 1.03 -> 0.75

        if (progress < 0.2f)
        {
            scale = Mathf.Lerp(
                1f,
                overshootScale,
                progress / 0.2f
            );
        }
        else
        {
            scale = Mathf.Lerp(
                overshootScale,
                endScale,
                (progress - 0.2f) / 0.8f
            );
        }

        rectTransform.localScale =
            Vector3.one * scale;
    }

    private void FinishClose()
    {
        closing = false;

        canvasGroup.alpha = 0f;

        rectTransform.localScale =
            Vector3.one * endScale;

        Action callback = onCloseFinished;
        onCloseFinished = null;

        callback?.Invoke();
    }

    // Използва се директно от Button OnClick()
    public void PlayClose()
    {
        PlayClose(() =>
        {
            gameObject.SetActive(false);
        });
    }

    // Използва се от други скриптове,
    // когато искаме действие след анимацията
    public void PlayClose(Action onFinished)
    {
        if (closing)
        {
            return;
        }

        timer = 0f;
        closing = true;

        onCloseFinished = onFinished;

        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsClosing()
    {
        return closing;
    }
}