using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(30000)]
[RequireComponent(typeof(AudioSource))]
public class BodyMapRewardUIController : MonoBehaviour
{
    // =========================================================
    // VITAMINS
    // =========================================================

    [Header("Vitamins")]
    [SerializeField] private RectTransform vitaminsPanel;
    [SerializeField] private TMP_Text vitaminsText;

    // =========================================================
    // ANTIBODIES
    // =========================================================

    [Header("Antibodies")]
    [SerializeField] private RectTransform antibodiesPanel;
    [SerializeField] private TMP_Text antibodiesText;

    // =========================================================
    // SERUM
    // =========================================================

    [Header("Serum")]
    [SerializeField] private RectTransform serumPanel;
    [SerializeField] private TMP_Text serumPercentText;
    [SerializeField] private Image serumFillImage;

    // =========================================================
    // PATIENT CURED
    // =========================================================

    [Header("Patient Cured")]
    [SerializeField] private GameObject patientCuredOverlay;
    [SerializeField] private RectTransform patientCuredPanel;
    [SerializeField] private CanvasGroup patientCuredCanvasGroup;
    [SerializeField] private Button continueButton;

    // =========================================================
    // PATIENT CURED CLOSE ANIMATION
    // =========================================================

    [Header("Patient Cured Close Animation")]
    [SerializeField] private float patientCloseDuration = 0.35f;

    [SerializeField] private float patientCloseEndScale = 0.75f;

    [SerializeField] private float patientCloseOvershootScale = 1.03f;

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Reward Sounds")]
    [SerializeField] private AudioClip pointReceiveSfx;
    [SerializeField] private AudioClip serumChargeSfx;

    [Header("Reward Sound Volumes")]

    [Range(0f, 1f)]
    [SerializeField] private float pointReceiveVolume = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float serumChargeVolume = 0.95f;

    [Header("Point Receive Repetition")]

    [Tooltip(
        "Колко често се пуска Point Receive звукът " +
        "докато числото се увеличава."
    )]
    [Min(0.02f)]
    [SerializeField] private float pointReceiveInterval = 0.055f;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Animation")]
    [SerializeField] private float zoomScale = 1.12f;
    [SerializeField] private float zoomDuration = 0.22f;
    [SerializeField] private float valueChangeDuration = 0.8f;
    [SerializeField] private float holdDuration = 0.25f;
    [SerializeField] private float returnDuration = 0.2f;
    [SerializeField] private float delayBetweenRewards = 0.15f;
    [SerializeField] private float startDelay = 0.3f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool sequenceRunning;
    private bool patientPanelClosing;

    private int displayedVitamins = int.MinValue;
    private int displayedAntibodies = int.MinValue;
    private int displayedSerum = int.MinValue;

    private float displayedSerumFill = -1f;

    private AudioSource sfxAudioSource;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // =====================================================
        // PATIENT CURED
        // =====================================================

        if (
            patientCuredCanvasGroup == null &&
            patientCuredPanel != null
        )
        {
            patientCuredCanvasGroup =
                patientCuredPanel.GetComponent<CanvasGroup>();
        }

        ResetPatientCuredVisuals();

