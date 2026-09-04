using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartGameOverPanel : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject gameOverOverlay;

    public void RetryLevel()
    {
        // Скриваме всички останали панели.
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(false);
        }

        // Връщаме се в началния панел.
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        // Връщаме Intro / Lesson музиката,
        // която се използва в Start + Video панелите.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayIntroMusic();
        }
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }
}