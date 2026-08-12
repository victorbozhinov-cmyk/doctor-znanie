using UnityEngine;

public class HeartBeatAnimation : MonoBehaviour
{
    [Header("Beat Settings")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float normalBeatScale = 1.06f;
    [SerializeField] private float highBeatScale = 1.09f;

    [Header("Beat Shape")]
    [SerializeField] private float beatUpPercent = 0.18f;
    [SerializeField] private float beatDownPercent = 0.32f;

    [Header("BPM")]
    [SerializeField] private float currentBPM = 80f;

    private Vector3 baseScale;
    private float beatTimer;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        beatTimer = 0f;
        transform.localScale = baseScale;
    }

    private void Update()
    {
        if (currentBPM <= 0f)
            return;

        float beatInterval = 60f / currentBPM;

        beatTimer += Time.deltaTime;

        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
        }

        float normalizedTime =
            beatTimer / beatInterval;

        float targetBeatScale =
            GetBeatScaleForBPM();

        float scaleMultiplier = 1f;

        // Първа част:
        // сърцето се уголемява бързо.
        if (normalizedTime < beatUpPercent)
        {
            float t =
                normalizedTime /
                beatUpPercent;

            scaleMultiplier =
                Mathf.Lerp(
                    1f,
                    targetBeatScale,
                    SmoothStep(t)
                );
        }

        // Втора част:
        // връща се обратно.
        else if (
            normalizedTime <
            beatUpPercent +
            beatDownPercent
        )
        {
            float t =
                (
                    normalizedTime -
                    beatUpPercent
                )
                /
                beatDownPercent;

            scaleMultiplier =
                Mathf.Lerp(
                    targetBeatScale,
                    1f,
                    SmoothStep(t)
                );
        }

        // Остатъкът от интервала
        // остава на нормален размер.
        else
        {
            scaleMultiplier = 1f;
        }

        transform.localScale =
            baseScale *
            scaleMultiplier;
    }

    private float GetBeatScaleForBPM()
    {
        // Нисък пулс:
        // по-меко биене.
        if (currentBPM < 70f)
        {
            return Mathf.Lerp(
                1.035f,
                normalBeatScale,
                Mathf.InverseLerp(
                    40f,
                    70f,
                    currentBPM
                )
            );
        }

        // Нормален пулс.
        if (currentBPM <= 100f)
        {
            return normalBeatScale;
        }

        // Висок пулс:
        // малко по-силно биене.
        return Mathf.Lerp(
            normalBeatScale,
            highBeatScale,
            Mathf.InverseLerp(
                100f,
                140f,
                currentBPM
            )
        );
    }

    private float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);

        return t * t * (3f - 2f * t);
    }

    public void SetBPM(float bpm)
    {
        currentBPM =
            Mathf.Max(1f, bpm);
    }

    public void ResetBeat()
    {
        beatTimer = 0f;
        transform.localScale = baseScale;
    }
}