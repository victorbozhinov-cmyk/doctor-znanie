using System.Collections;
using UnityEngine;

public class CriticalHeartShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float strength = 8f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void PlayShake()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine =
            StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        originalPosition =
            rectTransform.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress =
                timer / duration;

            float currentStrength =
                Mathf.Lerp(
                    strength,
                    0f,
                    progress
                );

            Vector2 offset =
                Random.insideUnitCircle *
                currentStrength;

            rectTransform.anchoredPosition =
                originalPosition + offset;

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalPosition;

        shakeCoroutine = null;
    }
}
