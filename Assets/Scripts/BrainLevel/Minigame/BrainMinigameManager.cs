using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private Image tensionFill;

    [SerializeField]
    private TMP_Text tensionText;

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
    private Coroutine spawnCoroutine;

    public float NervousTension =>
        nervousTension;

    public float RemainingRoundTime =>
        remainingRoundTime;

    private void Start()
    {
        StartRound();
    }

    private void Update()
    {
        if (!roundRunning)
            return;

        UpdateRoundTimer();
    }

    private void StartRound()
    {
        currentSettings =
            GetCurrentDifficultySettings();

        remainingRoundTime =
            currentSettings.roundDuration;

        nervousTension = 0f;
        roundRunning = true;

        activeStations.Clear();

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

    private void SetupStations()
    {
        if (stations == null)
            return;

        foreach (BrainOrganStation station in
                 stations)
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

    private IEnumerator SpawnOrdersRoutine()
    {
        while (roundRunning)
        {
            float delay =
                Random.Range(
                    currentSettings.minSpawnDelay,
                    currentSettings.maxSpawnDelay);

            yield return new WaitForSeconds(
                delay);

            if (!roundRunning)
                yield break;

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

        if (activeStations.Count >=
            currentSettings.maxActiveOrders)
        {
            return;
        }

        List<BrainOrganStation> available =
            new List<BrainOrganStation>();

        if (stations == null)
            return;

        foreach (BrainOrganStation station in
                 stations)
        {
            if (station == null)
                continue;

            if (!station.CanStartOrder)
                continue;

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

        if (activeStations.Count == 0)
        {
            TryStartRandomOrder();
        }
    }

    private void AddTension(float amount)
    {
        if (!roundRunning)
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
            $"{minutes}:{seconds:00}";
    }

    private void UpdateTensionUI()
    {
        if (tensionFill != null)
        {
            tensionFill.fillAmount =
                nervousTension / 100f;
        }

        if (tensionText != null)
        {
            tensionText.text =
                Mathf.RoundToInt(
                    nervousTension) +
                "%";
        }
    }

    private void WinRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine);
        }

        StopAllOrders();

        if (successObject != null)
        {
            successObject.SetActive(true);
        }

        Debug.Log(
            "Brain minigame survived successfully.");
    }

    private void LoseRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine);
        }

        StopAllOrders();

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

        foreach (BrainOrganStation station in
                 stations)
        {
            if (station == null)
                continue;

            station.CancelOrder();
        }

        activeStations.Clear();
    }

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

    private void OnDestroy()
    {
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
