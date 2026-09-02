using UnityEngine;

public class WorldResumeButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BrainMinigameManager brainMinigameManager;

    private void OnMouseUpAsButton()
    {
        if (brainMinigameManager == null)
            return;

        brainMinigameManager.ResumeGame();
    }
}