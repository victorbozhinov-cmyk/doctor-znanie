using UnityEngine;

public class QuizMusicTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayQuizMusic();
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