using UnityEngine;

public class HeartVideoPanelNavigation : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject videoPanel;

    [SerializeField]
    private GameObject puzzlePanel;

    [Header("Puzzle")]
    [SerializeField]
    private HeartPuzzleManager puzzleManager;

    public void ContinueToPuzzle()
    {
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        if (puzzlePanel == null)
        {
            Debug.LogError(
                "Puzzle Panel не е свързан."
            );

            return;
        }

        // Първо активираме PuzzlePanel.
        puzzlePanel.SetActive(true);

        // Всеки път, когато влизаме в пъзела
        // от Start/Video flow-а, започваме
        // чисто нов опит:
        //
        // - възстановяваме животите
        // - махаме старите поставени етикети
        // - отключваме пъзела
        // - избираме нов текущ етикет
        if (puzzleManager != null)
        {
            puzzleManager.RestartPuzzle();
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleManager не е свързан " +
                "в HeartVideoPanelNavigation."
            );
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPuzzleMusic();
        }
    }
}