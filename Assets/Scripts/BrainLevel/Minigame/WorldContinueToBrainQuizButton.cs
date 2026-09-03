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
        if (brainMinigameWorld != null)
        {
            brainMinigameWorld.SetActive(false);
        }

        if (successOverlay != null)
        {
            successOverlay.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
    }
}
