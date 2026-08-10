using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverPuzzleManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private UILifeHeartAnimation[] heartAnimations;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject successPanel;

    [Header("Puzzle")]
    [SerializeField] private int totalCards = 8;

    private int currentLives;
    private int maxLives;

    private int correctCards = 0;
    private bool puzzleCompleted = false;

    private void Start()
    {
        SetupLivesFromDifficulty();

        correctCards = 0;
        puzzleCompleted = false;

        if (successPanel != null)
            successPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void SetupLivesFromDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                maxLives = 3;
                break;

            case 1:
                maxLives = 2;
                break;

            case 2:
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
        if (currentLives <= 0 || puzzleCompleted)
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

    // Извиква се при всяка правилно поставена карта.
    public void RegisterCorrectCard()
    {
        if (puzzleCompleted)
            return;

        correctCards++;

        Debug.Log("Правилно поставени карти: " +
                  correctCards + "/" + totalCards);

        if (correctCards >= totalCards)
        {
            puzzleCompleted = true;
            StartCoroutine(ShowSuccessPanel());
        }
    }

    private IEnumerator ShowSuccessPanel()
    {
        // Изчакваме да се види зелената анимация
        // на последната поставена карта.
        yield return new WaitForSeconds(0.5f);

        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "SuccessPanel не е зададен в LiverPuzzleManager!"
            );
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