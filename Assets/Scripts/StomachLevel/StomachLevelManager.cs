using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachLevelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    // Става true САМО когато сме натиснали
    // "Опитай отново".
    private static bool returnToStartAfterReload = false;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        // При обикновено Play НЕ пипаме панелите.
        // Запазваме точно това, което си настроил
        // в сцената.

        if (returnToStartAfterReload)
        {
            returnToStartAfterReload = false;

            ShowStartPanel();
        }
    }

    // =========================================================
    // NORMAL LEVEL START
    // =========================================================

    public void StartLevel()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (videoPanel != null)
            videoPanel.SetActive(true);
    }

    // =========================================================
    // TRY AGAIN
    // =========================================================

    public void RestartLevel()
    {
        // Game Over е замразил играта.
        Time.timeScale = 1f;

        // Казваме на следващото зареждане,
        // че този път искаме StartPanel.
        returnToStartAfterReload = true;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
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