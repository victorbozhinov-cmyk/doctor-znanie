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
    [SerializeField] private int wavesPerStage = 2;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = true;

    private GameObject[] activeStages;

    private int currentStageIndex;
    private int successfulWavesInCurrentStage;

    public int CurrentStageIndex => currentStageIndex;
    public int SuccessfulWavesInCurrentStage => successfulWavesInCurrentStage;
    public bool IsDigestionComplete { get; private set; }

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
    }

    private void SetupForDifficulty()
    {
        HideAllFoodImages();

        int difficulty = PlayerPrefs.GetInt(
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

                Debug.Log("Food stages: EASY");
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

                Debug.Log("Food stages: HARD");
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

                Debug.Log("Food stages: MEDIUM");
                break;
        }

        currentStageIndex = 0;
        successfulWavesInCurrentStage = 0;
        IsDigestionComplete = false;

        ShowCurrentStage();
    }

    public void RegisterSuccessfulWave()
    {
        if (IsDigestionComplete)
            return;

        successfulWavesInCurrentStage++;

        Debug.Log(
            $"Successful wave: {successfulWavesInCurrentStage}/{wavesPerStage}"
        );

        if (successfulWavesInCurrentStage >= wavesPerStage)
        {
            AdvanceFoodStage();
        }
    }

    private void AdvanceFoodStage()
    {
        successfulWavesInCurrentStage = 0;

        if (currentStageIndex >= activeStages.Length - 1)
        {
            CompleteDigestion();
            return;
        }

        currentStageIndex++;

        ShowCurrentStage();

        Debug.Log(
            $"Food advanced to stage {currentStageIndex}"
        );

        if (currentStageIndex >= activeStages.Length - 1)
        {
            CompleteDigestion();
        }
    }

    private void ShowCurrentStage()
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

    private void CompleteDigestion()
    {
        IsDigestionComplete = true;

        Debug.Log(
            "Храната е напълно обработена - химус!"
        );
    }

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

    public void RestartFoodStages()
    {
        SetupForDifficulty();
    }
}