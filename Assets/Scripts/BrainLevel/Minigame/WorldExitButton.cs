using UnityEngine;

public class WorldExitButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BrainMinigameManager brainMinigameManager;

    private void OnMouseUpAsButton()
    {
        if (brainMinigameManager == null)
            return;

        brainMinigameManager.OpenExitConfirmation();
    }
}
