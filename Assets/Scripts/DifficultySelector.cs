using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    private const string DifficultyKey = "Difficulty";

    [Header("UI Elements")]
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private Image[] indicators = new Image[3];

    [Header("Difficulty Background")]
    [SerializeField] private Image selectorBackground;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite lockedBackground;

    [Header("Difficulty Arrows")]
    [SerializeField] private GameObject previousArrow;
    [SerializeField] private GameObject nextArrow;

    [Header("Indicator Sprites")]
    [SerializeField] private Sprite inactiveIndicator;
    [SerializeField] private Sprite activeIndicator;

    [SerializeField]
    private Vector2 inactiveSize =
        new Vector2(36f, 35f);

    [SerializeField]
    private Vector2 activeSize =
        new Vector2(80f, 80f);

    [SerializeField]
    private float inactivePositionY = -185.3f;

    [SerializeField]
    private float activePositionY = -205.9f;

    private readonly string[] difficultyNames =
    {
        "ЛЕСНО",
        "СРЕДНО",
        "ТРУДНО"
    };

    // 0 = лесно, 1 = средно, 2 = трудно
    private int currentDifficulty;

    private bool difficultyLocked;

    private void Start()
    {
        LoadDifficulty();

        UpdateUI();
        UpdateDifficultyLockUI();
    }

    private void LoadDifficulty()
    {
        currentDifficulty =
            PlayerPrefs.GetInt(DifficultyKey, 1);

        currentDifficulty =
            Mathf.Clamp(currentDifficulty, 0, 2);
    }

    public void SetLocked(bool locked)
    {
        difficultyLocked = locked;

        UpdateDifficultyLockUI();
    }

    public void PreviousDifficulty()
    {
        if (difficultyLocked)
        {
            return;
        }

        currentDifficulty--;

        if (currentDifficulty < 0)
        {
            currentDifficulty =
                difficultyNames.Length - 1;
        }

        SaveAndUpdate();
    }

    public void NextDifficulty()
    {
        if (difficultyLocked)
        {
            return;
        }

        currentDifficulty++;

        if (currentDifficulty >=
            difficultyNames.Length)
        {
            currentDifficulty = 0;
        }

        SaveAndUpdate();
    }

    private void SaveAndUpdate()
    {
        PlayerPrefs.SetInt(
            DifficultyKey,
            currentDifficulty
        );

        PlayerPrefs.Save();

        UpdateUI();
    }

    private void UpdateDifficultyLockUI()
    {
        if (previousArrow != null)
        {
            previousArrow.SetActive(
                !difficultyLocked
            );
        }

        if (nextArrow != null)
        {
            nextArrow.SetActive(
                !difficultyLocked
            );
        }

        if (selectorBackground != null)
        {
            Sprite selectedBackground =
                difficultyLocked
                    ? lockedBackground
                    : normalBackground;

            if (selectedBackground != null)
            {
                selectorBackground.sprite =
                    selectedBackground;
            }
        }
    }

    private void UpdateUI()
    {
        if (difficultyText != null)
        {
            difficultyText.text =
                difficultyNames[currentDifficulty];
        }

        for (int i = 0;
             i < indicators.Length;
             i++)
        {
            if (indicators[i] == null)
            {
                continue;
            }

            bool isActive =
                i == currentDifficulty;

            indicators[i].sprite =
                isActive
                    ? activeIndicator
                    : inactiveIndicator;

            indicators[i]
                .rectTransform
                .sizeDelta =
                isActive
                    ? activeSize
                    : inactiveSize;

            Vector2 position =
                indicators[i]
                    .rectTransform
                    .anchoredPosition;

            position.y =
                isActive
                    ? activePositionY
                    : inactivePositionY;

            indicators[i]
                .rectTransform
                .anchoredPosition =
                position;
        }
    }

    public static int GetSavedDifficulty()
    {
        return PlayerPrefs.GetInt(
            DifficultyKey,
            1
        );
    }
}