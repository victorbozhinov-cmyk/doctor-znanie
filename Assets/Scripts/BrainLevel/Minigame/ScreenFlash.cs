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

    private void Awake()
    {
        if (flashImage == null)
        {
            flashImage = GetComponent<Image>();
        }

        SetAlpha(0f);
    }

    public void PlayRedFlash()
    {
        if (flashImage == null)
            return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Бърз Fade In
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / fadeInDuration;
            float alpha = Mathf.Lerp(0f, maxAlpha, t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(maxAlpha);

        // Кратко задържане
        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        // По-плавен Fade Out
        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / fadeOutDuration;
            float alpha = Mathf.Lerp(maxAlpha, 0f, t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);

        flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = flashImage.color;

        color.r = 1f;
        color.g = 0f;
        color.b = 0f;
        color.a = alpha;

        flashImage.color = color;
    }
}
