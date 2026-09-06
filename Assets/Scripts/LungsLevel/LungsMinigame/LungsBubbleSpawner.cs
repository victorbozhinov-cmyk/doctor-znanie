using System.Collections;
using UnityEngine;

[System.Serializable]
public class LungsSpawnerDifficultySettings
{
    // =========================================================
    // BUBBLE AMOUNT
    // =========================================================

    [Header("Bubble Amount")]

    [Min(1)]
    public int initialBubbleCount = 10;

    [Min(1)]
    public int minimumBubblesOnScreen = 8;

    [Min(1)]
    public int maximumBubblesOnScreen = 14;

    // =========================================================
    // SPAWN SPEED
    // =========================================================

    [Header("Spawn Speed")]

    [Min(0.05f)]
    public float spawnInterval = 0.8f;

    // =========================================================
    // CORRECT BUBBLE CHANCE
    // =========================================================

    [Header("Correct Bubble Chance")]

    [Range(0.05f, 0.95f)]
    public float inhaleCorrectBubbleChance = 0.5f;

    [Range(0.05f, 0.95f)]
    public float exhaleCorrectBubbleChance = 0.5f;

    // =========================================================
    // RATIO CONTROL
    // =========================================================

    [Header("Ratio Control")]

    [Tooltip(
        "Колко отклонение от зададеното съотношение е позволено. " +
        "По-малка стойност = по-стриктно се поддържа ratio-то."
    )]
    [Range(0f, 0.3f)]
    public float ratioTolerance = 0.1f;

    // =========================================================
    // FINAL SECTION
    // =========================================================

    [Header("Final Phase Difficulty")]

    [Tooltip(
        "При какъв прогрес започва по-трудната финална част. " +
        "0.7 означава след 70%."
    )]
    [Range(0.5f, 0.95f)]
    public float finalPhaseThreshold = 0.8f;

    [Tooltip(
        "Колко се намалява шансът за правилно балонче " +
        "след достигане на Final Phase Threshold."
    )]
    [Range(0f, 0.5f)]
    public float finalPhaseCorrectChanceReduction = 0.1f;

    [Header("Final Phase Bubble Lifetime")]

    [Tooltip(
        "Минималният живот на балонче във финалната част."
    )]
    [Min(0.5f)]
    public float finalBubbleLifetimeMin = 3f;

    [Tooltip(
        "Максималният живот на балонче във финалната част."
    )]
    [Min(0.5f)]
    public float finalBubbleLifetimeMax = 5f;

    // =========================================================
    // NORMAL LIFETIME
    // =========================================================

    [Header("Normal Bubble Lifetime")]

    [Min(0.5f)]
    public float bubbleLifetimeMin = 4f;

    [Min(0.5f)]
    public float bubbleLifetimeMax = 6f;

    // =========================================================
    // SIZE
    // =========================================================

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
    // EASY
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

            inhaleCorrectBubbleChance = 0.5f,
            exhaleCorrectBubbleChance = 0.5f,

            ratioTolerance = 0.1f,

            finalPhaseThreshold = 0.7f,
            finalPhaseCorrectChanceReduction = 0.1f,

            bubbleLifetimeMin = 5f,
            bubbleLifetimeMax = 7f,

            finalBubbleLifetimeMin = 4f,
            finalBubbleLifetimeMax = 6f,

