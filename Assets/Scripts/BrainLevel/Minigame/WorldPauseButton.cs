using UnityEngine;

public class WorldPauseButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BrainMinigameManager brainMinigameManager;

    private void OnMouseUpAsButton()
    {
        if (brainMinigameManager == null)
            return;

        brainMinigameManager.PauseGame();
    }
}
