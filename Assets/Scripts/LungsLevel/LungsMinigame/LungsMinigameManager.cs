using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LungsBreathingPhase
{
    Inhale,
    Exhale
}

[System.Serializable]
public class LungsDifficultySettings
{
    [Header("Rounds")]
    [Min(1)]
    public int totalRounds = 3;

    [Header("Timer")]
    [Min(1f)]
    public float phaseDuration = 20f;

    [Header("Progress")]
    [Range(0.1f, 100f)]
    public float correctAmount = 10f;

    [Range(0f, 100f)]
    public float wrongPenalty = 10f;

    [Header("Wrong Click")]
    [Min(0f)]
    public float wrongClickLockDuration = 0.3f;
}

public class LungsMinigameManager : MonoBehaviour
{
    public static LungsMinigameManager Instance { get; private set; }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    [Header("Difficulty Settings")]

    [SerializeField]
    private LungsDifficultySettings easySettings =
        new LungsDifficultySettings
        {
            totalRounds = 3,
            phaseDuration = 25f,
            correctAmount = 10f,
            wrongPenalty = 10f,
            wrongClickLockDuration = 0.25f
        };

    [SerializeField]
    private LungsDifficultySettings mediumSettings =
        new LungsDifficultySettings
        {
            totalRounds = 4,
            phaseDuration = 22f,
            correctAmount = 8f,
            wrongPenalty = 12f,
            wrongClickLockDuration = 0.35f
        };

    [SerializeField]
    private LungsDifficultySettings hardSettings =
        new LungsDifficultySettings
        {
            totalRounds = 5,
            phaseDuration = 20f,
            correctAmount = 5f,
            wrongPenalty = 15f,
            wrongClickLockDuration = 0.5f
        };

    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Minigame References")]

    [SerializeField]
    private LungsBubbleSpawner bubbleSpawner;

    [SerializeField]
    private LungsBreathingAnimation breathingAnimation;

    [SerializeField]
    private LungsAirEffect airEffect;

    // =========================================================
    // PAUSE
    // =========================================================

    [Header("Pause")]

    [SerializeField]
    private GameObject pauseOverlay;

    [SerializeField]
    private GameObject settingsOverlay;

    [SerializeField]
    private GameObject infoOverlay;

    [SerializeField]
    private GameObject exitConfirmationOverlay;

    [SerializeField]
    private DifficultySelector difficultySelector;

    // =========================================================
    // END PANELS
    // =========================================================

    [Header("End Panels")]

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject successPanel;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Timer")]

    [SerializeField]
    private bool useTimer = true;

    // =========================================================
    // TRANSITION
    // =========================================================

    [Header("Phase Transition")]

    [Tooltip(
        "Колко време изчакваме цялата визуална анимация преди новата фаза."
    )]
    [Min(0f)]
    [SerializeField]
    private float phaseTransitionDelay = 1f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private LungsDifficultySettings currentSettings;

    private int currentRound = 1;

    private LungsBreathingPhase currentPhase;

    private float oxygenPercent;
    private float co2Percent;

    private float phaseTimeRemaining;
    private float wrongClickLockRemaining;

    private bool gameEnded;
    private bool isTransitioning;
    private bool isPaused;

    // =========================================================
    // PUBLIC
    // =========================================================

    public LungsBreathingPhase CurrentPhase =>
        currentPhase;

    public float OxygenPercent =>
        oxygenPercent;

    public float CO2Percent =>
        co2Percent;

    public float PhaseTimeRemaining =>
        phaseTimeRemaining;

    public int CurrentRound =>
        currentRound;

    public int TotalRounds =>
        currentSettings != null
            ? currentSettings.totalRounds
            : 0;

    public bool GameEnded =>
        gameEnded;

    public bool IsTransitioning =>
        isTransitioning;

    public bool IsPaused =>
        isPaused;

    public bool IsClickLocked =>
        gameEnded ||
        isPaused ||
        isTransitioning ||
        wrongClickLockRemaining > 0f;

