using UnityEngine;

public class PuzzleMusicTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPuzzleMusic();
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