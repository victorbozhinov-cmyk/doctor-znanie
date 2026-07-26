using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderIntroAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float delay = 0.15f;

    private Slider slider;
    private float targetValue;
    private float timer;
    private bool isAnimating;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        PlayAnimation();
    }

    private void Update()
    {
        if (!isAnimating)
            return;

        timer += Time.unscaledDeltaTime;

        if (timer < delay)
            return;

        float progress = Mathf.Clamp01(
            (timer - delay) / duration
        );

        slider.SetValueWithoutNotify(
            Mathf.Lerp(slider.minValue, targetValue, progress)
        );

        if (progress >= 1f)
        {
            slider.SetValueWithoutNotify(targetValue);
            isAnimating = false;
        }
    }

    public void PlayAnimation()
    {
        targetValue = slider.value;
        timer = 0f;
        isAnimating = true;

        slider.SetValueWithoutNotify(slider.minValue);
    }
}