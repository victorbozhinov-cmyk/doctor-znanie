using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    private const string DifficultyKey = "Difficulty";

    private const string LockDifficultyKey =
        "LockDifficultyInSettings";

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
        LoadLockState();

        UpdateUI();
        UpdateDifficultyLockUI();

        // Флагът важи само за текущото
        // отваряне на SettingsMenu.
        PlayerPrefs.DeleteKey(LockDifficultyKey);
        PlayerPrefs.Save();
    }

    private void LoadDifficulty()
    {
        currentDifficulty =
            PlayerPrefs.GetInt(DifficultyKey, 1);

        currentDifficulty =
            Mathf.Clamp(currentDifficulty, 0, 2);
    }

    private void LoadLockState()
    {
        difficultyLocked =
            PlayerPrefs.GetInt(
                LockDifficultyKey,
                0
            ) == 1;

        Debug.Log(
            $"Difficulty locked: {difficultyLocked}"
        );
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
        // Скриваме истинските бутони.
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

        // Сменяме общото изображение на контейнера.
        if (selectorBackground != null)
        {
            selectorBackground.sprite =
                difficultyLocked
                    ? lockedBackground
                    : normalBackground;
        }
        else
        {
            Debug.LogError(
                "Selector Background не е свързан."
            );
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