        if (patientCuredOverlay != null)
        {
            patientCuredOverlay.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                ClosePatientCuredPanel
            );
        }

        // =====================================================
        // AUDIO
        // =====================================================

        sfxAudioSource =
            GetComponent<AudioSource>();

        if (sfxAudioSource != null)
        {
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            sfxAudioSource.spatialBlend = 0f;
        }

        // =====================================================
        // REWARD SEQUENCE
        // =====================================================

        sequenceRunning =
            BodyMapRewardAnimationData.HasPendingSequence;

        if (BodyMapRewardAnimationData.HasPendingSequence)
        {
            SetVitaminsInstant(
                BodyMapRewardAnimationData.VitaminsBefore
            );

            SetAntibodiesInstant(
                BodyMapRewardAnimationData.AntibodiesBefore
            );

            SetSerumInstant(
                BodyMapRewardAnimationData.SerumBefore
            );

            Canvas.ForceUpdateCanvases();
        }
    }

    private IEnumerator Start()
    {
        if (BodyMapRewardAnimationData.HasPendingSequence)
        {
            yield return null;

            SetVitaminsInstant(
                BodyMapRewardAnimationData.VitaminsBefore
            );

            SetAntibodiesInstant(
                BodyMapRewardAnimationData.AntibodiesBefore
            );

            SetSerumInstant(
                BodyMapRewardAnimationData.SerumBefore
            );

            Canvas.ForceUpdateCanvases();

            yield return StartCoroutine(
                PlayRewardSequence()
            );
        }
        else
        {
            sequenceRunning = false;

            RefreshAllInstant();
        }
    }

    private void Update()
    {
        if (!sequenceRunning)
        {
            RefreshAllInstant();
        }
    }

    private void LateUpdate()
    {
        if (sequenceRunning)
        {
            ForceDisplayedValuesToUI();
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ClosePatientCuredPanel
            );
        }
    }

    // =========================================================
    // REWARD SEQUENCE
    // =========================================================

    private IEnumerator PlayRewardSequence()
    {
        sequenceRunning = true;

        int vitaminsBefore =
            BodyMapRewardAnimationData.VitaminsBefore;

        int vitaminsAfter =
            BodyMapRewardAnimationData.VitaminsAfter;

        int antibodiesBefore =
            BodyMapRewardAnimationData.AntibodiesBefore;

        int antibodiesAfter =
            BodyMapRewardAnimationData.AntibodiesAfter;

        int serumBefore =
            BodyMapRewardAnimationData.SerumBefore;

        int serumAfter =
            BodyMapRewardAnimationData.SerumAfter;

        bool completedSerumNow =
            serumBefore < 100 &&
            serumAfter >= 100;

        // =====================================================
        // START
        // =====================================================

        SetVitaminsInstant(vitaminsBefore);
        SetAntibodiesInstant(antibodiesBefore);
        SetSerumInstant(serumBefore);

        yield return new WaitForSeconds(
            startDelay
        );

        // =====================================================
        // 1. VITAMINS
        // =====================================================

        if (vitaminsBefore != vitaminsAfter)
        {
            yield return StartCoroutine(
                AnimateNumberReward(
                    vitaminsPanel,
                    vitaminsText,
                    vitaminsBefore,
                    vitaminsAfter
                )
            );

            yield return new WaitForSeconds(
                delayBetweenRewards
            );
        }

        // =====================================================
        // 2. ANTIBODIES
        // =====================================================

        if (antibodiesBefore != antibodiesAfter)
        {
            yield return StartCoroutine(
                AnimateNumberReward(
                    antibodiesPanel,
                    antibodiesText,
                    antibodiesBefore,
                    antibodiesAfter
                )
            );

            yield return new WaitForSeconds(
                delayBetweenRewards
            );
        }

        // =====================================================
        // 3. SERUM
        // =====================================================

        if (serumBefore != serumAfter)
        {
            yield return StartCoroutine(
                AnimateSerumReward(
                    serumBefore,
                    serumAfter
                )
            );
        }

        // =====================================================
        // FINISH
        // =====================================================

        BodyMapRewardAnimationData.Clear();

        sequenceRunning = false;

        RefreshAllInstant();

        if (completedSerumNow)
        {
            TryShowPatientCuredPanel();
        }
    }

    // =========================================================
    // PATIENT CURED
    // =========================================================

    private void TryShowPatientCuredPanel()
    {
        if (SerumManager.Instance == null)
        {
            return;
        }

        if (
            SerumManager.Instance
                .HasPatientCuredPanelBeenShown()
        )
        {
            return;
        }

        if (patientCuredOverlay == null)
        {
            Debug.LogWarning(
                "[BodyMapRewardUIController] " +
                "Patient Cured Overlay липсва."
            );

            return;
        }

        ResetPatientCuredVisuals();

        patientCuredOverlay.SetActive(true);

        SerumManager.Instance
            .MarkPatientCuredPanelAsShown();

        Debug.Log(
            "[BodyMapRewardUIController] " +
            "Patient Cured panel shown."
        );
    }

    // =========================================================
    // CLOSE PATIENT CURED
    // =========================================================

    public void ClosePatientCuredPanel()
    {
        if (patientPanelClosing)
        {
            return;
        }

        if (
            patientCuredOverlay == null ||
            !patientCuredOverlay.activeSelf
        )
        {
            return;
        }

        StartCoroutine(
            AnimatePatientCuredClose()
        );
    }

    private IEnumerator AnimatePatientCuredClose()
    {
        patientPanelClosing = true;

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        if (patientCuredCanvasGroup != null)
        {
            patientCuredCanvasGroup.interactable = false;
            patientCuredCanvasGroup.blocksRaycasts = false;
            patientCuredCanvasGroup.alpha = 1f;
        }

        if (patientCuredPanel == null)
        {
            if (patientCuredOverlay != null)
            {
                patientCuredOverlay.SetActive(false);
            }

            patientPanelClosing = false;

            yield break;
        }

        Vector3 startScale =
            Vector3.one;

        Vector3 overshootScale =
            Vector3.one *
            patientCloseOvershootScale;

        Vector3 endScale =
            Vector3.one *
            patientCloseEndScale;

        patientCuredPanel.localScale =
            startScale;

        // Защитаваме се от 0 duration.
        if (patientCloseDuration <= 0f)
        {
            if (patientCuredCanvasGroup != null)
            {
                patientCuredCanvasGroup.alpha = 0f;
            }

            patientCuredPanel.localScale =
                endScale;

            FinishPatientCuredClose();

            yield break;
        }

        float timer = 0f;

        while (timer < patientCloseDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / patientCloseDuration
                );

            // =================================================
            // FADE
            // =================================================

            if (patientCuredCanvasGroup != null)
            {
                patientCuredCanvasGroup.alpha =
                    1f - progress;
            }

            // =================================================
            // SCALE
            //
            // 1.00 -> 1.03 -> 0.75
            // =================================================

            if (progress < 0.2f)
            {
                float firstPart =
                    progress / 0.2f;

                patientCuredPanel.localScale =
                    Vector3.Lerp(
                        startScale,
                        overshootScale,
                        firstPart
                    );
            }
            else
            {
                float secondPart =
                    (progress - 0.2f) / 0.8f;

                patientCuredPanel.localScale =
                    Vector3.Lerp(
                        overshootScale,
                        endScale,
                        secondPart
                    );
            }

            yield return null;
        }

        if (patientCuredCanvasGroup != null)
        {
            patientCuredCanvasGroup.alpha = 0f;
        }

        patientCuredPanel.localScale =
            endScale;

        FinishPatientCuredClose();
    }

    private void FinishPatientCuredClose()
    {
        if (patientCuredOverlay != null)
        {
            patientCuredOverlay.SetActive(false);
        }

        // Връщаме визуалните стойности,
        // за да няма проблем при тестове в Editor.
        ResetPatientCuredVisuals();

        patientPanelClosing = false;
    }

    private void ResetPatientCuredVisuals()
    {
        if (patientCuredPanel != null)
        {
            patientCuredPanel.localScale =
                Vector3.one;
        }

        if (patientCuredCanvasGroup != null)
        {
            patientCuredCanvasGroup.alpha = 1f;

            patientCuredCanvasGroup.interactable =
                true;

            patientCuredCanvasGroup.blocksRaycasts =
                true;
        }

        if (continueButton != null)
        {
            continueButton.interactable =
                true;
        }
    }

    // =========================================================
    // NUMBER REWARD
    // =========================================================

    private IEnumerator AnimateNumberReward(
        RectTransform panel,
        TMP_Text valueText,
        int fromValue,
        int toValue
    )
    {
        if (panel == null || valueText == null)
        {
            yield break;
        }

        Vector3 normalScale =
            panel.localScale;

        Vector3 enlargedScale =
            normalScale * zoomScale;

        SetAnimatedNumber(
            valueText,
            fromValue
        );

        // =====================================================
        // 1. SCALE UP
        // =====================================================

        yield return StartCoroutine(
            AnimateScale(
                panel,
                normalScale,
                enlargedScale,
                zoomDuration
            )
        );

        // =====================================================
        // 2. VALUE
        // =====================================================

        float timer = 0f;

        float pointSoundTimer =
            pointReceiveInterval;

        int previousDisplayedValue =
            fromValue;

        while (timer < valueChangeDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / valueChangeDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            int currentValue =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        fromValue,
                        toValue,
                        smoothT
                    )
                );

            SetAnimatedNumber(
                valueText,
                currentValue
            );

            if (toValue > fromValue)
            {
                pointSoundTimer +=
                    Time.deltaTime;

                bool numberActuallyChanged =
                    currentValue !=
                    previousDisplayedValue;

                if (
                    numberActuallyChanged &&
                    pointSoundTimer >=
                    pointReceiveInterval
                )
                {
                    PlayPointReceiveSound();

                    pointSoundTimer = 0f;
                }
            }

            previousDisplayedValue =
                currentValue;

            yield return null;
        }

        SetAnimatedNumber(
            valueText,
            toValue
        );

        // =====================================================
        // 3. HOLD
        // =====================================================

        yield return new WaitForSeconds(
            holdDuration
        );

        // =====================================================
        // 4. RETURN
        // =====================================================

        yield return StartCoroutine(
            AnimateScale(
                panel,
                enlargedScale,
                normalScale,
                returnDuration
            )
        );

        panel.localScale =
            normalScale;
    }

    // =========================================================
    // SET ANIMATED NUMBER
    // =========================================================

    private void SetAnimatedNumber(
        TMP_Text targetText,
        int value
    )
    {
        if (targetText == vitaminsText)
        {
            displayedVitamins = value;
        }

        if (targetText == antibodiesText)
        {
            displayedAntibodies = value;
        }

        if (targetText != null)
        {
            targetText.text =
                value.ToString();
        }
    }

    // =========================================================
    // SERUM REWARD
    // =========================================================

    private IEnumerator AnimateSerumReward(
        int fromPercent,
        int toPercent
    )
    {
        if (serumPanel == null)
        {
            yield break;
        }

        Vector3 normalScale =
            serumPanel.localScale;

        Vector3 enlargedScale =
            normalScale * zoomScale;

        SetSerumInstant(
            fromPercent
        );

        // =====================================================
        // 1. SCALE UP
        // =====================================================

        yield return StartCoroutine(
            AnimateScale(
                serumPanel,
                normalScale,
                enlargedScale,
                zoomDuration
            )
        );

        // =====================================================
        // 2. FILL
        // =====================================================

        if (toPercent > fromPercent)
        {
            PlaySerumChargeSound();
        }

        float fromFill =
            Mathf.Clamp01(
                fromPercent / 100f
            );

        float toFill =
            Mathf.Clamp01(
                toPercent / 100f
            );

        float timer = 0f;

        while (timer < valueChangeDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / valueChangeDuration
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

            int currentPercent =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        fromPercent,
                        toPercent,
                        smoothT
                    )
                );

            displayedSerum =
                currentPercent;

            displayedSerumFill =
                currentFill;

            if (serumFillImage != null)
            {
                serumFillImage.fillAmount =
                    currentFill;
            }

            if (serumPercentText != null)
            {
                serumPercentText.text =
                    currentPercent + "%";
            }

            yield return null;
        }

        displayedSerum =
            toPercent;

        displayedSerumFill =
            toFill;

        if (serumFillImage != null)
        {
            serumFillImage.fillAmount =
                toFill;
        }

        if (serumPercentText != null)
        {
            serumPercentText.text =
                toPercent + "%";
        }

        // =====================================================
        // 3. HOLD
        // =====================================================

        yield return new WaitForSeconds(
            holdDuration
        );

        // =====================================================
        // 4. RETURN
        // =====================================================

        yield return StartCoroutine(
            AnimateScale(
                serumPanel,
                enlargedScale,
                normalScale,
                returnDuration
            )
        );

        serumPanel.localScale =
            normalScale;
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayPointReceiveSound()
    {
        if (
            sfxAudioSource == null ||
            pointReceiveSfx == null
        )
        {
            return;
        }

        sfxAudioSource.PlayOneShot(
            pointReceiveSfx,
            pointReceiveVolume
        );
    }

    private void PlaySerumChargeSound()
    {
        if (
            sfxAudioSource == null ||
            serumChargeSfx == null
        )
        {
            return;
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .DuckMusicForFeedback(
                    serumChargeSfx.length
                );
        }

        sfxAudioSource.PlayOneShot(
            serumChargeSfx,
            serumChargeVolume
        );
    }

    // =========================================================
    // SCALE
    // =========================================================

    private IEnumerator AnimateScale(
        RectTransform target,
        Vector3 fromScale,
        Vector3 toScale,
        float duration
    )
    {
        if (target == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            target.localScale =
                toScale;

            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            target.localScale =
                Vector3.Lerp(
                    fromScale,
                    toScale,
                    smoothT
                );

            yield return null;
        }

        target.localScale =
            toScale;
    }

    // =========================================================
    // FORCE VISUAL VALUES
    // =========================================================

    private void ForceDisplayedValuesToUI()
    {
        if (
            vitaminsText != null &&
            displayedVitamins != int.MinValue
        )
        {
            vitaminsText.text =
                displayedVitamins.ToString();
        }

        if (
            antibodiesText != null &&
            displayedAntibodies != int.MinValue
        )
        {
            antibodiesText.text =
                displayedAntibodies.ToString();
        }

        if (
            serumPercentText != null &&
            displayedSerum != int.MinValue
        )
        {
            serumPercentText.text =
                displayedSerum + "%";
        }

        if (
            serumFillImage != null &&
            displayedSerumFill >= 0f
        )
        {
            serumFillImage.fillAmount =
                displayedSerumFill;
        }
    }

    // =========================================================
    // INSTANT REFRESH
    // =========================================================

    private void RefreshAllInstant()
    {
        if (VitaminManager.Instance != null)
        {
            int vitamins =
                VitaminManager.Instance.CurrentVitamins;

            if (vitamins != displayedVitamins)
            {
                SetVitaminsInstant(
                    vitamins
                );
            }
        }

        if (AntibodyManager.Instance != null)
        {
            int antibodies =
                AntibodyManager.Instance.TotalBestScore;

            if (antibodies != displayedAntibodies)
            {
                SetAntibodiesInstant(
                    antibodies
                );
            }
        }

        if (SerumManager.Instance != null)
        {
            int serum =
                SerumManager.Instance.GetSerumPercent();

            if (serum != displayedSerum)
            {
                SetSerumInstant(
                    serum
                );
            }
        }
    }

    // =========================================================
    // SET VITAMINS
    // =========================================================

    private void SetVitaminsInstant(int value)
    {
        displayedVitamins =
            value;

        if (vitaminsText != null)
        {
            vitaminsText.text =
                value.ToString();
        }
    }

    // =========================================================
    // SET ANTIBODIES
    // =========================================================

    private void SetAntibodiesInstant(int value)
    {
        displayedAntibodies =
            value;

        if (antibodiesText != null)
        {
            antibodiesText.text =
                value.ToString();
        }
    }

    // =========================================================
    // SET SERUM
    // =========================================================

    private void SetSerumInstant(int percent)
    {
        displayedSerum =
            percent;

        displayedSerumFill =
            Mathf.Clamp01(
                percent / 100f
            );

        if (serumFillImage != null)
        {
            serumFillImage.fillAmount =
                displayedSerumFill;
        }

        if (serumPercentText != null)
        {
            serumPercentText.text =
                percent + "%";
        }
    }
}


