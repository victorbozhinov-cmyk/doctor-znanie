using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class StomachProgressBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text progressPercentText;

    [SerializeField] private RectTransform progressPanel;

    [Header("Systems")]
    [SerializeField] private StomachFoodStageController foodStageController;
    [SerializeField] private GastricJuiceTimingGame juiceGame;

    [Header("Juice Tasks Per Difficulty")]
    [SerializeField] private int easyJuiceTasks = 2;
    [SerializeField] private int mediumJuiceTasks = 2;
    [SerializeField] private int hardJuiceTasks = 3;

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Progress Sound")]
    [SerializeField] private AudioClip pointReceiveSfx;

    [Range(0f, 1f)]
    [SerializeField] private float pointReceiveVolume = 0.45f;

    [Tooltip(
        "Минималното време между два pop звука " +
        "докато процентът се увеличава."
    )]
    [Min(0.01f)]
    [SerializeField] private float pointReceiveInterval = 0.03f;

    // =========================================================
    // PROGRESS ANIMATION
    // =========================================================

    [Header("Progress Animation")]
    [SerializeField] private float enlargedScale = 1.05f;
    [SerializeField] private float scaleUpDuration = 0.15f;
    [SerializeField] private float fillDuration = 0.75f;
    [SerializeField] private float scaleDownDuration = 0.18f;

    private int completedProgressSteps;
    private int totalProgressSteps;

    private float currentVisualProgress;

    private Vector3 normalPanelScale;

    private Coroutine progressAnimation;

    private AudioSource sfxAudioSource;

    // =========================================================
    // PUBLIC INFO
    // =========================================================

    public bool IsAnimating =>
        progressAnimation != null;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (progressPanel == null)
        {
            progressPanel =
                transform as RectTransform;
        }

        if (progressPanel != null)
        {
            normalPanelScale =
                progressPanel.localScale;
        }

        sfxAudioSource =
            GetComponent<AudioSource>();

        if (sfxAudioSource != null)
        {
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            sfxAudioSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        if (foodStageController != null)
        {
            foodStageController.FoodStageAdvanced +=
                OnFoodStageAdvanced;
        }

        if (juiceGame != null)
        {
            juiceGame.JuiceSucceeded +=
                OnJuiceSucceeded;
        }
    }

    private void OnDisable()
    {
        if (foodStageController != null)
        {
            foodStageController.FoodStageAdvanced -=
                OnFoodStageAdvanced;
        }

        if (juiceGame != null)
        {
            juiceGame.JuiceSucceeded -=
                OnJuiceSucceeded;
        }
    }

    private void Start()
    {
        completedProgressSteps = 0;
        currentVisualProgress = 0f;

        SetVisualProgress(0f);

        StartCoroutine(
            InitializeNextFrame()
        );
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        CalculateTotalProgressSteps();
    }

    // =========================================================
    // TOTAL STEPS
    // =========================================================

    private void CalculateTotalProgressSteps()
    {
        if (foodStageController == null)
        {
            totalProgressSteps = 1;
            return;
        }

        int peristalsisStages =
            Mathf.Max(
                0,
                foodStageController.TotalStageCount - 1
            );

        int juiceTasks =
            GetJuiceTaskCount();

        totalProgressSteps =
            Mathf.Max(
                1,
                peristalsisStages + juiceTasks
            );
    }

    private int GetJuiceTaskCount()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                return easyJuiceTasks;

            case 2:
                return hardJuiceTasks;

            default:
                return mediumJuiceTasks;
        }
    }

    // =========================================================
    // SUCCESSFUL JUICES
    // =========================================================

    private void OnJuiceSucceeded()
    {
        AddProgressStep();
    }

    // =========================================================
    // COMPLETED PERISTALSIS STAGE
    // =========================================================

    private void OnFoodStageAdvanced(
        int newStageIndex)
    {
        AddProgressStep();
    }

    // =========================================================
    // ADD PROGRESS
    // =========================================================

    private void AddProgressStep()
    {
        if (totalProgressSteps <= 0)
        {
            CalculateTotalProgressSteps();
        }

        completedProgressSteps++;

        completedProgressSteps =
            Mathf.Clamp(
                completedProgressSteps,
                0,
                totalProgressSteps
            );

        float targetProgress =
            (float)completedProgressSteps /
            totalProgressSteps;

        if (progressAnimation != null)
        {
            StopCoroutine(
                progressAnimation
            );
        }

        progressAnimation =
            StartCoroutine(
                AnimateProgressGain(
                    targetProgress
                )
            );
    }

    // =========================================================
    // MAIN ANIMATION
    // =========================================================

    private IEnumerator AnimateProgressGain(
        float targetProgress)
    {
        // =====================================================
        // 1. ЛЕКО УГОЛЕМЯВАНЕ
        // =====================================================

        if (progressPanel != null)
        {
            yield return ScalePanelTo(
                normalPanelScale *
                enlargedScale,

                scaleUpDuration
            );
        }

        // =====================================================
        // 2. ПЛАВНО ПОКАЧВАНЕ НА ПРОГРЕСА
        // =====================================================

        float startProgress =
            currentVisualProgress;

        int previousPercentage =
            Mathf.RoundToInt(
                startProgress * 100f
            );

        float soundTimer =
            pointReceiveInterval;

        float elapsed = 0f;

        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / fillDuration
                );

            t =
                t * t *
                (3f - 2f * t);

            currentVisualProgress =
                Mathf.Lerp(
                    startProgress,
                    targetProgress,
                    t
                );

            int currentPercentage =
                Mathf.RoundToInt(
                    currentVisualProgress * 100f
                );

            // =================================================
            // POINT RECEIVE SOUND
            // =================================================
            //
            // Почти при всяка реална промяна
            // на процента пускаме кратък pop.
            //
            // Има и минимален интервал,
            // за да не станат прекалено много
            // звуци в един и същи момент.
            // =================================================

            soundTimer +=
                Time.deltaTime;

            if (
                currentPercentage >
                previousPercentage &&
                soundTimer >=
                pointReceiveInterval
            )
            {
                PlayPointReceiveSound();

                soundTimer = 0f;
            }

            previousPercentage =
                currentPercentage;

            SetVisualProgress(
                currentVisualProgress
            );

            yield return null;
        }

        currentVisualProgress =
            targetProgress;

        SetVisualProgress(
            currentVisualProgress
        );

        // =====================================================
        // 3. ВРЪЩАНЕ КЪМ НОРМАЛНИЯ РАЗМЕР
        // =====================================================

        if (progressPanel != null)
        {
            yield return ScalePanelTo(
                normalPanelScale,
                scaleDownDuration
            );
        }

        progressAnimation = null;
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayPointReceiveSound()
    {
        if (sfxAudioSource == null ||
            pointReceiveSfx == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(
            pointReceiveSfx,
            pointReceiveVolume
        );
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator ScalePanelTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 startScale =
            progressPanel.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            t =
                t * t *
                (3f - 2f * t);

            progressPanel.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        progressPanel.localScale =
            targetScale;
    }

    // =========================================================
    // VISUAL
    // =========================================================

    private void SetVisualProgress(
        float progress)
    {
        progress =
            Mathf.Clamp01(progress);

        if (progressFill != null)
        {
            progressFill.fillAmount =
                progress;
        }

        if (progressPercentText != null)
        {
            int percentage =
                Mathf.RoundToInt(
                    progress * 100f
                );

            progressPercentText.text =
                $"{percentage}%";
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetProgress()
    {
        completedProgressSteps = 0;
        currentVisualProgress = 0f;

        if (progressAnimation != null)
        {
            StopCoroutine(
                progressAnimation
            );

            progressAnimation = null;
        }

        if (progressPanel != null)
        {
            progressPanel.localScale =
                normalPanelScale;
        }

        SetVisualProgress(0f);
    }
}