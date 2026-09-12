using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LungsPuzzleFeedbackFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Colors")]
    [SerializeField]
    private Color correctColor =
        new Color(0.2f, 1f, 0.3f, 1f);

    [SerializeField]
    private Color wrongColor =
        new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Flash")]
    [SerializeField] private float maxAlpha = 0.18f;
    [SerializeField] private float flashDuration = 0.15f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (flashImage == null)
            flashImage = GetComponent<Image>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void FlashCorrect()
    {
        StartFlash(correctColor);
    }

    public void FlashWrong()
    {
        StartFlash(wrongColor);
    }

    private void StartFlash(Color color)
    {
        if (flashImage == null || canvasGroup == null)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine =
            StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color color)
    {
        flashImage.color = color;

        canvasGroup.alpha = maxAlpha;

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / flashDuration);

            canvasGroup.alpha =
                Mathf.Lerp(maxAlpha, 0f, progress);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        flashCoroutine = null;
    }
}