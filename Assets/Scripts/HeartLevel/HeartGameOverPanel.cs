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
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }
}