    public LungsDifficultySettings CurrentSettings =>
        currentSettings;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadDifficultySettings();

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
    }

    private void Start()
    {
        isPaused = false;

        Time.timeScale = 1f;

        currentRound = 1;

        StartInhalePhase();
    }

    private void Update()
    {
        if (gameEnded)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        UpdateWrongClickLock();
        UpdateTimer();
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void OpenPause()
    {
        if (gameEnded)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        isPaused = true;

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(true);
            pauseOverlay.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    // =========================================================
    // CONTINUE
    // =========================================================

    public void ContinueGame()
    {
        isPaused = false;

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    // =========================================================
    // SETTINGS FROM PAUSE
    // =========================================================

    public void OpenSettingsFromPause()
    {
        if (!isPaused)
        {
            return;
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }

        Time.timeScale = 0f;
    }

    public void CloseSettingsToPause()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(true);
            pauseOverlay.transform.SetAsLastSibling();
        }

        isPaused = true;

        Time.timeScale = 0f;
    }

    // =========================================================
    // INFO FROM PAUSE
    // =========================================================

    public void OpenInfoFromPause()
    {
        if (!isPaused)
        {
            return;
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(true);
            infoOverlay.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    public void CloseInfoToPause()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(true);
            pauseOverlay.transform.SetAsLastSibling();
        }

        isPaused = true;

        Time.timeScale = 0f;
    }

    // =========================================================
    // EXIT CONFIRMATION FROM PAUSE
    // =========================================================

    public void OpenExitConfirmationFromPause()
    {
        if (!isPaused)
        {
            return;
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(true);
            exitConfirmationOverlay.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    // =========================================================
    // STAY
    // =========================================================

    public void StayInMinigame()
    {
        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(true);
            pauseOverlay.transform.SetAsLastSibling();
        }

        isPaused = true;

        Time.timeScale = 0f;
    }

    // =========================================================
    // LEAVE TO BODY MAP
    // =========================================================

    public void LeaveToBodyMap()
    {
        isPaused = false;

        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void LoadDifficultySettings()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                currentSettings =
                    easySettings;
                break;

            case 2:
                currentSettings =
                    hardSettings;
                break;

            default:
                currentSettings =
                    mediumSettings;
                break;
        }
    }

    // =========================================================
    // TIMER
    // =========================================================

    private void UpdateTimer()
    {
        if (!useTimer)
        {
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        phaseTimeRemaining -=
            Time.deltaTime;

        if (phaseTimeRemaining <= 0f)
        {
            phaseTimeRemaining = 0f;

            FailGame();
        }
    }

    private void ResetPhaseTimer()
    {
        phaseTimeRemaining =
            currentSettings.phaseDuration;
    }

    // =========================================================
    // WRONG CLICK
    // =========================================================

    private void UpdateWrongClickLock()
    {
        if (wrongClickLockRemaining <= 0f)
        {
            return;
        }

        wrongClickLockRemaining -=
            Time.deltaTime;

        if (wrongClickLockRemaining < 0f)
        {
            wrongClickLockRemaining = 0f;
        }
    }

    private void ApplyWrongClickLock()
    {
        wrongClickLockRemaining =
            currentSettings.wrongClickLockDuration;
    }

    // =========================================================
    // CLICK
    // =========================================================

    public void HandleBubbleClicked(
        LungGasType gasType
    )
    {
        if (IsClickLocked)
        {
            return;
        }

        if (
            currentPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            HandleInhaleClick(gasType);
        }
        else
        {
            HandleExhaleClick(gasType);
        }
    }

    // =========================================================
    // INHALE
    // =========================================================

    private void HandleInhaleClick(
        LungGasType gasType
    )
    {
        if (gasType == LungGasType.O2)
        {
            oxygenPercent +=
                currentSettings.correctAmount;

            oxygenPercent =
                Mathf.Clamp(
                    oxygenPercent,
                    0f,
                    100f
                );

            if (oxygenPercent >= 100f)
            {
                oxygenPercent = 100f;

                CompleteInhalePhase();
            }
        }
        else
        {
            oxygenPercent -=
                currentSettings.wrongPenalty;

            oxygenPercent =
                Mathf.Clamp(
                    oxygenPercent,
                    0f,
                    100f
                );

            ApplyWrongClickLock();
        }
    }

    private void StartInhalePhase()
    {
        currentPhase =
            LungsBreathingPhase.Inhale;

        oxygenPercent = 0f;
        co2Percent = 100f;

        ResetPhaseTimer();
    }

    private void CompleteInhalePhase()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(
            TransitionToExhale()
        );
    }

    private IEnumerator TransitionToExhale()
    {
        isTransitioning = true;

        if (bubbleSpawner != null)
        {
            bubbleSpawner.BeginPhaseTransition();
        }

        if (breathingAnimation != null)
        {
            breathingAnimation.PlayInhaleComplete();
        }

        if (airEffect != null)
        {
            airEffect.PlayInhale();
        }

        yield return new WaitForSecondsRealtime(
            phaseTransitionDelay
        );

        StartExhalePhase();

        isTransitioning = false;

        if (bubbleSpawner != null)
        {
            bubbleSpawner.StartNewPhase();
        }
    }

    // =========================================================
    // EXHALE
    // =========================================================

    private void HandleExhaleClick(
        LungGasType gasType
    )
    {
        if (gasType == LungGasType.CO2)
        {
            co2Percent -=
                currentSettings.correctAmount;

            co2Percent =
                Mathf.Clamp(
                    co2Percent,
                    0f,
                    100f
                );

            if (co2Percent <= 0f)
            {
                co2Percent = 0f;

                CompleteExhalePhase();
            }
        }
        else
        {
            co2Percent +=
                currentSettings.wrongPenalty;

            co2Percent =
                Mathf.Clamp(
                    co2Percent,
                    0f,
                    100f
                );

            ApplyWrongClickLock();
        }
    }

    private void StartExhalePhase()
    {
        currentPhase =
            LungsBreathingPhase.Exhale;

        co2Percent = 100f;

        ResetPhaseTimer();
    }

    private void CompleteExhalePhase()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(
            FinishRoundTransition()
        );
    }

    private IEnumerator FinishRoundTransition()
    {
        isTransitioning = true;

        if (bubbleSpawner != null)
        {
            bubbleSpawner.BeginPhaseTransition();
        }

        if (breathingAnimation != null)
        {
            breathingAnimation.PlayExhaleComplete();
        }

        if (airEffect != null)
        {
            airEffect.PlayExhale();
        }

        yield return new WaitForSecondsRealtime(
            phaseTransitionDelay
        );

        if (currentRound >= TotalRounds)
        {
            CompleteGame();

            isTransitioning = false;

            yield break;
        }

        currentRound++;

        StartInhalePhase();

        isTransitioning = false;

        if (bubbleSpawner != null)
        {
            bubbleSpawner.StartNewPhase();
        }
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    private void CompleteGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        isPaused = false;

        Time.timeScale = 1f;

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (bubbleSpawner != null)
        {
            bubbleSpawner.BeginPhaseTransition();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }

        Debug.Log(
            "LUNGS MINIGAME COMPLETE!"
        );
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    private void FailGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        isPaused = false;
        isTransitioning = false;

        Time.timeScale = 1f;

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        if (bubbleSpawner != null)
        {
            bubbleSpawner.BeginPhaseTransition();
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log(
            "LUNGS MINIGAME FAILED!"
        );
    }
}