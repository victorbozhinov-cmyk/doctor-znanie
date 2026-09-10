using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevelRewardButton : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private OrganVitaminRewardManager vitaminRewardManager;

    [Header("Navigation")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    public void ClaimRewardAndExit()
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

        SceneManager.LoadScene(bodyMapSceneName);
    }
}
