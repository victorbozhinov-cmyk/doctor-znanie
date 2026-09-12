using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevelRewardButton : MonoBehaviour
{
    public enum LevelOrgan
    {
        Heart,
        Stomach,
        Brain
    }

    // =====================================================
    // ORGAN
    // =====================================================

    [Header("Level Organ")]
    [SerializeField] private LevelOrgan levelOrgan;

    // =====================================================
    // VITAMIN REWARD
    // =====================================================

    [Header("Reward")]
    [SerializeField] private OrganVitaminRewardManager vitaminRewardManager;

    // =====================================================
    // NAVIGATION
    // =====================================================

    [Header("Navigation")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    // =====================================================
    // CLAIM REWARD
    // =====================================================

    public void ClaimRewardAndExit()
    {
        GiveVitaminReward();

        SaveAntibodyScore();

        SceneManager.LoadScene(bodyMapSceneName);
    }

    // =====================================================
    // VITAMINS
    // =====================================================

    private void GiveVitaminReward()
    {
        if (vitaminRewardManager != null)
        {
            int earnedVitamins =
                vitaminRewardManager.TryGiveReward();

            Debug.Log(
                "Получени витамини при приключване на нивото: "
                + earnedVitamins
            );
        }
        else
        {
            Debug.LogWarning(
                "OrganVitaminRewardManager не е свързан."
            );
        }
    }

    // =====================================================
    // ANTIBODIES
    // =====================================================

    private void SaveAntibodyScore()
    {
        switch (levelOrgan)
        {
            case LevelOrgan.Heart:
                SaveHeartScore();
                break;

            case LevelOrgan.Stomach:
                SaveStomachScore();
                break;

            case LevelOrgan.Brain:
                SaveBrainScore();
                break;

            default:
                Debug.LogWarning(
                    "Няма зададен орган за Antibody Score."
                );
                break;
        }
    }

    // =====================================================
    // HEART
    // =====================================================

    private void SaveHeartScore()
    {
        if (HeartScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "HeartScoreManager.Instance липсва. " +
                "Heart антителата не могат да бъдат записани."
            );

            return;
        }

        HeartScoreManager.Instance.FinishHeartLevel();

        Debug.Log(
            "Heart Current Score: "
            + HeartScoreManager.Instance.CurrentScore
        );

        if (HeartScoreManager.Instance.IsNewBest)
        {
            Debug.Log(
                "Нов Heart Best Score!"
            );
        }
        else
        {
            Debug.Log(
                "Heart Best Score не е подобрен."
            );
        }
    }

    // =====================================================
    // STOMACH
    // =====================================================

    private void SaveStomachScore()
    {
        if (StomachScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "StomachScoreManager.Instance липсва. " +
                "Stomach антителата не могат да бъдат записани."
            );

            return;
        }

        StomachScoreManager.Instance.FinishStomachLevel();

        Debug.Log(
            "Stomach Current Score: "
            + StomachScoreManager.Instance.CurrentScore
        );

        if (StomachScoreManager.Instance.IsNewBest)
        {
            Debug.Log(
                "Нов Stomach Best Score!"
            );
        }
        else
        {
            Debug.Log(
                "Stomach Best Score не е подобрен."
            );
        }
    }

    // =====================================================
    // BRAIN
    // =====================================================

    private void SaveBrainScore()
    {
        if (BrainScoreManager.Instance == null)
        {
            Debug.LogWarning(
                "BrainScoreManager.Instance липсва. " +
                "Brain антителата не могат да бъдат записани."
            );

            return;
        }

        BrainScoreManager.Instance.FinishBrainLevel();

        Debug.Log(
            "Brain Current Score: "
            + BrainScoreManager.Instance.CurrentScore
        );

        if (BrainScoreManager.Instance.IsNewBest)
        {
            Debug.Log(
                "Нов Brain Best Score!"
            );
        }
        else
        {
            Debug.Log(
                "Brain Best Score не е подобрен."
            );
        }
    }
}