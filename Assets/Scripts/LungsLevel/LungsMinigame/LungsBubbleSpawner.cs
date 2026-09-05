using System.Collections;
using UnityEngine;

[System.Serializable]
public class LungsSpawnerDifficultySettings
{
    [Header("Bubble Amount")]
    [Min(1)]
    public int initialBubbleCount = 10;

    [Min(1)]
    public int minimumBubblesOnScreen = 8;

    [Min(1)]
    public int maximumBubblesOnScreen = 14;

    [Header("Spawn Speed")]
    [Min(0.05f)]
    public float spawnInterval = 0.8f;

    [Header("Correct Bubble Chance")]
    [Range(0.05f, 0.95f)]
    public float correctBubbleChance = 0.5f;

    [Header("Final Phase Difficulty")]
    [Range(0.5f, 0.95f)]
    public float finalPhaseThreshold = 0.8f;

    [Range(0.2f, 1f)]
    public float finalPhaseSpawnIntervalMultiplier = 0.75f;

    [Range(0f, 0.5f)]
    public float finalPhaseCorrectChanceReduction = 0.1f;

    [Header("Bubble Lifetime")]
    [Min(0.5f)]
    public float bubbleLifetimeMin = 4f;

    [Min(0.5f)]
    public float bubbleLifetimeMax = 6f;

    [Header("Bubble Size")]
    [Range(0.5f, 1.5f)]
    public float bubbleScaleMin = 0.9f;

    [Range(0.5f, 1.5f)]
    public float bubbleScaleMax = 1.1f;
}

