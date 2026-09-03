using System.Collections;
using UnityEngine;

public class BrainOrderTimerWarningAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform animationTarget;

    [Header("Pulse")]
    [SerializeField]
    private float lightScale = 1.05f;

    [SerializeField]
    private float mediumScale = 1.08f;

    [SerializeField]
    private float strongScale = 1.12f;

    [Header("Shake")]
    [SerializeField]
    private float lightShake = 0.02f;

    [SerializeField]
    private float mediumShake = 0.04f;

    [SerializeField]
    private float strongShake = 0.06f;

    [Header("Timing")]
    [SerializeField]
    private float animationDuration = 0.22f;

    [SerializeField]
    private float shakeSpeed = 40f;

    private Vector3 originalScale;
    private Vector3 originalLocalPosition;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (animationTarget == null)
        {
            animationTarget = transform;
        }

        originalScale =
            animationTarget.localScale;

        originalLocalPosition =
            animationTarget.localPosition;
    }

    public void PlayWarning(
        float remainingSeconds)
    {
        if (animationTarget == null)
            return;

        float targetScale;
        float shakeStrength;

        if (remainingSeconds <= 2f)
        {
            targetScale = strongScale;
            shakeStrength = strongShake;
        }
        else if (remainingSeconds <= 5f)
        {
            targetScale = mediumScale;
            shakeStrength = mediumShake;
        }
        else
        {
            targetScale = lightScale;
            shakeStrength = lightShake;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine);
        }

        animationTarget.localScale =
            originalScale;

        animationTarget.localPosition =
            originalLocalPosition;

        animationCoroutine =
            StartCoroutine(
                PlayWarningRoutine(
                    targetScale,
                    shakeStrength));
    }

    private IEnumerator PlayWarningRoutine(
        float targetScaleMultiplier,
        float shakeStrength)
    {
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / animationDuration);

            float pulse =
                Mathf.Sin(
                    progress * Mathf.PI);

            float scaleMultiplier =
                Mathf.Lerp(
                    1f,
                    targetScaleMultiplier,
                    pulse);

            animationTarget.localScale =
                originalScale *
                scaleMultiplier;

            float fade =
                1f - progress;

            float offsetX =
                Mathf.Sin(
                    timer * shakeSpeed) *
                shakeStrength *
                fade;

            animationTarget.localPosition =
                originalLocalPosition +
                new Vector3(
                    offsetX,
                    0f,
                    0f);

            yield return null;
        }

        animationTarget.localScale =
            originalScale;

        animationTarget.localPosition =
            originalLocalPosition;

        animationCoroutine = null;
    }

    public void ResetAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine);

            animationCoroutine = null;
        }

        if (animationTarget != null)
        {
            animationTarget.localScale =
                originalScale;

            animationTarget.localPosition =
                originalLocalPosition;
        }
    }

    private void OnDisable()
    {
        ResetAnimation();
    }
}