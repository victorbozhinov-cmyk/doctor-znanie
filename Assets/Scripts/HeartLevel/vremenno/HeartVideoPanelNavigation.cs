using UnityEngine;

public class HeartVideoPanelNavigation : MonoBehaviour
{
    [Header("Level Manager")]
    [SerializeField]
    private HeartLevelManager levelManager;

    public void ContinueToPuzzle()
    {
        if (levelManager == null)
        {
            Debug.LogError(
                "HeartLevelManager не е свързан " +
                "в HeartVideoPanelNavigation."
            );

            return;
        }

        levelManager.ShowPuzzlePanel();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPuzzleMusic();
        }
    }
}