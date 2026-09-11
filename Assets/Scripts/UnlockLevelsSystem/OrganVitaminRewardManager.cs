using UnityEngine;

public class OrganVitaminRewardManager : MonoBehaviour
{
    public enum OrganType
    {
        Heart,
        Liver,
        Lungs,
        Stomach,
        Brain
    }

    [Header("Organ")]
    [SerializeField] private OrganType organType;

    [Header("Rewards")]
    [SerializeField] private int easyReward = 100;
    [SerializeField] private int mediumReward = 130;
    [SerializeField] private int hardReward = 150;

    public int TryGiveReward()
    {
        if (VitaminManager.Instance == null)
        {
            Debug.LogWarning("VitaminManager не е намерен.");
            return 0;
        }

        int difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        string rewardKey =
            GetRewardKey(difficulty);

        if (PlayerPrefs.GetInt(
                rewardKey,
                0
            ) == 1)
        {
            Debug.Log(
                "Наградата за този орган и тази трудност " +
                "вече е получена."
            );

            return 0;
        }

        int rewardAmount =
            GetRewardAmount(difficulty);

        VitaminManager.Instance.AddVitamins(
            rewardAmount
        );

        PlayerPrefs.SetInt(
            rewardKey,
            1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Получени витамини: " +
            rewardAmount
        );

        return rewardAmount;
    }

    // =========================================================
    // PREVIEW / DISPLAY
    // =========================================================

    public int GetAvailableReward()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        string rewardKey =
            GetRewardKey(difficulty);

        bool rewardAlreadyClaimed =
            PlayerPrefs.GetInt(
                rewardKey,
                0
            ) == 1;

        if (rewardAlreadyClaimed)
        {
            return 0;
        }

        return GetRewardAmount(
            difficulty
        );
    }

    public bool IsRewardAlreadyClaimed()
    {
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        string rewardKey =
            GetRewardKey(difficulty);

        return PlayerPrefs.GetInt(
            rewardKey,
            0
        ) == 1;
    }

    // =========================================================
    // REWARD AMOUNT
    // =========================================================

    private int GetRewardAmount(
        int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                return easyReward;

            case 1:
                return mediumReward;

            case 2:
                return hardReward;

            default:
                return mediumReward;
        }
    }

    // =========================================================
    // PLAYER PREFS KEY
    // =========================================================

    private string GetRewardKey(
        int difficulty)
    {
        string difficultyName;

        switch (difficulty)
        {
            case 0:
                difficultyName = "Easy";
                break;

            case 1:
                difficultyName = "Medium";
                break;

            case 2:
                difficultyName = "Hard";
                break;

            default:
                difficultyName = "Medium";
                break;
        }

        return organType.ToString()
            + "_"
            + difficultyName
            + "_VitaminRewardClaimed";
    }
}