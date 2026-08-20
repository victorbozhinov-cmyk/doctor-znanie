using UnityEngine;
using UnityEngine.UI;

public class LiverHealthController : MonoBehaviour
{
    public enum DifficultyMode
    {
        Easy,
        Medium,
        Hard
    }

    [Header("References")]
    [SerializeField] private Image liverImage;

    [Header("Sprites")]
    [SerializeField] private Sprite verySickSprite;
    [SerializeField] private Sprite sickSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite healthySprite;
    [SerializeField] private Sprite veryHealthySprite;

    [Header("Difficulty")]
    [SerializeField] private bool usePlayerPrefsDifficulty = true;
    [SerializeField] private DifficultyMode testDifficulty = DifficultyMode.Medium;

    private DifficultyMode currentDifficulty;

    // Масив с всички 5 възможни състояния
    private Sprite[] allStates;

    // Тук пазим кои от тях са активни за текущата трудност
    private int[] activeStateIndices;

    // Това е текущата позиция в activeStateIndices
    private int currentStateStep;

    public int CurrentStateStep => currentStateStep;
    public DifficultyMode CurrentDifficulty => currentDifficulty;

    private void Awake()
    {
        if (liverImage == null)
            liverImage = GetComponent<Image>();
    }

    private void Start()
    {
        SetupAllStates();
        DetermineDifficulty();
        BuildStatePathForDifficulty();
        SetStartState();
        RefreshVisual();
    }

    private void SetupAllStates()
    {
        allStates = new Sprite[5];
        allStates[0] = verySickSprite;
        allStates[1] = sickSprite;
        allStates[2] = neutralSprite;
        allStates[3] = healthySprite;
        allStates[4] = veryHealthySprite;
    }

    private void DetermineDifficulty()
    {
        if (!usePlayerPrefsDifficulty)
        {
            currentDifficulty = testDifficulty;
            return;
        }

        // Опит 1: ако трудността е записана като string
        string difficultyString = PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            currentDifficulty = DifficultyMode.Easy;
            return;
        }
        else if (difficultyString == "medium")
        {
            currentDifficulty = DifficultyMode.Medium;
            return;
        }
        else if (difficultyString == "hard")
        {
            currentDifficulty = DifficultyMode.Hard;
            return;
        }

        // Опит 2: ако трудността е записана като int
        // Приемаме:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard
        int difficultyInt = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficultyInt)
        {
            case 0:
                currentDifficulty = DifficultyMode.Easy;
                break;
            case 2:
                currentDifficulty = DifficultyMode.Hard;
                break;
            default:
                currentDifficulty = DifficultyMode.Medium;
                break;
        }
    }

    private void BuildStatePathForDifficulty()
    {
        switch (currentDifficulty)
        {
            case DifficultyMode.Easy:
                // 5 състояния:
                // VerySick -> Sick -> Neutral -> Healthy -> VeryHealthy
                activeStateIndices = new int[] { 0, 1, 2, 3, 4 };
                break;

            case DifficultyMode.Medium:
                // 4 състояния:
                // VerySick -> Sick -> Neutral -> VeryHealthy
                // (прескачаме Healthy)
                activeStateIndices = new int[] { 0, 1, 2, 4 };
                break;

            case DifficultyMode.Hard:
                // 3 състояния:
                // VerySick -> Neutral -> VeryHealthy
                // (прескачаме Sick и Healthy)
                activeStateIndices = new int[] { 0, 2, 4 };
                break;
        }
    }

    private void SetStartState()
    {
        // ВИНАГИ започва от Neutral
        // намираме коя позиция в activeStateIndices сочи към allStates[2]
        for (int i = 0; i < activeStateIndices.Length; i++)
        {
            if (activeStateIndices[i] == 2)
            {
                currentStateStep = i;
                return;
            }
        }

        // за всеки случай fallback
        currentStateStep = 0;
    }

    private void RefreshVisual()
    {
        if (liverImage == null || allStates == null || activeStateIndices == null)
            return;

        int spriteIndex = activeStateIndices[currentStateStep];

        if (spriteIndex >= 0 && spriteIndex < allStates.Length)
        {
            liverImage.sprite = allStates[spriteIndex];
        }
    }

    public void IncreaseHealth()
    {
        if (currentStateStep < activeStateIndices.Length - 1)
        {
            currentStateStep++;
            RefreshVisual();
        }
    }

    public void DecreaseHealth()
    {
        if (currentStateStep > 0)
        {
            currentStateStep--;
            RefreshVisual();
        }
    }

    public bool IsAtMinimumHealth()
    {
        return currentStateStep == 0;
    }

    public bool IsAtMaximumHealth()
    {
        return currentStateStep == activeStateIndices.Length - 1;
    }

    public void ResetToStartState()
    {
        SetStartState();
        RefreshVisual();
    }

    [ContextMenu("TEST Increase Health")]
    private void TestIncreaseHealth()
    {
        IncreaseHealth();
    }

    [ContextMenu("TEST Decrease Health")]
    private void TestDecreaseHealth()
    {
        DecreaseHealth();
    }

    [ContextMenu("TEST Reset To Start")]
    private void TestResetToStart()
    {
        ResetToStartState();
    }
}