using UnityEngine;

public enum LungsBreathingPhase
{
    Inhale,
    Exhale
}

public class LungsMinigameManager : MonoBehaviour
{
    public static LungsMinigameManager Instance { get; private set; }

    [Header("Round Settings")]
    [SerializeField] private int totalRounds = 3;

    [Header("Timer - Disabled For Now")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float phaseDuration = 10f;

    [Header("Progress Settings")]
    [SerializeField] private float correctAmount = 20f;
    [SerializeField] private float wrongPenalty = 10f;

    private int currentRound = 1;

    private LungsBreathingPhase currentPhase;

    private float oxygenPercent;
    private float co2Percent;

    private float phaseTimeRemaining;

    private bool gameEnded;

    public LungsBreathingPhase CurrentPhase => currentPhase;

    public float OxygenPercent => oxygenPercent;

    public float CO2Percent => co2Percent;

    public float PhaseTimeRemaining => phaseTimeRemaining;

    public int CurrentRound => currentRound;

    public int TotalRounds => totalRounds;

    public bool GameEnded => gameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartInhalePhase();
    }

    private void Update()
    {
        if (gameEnded)
        {
            return;
        }

        // Засега таймерът е изключен.
        if (!useTimer)
        {
            return;
        }

        phaseTimeRemaining -= Time.deltaTime;

        if (phaseTimeRemaining <= 0f)
        {
            phaseTimeRemaining = 0f;

            FailGame();
        }
    }

    // =========================================================
    // BUBBLE CLICK
    // =========================================================

    public void HandleBubbleClicked(LungGasType gasType)
    {
        if (gameEnded)
        {
            return;
        }

        if (currentPhase == LungsBreathingPhase.Inhale)
        {
            HandleInhaleClick(gasType);
        }
        else
        {
            HandleExhaleClick(gasType);
        }

        PrintCurrentStatus();
    }

    // =========================================================
    // INHALE
    // =========================================================

    private void HandleInhaleClick(LungGasType gasType)
    {
        if (gasType == LungGasType.O2)
        {
            oxygenPercent += correctAmount;

            oxygenPercent = Mathf.Clamp(
                oxygenPercent,
                0f,
                100f
            );

            if (oxygenPercent >= 100f)
            {
                CompleteInhalePhase();
            }
        }
        else
        {
            oxygenPercent -= wrongPenalty;

            oxygenPercent = Mathf.Clamp(
                oxygenPercent,
                0f,
                100f
            );
        }
    }

    private void StartInhalePhase()
    {
        currentPhase = LungsBreathingPhase.Inhale;

        oxygenPercent = 0f;
        co2Percent = 100f;

        ResetPhaseTimer();

        Debug.Log(
            "START INHALE | Round " +
            currentRound +
            "/" +
            totalRounds
        );
    }

    private void CompleteInhalePhase()
    {
        Debug.Log("INHALE COMPLETE");

        StartExhalePhase();
    }

    // =========================================================
    // EXHALE
    // =========================================================

    private void HandleExhaleClick(LungGasType gasType)
    {
        if (gasType == LungGasType.CO2)
        {
            co2Percent -= correctAmount;

            co2Percent = Mathf.Clamp(
                co2Percent,
                0f,
                100f
            );

            if (co2Percent <= 0f)
            {
                CompleteExhalePhase();
            }
        }
        else
        {
            co2Percent += wrongPenalty;

            co2Percent = Mathf.Clamp(
                co2Percent,
                0f,
                100f
            );
        }
    }

    private void StartExhalePhase()
    {
        currentPhase = LungsBreathingPhase.Exhale;

        co2Percent = 100f;

        ResetPhaseTimer();

        Debug.Log(
            "START EXHALE | Round " +
            currentRound +
            "/" +
            totalRounds
        );
    }

    private void CompleteExhalePhase()
    {
        Debug.Log("EXHALE COMPLETE");

        if (currentRound >= totalRounds)
        {
            CompleteGame();
            return;
        }

        currentRound++;

        StartInhalePhase();
    }

    // =========================================================
    // TIMER
    // =========================================================

    private void ResetPhaseTimer()
    {
        phaseTimeRemaining = phaseDuration;
    }

    // =========================================================
    // GAME END
    // =========================================================

    private void CompleteGame()
    {
        gameEnded = true;

        Debug.Log("LUNGS MINIGAME COMPLETE!");
    }

    private void FailGame()
    {
        gameEnded = true;

        Debug.Log("LUNGS MINIGAME FAILED!");
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void PrintCurrentStatus()
    {
        Debug.Log(
            "Phase: " +
            currentPhase +
            " | O2: " +
            oxygenPercent +
            "% | CO2: " +
            co2Percent +
            "% | Round: " +
            currentRound +
            "/" +
            totalRounds
        );
    }
}