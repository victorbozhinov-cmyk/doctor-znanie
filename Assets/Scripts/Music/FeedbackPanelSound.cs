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
        PlayFeedbackSound();
    }

    private void PlayFeedbackSound()
    {
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