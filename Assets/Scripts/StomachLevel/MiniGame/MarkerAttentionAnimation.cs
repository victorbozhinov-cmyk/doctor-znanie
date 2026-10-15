using System.Collections;
using UnityEngine;

public class MarkerAttentionAnimation : MonoBehaviour
{
    [Header("Attention Scale")]
    [SerializeField] private float popScale = 1.28f;

    [Header("Wiggle")]
    [SerializeField] private float wiggleAngle = 8f;
    [SerializeField] private int wiggleCount = 2;

    [Header("Timing")]
    [SerializeField] private float popDuration = 0.10f;
    [SerializeField] private float wiggleStepDuration = 0.07f;
    [SerializeField] private float settleDuration = 0.12f;

    [Header("Reminder")]
    [SerializeField] private float repeatDelay = 0.75f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Coroutine attentionCoroutine;
    private bool attentionStoppedByPlayer;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
        {
            originalScale =
                rectTransform.localScale;

            originalRotation =
                rectTransform.localRotation;
        }
    }

    private void OnEnable()
    {
        attentionStoppedByPlayer = false;
        PlayAttention();
    }

    private void OnDisable()
    {
        StopAttentionImmediate();
        attentionStoppedByPlayer = false;
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayAttention()
    {
        if (rectTransform == null ||
            attentionStoppedByPlayer)
        {
            return;
        }

        if (attentionCoroutine != null)
        {
            StopCoroutine(
                attentionCoroutine
            );
        }

        ResetTransform();

        attentionCoroutine =
            StartCoroutine(
                AttentionLoop()
            );
    }

    public void StopAttention()
    {
        attentionStoppedByPlayer = true;
        StopAttentionImmediate();
    }

    // =========================================================
    // LOOP
    // =========================================================

    private IEnumerator AttentionLoop()
    {
        while (!attentionStoppedByPlayer)
        {
            // Силен, но кратък pop.
            yield return AnimateTransform(
                originalScale * popScale,
                originalRotation,
                popDuration
            );

            // Няколко ясни поклащания.
            for (int i = 0;
                 i < wiggleCount;
                 i++)
            {
                yield return AnimateTransform(
                    originalScale * 1.16f,
                    originalRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        -wiggleAngle
                    ),
                    wiggleStepDuration
                );

                yield return AnimateTransform(
                    originalScale * 1.16f,
                    originalRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        wiggleAngle
                    ),
                    wiggleStepDuration
                );
            }

            // Връщане към нормалния marker.
            yield return AnimateTransform(
                originalScale,
                originalRotation,
                settleDuration
            );

            ResetTransform();

            if (attentionStoppedByPlayer)
                break;

            yield return new WaitForSeconds(
                repeatDelay
            );
        }

        ResetTransform();
        attentionCoroutine = null;
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator AnimateTransform(
        Vector3 targetScale,
        Quaternion targetRotation,
        float duration)
    {
        Vector3 startScale =
            rectTransform.localScale;

        Quaternion startRotation =
            rectTransform.localRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            rectTransform.localRotation =
                Quaternion.Lerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        rectTransform.localScale =
            targetScale;

        rectTransform.localRotation =
            targetRotation;
    }

    // =========================================================
    // STOP / RESET
    // =========================================================

    private void StopAttentionImmediate()
    {
        if (attentionCoroutine != null)
        {
            StopCoroutine(
                attentionCoroutine
            );

            attentionCoroutine = null;
        }

        ResetTransform();
    }

    private void ResetTransform()
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale =
            originalScale;

        rectTransform.localRotation =
            originalRotation;
    }
}
