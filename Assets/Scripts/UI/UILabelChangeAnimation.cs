using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UILabelChangeAnimation : MonoBehaviour
{
    [Header("Pop Animation")]
    [SerializeField] private float startScale = 0.82f;
    [SerializeField] private float overshootScale = 1.10f;
    [SerializeField] private float growDuration = 0.14f;
    [SerializeField] private float settleDuration = 0.08f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void ChangeSprite(
        Image image,
        Sprite newSprite,
        Action onFinished = null)
    {
        if (image == null)
        {
            Debug.LogError(
                "Image не е подаден към UILabelChangeAnimation."
            );

            onFinished?.Invoke();
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        image.sprite = newSprite;
        image.preserveAspect = true;

        animationCoroutine = StartCoroutine(
            PopRoutine(onFinished)
        );
    }

    private IEnumerator PopRoutine(Action onFinished)
    {
        rectTransform.localScale =
            originalScale * startScale;

        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / growDuration
            );

            float eased = EaseOutBack(progress);

            float scaleMultiplier = Mathf.Lerp(
                startScale,
                overshootScale,
                eased
            );

            rectTransform.localScale =
                originalScale * scaleMultiplier;

            yield return null;
        }

        rectTransform.localScale =
            originalScale * overshootScale;

        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / settleDuration
            );

            float eased = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            rectTransform.localScale =
                Vector3.Lerp(
                    originalScale * overshootScale,
                    originalScale,
                    eased
                );

            yield return null;
        }

        rectTransform.localScale = originalScale;
        animationCoroutine = null;

        onFinished?.Invoke();
    }

    private float EaseOutBack(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        float shiftedValue = value - 1f;

        return 1f
            + c3 * shiftedValue
            * shiftedValue
            * shiftedValue
            + c1 * shiftedValue
            * shiftedValue;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }
}