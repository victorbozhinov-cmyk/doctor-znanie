using UnityEngine;

public class LiverMinigameMusicTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .PlayLiverMinigameMusic();
        }
        else
        {
            Debug.LogWarning(
                "MusicManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }
    }
}