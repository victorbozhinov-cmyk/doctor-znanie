using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SerumUIController : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private Image serumFillImage;

    [SerializeField] private TMP_Text percentText;

    [SerializeField] private RectTransform syringeRoot;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Fill Animation")]
    [SerializeField] private float fillDuration = 1.2f;

    [Header("Pulse Animation")]
    [SerializeField] private float pulseScale = 1.06f;

    [SerializeField] private float pulseDuration = 0.22f;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        RefreshSerum();
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void RefreshSerum()
    {
        if (SerumManager.Instance == null)
        {
            Debug.LogWarning(
                "SerumManager.Instance липсва."
            );

            SetSerumInstant(0);

            return;
        }

        int currentPercent =
            SerumManager.Instance.GetSerumPercent();

        bool shouldAnimate =
            SerumManager.Instance.TryConsumeFillAnimation(
                out int fromPercent,
                out int toPercent
            );

        if (shouldAnimate)
        {
            StartCoroutine(
                AnimateSerum(
                    fromPercent,
                    toPercent
                )
            );
        }
        else
        {
            SetSerumInstant(currentPercent);
        }
    }

    // =========================================================
    // INSTANT
    // =========================================================

    private void SetSerumInstant(int percent)
    {
        float normalized =
            Mathf.Clamp01(percent / 100f);

        if (serumFillImage != null)
        {
            serumFillImage.fillAmount = normalized;
        }

        UpdatePercentText(percent);
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator AnimateSerum(
        int fromPercent,
        int toPercent
    )
    {
        float fromFill =
            Mathf.Clamp01(fromPercent / 100f);

        float toFill =
            Mathf.Clamp01(toPercent / 100f);

        if (serumFillImage != null)
        {
            serumFillImage.fillAmount = fromFill;
        }

        UpdatePercentText(fromPercent);

        yield return new WaitForSeconds(0.25f);

        float timer = 0f;

        while (timer < fillDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fillDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            float currentFill =
                Mathf.Lerp(
                    fromFill,
                    toFill,
                    smoothT
                );

            if (serumFillImage != null)
            {
                serumFillImage.fillAmount =
                    currentFill;
            }

            int displayedPercent =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        fromPercent,
                        toPercent,
                        smoothT
                    )
                );

            UpdatePercentText(
                displayedPercent
            );

            yield return null;
        }

        if (serumFillImage != null)
        {
            serumFillImage.fillAmount = toFill;
        }

        UpdatePercentText(toPercent);

        yield return StartCoroutine(
            PulseSyringe()
        );
    }

    // =========================================================
    // PULSE
    // =========================================================

    private IEnumerator PulseSyringe()
    {
        if (syringeRoot == null)
        {
            yield break;
        }

        Vector3 startScale =
            syringeRoot.localScale;

        Vector3 targetScale =
            startScale * pulseScale;

        float timer = 0f;

        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / pulseDuration;

            syringeRoot.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        timer = 0f;

        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / pulseDuration;

            syringeRoot.localScale =
                Vector3.Lerp(
                    targetScale,
                    startScale,
                    t
                );

            yield return null;
        }

        syringeRoot.localScale =
            startScale;
    }

    // =========================================================
    // TEXT
    // =========================================================

    private void UpdatePercentText(int percent)
    {
        if (percentText != null)
        {
            percentText.text =
                percent + "%";
        }
    }
}