// =============================================================
// REWARD ANIMATION DATA
// =============================================================

public static class BodyMapRewardAnimationData
{
    public static bool HasPendingSequence
    {
        get;
        private set;
    }

    public static int VitaminsBefore
    {
        get;
        private set;
    }

    public static int VitaminsAfter
    {
        get;
        private set;
    }

    public static int AntibodiesBefore
    {
        get;
        private set;
    }

    public static int AntibodiesAfter
    {
        get;
        private set;
    }

    public static int SerumBefore
    {
        get;
        private set;
    }

    public static int SerumAfter
    {
        get;
        private set;
    }

    // =========================================================
    // BEFORE
    // =========================================================

    public static void CaptureBeforeRewards()
    {
        VitaminsBefore =
            VitaminManager.Instance != null
                ? VitaminManager.Instance.CurrentVitamins
                : 0;

        AntibodiesBefore =
            AntibodyManager.Instance != null
                ? AntibodyManager.Instance.TotalBestScore
                : 0;

        SerumBefore =
            SerumManager.Instance != null
                ? SerumManager.Instance.GetSerumPercent()
                : 0;

        HasPendingSequence =
            false;
    }

    // =========================================================
    // AFTER
    // =========================================================

    public static void CaptureAfterRewards()
    {
        VitaminsAfter =
            VitaminManager.Instance != null
                ? VitaminManager.Instance.CurrentVitamins
                : VitaminsBefore;

        AntibodiesAfter =
            AntibodyManager.Instance != null
                ? AntibodyManager.Instance.TotalBestScore
                : AntibodiesBefore;

        SerumAfter =
            SerumManager.Instance != null
                ? SerumManager.Instance.GetSerumPercent()
                : SerumBefore;

        HasPendingSequence =
            VitaminsBefore != VitaminsAfter ||
            AntibodiesBefore != AntibodiesAfter ||
            SerumBefore != SerumAfter;

        Debug.Log(
            "[Reward Sequence] " +
            $"Vitamins: {VitaminsBefore} -> {VitaminsAfter} | " +
            $"Antibodies: {AntibodiesBefore} -> {AntibodiesAfter} | " +
            $"Serum: {SerumBefore}% -> {SerumAfter}%"
        );
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public static void Clear()
    {
        HasPendingSequence =
            false;
    }
}