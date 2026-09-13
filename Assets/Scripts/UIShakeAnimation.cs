using System.Collections;
using UnityEngine;

public class UIShakeAnimation : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float strength = 12f;
    [SerializeField] private int vibrations = 8;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            originalPosition =
                rectTransform.anchoredPosition;
        }
    }

    public void Shake()
    {
        if (rectTransform == null)
        {
            return;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);

            rectTransform.anchoredPosition =
                originalPosition;
        }

        shakeCoroutine =
            StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        originalPosition =
            rectTransform.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Постепенно отслабва разтрисането.
            float currentStrength =
                strength * (1f - progress);

            float wave =
                Mathf.Sin(
                    progress *
                    vibrations *
                    Mathf.PI *
                    2f
                );

            Vector2 offset =
                new Vector2(
                    wave * currentStrength,
                    0f
                );

            rectTransform.anchoredPosition =
                originalPosition + offset;

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalPosition;

        shakeCoroutine = null;
    }
}
