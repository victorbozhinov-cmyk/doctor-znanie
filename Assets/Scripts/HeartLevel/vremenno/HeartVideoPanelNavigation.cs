using UnityEngine;

public class HeartVideoPanelNavigation : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;

    public void ContinueToPuzzle()
    {
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPuzzleMusic();
        }
    }
}