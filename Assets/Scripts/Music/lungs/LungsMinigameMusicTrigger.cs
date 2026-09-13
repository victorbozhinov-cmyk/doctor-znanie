using UnityEngine;

public class LungsMinigameMusicTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .PlayLungsMinigameMusic();
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