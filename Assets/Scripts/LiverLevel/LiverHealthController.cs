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

    [Header("Liver Sprites")]
    [SerializeField] private Sprite verySickSprite;
    [SerializeField] private Sprite sickSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite healthySprite;
    [SerializeField] private Sprite veryHealthySprite;

    [Header("Difficulty")]
    [SerializeField] private bool usePlayerPrefsDifficulty = true;
    [SerializeField] private DifficultyMode testDifficulty = DifficultyMode.Medium;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    private DifficultyMode currentDifficulty;

    private Sprite[] allStates;
    private int[] activeStateIndices;

    private int currentStateStep;

    private bool gameOverTriggered = false;

    public int CurrentStateStep => currentStateStep;
    public DifficultyMode CurrentDifficulty => currentDifficulty;
    public bool IsGameOver => gameOverTriggered;

    // 0 = VerySick
    // 1 = Sick
    // 2 = Neutral
    // 3 = Healthy
    // 4 = VeryHealthy
    public int CurrentVisualStateIndex
    {
        get
        {
            if (activeStateIndices == null ||
                activeStateIndices.Length == 0)
            {
                return 2;
            }

            return activeStateIndices[currentStateStep];
        }
    }

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

        gameOverTriggered = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
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

        string difficultyString =
            PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            currentDifficulty = DifficultyMode.Easy;
            return;
        }

        if (difficultyString == "medium")
        {
            currentDifficulty = DifficultyMode.Medium;
            return;
        }

        if (difficultyString == "hard")
        {
            currentDifficulty = DifficultyMode.Hard;
            return;
        }

        int difficultyInt =
            PlayerPrefs.GetInt("Difficulty", 1);

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

                // VerySick → Sick → Neutral → Healthy → VeryHealthy
                activeStateIndices =
                    new int[] { 0, 1, 2, 3, 4 };

                break;

            case DifficultyMode.Medium:

                // VerySick → Sick → Neutral → VeryHealthy
                activeStateIndices =
                    new int[] { 0, 1, 2, 4 };

                break;

            case DifficultyMode.Hard:

                // VerySick → Neutral → VeryHealthy
                activeStateIndices =
                    new int[] { 0, 2, 4 };

                break;
        }
    }

    private void SetStartState()
    {
        // Винаги започваме от Neutral = index 2.
        for (int i = 0; i < activeStateIndices.Length; i++)
        {
            if (activeStateIndices[i] == 2)
            {
                currentStateStep = i;
                return;
            }
        }

        currentStateStep = 0;
    }

    private void RefreshVisual()
    {
        if (liverImage == null ||
            allStates == null ||
            activeStateIndices == null)
        {
            return;
        }

        int spriteIndex =
            activeStateIndices[currentStateStep];

        if (spriteIndex >= 0 &&
            spriteIndex < allStates.Length)
        {
            liverImage.sprite =
                allStates[spriteIndex];
        }
    }

    public void IncreaseHealth()
    {
        if (gameOverTriggered)
            return;

        if (currentStateStep <
            activeStateIndices.Length - 1)
        {
            currentStateStep++;
            RefreshVisual();
        }
    }

    public void DecreaseHealth()
    {
        if (gameOverTriggered)
            return;

        // VerySick + още едно вредно = Game Over.
        if (currentStateStep == 0)
        {
            TriggerGameOver();
            return;
        }

        currentStateStep--;
        RefreshVisual();
    }

    private void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        Debug.Log(
            "GAME OVER - Черният дроб не може да понесе повече вредни вещества!"
        );

        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public bool IsAtMinimumHealth()
    {
        return currentStateStep == 0;
    }

    public bool IsAtMaximumHealth()
    {
        return currentStateStep ==
               activeStateIndices.Length - 1;
    }

    public void ResetToStartState()
    {
        gameOverTriggered = false;

        SetStartState();
        RefreshVisual();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
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