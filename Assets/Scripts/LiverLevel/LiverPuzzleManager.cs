using UnityEngine;
using UnityEngine.SceneManagement;
public class LiverPuzzleManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private UILifeHeartAnimation[] heartAnimations;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;

    private int currentLives;
    private int maxLives;

    private void Start()
    {
        SetupLivesFromDifficulty();
    }

    private void SetupLivesFromDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0: // Easy
                maxLives = 3;
                break;

            case 1: // Medium
                maxLives = 2;
                break;

            case 2: // Hard
                maxLives = 1;
                break;

            default:
                maxLives = 2;
                break;
        }

        currentLives = maxLives;

        for (int i = 0; i < heartAnimations.Length; i++)
        {
            if (heartAnimations[i] != null)
            {
                heartAnimations[i].gameObject.SetActive(i < maxLives);
            }
        }
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
            return;

        int heartIndexToRemove = currentLives - 1;
        currentLives--;

        if (heartIndexToRemove >= 0 &&
            heartIndexToRemove < heartAnimations.Length &&
            heartAnimations[heartIndexToRemove] != null)
        {
            heartAnimations[heartIndexToRemove].PlayLoseAnimation();
        }

        if (currentLives <= 0)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }
}