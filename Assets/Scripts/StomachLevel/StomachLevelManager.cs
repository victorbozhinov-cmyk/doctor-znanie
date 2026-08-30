using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachLevelManager : MonoBehaviour
{
    private const string ReturnToStartKey =
        "StomachReturnToStartAfterReload";

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    private void Start()
    {
        bool shouldReturnToStart =
            PlayerPrefs.GetInt(ReturnToStartKey, 0) == 1;

        if (shouldReturnToStart)
        {
            StartCoroutine(
                ReturnToStartAfterSceneLoaded()
            );
        }
    }

    // =========================================================
    // START LEVEL
    // =========================================================

    public void StartLevel()
    {
        SetPanel(startPanel, false);
        SetPanel(videoPanel, true);
    }

    // =========================================================
    // QUIZ -> PUZZLE
    // =========================================================

    public void GoFromQuizToPuzzle()
    {
        SetPanel(quizPanel, false);
        SetPanel(puzzlePanel, true);

        Debug.Log(
            "Преминаване от Quiz към Puzzle Welcome."
        );
    }

    // =========================================================
    // RESTART LEVEL
    // =========================================================

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt(
            ReturnToStartKey,
            1
        );

        PlayerPrefs.Save();

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    private IEnumerator ReturnToStartAfterSceneLoaded()
    {
        yield return null;

        ShowStartPanel();

        PlayerPrefs.DeleteKey(
            ReturnToStartKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "StomachLevel: върнато е към StartPanel след Retry."
        );
    }

    // =========================================================
    // START PANEL
    // =========================================================

    private void ShowStartPanel()
    {
        SetPanel(startPanel, true);
        SetPanel(videoPanel, false);
        SetPanel(puzzlePanel, false);
        SetPanel(miniGamePanel, false);
        SetPanel(quizPanel, false);
        SetPanel(finishPanel, false);
    }

    // =========================================================
    // PANEL HELPER
    // =========================================================

    private void SetPanel(
        GameObject panel,
        bool active
    )
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}