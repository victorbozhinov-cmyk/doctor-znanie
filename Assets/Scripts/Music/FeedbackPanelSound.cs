using UnityEngine;

public class FeedbackPanelSound : MonoBehaviour
{
    public enum FeedbackType
    {
        Success,
        GameOver
    }

    [Header("Feedback Type")]
    [SerializeField]
    private FeedbackType feedbackType;

    private void OnEnable()
    {
        PlayFeedback();
    }

    private void PlayFeedback()
    {
        // =============================================
        // BACKGROUND MUSIC
        // =============================================

        // Всеки Success / Game Over панел
        // преминава към Lobby / Main Menu музиката.
        //
        // След това Feedback SFX автоматично
        // ще я duck-не за продължителността си.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayLobbyMusic();
        }
        else
        {
            Debug.LogWarning(
                "MusicManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }

        // =============================================
        // FEEDBACK SOUND
        // =============================================

        if (GameFeedbackSoundManager.Instance == null)
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );

            return;
        }

        switch (feedbackType)
        {
            case FeedbackType.Success:

                GameFeedbackSoundManager
                    .Instance
                    .PlaySuccess();

                break;

            case FeedbackType.GameOver:

                GameFeedbackSoundManager
                    .Instance
                    .PlayGameOver();

                break;
        }
    }
}