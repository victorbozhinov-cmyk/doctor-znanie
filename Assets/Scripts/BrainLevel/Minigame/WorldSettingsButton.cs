using UnityEngine;

public class WorldSettingsButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BrainMinigameManager brainMinigameManager;

    private void OnMouseUpAsButton()
    {
        if (brainMinigameManager == null)
            return;

        brainMinigameManager.OpenSettingsPanel();
    }
}