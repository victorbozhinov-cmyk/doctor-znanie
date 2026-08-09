using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Panels")]
    [SerializeField] private GameObject minigamePanel;

    [Header("UI")]
    [SerializeField] private TMP_Text pulseValueText;

    [Header("ECG")]
    [SerializeField] private ECGLineGraphic ecgLine;

    [Header("Heart States")]
    [SerializeField] private GameObject heartNormal;
    [SerializeField] private GameObject heartLow;
    [SerializeField] private GameObject heartHigh;
    [SerializeField] private GameObject heartDead;

    [Header("Lives")]
    [SerializeField] private HeartMinigameLives heartLives;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Warning Panels")]
    [SerializeField] private GameObject highPulseWarningPanel;
    [SerializeField] private GameObject lowPulseWarningPanel;

    [SerializeField] private Image highWarningFill;
    [SerializeField] private Image lowWarningFill;

    [Header("Danger Countdown By Difficulty")]
    [SerializeField] private float easyDangerDuration = 4f;
    [SerializeField] private float mediumDangerDuration = 3f;
    [SerializeField] private float hardDangerDuration = 2f;

    [Header("Drift By Difficulty")]
    [SerializeField] private int easyDriftAmount = 7;
    [SerializeField] private int mediumDriftAmount = 10;
    [SerializeField] private int hardDriftAmount = 15;

    [SerializeField] private float easyDriftInterval = 4f;
    [SerializeField] private float mediumDriftInterval = 3f;
    [SerializeField] private float hardDriftInterval = 2f;

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

    private int driftAmount;
    private float driftInterval;
    private float driftTimer;

    // Пази ни от загуба на няколко живота веднага,
    // докато стоим в critical зоната.
    private bool criticalImmediateLossUsed;

    // Спира gameplay логиката след Game Over.
    private bool isGameOver;

    private void Start()
    {
        isGameOver = false;

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);

        SetDifficultyValues();

        if (highPulseWarningPanel != null)
            highPulseWarningPanel.SetActive(false);

        if (lowPulseWarningPanel != null)
            lowPulseWarningPanel.SetActive(false);

        if (highWarningFill != null)
            highWarningFill.fillAmount = 0f;

        if (lowWarningFill != null)
            lowWarningFill.fillAmount = 0f;

        driftTimer = 0f;

        UpdatePulseUI();
        UpdatePulseState();
    }

    private void Update()
    {
        if (isGameOver)
            return;

        // Gameplay логиката работи само
        // когато реално сме в MinigamePanel.
        if (minigamePanel == null ||
            !minigamePanel.activeInHierarchy)
        {
            driftTimer = 0f;
            return;
        }

        HandleKeyboardInput();
        UpdateDangerCountdown();
        UpdatePulseDrift();
    }

    private void SetDifficultyValues()
    {
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                dangerDuration = easyDangerDuration;

                driftAmount = easyDriftAmount;
                driftInterval = easyDriftInterval;
                break;

            case 2:
                dangerDuration = hardDangerDuration;

                driftAmount = hardDriftAmount;
                driftInterval = hardDriftInterval;
                break;

            default:
                dangerDuration = mediumDangerDuration;

                driftAmount = mediumDriftAmount;
                driftInterval = mediumDriftInterval;
                break;
        }

        Debug.Log(
            "Difficulty: " +
            difficulty +
            " | Danger countdown: " +
            dangerDuration +
            " sec." +
            " | Drift: " +
            driftAmount +
            " BPM every " +
            driftInterval +
            " sec."
        );
    }

    private void UpdatePulseDrift()
    {
        driftTimer += Time.deltaTime;

        if (driftTimer < driftInterval)
            return;

        driftTimer = 0f;

        // 50/50 шанс за нагоре или надолу.
        int direction =
            Random.value < 0.5f
                ? -1
                : 1;

        currentBPM += driftAmount * direction;

        UpdatePulseUI();
        UpdatePulseState();

        Debug.Log(
            "Pulse drift: " +
            (direction > 0 ? "+" : "-") +
            driftAmount +
            " BPM | Current BPM: " +
            currentBPM
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
        bool needsCountdown =
            currentPulseState == PulseState.DangerLow ||
            currentPulseState == PulseState.DangerHigh ||
            (
                currentPulseState == PulseState.CriticalLow &&
                criticalImmediateLossUsed
            ) ||
            (
                currentPulseState == PulseState.CriticalHigh &&
                criticalImmediateLossUsed
            );

        if (!needsCountdown)
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

        bool isLow =
            currentPulseState == PulseState.DangerLow ||
            currentPulseState == PulseState.CriticalLow;

        bool isHigh =
            currentPulseState == PulseState.DangerHigh ||
            currentPulseState == PulseState.CriticalHigh;

        // LOW WARNING
        if (isLow)
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

        // HIGH WARNING
        if (isHigh)
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

        if (progress >= 1f)
        {
            LoseLife();

            ResetDangerCountdown();
        }
    }

    private void LoseLife()
    {
        if (heartLives == null)
        {
            Debug.LogWarning(
                "HeartMinigameLives reference is missing!"
            );

            return;
        }

        if (!heartLives.HasLives || isGameOver)
            return;

        heartLives.LoseLife(
            () =>
            {
                if (!heartLives.HasLives)
                {
                    TriggerGameOver();
                }
            }
        );

        Debug.Log(
            "Life lost. Remaining: " +
            heartLives.CurrentLives
        );
    }

    private void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        ResetDangerCountdown();

        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(true);

        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(true);

        Debug.Log("GAME OVER!");
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
            pulseValueText.text =
                currentBPM.ToString();
        }

        if (ecgLine != null)
        {
            ecgLine.SetBPM(currentBPM);
        }
    }

    private void UpdatePulseState()
    {
        PulseState previousState =
            currentPulseState;

        // NORMAL: 70 - 100
        if (currentBPM >= 70 &&
            currentBPM <= 100)
        {
            currentPulseState =
                PulseState.Normal;
        }

        // DANGER LOW: 40 - 69
        else if (currentBPM >= 40 &&
                 currentBPM < 70)
        {
            currentPulseState =
                PulseState.DangerLow;
        }

        // DANGER HIGH: 101 - 130
        else if (currentBPM > 100 &&
                 currentBPM <= 130)
        {
            currentPulseState =
                PulseState.DangerHigh;
        }

        // CRITICAL LOW: под 40
        else if (currentBPM < 40)
        {
            currentPulseState =
                PulseState.CriticalLow;
        }

        // CRITICAL HIGH: над 130
        else
        {
            currentPulseState =
                PulseState.CriticalHigh;
        }

        HandleCriticalState(previousState);

        UpdateHeartVisual();
    }

    private void HandleCriticalState(
        PulseState previousState
    )
    {
        bool isCritical =
            currentPulseState == PulseState.CriticalLow ||
            currentPulseState == PulseState.CriticalHigh;

        bool wasCritical =
            previousState == PulseState.CriticalLow ||
            previousState == PulseState.CriticalHigh;

        if (!isCritical)
        {
            criticalImmediateLossUsed = false;
            return;
        }

        if (!wasCritical &&
            !criticalImmediateLossUsed)
        {
            criticalImmediateLossUsed = true;

            ResetDangerCountdown();

            LoseLife();
        }
    }

    private void UpdateHeartVisual()
    {
        if (isGameOver)
            return;

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

                if (heartLow != null)
                    heartLow.SetActive(true);

                break;

            case PulseState.DangerHigh:

                if (heartHigh != null)
                    heartHigh.SetActive(true);

                break;

            case PulseState.CriticalLow:
            case PulseState.CriticalHigh:

                if (heartDead != null)
                    heartDead.SetActive(true);

                break;
        }
    }

    public void DecreasePulse()
    {
        if (isGameOver)
            return;

        currentBPM -= 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    public void IncreasePulse()
    {
        if (isGameOver)
            return;

        currentBPM += 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}