public class LungsBubbleSpawner : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Bubble Prefabs")]
    [SerializeField] private GameObject o2BubblePrefab;
    [SerializeField] private GameObject co2BubblePrefab;

    [Header("Left Lung Spawn Areas")]
    [SerializeField] private RectTransform[] leftSpawnAreas;

    [Header("Right Lung Spawn Areas")]
    [SerializeField] private RectTransform[] rightSpawnAreas;

    [Header("Bubble Container")]
    [SerializeField] private RectTransform bubblesContainer;

    // =========================================================
    // DIFFICULTY
    // =========================================================

    [Header("Easy Settings")]
    [SerializeField]
    private LungsSpawnerDifficultySettings easySettings =
        new LungsSpawnerDifficultySettings
        {
            initialBubbleCount = 8,
            minimumBubblesOnScreen = 7,
            maximumBubblesOnScreen = 12,
            spawnInterval = 0.9f,
            correctBubbleChance = 0.65f,
            finalPhaseThreshold = 0.8f,
            finalPhaseSpawnIntervalMultiplier = 0.85f,
            finalPhaseCorrectChanceReduction = 0.05f,
            bubbleLifetimeMin = 5f,
            bubbleLifetimeMax = 7f,
            bubbleScaleMin = 0.95f,
            bubbleScaleMax = 1.1f
        };

    [Header("Medium Settings")]
    [SerializeField]
    private LungsSpawnerDifficultySettings mediumSettings =
        new LungsSpawnerDifficultySettings
        {
            initialBubbleCount = 10,
            minimumBubblesOnScreen = 8,
            maximumBubblesOnScreen = 14,
            spawnInterval = 0.7f,
            correctBubbleChance = 0.5f,
            finalPhaseThreshold = 0.8f,
            finalPhaseSpawnIntervalMultiplier = 0.75f,
            finalPhaseCorrectChanceReduction = 0.1f,
            bubbleLifetimeMin = 4f,
            bubbleLifetimeMax = 6f,
            bubbleScaleMin = 0.9f,
            bubbleScaleMax = 1.1f
        };

    [Header("Hard Settings")]
    [SerializeField]
    private LungsSpawnerDifficultySettings hardSettings =
        new LungsSpawnerDifficultySettings
        {
            initialBubbleCount = 12,
            minimumBubblesOnScreen = 10,
            maximumBubblesOnScreen = 16,
            spawnInterval = 0.55f,
            correctBubbleChance = 0.38f,
            finalPhaseThreshold = 0.8f,
            finalPhaseSpawnIntervalMultiplier = 0.65f,
            finalPhaseCorrectChanceReduction = 0.08f,
            bubbleLifetimeMin = 3f,
            bubbleLifetimeMax = 5f,
            bubbleScaleMin = 0.85f,
            bubbleScaleMax = 1.1f
        };

    // =========================================================
    // RUNTIME
    // =========================================================

    private LungsSpawnerDifficultySettings currentSettings;

    private Coroutine spawnCoroutine;

    private bool initialized;
    private bool spawningEnabled = true;

    // =========================================================
    // UNITY
    // =========================================================

    private IEnumerator Start()
    {
        LoadDifficultySettings();

        yield return new WaitUntil(
            () => LungsMinigameManager.Instance != null
        );

        initialized = true;

        ClearAllBubblesImmediate();

        SpawnInitialBubbles();

        spawnCoroutine =
            StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        if (LungsMinigameManager.Instance.GameEnded)
        {
            return;
        }

        if (!spawningEnabled)
        {
            return;
        }

        KeepMinimumBubbleCount();
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void LoadDifficultySettings()
    {
        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                currentSettings = easySettings;
                break;

            case 2:
                currentSettings = hardSettings;
                break;

            default:
                currentSettings = mediumSettings;
                break;
        }
    }

    // =========================================================
    // PHASE TRANSITION
    // =========================================================

    public void BeginPhaseTransition()
    {
        spawningEnabled = false;

        ClearAllBubblesImmediate();
    }

    public void StartNewPhase()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return;
        }

        if (LungsMinigameManager.Instance.GameEnded)
        {
            return;
        }

        spawningEnabled = true;

        SpawnInitialBubbles();
    }

    // =========================================================
    // SPAWN LOOP
    // =========================================================

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (
                initialized &&
                spawningEnabled &&
                LungsMinigameManager.Instance != null &&
                !LungsMinigameManager.Instance.GameEnded &&
                !LungsMinigameManager.Instance.IsTransitioning
            )
            {
                if (
                    bubblesContainer != null &&
                    bubblesContainer.childCount <
                    currentSettings.maximumBubblesOnScreen
                )
                {
                    SpawnBubble();
                }
            }

            yield return new WaitForSeconds(
                GetCurrentSpawnInterval()
            );
        }
    }

    // =========================================================
    // INITIAL SPAWN
    // =========================================================

    private void SpawnInitialBubbles()
    {
        if (currentSettings == null)
        {
            return;
        }

        int amount =
            Mathf.Min(
                currentSettings.initialBubbleCount,
                currentSettings.maximumBubblesOnScreen
            );

        for (int i = 0; i < amount; i++)
        {
            SpawnBubble();
        }
    }

    // =========================================================
    // MINIMUM COUNT
    // =========================================================

    private void KeepMinimumBubbleCount()
    {
        if (bubblesContainer == null)
        {
            return;
        }

        int missing =
            currentSettings.minimumBubblesOnScreen -
            bubblesContainer.childCount;

        if (missing <= 0)
        {
            return;
        }

        int availableSpace =
            currentSettings.maximumBubblesOnScreen -
            bubblesContainer.childCount;

        int amount =
            Mathf.Min(
                missing,
                availableSpace
            );

        for (int i = 0; i < amount; i++)
        {
            SpawnBubble();
        }
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnBubble()
    {
        if (
            !spawningEnabled ||
            o2BubblePrefab == null ||
            co2BubblePrefab == null ||
            bubblesContainer == null ||
            currentSettings == null
        )
        {
            return;
        }

        if (
            bubblesContainer.childCount >=
            currentSettings.maximumBubblesOnScreen
        )
        {
            return;
        }

        bool correctBubble =
            Random.value <
            GetCurrentCorrectBubbleChance();

        LungGasType gasType =
            GetGasTypeForSpawn(correctBubble);

        GameObject prefab =
            gasType == LungGasType.O2
                ? o2BubblePrefab
                : co2BubblePrefab;

        RectTransform[] areas =
            Random.value < 0.5f
                ? leftSpawnAreas
                : rightSpawnAreas;

        if (areas == null || areas.Length == 0)
        {
            return;
        }

        RectTransform area =
            areas[
                Random.Range(
                    0,
                    areas.Length
                )
            ];

        if (area == null)
        {
            return;
        }

        GameObject bubble =
            Instantiate(
                prefab,
                bubblesContainer
            );

        RectTransform bubbleRect =
            bubble.GetComponent<RectTransform>();

        if (bubbleRect == null)
        {
            Destroy(bubble);
            return;
        }

        bubbleRect.position =
            GetRandomWorldPositionInside(area);

        ApplyRandomScale(bubbleRect);

        ApplyRandomLifetime(bubble);
    }

    // =========================================================
    // GAS TYPE
    // =========================================================

    private LungGasType GetGasTypeForSpawn(
        bool correctBubble
    )
    {
        if (
            LungsMinigameManager.Instance.CurrentPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            return correctBubble
                ? LungGasType.O2
                : LungGasType.CO2;
        }

        return correctBubble
            ? LungGasType.CO2
            : LungGasType.O2;
    }

    // =========================================================
    // FINAL SECTION
    // =========================================================

    private bool IsInFinalPhaseSection()
    {
        if (LungsMinigameManager.Instance == null)
        {
            return false;
        }

        float progress;

        if (
            LungsMinigameManager.Instance.CurrentPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            progress =
                LungsMinigameManager.Instance.OxygenPercent /
                100f;
        }
        else
        {
            progress =
                1f -
                (
                    LungsMinigameManager.Instance.CO2Percent /
                    100f
                );
        }

        return progress >=
            currentSettings.finalPhaseThreshold;
    }

    private float GetCurrentSpawnInterval()
    {
        if (currentSettings == null)
        {
            return 1f;
        }

        float interval =
            currentSettings.spawnInterval;

        if (IsInFinalPhaseSection())
        {
            interval *=
                currentSettings
                    .finalPhaseSpawnIntervalMultiplier;
        }

        return Mathf.Max(
            0.05f,
            interval
        );
    }

    private float GetCurrentCorrectBubbleChance()
    {
        float chance =
            currentSettings.correctBubbleChance;

        if (IsInFinalPhaseSection())
        {
            chance -=
                currentSettings
                    .finalPhaseCorrectChanceReduction;
        }

        return Mathf.Clamp(
            chance,
            0.05f,
            0.95f
        );
    }

    // =========================================================
    // POSITION
    // =========================================================

    private Vector3 GetRandomWorldPositionInside(
        RectTransform area
    )
    {
        Rect rect = area.rect;

        float x =
            Random.Range(
                rect.xMin,
                rect.xMax
            );

        float y =
            Random.Range(
                rect.yMin,
                rect.yMax
            );

        return area.TransformPoint(
            new Vector3(
                x,
                y,
                0f
            )
        );
    }

    // =========================================================
    // SCALE
    // =========================================================

    private void ApplyRandomScale(
        RectTransform bubbleRect
    )
    {
        float scale =
            Random.Range(
                currentSettings.bubbleScaleMin,
                currentSettings.bubbleScaleMax
            );

        bubbleRect.localScale =
            Vector3.one * scale;
    }

    // =========================================================
    // LIFETIME
    // =========================================================

    private void ApplyRandomLifetime(
        GameObject bubble
    )
    {
        float min =
            Mathf.Min(
                currentSettings.bubbleLifetimeMin,
                currentSettings.bubbleLifetimeMax
            );

        float max =
            Mathf.Max(
                currentSettings.bubbleLifetimeMin,
                currentSettings.bubbleLifetimeMax
            );

        Destroy(
            bubble,
            Random.Range(min, max)
        );
    }

    // =========================================================
    // CLEAR
    // =========================================================

    private void ClearAllBubblesImmediate()
    {
        if (bubblesContainer == null)
        {
            return;
        }

        for (
            int i = bubblesContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                bubblesContainer
                    .GetChild(i)
                    .gameObject;

            child.SetActive(false);

            Destroy(child);
        }
    }
}