using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Image flashImage;

    [Header("Animation")]
    [SerializeField] private float maxAlpha = 0.45f;
    [SerializeField] private float fadeInDuration = 0.08f;
    [SerializeField] private float holdDuration = 0.05f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private Coroutine flashCoroutine;

    private Color currentFlashColor = Color.red;

    private void Awake()
    {
        if (flashImage == null)
        {
            Debug.LogWarning(
                "ScreenFlash: Flash Image is not assigned.");

            return;
        }

        SetAlpha(0f);
        flashImage.gameObject.SetActive(false);
    }

    public void PlayRedFlash()
    {
        PlayFlash(Color.red);
    }

    public void PlayGreenFlash()
    {
        PlayFlash(Color.green);
    }

    private void PlayFlash(Color flashColor)
    {
        if (flashImage == null)
            return;

        currentFlashColor = flashColor;

        flashImage.gameObject.SetActive(true);

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        SetAlpha(0f);

        flashCoroutine =
            StartCoroutine(
                FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;

        // Fade In
        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeInDuration);

            float alpha =
                Mathf.Lerp(
                    0f,
                    maxAlpha,
                    t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(maxAlpha);

        // Hold
        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                holdDuration);
        }

        timer = 0f;

        // Fade Out
        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeOutDuration);

            float alpha =
                Mathf.Lerp(
                    maxAlpha,
                    0f,
                    t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);

        flashImage.gameObject.SetActive(false);

        flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (flashImage == null)
            return;

        Color color =
            currentFlashColor;

        color.a = alpha;

        flashImage.color = color;
    }
}