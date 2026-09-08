using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainMinigameManager : MonoBehaviour
{
    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Round")]
        [Min(1f)]
        public float roundDuration = 120f;

        [Header("Orders")]
        [Min(1)]
        public int maxActiveOrders = 2;

        [Min(1f)]
        public float orderDuration = 28f;

        [Header("Order Variety")]
        [Min(0)]
        public int recentOrderBlockCount = 3;

        [Header("New Order Delay")]
        [Min(0f)]
        public float minSpawnDelay = 8f;

        [Min(0f)]
        public float maxSpawnDelay = 11f;

        [Header("Brain Processing")]
        [Min(0f)]
        public float brainProcessingTime = 2f;

        [Header("Nervous Tension")]
        [Range(0f, 100f)]
        public float failedOrderTension = 20f;

        [Range(0f, 100f)]
        public float wrongBrainZoneTension = 5f;

        [Min(0f)]
        public float passiveTensionPerActiveOrderPerSecond = 0f;
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    [Header("Difficulty Settings")]
    [SerializeField]
    private DifficultySettings easySettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 2,
            orderDuration = 28f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 8f,
            maxSpawnDelay = 11f,
            brainProcessingTime = 2f,
            failedOrderTension = 20f,
            wrongBrainZoneTension = 5f,
            passiveTensionPerActiveOrderPerSecond = 0f
        };

    [SerializeField]
    private DifficultySettings mediumSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 3,
            orderDuration = 23f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 6f,
            maxSpawnDelay = 9f,
            brainProcessingTime = 3f,
            failedOrderTension = 25f,
            wrongBrainZoneTension = 8f,
            passiveTensionPerActiveOrderPerSecond = 0.15f
        };

    [SerializeField]
    private DifficultySettings hardSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 4,
            orderDuration = 19f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 4f,
            maxSpawnDelay = 7f,
            brainProcessingTime = 4f,
            failedOrderTension = 34f,
            wrongBrainZoneTension = 10f,
            passiveTensionPerActiveOrderPerSecond = 0.5f
        };

    // =========================================================
    // STATIONS
    // =========================================================

    [Header("Stations")]
    [SerializeField]
    private BrainOrganStation[] stations;

    // =========================================================
    // ROUND UI
    // =========================================================

    [Header("Optional Round UI")]
    [SerializeField]
    private TMP_Text roundTimeText;

    [Header("Round Timer Pulse")]
    [SerializeField]
    private BrainRoundTimerPulse roundTimerPulse;

    [SerializeField]
    private float roundTimerPulseStartTime = 10f;

    // =========================================================
    // TENSION
    // =========================================================

    [Header("Optional Tension UI")]
    [SerializeField]
    private StressFillMaskController tensionFillMask;

    [SerializeField]
    private TMP_Text tensionText;

    [Header("Tension Brain")]
    [SerializeField]
    private SpriteRenderer tensionBrainRenderer;

    [SerializeField]
    private Sprite calmBrainSprite;

    [SerializeField]
    private Sprite stressedBrainSprite;

    [SerializeField]
    private Sprite criticalBrainSprite;

    [Header("Tension Brain Animation")]
    [SerializeField]
    private BrainTensionStateAnimation tensionStateAnimation;

    // =========================================================
    // WELCOME
    // =========================================================

    [Header("Welcome")]
    [SerializeField]
    private GameObject welcomeOverlay;

    [SerializeField]
    private UIPopupAnimation welcomePanelAnimation;

    // =========================================================
    // PAUSE
    // =========================================================

    [Header("Pause")]
    [SerializeField]
    private GameObject pauseWorld;

    [Header("Pause Panels")]
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private WorldPopupAnimation pausePopupAnimation;

    [SerializeField]
    private GameObject infoPanel;

    [SerializeField]
    private WorldPopupAnimation infoPopupAnimation;

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    [SerializeField]
    private GameObject settingsOverlay;

    [SerializeField]
    private PauseUIPopupAnimation settingsPopupAnimation;

    [SerializeField]
    private DifficultySelector difficultySelector;

    // =========================================================
    // EXIT
    // =========================================================

    [Header("Exit Confirmation")]
    [SerializeField]
    private GameObject exitConfirmationPanel;

    [SerializeField]
    private PauseUIPopupAnimation exitPopupAnimation;

    // =========================================================
    // END OBJECTS
    // =========================================================

    [Header("Optional End Objects")]
    [SerializeField]
    private GameObject successObject;

    [SerializeField]
    private GameObject gameOverObject;

    // =========================================================
    // INTERNAL
    // =========================================================

    private DifficultySettings currentSettings;

    private readonly List<BrainOrganStation>
        activeStations =
            new List<BrainOrganStation>();

    private readonly List<BrainOrganStation>
        recentStations =
            new List<BrainOrganStation>();

    private float remainingRoundTime;
    private float nervousTension = 0f;

    private bool roundRunning = false;
    private bool isPaused = false;

    private bool minigameStarted = false;
    private bool welcomeTransitionInProgress = false;
    private bool menuTransitionInProgress = false;
    private bool infoOpenedFromWelcome = false;

    private Coroutine spawnCoroutine;

    private int currentTensionBrainState = 0;
    private int lastRoundTimerPulseSecond = -1;

    public float NervousTension =>
        nervousTension;

    public float RemainingRoundTime =>
        remainingRoundTime;

    public bool IsPaused =>
        isPaused;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        Time.timeScale = 1f;

        PrepareForWelcome();
    }

    private void Update()
    {
        if (!minigameStarted)
            return;

        if (!roundRunning)
            return;

        if (isPaused)
            return;

        UpdatePassiveTension();

        if (!roundRunning)
            return;

        UpdateRoundTimer();
    }

    // =========================================================
    // WELCOME PREPARATION
    // =========================================================

    public void PrepareForWelcome()
    {
        Time.timeScale = 1f;

        minigameStarted = false;
        roundRunning = false;
        isPaused = false;

        welcomeTransitionInProgress = false;
        menuTransitionInProgress = false;
        infoOpenedFromWelcome = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopAllOrders();

        currentSettings =
            GetCurrentDifficultySettings();

        remainingRoundTime =
            currentSettings.roundDuration;

        nervousTension = 0f;

        currentTensionBrainState = 0;
        lastRoundTimerPulseSecond = -1;

        activeStations.Clear();
        recentStations.Clear();

        if (roundTimerPulse != null)
        {
            roundTimerPulse.ResetAnimation();
        }

        HidePauseWorld();

        if (successObject != null)
        {
            successObject.SetActive(false);
        }

        if (gameOverObject != null)
        {
            gameOverObject.SetActive(false);
        }

        UpdateRoundTimeUI();
        UpdateTensionUI();

        ShowWelcome();

        Debug.Log(
            "Brain Minigame чака Welcome панела."
        );
    }

    private void ShowWelcome()
    {
        if (welcomeOverlay == null)
        {
            Debug.LogError(
                "Minigame Welcome Overlay не е свързан " +
                "в BrainMinigameManager."
            );

            return;
        }

        welcomeOverlay.SetActive(true);
        welcomeOverlay.transform.SetAsLastSibling();

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayOpen();
        }
    }

    public void ShowWelcomeAfterInfo()
    {
        if (minigameStarted)
        {
            return;
        }

        welcomeTransitionInProgress = false;
        menuTransitionInProgress = false;

        ShowWelcome();
    }

    // =========================================================
    // START FROM WELCOME
    // =========================================================

    public void StartMinigameFromWelcome()
    {
        if (minigameStarted ||
            welcomeTransitionInProgress)
        {
            return;
        }

        welcomeTransitionInProgress = true;

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                FinishStartingFromWelcome
            );
        }
        else
        {
            FinishStartingFromWelcome();
        }
    }

    private void FinishStartingFromWelcome()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.SetActive(false);
        }

        welcomeTransitionInProgress = false;

        minigameStarted = true;

        StartRound();
    }

    // =========================================================
    // INFO FROM WELCOME
    // =========================================================

    public void OpenInfoFromWelcome()
    {
        if (minigameStarted ||
            welcomeTransitionInProgress ||
            menuTransitionInProgress)
        {
            return;
        }

        infoOpenedFromWelcome = true;
        welcomeTransitionInProgress = true;

        if (welcomePanelAnimation != null)
        {
            welcomePanelAnimation.PlayClose(
                FinishOpeningInfoFromWelcome
            );
        }
        else
        {
            FinishOpeningInfoFromWelcome();
        }
    }

    private void FinishOpeningInfoFromWelcome()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.SetActive(false);
        }

        welcomeTransitionInProgress = false;

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    // =========================================================
    // ROUND
    // =========================================================

    private void StartRound()
    {
        currentSettings =
            GetCurrentDifficultySettings();

        remainingRoundTime =
            currentSettings.roundDuration;

        nervousTension = 0f;
        currentTensionBrainState = 0;
        lastRoundTimerPulseSecond = -1;

        if (roundTimerPulse != null)
        {
            roundTimerPulse.ResetAnimation();
        }

        roundRunning = true;
        isPaused = false;
        minigameStarted = true;

        activeStations.Clear();
        recentStations.Clear();

        HidePauseWorld();

        if (successObject != null)
        {
            successObject.SetActive(false);
        }

        if (gameOverObject != null)
        {
            gameOverObject.SetActive(false);
        }

        SetupStations();
        SetupBrainDropZones();

        UpdateRoundTimeUI();
        UpdateTensionUI();

        TryStartRandomOrder();

        spawnCoroutine =
            StartCoroutine(
                SpawnOrdersRoutine()
            );

        Debug.Log(
            "Brain minigame started. Difficulty: " +
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            )
        );
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void PauseGame()
    {
        if (!minigameStarted ||
            !roundRunning ||
            isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        isPaused = true;

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        ShowMainPausePanel();

        Time.timeScale = 0f;

        Debug.Log(
            "Brain minigame paused."
        );
    }

    public void ResumeGame()
    {
        if (!roundRunning ||
            !isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
        {
            pausePopupAnimation.PlayClose(
                FinishResumingGame
            );
        }
        else
        {
            FinishResumingGame();
        }
    }

    private void FinishResumingGame()
    {
        HidePauseWorld();

        isPaused = false;
        menuTransitionInProgress = false;

        Time.timeScale = 1f;

        Debug.Log(
            "Brain minigame resumed."
        );
    }

    // =========================================================
    // INFO FROM PAUSE
    // =========================================================

    public void OpenInfoPanel()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
        {
            pausePopupAnimation.PlayClose(
                FinishOpeningInfoFromPause
            );
        }
        else
        {
            FinishOpeningInfoFromPause();
        }
    }

    private void FinishOpeningInfoFromPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        menuTransitionInProgress = false;
    }

    public void CloseInfoPanel()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (!isPaused &&
            !infoOpenedFromWelcome)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (infoPopupAnimation != null)
        {
            infoPopupAnimation.PlayClose(
                FinishClosingInfo
            );
        }
        else
        {
            FinishClosingInfo();
        }
    }

    private void FinishClosingInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (infoOpenedFromWelcome)
        {
            infoOpenedFromWelcome = false;

            if (pauseWorld != null)
            {
                pauseWorld.SetActive(false);
            }

            menuTransitionInProgress = false;

            ShowWelcomeAfterInfo();

            return;
        }

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        menuTransitionInProgress = false;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettingsPanel()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }

        if (pausePopupAnimation != null)
        {
            pausePopupAnimation.PlayClose(
                FinishOpeningSettings
            );
        }
        else
        {
            FinishOpeningSettings();
        }
    }

    private void FinishOpeningSettings()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }

        menuTransitionInProgress = false;
    }

    public void CloseSettingsPanel()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (settingsPopupAnimation != null)
        {
            settingsPopupAnimation.PlayClose(
                FinishClosingSettings
            );
        }
        else
        {
            FinishClosingSettings();
        }
    }

    private void FinishClosingSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        menuTransitionInProgress = false;
    }

    // =========================================================
    // EXIT
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
        {
            pausePopupAnimation.PlayClose(
                FinishOpeningExit
            );
        }
        else
        {
            FinishOpeningExit();
        }
    }

    private void FinishOpeningExit()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
            exitConfirmationPanel.transform.SetAsLastSibling();
        }

        menuTransitionInProgress = false;
    }

    public void CloseExitConfirmation()
    {
        if (!isPaused ||
            menuTransitionInProgress)
        {
            return;
        }

        menuTransitionInProgress = true;

        if (exitPopupAnimation != null)
        {
            exitPopupAnimation.PlayClose(
                FinishClosingExitConfirmation
            );
        }
        else
        {
            FinishClosingExitConfirmation();
        }
    }

    private void FinishClosingExitConfirmation()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        menuTransitionInProgress = false;
    }

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // PAUSE HELPERS
    // =========================================================

    private void ShowMainPausePanel()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    private void HidePauseWorld()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(false);
        }
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupStations()
    {
        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null)
                continue;

            station.OrderCompleted -=
                HandleOrderCompleted;

            station.OrderFailed -=
                HandleOrderFailed;

            station.WrongCommandDelivered -=
                HandleWrongCommandDelivered;

            station.OrderCompleted +=
                HandleOrderCompleted;

            station.OrderFailed +=
                HandleOrderFailed;

            station.WrongCommandDelivered +=
                HandleWrongCommandDelivered;

            station.PrepareForManager();
        }
    }

    private void SetupBrainDropZones()
    {
        BrainMinigameDropZone[] dropZones =
            FindObjectsByType<BrainMinigameDropZone>(
                FindObjectsSortMode.None
            );

        foreach (BrainMinigameDropZone dropZone
                 in dropZones)
        {
            if (dropZone == null)
                continue;

            dropZone.SetProcessingDuration(
                currentSettings.brainProcessingTime
            );

            dropZone.WrongProblemDelivered -=
                HandleWrongBrainZone;

            dropZone.WrongProblemDelivered +=
                HandleWrongBrainZone;
        }
    }

    // =========================================================
    // ORDER SPAWNING
    // =========================================================

    private IEnumerator SpawnOrdersRoutine()
    {
        while (roundRunning)
        {
            float delay =
                Random.Range(
                    currentSettings.minSpawnDelay,
                    currentSettings.maxSpawnDelay
                );

            yield return new WaitForSeconds(delay);

            if (!roundRunning)
                yield break;

            if (isPaused)
                continue;

            if (activeStations.Count <
                currentSettings.maxActiveOrders)
            {
                TryStartRandomOrder();
            }
        }
    }

    private void TryStartRandomOrder()
    {
        if (!roundRunning)
            return;

        if (isPaused)
            return;

        if (activeStations.Count >=
            currentSettings.maxActiveOrders)
        {
            return;
        }

        List<BrainOrganStation> available =
            new List<BrainOrganStation>();

        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null)
                continue;

            if (!station.CanStartOrder)
                continue;

            if (IsRecentlyUsed(station))
                continue;

            station.PrepareNextVariant();

            if (HasActiveOrderForSameBrainZone(
                    station))
            {
                continue;
            }

            available.Add(station);
        }

        if (available.Count == 0)
        {
            foreach (BrainOrganStation station in stations)
            {
                if (station == null)
                    continue;

                if (!station.CanStartOrder)
                    continue;

                station.PrepareNextVariant();

                if (HasActiveOrderForSameBrainZone(
                        station))
                {
                    continue;
                }

                available.Add(station);
            }
        }

        if (available.Count == 0)
            return;

        BrainOrganStation selected =
            available[
                Random.Range(
                    0,
                    available.Count
                )
            ];

        selected.StartManagedOrder(
            currentSettings.orderDuration
        );

        activeStations.Add(selected);

        RegisterRecentStation(selected);

        Debug.Log(
            "New brain order: " +
            selected.gameObject.name
        );
    }

    private bool HasActiveOrderForSameBrainZone(
        BrainOrganStation candidate)
    {
        if (candidate == null)
            return false;

        BrainMinigameDropZone candidateZone =
            candidate.BrainDropZone;

        if (candidateZone == null)
            return false;

        foreach (BrainOrganStation active in
                 activeStations)
        {
            if (active == null)
                continue;

            if (active.BrainDropZone ==
                candidateZone)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // ORDER VARIETY
    // =========================================================

    private bool IsRecentlyUsed(
        BrainOrganStation station)
    {
        if (station == null)
            return false;

        return recentStations.Contains(
            station
        );
    }

    private void RegisterRecentStation(
        BrainOrganStation station)
    {
        if (station == null)
            return;

        int maxRecent =
            Mathf.Max(
                0,
                currentSettings
                    .recentOrderBlockCount
            );

        if (maxRecent == 0)
        {
            recentStations.Clear();
            return;
        }

        recentStations.Add(station);

        while (recentStations.Count >
               maxRecent)
        {
            recentStations.RemoveAt(0);
        }
    }

    // =========================================================
    // ORDER EVENTS
    // =========================================================

    private void HandleOrderCompleted(
        BrainOrganStation station)
    {
        activeStations.Remove(station);

        Debug.Log(
            station.gameObject.name +
            " completed."
        );

        TryFillAvailableOrderSlots();
    }

    private void HandleOrderFailed(
        BrainOrganStation station)
    {
        activeStations.Remove(station);

        AddTension(
            currentSettings.failedOrderTension
        );

        Debug.Log(
            station.gameObject.name +
            " failed."
        );

        TryFillAvailableOrderSlots();
    }

    private void HandleWrongBrainZone()
    {
        AddTension(
            currentSettings.wrongBrainZoneTension
        );
    }

    private void HandleWrongCommandDelivered()
    {
        AddTension(
            currentSettings.wrongBrainZoneTension
        );
    }

    private void TryFillAvailableOrderSlots()
    {
        if (!roundRunning)
            return;

        if (isPaused)
            return;

        if (activeStations.Count == 0)
        {
            TryStartRandomOrder();
        }
    }

    // =========================================================
    // PASSIVE TENSION
    // =========================================================

    private void UpdatePassiveTension()
    {
        if (currentSettings == null)
            return;

        if (activeStations.Count <= 0)
            return;

        float rate =
            currentSettings
                .passiveTensionPerActiveOrderPerSecond;

        if (rate <= 0f)
            return;

        float tensionToAdd =
            rate *
            activeStations.Count *
            Time.deltaTime;

        AddTension(
            tensionToAdd
        );
    }

    // =========================================================
    // TENSION
    // =========================================================

    private void AddTension(float amount)
    {
        if (!roundRunning)
            return;

        if (isPaused)
            return;

        nervousTension += amount;

        nervousTension =
            Mathf.Clamp(
                nervousTension,
                0f,
                100f
            );

        UpdateTensionUI();

        if (nervousTension >= 100f)
        {
            LoseRound();
        }
    }

    private void UpdateTensionUI()
    {
        if (tensionFillMask != null)
        {
            tensionFillMask.SetFill(
                nervousTension / 100f
            );
        }

        if (tensionText != null)
        {
            tensionText.text =
                Mathf.RoundToInt(
                    nervousTension
                ) +
                "%";
        }

        UpdateTensionBrain();
    }

    private void UpdateTensionBrain()
    {
        if (tensionBrainRenderer == null)
            return;

        int newState;

        if (nervousTension >= 80f)
        {
            newState = 2;

            if (criticalBrainSprite != null)
            {
                tensionBrainRenderer.sprite =
                    criticalBrainSprite;
            }
        }
        else if (nervousTension >= 50f)
        {
            newState = 1;

            if (stressedBrainSprite != null)
            {
                tensionBrainRenderer.sprite =
                    stressedBrainSprite;
            }
        }
        else
        {
            newState = 0;

            if (calmBrainSprite != null)
            {
                tensionBrainRenderer.sprite =
                    calmBrainSprite;
            }
        }

        if (newState >
            currentTensionBrainState)
        {
            if (tensionStateAnimation != null)
            {
                tensionStateAnimation.Play();
            }
        }

        currentTensionBrainState =
            newState;
    }

    // =========================================================
    // ROUND TIMER
    // =========================================================

    private void UpdateRoundTimer()
    {
        remainingRoundTime -=
            Time.deltaTime;

        if (remainingRoundTime <= 0f)
        {
            remainingRoundTime = 0f;

            UpdateRoundTimeUI();

            WinRound();

            return;
        }

        UpdateRoundTimerPulse();
        UpdateRoundTimeUI();
    }

    private void UpdateRoundTimerPulse()
    {
        if (roundTimerPulse == null)
            return;

        if (remainingRoundTime >
            roundTimerPulseStartTime)
        {
            return;
        }

        int currentSecond =
            Mathf.CeilToInt(
                remainingRoundTime
            );

        if (currentSecond <= 0)
            return;

        if (currentSecond ==
            lastRoundTimerPulseSecond)
        {
            return;
        }

        lastRoundTimerPulseSecond =
            currentSecond;

        roundTimerPulse.Play();
    }

    private void UpdateRoundTimeUI()
    {
        if (roundTimeText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(
                remainingRoundTime
            );

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        roundTimeText.text =
            $"{minutes:00}:{seconds:00}";
    }

    // =========================================================
    // WIN / LOSE
    // =========================================================

    private void WinRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;
        minigameStarted = false;
        isPaused = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopAllOrders();
        HidePauseWorld();

        if (successObject != null)
        {
            successObject.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log(
            "Brain minigame survived successfully."
        );
    }

    private void LoseRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;
        minigameStarted = false;
        isPaused = false;

        Time.timeScale = 1f;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopAllOrders();
        HidePauseWorld();

        if (gameOverObject != null)
        {
            gameOverObject.SetActive(true);
        }

        Debug.Log(
            "Brain minigame lost. " +
            "Nervous tension reached 100%."
        );
    }

    private void StopAllOrders()
    {
        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null)
                continue;

            station.CancelOrder();
        }

        activeStations.Clear();
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private DifficultySettings
        GetCurrentDifficultySettings()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                return easySettings;

            case 2:
                return hardSettings;

            default:
                return mediumSettings;
        }
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (stations != null)
        {
            foreach (BrainOrganStation station in stations)
            {
                if (station == null)
                    continue;

                station.OrderCompleted -=
                    HandleOrderCompleted;

                station.OrderFailed -=
                    HandleOrderFailed;

                station.WrongCommandDelivered -=
                    HandleWrongCommandDelivered;
            }
        }
    }
}