            bubbleScaleMin = 0.9f,
            bubbleScaleMax = 1.1f
        };

    // =========================================================
    // MEDIUM
    // =========================================================

    [Header("Medium Settings")]

    [SerializeField]
    private LungsSpawnerDifficultySettings mediumSettings =
        new LungsSpawnerDifficultySettings
        {
            initialBubbleCount = 10,
            minimumBubblesOnScreen = 8,
            maximumBubblesOnScreen = 14,

            spawnInterval = 0.7f,

            inhaleCorrectBubbleChance = 0.5f,
            exhaleCorrectBubbleChance = 0.5f,

            ratioTolerance = 0.1f,

            finalPhaseThreshold = 0.75f,
            finalPhaseCorrectChanceReduction = 0.12f,

            bubbleLifetimeMin = 4f,
            bubbleLifetimeMax = 6f,

            finalBubbleLifetimeMin = 3f,
            finalBubbleLifetimeMax = 5f,

            bubbleScaleMin = 0.9f,
            bubbleScaleMax = 1.1f
        };

    // =========================================================
    // HARD
    // =========================================================

    [Header("Hard Settings")]

    [SerializeField]
    private LungsSpawnerDifficultySettings hardSettings =
        new LungsSpawnerDifficultySettings
        {
            initialBubbleCount = 12,
            minimumBubblesOnScreen = 10,
            maximumBubblesOnScreen = 16,

            spawnInterval = 0.55f,

            inhaleCorrectBubbleChance = 0.4f,
            exhaleCorrectBubbleChance = 0.4f,

            ratioTolerance = 0.08f,

            finalPhaseThreshold = 0.75f,
            finalPhaseCorrectChanceReduction = 0.1f,

            bubbleLifetimeMin = 3f,
            bubbleLifetimeMax = 5f,

            finalBubbleLifetimeMin = 2f,
            finalBubbleLifetimeMax = 4f,

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

    // Ratio tracking
    private int correctSpawned;
    private int wrongSpawned;

    private bool previousFinalSectionState;

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

        ResetRatioTracking();

        SpawnInitialBubbles();

        spawnCoroutine =
            StartCoroutine(
                SpawnLoop()
            );
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

        ResetRatioTracking();

        SpawnInitialBubbles();
    }

    // =========================================================
    // RATIO TRACKING
    // =========================================================

    private void ResetRatioTracking()
    {
        correctSpawned = 0;
        wrongSpawned = 0;

        previousFinalSectionState =
            IsInFinalPhaseSection();
    }

    private bool ShouldSpawnCorrectBubble()
    {
        bool finalSection =
            IsInFinalPhaseSection();

        /*
         * Когато преминем във финалната част,
         * започваме ново ratio броене.
         *
         * Така новият по-нисък шанс започва
         * да се прилага веднага.
         */
        if (
            finalSection !=
            previousFinalSectionState
        )
        {
            correctSpawned = 0;
            wrongSpawned = 0;

            previousFinalSectionState =
                finalSection;
        }

        float targetChance =
            GetCurrentCorrectBubbleChance();

        int totalSpawned =
            correctSpawned +
            wrongSpawned;

        /*
         * При първите няколко балончета
         * оставяме малко повече случайност.
         */
        if (totalSpawned < 2)
        {
            return Random.value <
                targetChance;
        }

        float currentCorrectRatio =
            (float)correctSpawned /
            totalSpawned;

        float tolerance =
            currentSettings.ratioTolerance;

        /*
         * Ако имаме прекалено малко правилни,
         * принудително spawn-ваме правилно.
         */
        if (
            currentCorrectRatio <
            targetChance - tolerance
        )
        {
            return true;
        }

        /*
         * Ако имаме прекалено много правилни,
         * принудително spawn-ваме грешно.
         */
        if (
            currentCorrectRatio >
            targetChance + tolerance
        )
        {
            return false;
        }

        /*
         * Ако сме в допустимото отклонение,
         * запазваме случайността.
         */
        return Random.value <
            targetChance;
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
            ShouldSpawnCorrectBubble();

        if (correctBubble)
        {
            correctSpawned++;
        }
        else
        {
            wrongSpawned++;
        }

        LungGasType gasType =
            GetGasTypeForSpawn(
                correctBubble
            );

        GameObject prefab =
            gasType == LungGasType.O2
                ? o2BubblePrefab
                : co2BubblePrefab;

        RectTransform[] areas =
            Random.value < 0.5f
                ? leftSpawnAreas
                : rightSpawnAreas;

        if (
            areas == null ||
            areas.Length == 0
        )
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
            GetRandomWorldPositionInside(
                area
            );

        ApplyRandomScale(
            bubbleRect
        );

        ApplyRandomLifetime(
            bubble
        );
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
        if (
            LungsMinigameManager.Instance == null ||
            currentSettings == null
        )
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

    // =========================================================
    // SPAWN INTERVAL
    // =========================================================

    private float GetCurrentSpawnInterval()
    {
        if (currentSettings == null)
        {
            return 1f;
        }

        /*
         * Final Phase вече НЕ променя
         * скоростта на spawn.
         */
        return Mathf.Max(
            0.05f,
            currentSettings.spawnInterval
        );
    }

    // =========================================================
    // CORRECT CHANCE
    // =========================================================

    private float GetCurrentCorrectBubbleChance()
    {
        if (
            currentSettings == null ||
            LungsMinigameManager.Instance == null
        )
        {
            return 0.5f;
        }

        float chance;

        if (
            LungsMinigameManager.Instance.CurrentPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            chance =
                currentSettings
                    .inhaleCorrectBubbleChance;
        }
        else
        {
            chance =
                currentSettings
                    .exhaleCorrectBubbleChance;
        }

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
        Rect rect =
            area.rect;

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
        float min;
        float max;

        if (IsInFinalPhaseSection())
        {
            min =
                Mathf.Min(
                    currentSettings.finalBubbleLifetimeMin,
                    currentSettings.finalBubbleLifetimeMax
                );

            max =
                Mathf.Max(
                    currentSettings.finalBubbleLifetimeMin,
                    currentSettings.finalBubbleLifetimeMax
                );
        }
        else
        {
            min =
                Mathf.Min(
                    currentSettings.bubbleLifetimeMin,
                    currentSettings.bubbleLifetimeMax
                );

            max =
                Mathf.Max(
                    currentSettings.bubbleLifetimeMin,
                    currentSettings.bubbleLifetimeMax
                );
        }

        Destroy(
            bubble,
            Random.Range(
                min,
                max
            )
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
            int i =
                bubblesContainer.childCount - 1;
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