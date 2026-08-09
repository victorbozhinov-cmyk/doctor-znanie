using System;
using UnityEngine;

public class HeartMinigameLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private UILifeHeartAnimation[] lifeHearts;

    private int currentLives;
    private int maxLives;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool HasLives => currentLives > 0;

    private void Start()
    {
        SetupLivesFromDifficulty();
    }

    private void SetupLivesFromDifficulty()
    {
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                maxLives = 3;
                break;

            case 2:
                maxLives = 1;
                break;

            default:
                maxLives = 2;
                break;
        }

        currentLives = maxLives;

        ResetLifeHearts();

        Debug.Log(
            "Heart Minigame Lives: " +
            currentLives
        );
    }

    private void ResetLifeHearts()
    {
        if (lifeHearts == null)
            return;

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
                continue;

            if (i < maxLives)
            {
                lifeHearts[i].ResetHeart();
            }
            else
            {
                lifeHearts[i].gameObject.SetActive(false);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    public void LoseLife()
    {
        LoseLife(null);
    }

    public void LoseLife(Action onAnimationFinished)
    {
        if (currentLives <= 0)
            return;

        int heartIndex = currentLives - 1;

        currentLives--;

        Debug.Log(
            "Life lost! Remaining lives: " +
            currentLives
        );

        if (heartIndex >= 0 &&
            heartIndex < lifeHearts.Length &&
            lifeHearts[heartIndex] != null)
        {
            lifeHearts[heartIndex].PlayLoseAnimation(
                () =>
                {
                    Canvas.ForceUpdateCanvases();

                    if (currentLives <= 0)
                    {
                        Debug.Log(
                            "No lives remaining!"
                        );
                    }

                    // Казваме на този, който е извикал LoseLife(),
                    // че анимацията вече е приключила.
                    onAnimationFinished?.Invoke();
                }
            );
        }
        else
        {
            // За защита, ако липсва reference към сърцето.
            onAnimationFinished?.Invoke();
        }
    }

    public void ResetLives()
    {
        currentLives = maxLives;

        ResetLifeHearts();
    }
}