using UnityEngine;

public class WorldContinueToBrainQuizButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject brainMinigameWorld;

    [SerializeField]
    private GameObject successOverlay;

    [SerializeField]
    private GameObject quizPanel;

    private void OnMouseUpAsButton()
    {
        // Минииграта при победа спира времето.
        // Връщаме го преди да преминем към куиза.
        Time.timeScale = 1f;

        if (successOverlay != null)
        {
            successOverlay.SetActive(false);
        }

        if (brainMinigameWorld != null)
        {
            brainMinigameWorld.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "QuizPanel не е свързан в WorldContinueToBrainQuizButton."
            );
        }
    }
}