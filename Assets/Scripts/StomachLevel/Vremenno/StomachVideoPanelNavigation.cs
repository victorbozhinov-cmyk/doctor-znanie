using UnityEngine;

public class StomachVideoPanelNavigation : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject quizPanel;

    public void ContinueToQuiz()
    {
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayQuizMusic();
        }
    }
}