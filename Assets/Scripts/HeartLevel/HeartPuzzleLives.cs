using UnityEngine;

public class HeartPuzzleLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private GameObject[] lifeHearts =
        new GameObject[3];

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverOverlay;

    private int currentLives;
    private bool isLosingLife;

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
        isLosingLife = false;

        LoadLivesFromDifficulty();
        HideGameOver();
        ResetHeartAnimations();
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
        if (IsGameOver || isLosingLife)
        {
            return;
        }

        int lostHeartIndex = currentLives - 1;

        currentLives--;
        isLosingLife = true;

        Debug.Log(
            $"Загубен живот. Остават: {currentLives}"
        );

        if (lostHeartIndex < 0 ||
            lostHeartIndex >= lifeHearts.Length ||
            lifeHearts[lostHeartIndex] == null)
        {
            FinishLoseLife();
            return;
        }

        GameObject lostHeart =
            lifeHearts[lostHeartIndex];

        UILifeHeartAnimation heartAnimation =
            lostHeart.GetComponent<UILifeHeartAnimation>();

        if (heartAnimation != null)
        {
            heartAnimation.PlayLoseAnimation(
                FinishLoseLife
            );
        }
        else
        {
            lostHeart.SetActive(false);
            FinishLoseLife();
        }
    }

    private void FinishLoseLife()
    {
        isLosingLife = false;

        Canvas.ForceUpdateCanvases();

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
        gameOverOverlay.transform.SetAsLastSibling();
    }

    private void HideGameOver()
    {
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }
    }

    private void ResetHeartAnimations()
    {
        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
            {
                continue;
            }

            UILifeHeartAnimation animation =
                lifeHearts[i]
                    .GetComponent<UILifeHeartAnimation>();

            if (animation != null)
            {
                animation.ResetHeart();
            }
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

            lifeHearts[i].SetActive(
                i < currentLives
            );
        }

        Canvas.ForceUpdateCanvases();
    }
}