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

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        bool shouldReturnToStart =
            PlayerPrefs.GetInt(
                ReturnToStartKey,
                0
            ) == 1;

        if (shouldReturnToStart)
        {
            StartCoroutine(
                ReturnToStartAfterSceneLoaded()
            );
        }
    }

    // =========================================================
    // NORMAL LEVEL START
    // =========================================================

    public void StartLevel()
    {
        SetPanel(startPanel, false);
        SetPanel(videoPanel, true);
    }

    // =========================================================
    // TRY AGAIN
    // =========================================================

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Запомняме, че след reload
        // трябва да отидем в StartPanel.
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

    // =========================================================
    // RETURN TO START AFTER RELOAD
    // =========================================================

    private IEnumerator ReturnToStartAfterSceneLoaded()
    {
        // Изчакваме всички Start() методи
        // в новозаредената сцена да приключат.
        yield return null;

        ShowStartPanel();

        // Флагът вече е използван.
        PlayerPrefs.DeleteKey(
            ReturnToStartKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "StomachLevel: върнато е към StartPanel след Retry."
        );
    }

    // =========================================================
    // SHOW START PANEL
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
    // HELPER
    // =========================================================

    private void SetPanel(
        GameObject panel,
        bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}