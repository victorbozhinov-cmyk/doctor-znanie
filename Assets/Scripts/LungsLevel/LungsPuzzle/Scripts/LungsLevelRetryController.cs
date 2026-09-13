using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public class LungsLevelRetryController : MonoBehaviour
{
    [Header("Main Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    private const string ReturnToStartKey =
        "LungsReturnToStartAfterReload";

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (PlayerPrefs.GetInt(
                ReturnToStartKey,
                0
            ) != 1)
        {
            return;
        }

        PlayerPrefs.DeleteKey(
            ReturnToStartKey
        );

        PlayerPrefs.Save();

        OpenStartPanel();
    }

    // =========================================================
    // RETRY WHOLE LEVEL
    // =========================================================

    public void RestartLevelFromStart()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt(
            ReturnToStartKey,
            1
        );

        PlayerPrefs.Save();

        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .name
        );
    }

    // =========================================================
    // OPEN START
    // =========================================================

    private void OpenStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

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
    }
}