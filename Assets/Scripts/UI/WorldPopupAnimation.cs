using System;
using UnityEngine;

public class WorldPopupAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float openDuration = 0.7f;
    [SerializeField] private float closeDuration = 0.35f;
    [SerializeField] private float startScale = 0.75f;
    [SerializeField] private float overshootScale = 1.03f;

    private Vector3 originalScale;

    private float timer;
    private bool opening;
    private bool closing;

    private Action onCloseFinished;

    private void Awake()
    {
        originalScale = transform.localScale;
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

        float duration =
            opening
                ? openDuration
                : closeDuration;

        if (duration <= 0f)
            duration = 0.0001f;

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
        float scaleMultiplier;

        if (progress < 0.8f)
        {
            scaleMultiplier =
                Mathf.Lerp(
                    startScale,
                    overshootScale,
                    progress / 0.8f
                );
        }
        else
        {
            scaleMultiplier =
                Mathf.Lerp(
                    overshootScale,
                    1f,
                    (progress - 0.8f) / 0.2f
                );
        }

        transform.localScale =
            originalScale * scaleMultiplier;
    }

    private void PlayClosingAnimation(float progress)
    {
        float scaleMultiplier =
            Mathf.Lerp(
                1f,
                startScale,
                progress
            );

        transform.localScale =
            originalScale * scaleMultiplier;
    }

    private void FinishAnimation()
    {
        if (opening)
        {
            opening = false;

            transform.localScale =
                originalScale;
        }
        else if (closing)
        {
            closing = false;

            transform.localScale =
                originalScale * startScale;

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

        transform.localScale =
            originalScale * startScale;
    }

    public void PlayClose(Action onFinished)
    {
        if (closing)
            return;

        timer = 0f;

        opening = false;
        closing = true;

        onCloseFinished = onFinished;

        transform.localScale =
            originalScale;
    }

    private void OnDisable()
    {
        opening = false;
        closing = false;

        timer = 0f;

        transform.localScale =
            originalScale;

        onCloseFinished = null;
    }
}
