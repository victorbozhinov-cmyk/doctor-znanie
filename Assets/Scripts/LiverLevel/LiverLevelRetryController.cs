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

    // Използва се само при Retry.
    // След презареждането казва на сцената
    // да отвори директно StartPanel.
    private static bool returnToStartAfterReload = false;

    private void Start()
    {
        // При нормално влизане в LiverLevel
        // не променяме нищо.
        if (!returnToStartAfterReload)
            return;

        // Използваме флага само веднъж.
        returnToStartAfterReload = false;

        // Ако минииграта е била паузирана,
        // връщаме нормалното време.
        Time.timeScale = 1f;

        OpenStartOnly();
    }

    // =========================================================
    // START PANEL
    // =========================================================

    private void OpenStartOnly()
    {
        // Показваме началния панел.
        if (startPanel != null)
            startPanel.SetActive(true);

        // Скриваме теорията.
        if (theoryPanel != null)
            theoryPanel.SetActive(false);

        // Скриваме пъзела.
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (puzzleSuccessPanel != null)
            puzzleSuccessPanel.SetActive(false);

        // Спираме цялата миниигра.
        if (liverMinigameRoot != null)
            liverMinigameRoot.SetActive(false);

        // Скриваме всички допълнителни
        // панели на минииграта.
        if (minigameInfoPanel != null)
            minigameInfoPanel.SetActive(false);

        if (minigamePausePanel != null)
            minigamePausePanel.SetActive(false);

        if (minigameGameOverPanel != null)
            minigameGameOverPanel.SetActive(false);

        if (minigameSuccessPanel != null)
            minigameSuccessPanel.SetActive(false);
    }

    // =========================================================
    // RETRY - PUZZLE
    // =========================================================

    public void RetryFromPuzzleStart()
    {
        RetryToLevelStart();
    }

    // =========================================================
    // RETRY - MINIGAME
    // =========================================================

    public void RetryFromMinigameStart()
    {
        RetryToLevelStart();
    }

    // =========================================================
    // ОБЩ RETRY
    // =========================================================

    private void RetryToLevelStart()
    {
        // Ако Game Over е спрял времето,
        // връщаме го преди reload.
        Time.timeScale = 1f;

        // След reload-а искаме StartPanel.
        returnToStartAfterReload = true;

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

        // За всеки случай изчистваме флага.
        returnToStartAfterReload = false;

        SceneManager.LoadScene(bodyMapSceneName);
    }
}