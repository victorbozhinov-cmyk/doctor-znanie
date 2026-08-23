using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StomachStatePanelAnimation : MonoBehaviour
{
    [Header("Pop")]
    [SerializeField] private float popScale = 1.04f;
    [SerializeField] private float popUpDuration = 0.10f;
    [SerializeField] private float settleDuration = 0.14f;

    [Header("Small Jump")]
    [SerializeField] private float jumpDistance = 8f;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = true;

    private RectTransform rectTransform;

    private Vector2 normalPosition;
    private Vector3 normalScale;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
        {
            normalPosition =
                rectTransform.anchoredPosition;

            normalScale =
                rectTransform.localScale;
        }
    }

    private void Update()
    {
        if (!enableTestKey ||
            Keyboard.current == null)
        {
            return;
        }

        // P = временен тест
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlayCardChange();
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayCardChange()
    {
        if (rectTransform == null)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            rectTransform.anchoredPosition =
                normalPosition;

            rectTransform.localScale =
                normalScale;
        }

        animationCoroutine =
            StartCoroutine(
                CardChangeRoutine()
            );
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator CardChangeRoutine()
    {
        Vector2 raisedPosition =
            normalPosition +
            Vector2.up * jumpDistance;

        Vector3 raisedScale =
            normalScale * popScale;

        // Леко нагоре + уголемяване.
        yield return AnimateTo(
            raisedPosition,
            raisedScale,
            popUpDuration
        );

        // Плавно обратно на мястото.
        yield return AnimateTo(
            normalPosition,
            normalScale,
            settleDuration
        );

        rectTransform.anchoredPosition =
            normalPosition;

        rectTransform.localScale =
            normalScale;

        animationCoroutine = null;
    }

    // =========================================================
    // HELPER
    // =========================================================

    private IEnumerator AnimateTo(
        Vector2 targetPosition,
        Vector3 targetScale,
        float duration)
    {
        Vector2 startPosition =
            rectTransform.anchoredPosition;

        Vector3 startScale =
            rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            // SmoothStep
            t =
                t * t *
                (3f - 2f * t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        rectTransform.anchoredPosition =
            targetPosition;

        rectTransform.localScale =
            targetScale;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                normalPosition;

            rectTransform.localScale =
                normalScale;
        }
    }
}