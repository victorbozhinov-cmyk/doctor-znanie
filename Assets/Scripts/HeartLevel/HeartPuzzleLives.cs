using UnityEngine;

public class HeartPuzzleLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private GameObject[] lifeHearts =
        new GameObject[3];

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    private int currentLives;

    public int CurrentLives => currentLives;
    public bool IsGameOver => currentLives <= 0;

    private void Awake()
    {
        HideGameOver();
    }

    private void Start()
    {
        ResetLives();
    }

    public void ResetLives()
    {
        LoadLivesFromDifficulty();
        HideGameOver();
        UpdateLivesUI();
    }

    private void LoadLivesFromDifficulty()
    {
        int difficulty =
            DifficultySelector.GetSavedDifficulty();

        switch (difficulty)
        {
            case 0: // Лесно
                currentLives = 3;
                break;

            case 1: // Нормално
                currentLives = 2;
                break;

            case 2: // Трудно
                currentLives = 1;
                break;

            default:
                currentLives = 2;
                break;
        }
    }

    public void LoseLife()
    {
        if (IsGameOver)
        {
            return;
        }

        currentLives--;

        UpdateLivesUI();

        Debug.Log(
            $"Загубен живот. Остават: {currentLives}"
        );

        if (IsGameOver)
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        Debug.Log("Няма останали животи!");

        if (gameOverOverlay == null)
        {
            Debug.LogError(
                "GameOverOverlay не е свързан в HeartPuzzleLives!"
            );

            return;
        }

        gameOverOverlay.SetActive(true);

        // Поставя прозореца над всички останали UI елементи.
        gameOverOverlay.transform.SetAsLastSibling();
    }

    private void HideGameOver()
    {
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
            {
                continue;
            }

            lifeHearts[i].SetActive(i < currentLives);
        }

        Canvas.ForceUpdateCanvases();
    }
}