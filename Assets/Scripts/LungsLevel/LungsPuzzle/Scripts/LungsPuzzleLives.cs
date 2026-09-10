using UnityEngine;
using UnityEngine.UI;

public class LungsPuzzleLives : MonoBehaviour
{
    [Header("Life Hearts")]
    [SerializeField] private GameObject[] lifeHearts;

    private int currentLives;
    private int maxLives;

    public int CurrentLives => currentLives;

    public void ResetLives()
    {
        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        difficulty =
            Mathf.Clamp(difficulty, 0, 2);

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
        }

        currentLives = maxLives;

        UpdateLivesUI();
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
            return;

        currentLives--;

        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        if (lifeHearts == null)
            return;

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            GameObject heart = lifeHearts[i];

            if (heart == null)
            {
                Debug.LogWarning(
                    "LungsPuzzleLives: Life " +
                    (i + 1) +
                    " не е свързан."
                );

                continue;
            }

            heart.SetActive(i < currentLives);
        }

        RectTransform panelRect =
            transform as RectTransform;

        if (panelRect != null)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    panelRect
                );
        }
    }
}