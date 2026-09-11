using System;
using UnityEngine;
using UnityEngine.UI;

public class LungsPuzzleLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private GameObject[] lifeHearts;

    private int currentLives;
    private int maxLives;

    private bool lifeLossInProgress;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool LifeLossInProgress => lifeLossInProgress;

    // =====================================================
    // RESET
    // =====================================================

    public void ResetLives()
    {
        lifeLossInProgress = false;

        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        difficulty =
            Mathf.Clamp(difficulty, 0, 2);

        switch (difficulty)
        {
            case 0: // EASY
                maxLives = 3;
                break;

            case 1: // MEDIUM
                maxLives = 2;
                break;

            case 2: // HARD
                maxLives = 1;
                break;
        }

        currentLives = maxLives;

        ResetAllHearts();
        UpdateLivesUI();

        ForceLayoutUpdate();
    }

    // =====================================================
    // LOSE LIFE
    // =====================================================

    public void LoseLife()
    {
        LoseLife(null);
    }

    public void LoseLife(Action onFinished)
    {
        if (lifeLossInProgress)
            return;

        if (currentLives <= 0)
        {
            onFinished?.Invoke();
            return;
        }

        // Ако имаме 3 живота:
        // губим Life3.
        //
        // Ако имаме 2:
        // губим Life2.
        //
        // Ако имаме 1:
        // губим Life1.

        int heartIndex =
            currentLives - 1;

        currentLives--;

        if (lifeHearts == null ||
            heartIndex < 0 ||
            heartIndex >= lifeHearts.Length)
        {
            UpdateLivesUI();

            onFinished?.Invoke();

            return;
        }

        GameObject heart =
            lifeHearts[heartIndex];

        if (heart == null)
        {
            UpdateLivesUI();

            onFinished?.Invoke();

            return;
        }

        UILifeHeartAnimation animation =
            heart.GetComponent<UILifeHeartAnimation>();

        // =================================================
        // ИМА АНИМАЦИЯ
        // =================================================

        if (animation != null)
        {
            lifeLossInProgress = true;

            animation.PlayLoseAnimation(
                () =>
                {
                    lifeLossInProgress = false;

                    ForceLayoutUpdate();

                    onFinished?.Invoke();
                }
            );

            return;
        }

        // =================================================
        // НЯМА АНИМАЦИЯ
        // =================================================

        Debug.LogWarning(
            heart.name +
            " няма UILifeHeartAnimation."
        );

        heart.SetActive(false);

        ForceLayoutUpdate();

        onFinished?.Invoke();
    }

    // =====================================================
    // RESET HEARTS
    // =====================================================

    private void ResetAllHearts()
    {
        if (lifeHearts == null)
            return;

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            GameObject heart =
                lifeHearts[i];

            if (heart == null)
                continue;

            UILifeHeartAnimation animation =
                heart.GetComponent<UILifeHeartAnimation>();

            if (animation != null)
            {
                animation.ResetHeart();
            }
            else
            {
                heart.SetActive(true);
            }
        }
    }

    // =====================================================
    // UPDATE UI
    // =====================================================

    private void UpdateLivesUI()
    {
        if (lifeHearts == null)
            return;

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            GameObject heart =
                lifeHearts[i];

            if (heart == null)
                continue;

            bool shouldBeActive =
                i < currentLives;

            heart.SetActive(
                shouldBeActive
            );
        }
    }

    // =====================================================
    // LAYOUT
    // =====================================================

    private void ForceLayoutUpdate()
    {
        Canvas.ForceUpdateCanvases();

        RectTransform rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    rectTransform
                );
        }
    }
}