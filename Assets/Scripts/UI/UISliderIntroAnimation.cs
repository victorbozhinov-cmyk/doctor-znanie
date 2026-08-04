using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderIntroAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)]
    private float duration = 0.6f;

    [SerializeField, Min(0f)]
    private float delay = 0.15f;

    [Tooltip("Анимацията се пуска при всяко отваряне на панела.")]
    [SerializeField]
    private bool playEveryTimeEnabled = true;

    private Slider slider;
    private Coroutine animationCoroutine;

    private float targetValue;
    private bool hasTargetValue;
    private bool startHasRun;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        startHasRun = true;
        StartAnimation();
    }

    private void OnEnable()
    {
        // OnEnable се извиква преди Start при първото активиране.
        // Затова тук пускаме анимацията само при следващи отваряния.
        if (startHasRun && playEveryTimeEnabled)
        {
            StartAnimation();
        }
    }

    public void PlayAnimation()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        if (slider == null || !isActiveAndEnabled)
        {
            return;
        }

        StopCurrentAnimation(true);
        animationCoroutine = StartCoroutine(AnimateSlider());
    }

    private IEnumerator AnimateSlider()
    {
        /*
         * Изчакваме един кадър, за да могат:
         * MasterVolumeController и BrightnessSliderController
         * първо да заредят запазените стойности.
         */
        yield return null;

        targetValue = slider.value;
        hasTargetValue = true;

        slider.SetValueWithoutNotify(slider.minValue);

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        float elapsedTime = 0f;
        float safeDuration = Mathf.Max(duration, 0.01f);

        while (elapsedTime < safeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / safeDuration
            );

            // Плавно забавяне към края на анимацията.
            float easedProgress =
                1f - Mathf.Pow(1f - progress, 3f);

            float animatedValue = Mathf.Lerp(
                slider.minValue,
                targetValue,
                easedProgress
            );

            slider.SetValueWithoutNotify(animatedValue);

            yield return null;
        }

        slider.SetValueWithoutNotify(targetValue);

        animationCoroutine = null;
        hasTargetValue = false;
    }

    private void OnDisable()
    {
        /*
         * Ако панелът бъде затворен по средата на анимацията,
         * възстановяваме реалната стойност.
         */
        StopCurrentAnimation(true);
    }

    private void OnDestroy()
    {
        StopCurrentAnimation(false);
    }

    private void StopCurrentAnimation(bool restoreTargetValue)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (
            restoreTargetValue &&
            slider != null &&
            hasTargetValue
        )
        {
            slider.SetValueWithoutNotify(targetValue);
        }

        hasTargetValue = false;
    }
}