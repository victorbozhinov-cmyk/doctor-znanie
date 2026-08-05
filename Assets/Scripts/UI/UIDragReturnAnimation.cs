using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIDragReturnAnimation : MonoBehaviour
{
    [Header("Return Animation")]
    [SerializeField] private float returnDuration = 0.24f;

    private RectTransform rectTransform;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void PlayReturn(
        Transform targetParent,
        Vector2 targetAnchoredPosition,
        Vector3 targetScale,
        Action onFinished = null)
    {
        if (targetParent == null)
        {
            Debug.LogError(
                "UIDragReturnAnimation няма зададен target parent.",
                this
            );

            onFinished?.Invoke();
            return;
        }

        CancelAnimation();

        returnCoroutine = StartCoroutine(
            ReturnRoutine(
                targetParent,
                targetAnchoredPosition,
                targetScale,
                onFinished
            )
        );
    }

    public void CancelAnimation()
    {
        if (returnCoroutine == null)
        {
            return;
        }

        StopCoroutine(returnCoroutine);
        returnCoroutine = null;
    }

    private IEnumerator ReturnRoutine(
        Transform targetParent,
        Vector2 targetAnchoredPosition,
        Vector3 targetScale,
        Action onFinished)
    {
        /*
         * Запазваме текущата световна позиция,
         * преди да върнем етикета към стария parent.
         */
        Vector3 currentWorldPosition =
            rectTransform.position;

        rectTransform.SetParent(
            targetParent,
            true
        );

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        /*
         * След промяната на anchor и pivot
         * възстановяваме видимата текуща позиция.
         */
        rectTransform.position =
            currentWorldPosition;

        Vector2 startPosition =
            rectTransform.anchoredPosition;

        Vector3 startScale =
            rectTransform.localScale;

        float timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / returnDuration
            );

            float eased = EaseOutCubic(
                progress
            );

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetAnchoredPosition,
                    eased
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    eased
                );

            yield return null;
        }

        rectTransform.anchoredPosition =
            targetAnchoredPosition;

        rectTransform.localScale =
            targetScale;

        rectTransform.localRotation =
            Quaternion.identity;

        returnCoroutine = null;

        onFinished?.Invoke();
    }

    private float EaseOutCubic(float value)
    {
        float inverted = 1f - value;

        return 1f -
            inverted *
            inverted *
            inverted;
    }

    private void OnDisable()
    {
        CancelAnimation();
    }
}