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

    // =========================================================
    // SETUP
    // =========================================================

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

        // Ако Difficulty се пази като int:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

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

                // 5 състояния:
                //
                // VerySick
                // Sick
                // Neutral
                // Healthy
                // VeryHealthy

                activeStateIndices =
                    new int[] { 0, 1, 2, 3, 4 };

                break;


            case DifficultyMode.Medium:

                // 4 състояния

                activeStateIndices =
                    new int[] { 0, 1, 2, 4 };

                break;


            case DifficultyMode.Hard:

                // 3 състояния:
                //
                // VerySick
                // Neutral
                // VeryHealthy

                activeStateIndices =
                    new int[] { 0, 2, 4 };

                break;
        }
    }

    private void SetStartState()
    {
        // ВИНАГИ започваме от Neutral.
        // Neutral е allStates[2].

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

    // =========================================================
    // VISUAL
    // =========================================================

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

    // =========================================================
    // HEALTH +
    // =========================================================

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

    // =========================================================
    // HEALTH -
    // =========================================================

    public void DecreaseHealth()
    {
        if (gameOverTriggered)
            return;

        /*
         * Ако вече сме на най-ниското състояние,
         * НЕ слизаме повече.
         *
         * Това означава:
         *
         * VerySick + още едно вредно =
         * GAME OVER.
         */

        if (currentStateStep == 0)
        {
            TriggerGameOver();
            return;
        }

        currentStateStep--;

        RefreshVisual();
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    private void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        Debug.Log(
            "GAME OVER - Черният дроб не може да понесе повече вредни вещества!"
        );

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // =========================================================
    // STATE CHECKS
    // =========================================================

    public bool IsAtMinimumHealth()
    {
        return currentStateStep == 0;
    }

    public bool IsAtMaximumHealth()
    {
        return currentStateStep ==
               activeStateIndices.Length - 1;
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetToStartState()
    {
        gameOverTriggered = false;

        SetStartState();

        RefreshVisual();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // =========================================================
    // TESTS
    // =========================================================

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