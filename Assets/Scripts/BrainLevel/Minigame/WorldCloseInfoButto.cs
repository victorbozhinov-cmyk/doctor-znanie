using UnityEngine;

public class WorldCloseInfoButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BrainMinigameManager brainMinigameManager;

    private void OnMouseUpAsButton()
    {
        if (brainMinigameManager == null)
            return;

        brainMinigameManager.CloseInfoPanel();
    }
}
