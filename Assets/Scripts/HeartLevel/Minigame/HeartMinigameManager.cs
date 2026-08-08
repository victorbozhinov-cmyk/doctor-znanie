using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeartMinigameManager : MonoBehaviour
{
    public enum PulseState
    {
        Normal,
        DangerLow,
        DangerHigh,
        CriticalLow,
        CriticalHigh
    }

    [Header("Pulse")]
    [SerializeField] private int currentBPM = 80;

    [Header("UI")]
    [SerializeField] private TMP_Text pulseValueText;

    [Header("ECG")]
    [SerializeField] private ECGLineGraphic ecgLine;

    [Header("Heart States")]
    [SerializeField] private GameObject heartNormal;
    [SerializeField] private GameObject heartLow;
    [SerializeField] private GameObject heartHigh;
    [SerializeField] private GameObject heartDead;

    [Header("Warning Panels")]
    [SerializeField] private GameObject highPulseWarningPanel;
    [SerializeField] private GameObject lowPulseWarningPanel;

    [SerializeField] private Image highWarningFill;
    [SerializeField] private Image lowWarningFill;

    [Header("Danger Countdown By Difficulty")]
    [SerializeField] private float easyDangerDuration = 4f;
    [SerializeField] private float mediumDangerDuration = 3f;
    [SerializeField] private float hardDangerDuration = 2f;

    [Header("Keyboard Button Animation")]
    [SerializeField] private RectTransform decreaseButtonHitbox;
    [SerializeField] private RectTransform increaseButtonHitbox;

    [SerializeField] private float keyboardPressedScale = 0.96f;
    [SerializeField] private float keyboardPressDuration = 0.08f;

    private Coroutine decreaseAnimation;
    private Coroutine increaseAnimation;

    private PulseState currentPulseState;

    private float dangerDuration;
    private float dangerTimer;
    private bool dangerCountdownActive;

    private void Start()
    {
        SetDangerDurationFromDifficulty();

        if (highPulseWarningPanel != null)
            highPulseWarningPanel.SetActive(false);

        if (lowPulseWarningPanel != null)
            lowPulseWarningPanel.SetActive(false);

        if (highWarningFill != null)
            highWarningFill.fillAmount = 0f;

        if (lowWarningFill != null)
            lowWarningFill.fillAmount = 0f;

        UpdatePulseUI();
        UpdatePulseState();
    }

    private void Update()
    {
        HandleKeyboardInput();
        UpdateDangerCountdown();
    }

    private void SetDangerDurationFromDifficulty()
    {
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                dangerDuration = easyDangerDuration;
                break;

            case 2:
                dangerDuration = hardDangerDuration;
                break;

            default:
                dangerDuration = mediumDangerDuration;
                break;
        }

        Debug.Log(
            "Difficulty: " +
            difficulty +
            " | Danger countdown: " +
            dangerDuration +
            " sec."
        );
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        // ↓ или S = намалява пулса
        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            DecreasePulse();

            // Същият звук като при click с мишката
            UISoundManager.Instance?.PlayClick();

            if (decreaseButtonHitbox != null)
            {
                if (decreaseAnimation != null)
                    StopCoroutine(decreaseAnimation);

                decreaseAnimation = StartCoroutine(
                    PlayKeyboardPressAnimation(decreaseButtonHitbox)
                );
            }
        }

        // ↑ или W = увеличава пулса
        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            IncreasePulse();

            // Същият звук като при click с мишката
            UISoundManager.Instance?.PlayClick();

            if (increaseButtonHitbox != null)
            {
                if (increaseAnimation != null)
                    StopCoroutine(increaseAnimation);

                increaseAnimation = StartCoroutine(
                    PlayKeyboardPressAnimation(increaseButtonHitbox)
                );
            }
        }
    }

    private void UpdateDangerCountdown()
    {
        bool isDanger =
            currentPulseState == PulseState.DangerLow ||
            currentPulseState == PulseState.DangerHigh;

        if (!isDanger)
        {
            ResetDangerCountdown();
            return;
        }

        if (!dangerCountdownActive)
        {
            dangerCountdownActive = true;
            dangerTimer = 0f;
        }

        dangerTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            dangerTimer / dangerDuration
        );

        // LOW WARNING: 40 - 69 BPM
        if (currentPulseState == PulseState.DangerLow)
        {
            if (lowPulseWarningPanel != null)
                lowPulseWarningPanel.SetActive(true);

            if (highPulseWarningPanel != null)
                highPulseWarningPanel.SetActive(false);

            if (lowWarningFill != null)
                lowWarningFill.fillAmount = progress;

            if (highWarningFill != null)
                highWarningFill.fillAmount = 0f;
        }

        // HIGH WARNING: 101 - 130 BPM
        else if (currentPulseState == PulseState.DangerHigh)
        {
            if (highPulseWarningPanel != null)
                highPulseWarningPanel.SetActive(true);

            if (lowPulseWarningPanel != null)
                lowPulseWarningPanel.SetActive(false);

            if (highWarningFill != null)
                highWarningFill.fillAmount = progress;

            if (lowWarningFill != null)
                lowWarningFill.fillAmount = 0f;
        }

        // Засега тук само тестваме.
        // Следващата стъпка ще е LoseLife().
        if (progress >= 1f)
        {
            Debug.Log(
                "Warning bar completed - player should lose a life."
            );

            ResetDangerCountdown();
        }
    }

    private void ResetDangerCountdown()
    {
        dangerCountdownActive = false;
        dangerTimer = 0f;

        if (highPulseWarningPanel != null)
            highPulseWarningPanel.SetActive(false);

        if (lowPulseWarningPanel != null)
            lowPulseWarningPanel.SetActive(false);

        if (highWarningFill != null)
            highWarningFill.fillAmount = 0f;

        if (lowWarningFill != null)
            lowWarningFill.fillAmount = 0f;
    }

    private IEnumerator PlayKeyboardPressAnimation(
        RectTransform button
    )
    {
        Vector3 originalScale = Vector3.one;

        Vector3 pressedScale =
            originalScale * keyboardPressedScale;

        float timer = 0f;

        // Свиване
        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale = Vector3.Lerp(
                originalScale,
                pressedScale,
                timer / keyboardPressDuration
            );

            yield return null;
        }

        timer = 0f;

        // Връщане
        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale = Vector3.Lerp(
                pressedScale,
                originalScale,
                timer / keyboardPressDuration
            );

            yield return null;
        }

        button.localScale = originalScale;
    }

    private void UpdatePulseUI()
    {
        if (pulseValueText != null)
        {
            pulseValueText.text = currentBPM.ToString();
        }

        if (ecgLine != null)
        {
            ecgLine.SetBPM(currentBPM);
        }
    }

    private void UpdatePulseState()
    {
        // NORMAL: 70 - 100
        if (currentBPM >= 70 && currentBPM <= 100)
        {
            currentPulseState = PulseState.Normal;
        }

        // DANGER LOW: 40 - 69
        else if (currentBPM >= 40 && currentBPM < 70)
        {
            currentPulseState = PulseState.DangerLow;
        }

        // DANGER HIGH: 101 - 130
        else if (currentBPM > 100 && currentBPM <= 130)
        {
            currentPulseState = PulseState.DangerHigh;
        }

        // CRITICAL LOW: под 40
        else if (currentBPM < 40)
        {
            currentPulseState = PulseState.CriticalLow;
        }

        // CRITICAL HIGH: над 130
        else
        {
            currentPulseState = PulseState.CriticalHigh;
        }

        UpdateHeartVisual();
    }

    private void UpdateHeartVisual()
    {
        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(false);

        switch (currentPulseState)
        {
            case PulseState.Normal:

                if (heartNormal != null)
                    heartNormal.SetActive(true);

                break;

            case PulseState.DangerLow:
            case PulseState.CriticalLow:

                if (heartLow != null)
                    heartLow.SetActive(true);

                break;

            case PulseState.DangerHigh:
            case PulseState.CriticalHigh:

                if (heartHigh != null)
                    heartHigh.SetActive(true);

                break;
        }
    }

    public void DecreasePulse()
    {
        currentBPM -= 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    public void IncreasePulse()
    {
        currentBPM += 5;

        UpdatePulseUI();
        UpdatePulseState();
    }
}