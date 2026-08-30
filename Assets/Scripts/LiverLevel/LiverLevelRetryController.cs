using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public class LiverLevelRetryController : MonoBehaviour
{
    [Header("Main Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject theoryPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject puzzleSuccessPanel;

    [Header("Minigame")]
    [SerializeField] private GameObject liverMinigameRoot;
    [SerializeField] private GameObject minigameInfoPanel;
    [SerializeField] private GameObject minigamePausePanel;
    [SerializeField] private GameObject minigameGameOverPanel;
    [SerializeField] private GameObject minigameSuccessPanel;

    [Header("Scenes")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    // Запомня се само докато играта работи.
    private static bool startDirectlyAtPuzzle = false;

    private void Start()
    {
        // При нормално влизане в LiverLevel не правим нищо.
        if (!startDirectlyAtPuzzle)
            return;

        // Използваме флага само веднъж.
        startDirectlyAtPuzzle = false;

        Time.timeScale = 1f;

        OpenPuzzleOnly();
    }

    private void OpenPuzzleOnly()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (theoryPanel != null)
            theoryPanel.SetActive(false);

        if (puzzleSuccessPanel != null)
            puzzleSuccessPanel.SetActive(false);

        if (liverMinigameRoot != null)
            liverMinigameRoot.SetActive(false);

        if (minigameInfoPanel != null)
            minigameInfoPanel.SetActive(false);

        if (minigamePausePanel != null)
            minigamePausePanel.SetActive(false);

        if (minigameGameOverPanel != null)
            minigameGameOverPanel.SetActive(false);

        if (minigameSuccessPanel != null)
            minigameSuccessPanel.SetActive(false);

        // Показваме чисто новия пъзел.
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);
    }

    // =========================================================
    // ОПИТАЙ ОТНОВО
    // =========================================================

    public void RetryFromPuzzleStart()
    {
        Time.timeScale = 1f;

        // След reload-а сцената трябва да отвори PuzzlePanel.
        startDirectlyAtPuzzle = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // =========================================================
    // ИЗХОД
    // =========================================================

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(bodyMapSceneName);
    }
}