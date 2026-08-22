using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class StomachFoodStageController : MonoBehaviour
{
    [Header("Light Food")]
    [SerializeField] private GameObject startLight;
    [SerializeField] private GameObject lightStage1;
    [SerializeField] private GameObject lightStage2;
    [SerializeField] private GameObject himusLight;

    [Header("Medium Food")]
    [SerializeField] private GameObject startMedium;
    [SerializeField] private GameObject mediumStage1;
    [SerializeField] private GameObject mediumStage2;
    [SerializeField] private GameObject mediumStage3;
    [SerializeField] private GameObject himusMedium;

    [Header("Hard Food")]
    [SerializeField] private GameObject startHard;
    [SerializeField] private GameObject hardStage1;
    [SerializeField] private GameObject hardStage2;
    [SerializeField] private GameObject hardStage3;
    [SerializeField] private GameObject hardStage4;
    [SerializeField] private GameObject himusHard;

    [Header("Wave Settings")]
    [SerializeField] private int easyWavesPerStage = 2;
    [SerializeField] private int mediumWavesPerStage = 2;
    [SerializeField] private int hardWavesPerStage = 3;

    [Header("Food Transition")]
    [SerializeField]
    private StomachFoodTransitionAnimator foodTransitionAnimator;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = false;

    private GameObject[] activeStages;

    private int currentStageIndex;
    private int successfulWavesInCurrentStage;
    private int wavesPerStage;
    private int difficulty;

    private bool isTransitioning;

    // =========================================================
    // EVENTS
    // =========================================================

    public event Action<int, int> SuccessfulWaveRegistered;
    public event Action<int, int> WaveProgressReset;
    public event Action<int> FoodStageAdvanced;
    public event Action DigestionCompleted;

    // =========================================================
    // PUBLIC INFO
    // =========================================================

    public int CurrentStageIndex =>
        currentStageIndex;

    public int SuccessfulWavesInCurrentStage =>
        successfulWavesInCurrentStage;

    public int WavesPerStage =>
        wavesPerStage;

    public int TotalStageCount =>
        activeStages != null
            ? activeStages.Length
            : 0;

    public int Difficulty =>
        difficulty;

    public bool IsHardDifficulty =>
        difficulty == 2;

    public bool IsTransitioning =>
        isTransitioning;

    public bool IsDigestionComplete
    {
        get;
        private set;
    }

    public bool IsWaveSetComplete =>
        successfulWavesInCurrentStage >=
        wavesPerStage;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupForDifficulty();
    }

    private void Update()
    {
        if (!enableTestKey)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.nKey.wasPressedThisFrame)
        {
            RegisterSuccessfulWave();
        }

        if (Keyboard.current != null &&
            Keyboard.current.mKey.wasPressedThisFrame)
        {
            AdvanceAfterCompletedWaveSet();
        }
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupForDifficulty()
    {
        HideAllFoodImages();

        difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        switch (difficulty)
        {
            case 0:
                activeStages = new GameObject[]
                {
                    startLight,
                    lightStage1,
                    lightStage2,
                    himusLight
                };

                wavesPerStage =
                    easyWavesPerStage;

                break;

            case 2:
                activeStages = new GameObject[]
                {
                    startHard,
                    hardStage1,
                    hardStage2,
                    hardStage3,
                    hardStage4,
                    himusHard
                };

                wavesPerStage =
                    hardWavesPerStage;

                break;

            default:
                activeStages = new GameObject[]
                {
                    startMedium,
                    mediumStage1,
                    mediumStage2,
                    mediumStage3,
                    himusMedium
                };

                wavesPerStage =
                    mediumWavesPerStage;

                break;
        }

        currentStageIndex = 0;
        successfulWavesInCurrentStage = 0;

        isTransitioning = false;
        IsDigestionComplete = false;

        ShowCurrentStageImmediate();
    }

    // =========================================================
    // SUCCESSFUL WAVE
    // =========================================================

    public void RegisterSuccessfulWave()
    {
        if (IsDigestionComplete ||
            isTransitioning)
        {
            return;
        }

        if (IsWaveSetComplete)
            return;

        successfulWavesInCurrentStage++;

        SuccessfulWaveRegistered?.Invoke(
            successfulWavesInCurrentStage,
            wavesPerStage
        );
    }

    // =========================================================
    // RESET WAVE PROGRESS
    // =========================================================

    public void ResetCurrentWaveProgress()
    {
        if (IsDigestionComplete ||
            isTransitioning)
        {
            return;
        }

        successfulWavesInCurrentStage = 0;

        WaveProgressReset?.Invoke(
            successfulWavesInCurrentStage,
            wavesPerStage
        );
    }

    // =========================================================
    // ADVANCE REQUEST
    // =========================================================

    public void AdvanceAfterCompletedWaveSet()
    {
        if (IsDigestionComplete ||
            isTransitioning)
        {
            return;
        }

        if (!IsWaveSetComplete)
            return;

        BeginFoodStageTransition();
    }

    // =========================================================
    // FOOD TRANSITION
    // =========================================================

    private void BeginFoodStageTransition()
    {
        if (activeStages == null ||
            activeStages.Length == 0)
        {
            return;
        }

        int nextStageIndex =
            currentStageIndex + 1;

        if (nextStageIndex >=
            activeStages.Length)
        {
            CompleteDigestion();
            return;
        }

        GameObject currentFood =
            activeStages[currentStageIndex];

        GameObject nextFood =
            activeStages[nextStageIndex];

        isTransitioning = true;

        // Ако animator-ът липсва,
        // запазваме функционалността
        // с моментална смяна.
        if (foodTransitionAnimator == null)
        {
            if (currentFood != null)
            {
                currentFood.SetActive(false);
            }

            if (nextFood != null)
            {
                nextFood.SetActive(true);
            }

            FinishFoodStageTransition(
                nextStageIndex
            );

            return;
        }

        foodTransitionAnimator.PlayTransition(
            currentFood,
            nextFood,
            () =>
            {
                FinishFoodStageTransition(
                    nextStageIndex
                );
            }
        );
    }

    private void FinishFoodStageTransition(
        int newStageIndex)
    {
        currentStageIndex =
            newStageIndex;

        successfulWavesInCurrentStage = 0;

        isTransitioning = false;

        // ВАЖНО:
        // Този event вече се изпраща ЧАК
        // след края на food анимацията.
        FoodStageAdvanced?.Invoke(
            currentStageIndex
        );

        if (currentStageIndex >=
            activeStages.Length - 1)
        {
            CompleteDigestion();
        }
    }

    // =========================================================
    // INITIAL FOOD
    // =========================================================

    private void ShowCurrentStageImmediate()
    {
        if (activeStages == null ||
            activeStages.Length == 0)
        {
            return;
        }

        HideAllFoodImages();

        GameObject currentFood =
            activeStages[currentStageIndex];

        if (currentFood != null)
        {
            currentFood.SetActive(true);
        }
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteDigestion()
    {
        if (IsDigestionComplete)
            return;

        IsDigestionComplete = true;

        DigestionCompleted?.Invoke();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void HideAllFoodImages()
    {
        SetActive(startLight, false);
        SetActive(lightStage1, false);
        SetActive(lightStage2, false);
        SetActive(himusLight, false);

        SetActive(startMedium, false);
        SetActive(mediumStage1, false);
        SetActive(mediumStage2, false);
        SetActive(mediumStage3, false);
        SetActive(himusMedium, false);

        SetActive(startHard, false);
        SetActive(hardStage1, false);
        SetActive(hardStage2, false);
        SetActive(hardStage3, false);
        SetActive(hardStage4, false);
        SetActive(himusHard, false);
    }

    private void SetActive(
        GameObject target,
        bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartFoodStages()
    {
        SetupForDifficulty();
    }
}