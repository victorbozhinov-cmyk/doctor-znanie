using UnityEngine;

public class HeartPuzzleLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private GameObject[] lifeHearts =
        new GameObject[3];

    private int currentLives;

    public int CurrentLives => currentLives;

    private void Start()
    {
        LoadLivesFromDifficulty();
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

            case 1: // Средно
                currentLives = 2;
                break;

            case 2: // Трудно
                currentLives = 1;
                break;

            default:
                currentLives = 2;
                break;
        }

        UpdateLivesUI();
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
        {
            return;
        }

        currentLives--;

        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Debug.Log("Няма останали животи!");
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

        // Принуждаваме Layout Group да се обнови веднага.
        Canvas.ForceUpdateCanvases();
    }
}