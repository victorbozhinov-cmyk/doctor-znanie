using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverPuzzleManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField]
    private UILifeHeartAnimation[] heartAnimations;

    [Header("Panels")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject successPanel;

    [Header("Puzzle")]
    [SerializeField]
    private int totalCards = 8;

    [Header("Game Over")]
    [SerializeField]
    private float gameOverDelay = 2f;

    private int currentLives;
    private int maxLives;

    private int correctCards = 0;

    private bool puzzleCompleted = false;
    private bool gameOverStarted = false;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupLivesFromDifficulty();

        correctCards = 0;
        puzzleCompleted = false;
        gameOverStarted = false;

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // =========================================================
    // LIVES SETUP
    // =========================================================

    private void SetupLivesFromDifficulty()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

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

        currentLives =
            maxLives;

        for (int i = 0;
             i < heartAnimations.Length;
             i++)
        {
            if (heartAnimations[i] != null)
            {
                heartAnimations[i]
                    .gameObject
                    .SetActive(
                        i < maxLives
                    );
            }
        }
    }

    // =========================================================
    // WRONG PLACEMENT
    // =========================================================

    public void LoseLife()
    {
        if (currentLives <= 0 ||
            puzzleCompleted ||
            gameOverStarted)
        {
            return;
        }

        /*
         * Wrong звукът се пуска веднага,
         * но НЕ и ако това е последният живот.
         *
         * При последния живот следва
         * Game Over feedback звукът.
         */
        PlayWrongSoundIfNotGameOver();

        int heartIndexToRemove =
            currentLives - 1;

        currentLives--;

        if (heartIndexToRemove >= 0 &&
            heartIndexToRemove <
                heartAnimations.Length &&
            heartAnimations[
                heartIndexToRemove
            ] != null)
        {
            heartAnimations[
                heartIndexToRemove
            ].PlayLoseAnimation();
        }

        if (currentLives <= 0)
        {
            gameOverStarted = true;

            StartCoroutine(
                ShowGameOverPanel()
            );
        }
    }

    private void PlayWrongSoundIfNotGameOver()
    {
        if (currentLives <= 1)
        {
            return;
        }

        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayWrong();
        }
        else
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    private IEnumerator ShowGameOverPanel()
    {
        /*
         * Даваме време на последната
         * грешна карта да завърши
         * червеното сияние и разклащането.
         */
        yield return new WaitForSeconds(
            gameOverDelay
        );

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "GameOverPanel не е зададен " +
                "в LiverPuzzleManager!"
            );
        }
    }

    // =========================================================
    // CORRECT PLACEMENT
    // =========================================================

    public void RegisterCorrectCard()
    {
        if (puzzleCompleted ||
            gameOverStarted)
        {
            return;
        }

        /*
         * Correct звукът се пуска
         * веднага при правилния drop.
         *
         * ВАЖНО:
         * Пуска се и за последната карта.
         */
        PlayCorrectSound();

        correctCards++;

        Debug.Log(
            "Правилно поставени карти: " +
            correctCards +
            "/" +
            totalCards
        );

        if (correctCards >= totalCards)
        {
            puzzleCompleted = true;

            StartCoroutine(
                ShowSuccessPanel()
            );
        }
    }

    private void PlayCorrectSound()
    {
        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayCorrect();
        }
        else
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    private IEnumerator ShowSuccessPanel()
    {
        /*
         * Correct звукът вече е прозвучал
         * веднага при drop-а.
         *
         * Изчакваме зелената анимация,
         * след което Success Panel
         * пуска глобалния Success звук.
         */
        yield return new WaitForSeconds(
            0.5f
        );

        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "SuccessPanel не е зададен " +
                "в LiverPuzzleManager!"
            );
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .name
        );
    }

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "BodyMap"
        );
    }
}