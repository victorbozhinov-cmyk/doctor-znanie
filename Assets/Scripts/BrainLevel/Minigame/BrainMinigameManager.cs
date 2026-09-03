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
    }

    [Header("Difficulty Settings")]
    [SerializeField]
    private DifficultySettings easySettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 2,
            orderDuration = 28f,
            minSpawnDelay = 8f,
            maxSpawnDelay = 11f,
            brainProcessingTime = 2f,
            failedOrderTension = 20f,
            wrongBrainZoneTension = 5f
        };

    [SerializeField]
    private DifficultySettings mediumSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 3,
            orderDuration = 23f,
            minSpawnDelay = 6f,
            maxSpawnDelay = 9f,
            brainProcessingTime = 3f,
            failedOrderTension = 25f,
            wrongBrainZoneTension = 8f
        };

    [SerializeField]
    private DifficultySettings hardSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 4,
            orderDuration = 19f,
            minSpawnDelay = 4f,
            maxSpawnDelay = 7f,
            brainProcessingTime = 4f,
            failedOrderTension = 34f,
            wrongBrainZoneTension = 10f
        };

    [Header("Stations")]
    [SerializeField]
    private BrainOrganStation[] stations;

    [Header("Optional Round UI")]
    [SerializeField]
    private TMP_Text roundTimeText;

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
    private GameObject infoPanel;

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
    // EXIT CONFIRMATION
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

    private DifficultySettings currentSettings;

    private readonly List<BrainOrganStation>
        activeStations =
            new List<BrainOrganStation>();

    private float remainingRoundTime;
    private float nervousTension = 0f;

    private bool roundRunning = false;
    private bool isPaused = false;

    private Coroutine spawnCoroutine;

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

        HidePauseWorld();

        StartRound();
    }

    private void Update()
    {
        if (!roundRunning)
            return;

        if (isPaused)
            return;

        UpdateRoundTimer();
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
        roundRunning = true;
        isPaused = false;

        activeStations.Clear();

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
                SpawnOrdersRoutine());

        Debug.Log(
            "Brain minigame started. Difficulty: " +
            PlayerPrefs.GetInt(
                "Difficulty",
                1));
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void PauseGame()
    {
        if (!roundRunning)
            return;

        if (isPaused)
            return;

        isPaused = true;

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        ShowMainPausePanel();

        Time.timeScale = 0f;

        Debug.Log(
            "Brain minigame paused.");
    }

    public void ResumeGame()
    {
        if (!roundRunning)
            return;

        if (!isPaused)
            return;

        HidePauseWorld();

        isPaused = false;
        Time.timeScale = 1f;

        Debug.Log(
            "Brain minigame resumed.");
    }

    // =========================================================
    // INFO
    // =========================================================

    public void OpenInfoPanel()
    {
        if (!isPaused)
            return;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public void CloseInfoPanel()
    {
        if (!isPaused)
            return;

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettingsPanel()
    {
        if (!isPaused)
            return;

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

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
        }
    }

    public void CloseSettingsPanel()
    {
        if (!isPaused)
            return;

        if (settingsPopupAnimation != null)
        {
            settingsPopupAnimation.PlayClose(
                FinishClosingSettings);

            return;
        }

        FinishClosingSettings();
    }

    private void FinishClosingSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (pauseWorld != null)
        {
            pauseWorld.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused)
            return;

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
        }
    }

    public void CloseExitConfirmation()
    {
        if (!isPaused)
            return;

        if (exitPopupAnimation != null)
        {
            exitPopupAnimation.PlayClose(
                FinishClosingExitConfirmation);

            return;
        }

        FinishClosingExitConfirmation();
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
    }

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // PAUSE PANEL HELPERS
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

            station.OrderCompleted +=
                HandleOrderCompleted;

            station.OrderFailed +=
                HandleOrderFailed;

            station.PrepareForManager();
        }
    }

    private void SetupBrainDropZones()
    {
        BrainMinigameDropZone[] dropZones =
            FindObjectsByType<BrainMinigameDropZone>(
                FindObjectsSortMode.None);

        foreach (BrainMinigameDropZone dropZone in
                 dropZones)
        {
            if (dropZone == null)
                continue;

            dropZone.SetProcessingDuration(
                currentSettings.brainProcessingTime);

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
                    currentSettings.maxSpawnDelay);

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

            // Първо избираме конкретния проблем
            // за тази станция.
            station.PrepareNextVariant();

            // След това проверяваме дали
            // неговата brain zone вече е заета.
            if (HasActiveOrderForSameBrainZone(
                    station))
            {
                continue;
            }

            available.Add(station);
        }

        if (available.Count == 0)
            return;

        BrainOrganStation selected =
            available[
                Random.Range(
                    0,
                    available.Count)];

        selected.StartManagedOrder(
            currentSettings.orderDuration);

        activeStations.Add(selected);

        Debug.Log(
            "New brain order: " +
            selected.gameObject.name);
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
    // ORDER EVENTS
    // =========================================================

    private void HandleOrderCompleted(
        BrainOrganStation station)
    {
        activeStations.Remove(station);

        Debug.Log(
            station.gameObject.name +
            " completed.");

        TryFillAvailableOrderSlots();
    }

    private void HandleOrderFailed(
        BrainOrganStation station)
    {
        activeStations.Remove(station);

        AddTension(
            currentSettings.failedOrderTension);

        Debug.Log(
            station.gameObject.name +
            " failed.");

        TryFillAvailableOrderSlots();
    }

    private void HandleWrongBrainZone()
    {
        AddTension(
            currentSettings.wrongBrainZoneTension);
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
                100f);

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
                nervousTension / 100f);
        }

        if (tensionText != null)
        {
            tensionText.text =
                Mathf.RoundToInt(
                    nervousTension) +
                "%";
        }

        UpdateTensionBrain();
    }

    private void UpdateTensionBrain()
    {
        if (tensionBrainRenderer == null)
            return;

        if (nervousTension >= 80f)
        {
            if (criticalBrainSprite != null)
            {
                tensionBrainRenderer.sprite =
                    criticalBrainSprite;
            }

            return;
        }

        if (nervousTension >= 50f)
        {
            if (stressedBrainSprite != null)
            {
                tensionBrainRenderer.sprite =
                    stressedBrainSprite;
            }

            return;
        }

        if (calmBrainSprite != null)
        {
            tensionBrainRenderer.sprite =
                calmBrainSprite;
        }
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

        UpdateRoundTimeUI();
    }

    private void UpdateRoundTimeUI()
    {
        if (roundTimeText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(
                remainingRoundTime);

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
            "Brain minigame survived successfully.");
    }

    private void LoseRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;
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
            "Nervous tension reached 100%.");
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
                1);

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
            foreach (BrainOrganStation station in
                     stations)
            {
                if (station == null)
                    continue;

                station.OrderCompleted -=
                    HandleOrderCompleted;

                station.OrderFailed -=
                    HandleOrderFailed;
            }
        }
    }
}