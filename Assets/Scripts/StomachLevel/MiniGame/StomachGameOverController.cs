using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachGameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StomachMinigameTimer totalTimer;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scenes")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";

    private bool gameOverShown;

    // =========================================================
    // UNITY
    // =========================================================

    private void OnEnable()
    {
        if (totalTimer != null)
        {
            totalTimer.TotalTimerExpired +=
                OnTotalTimerExpired;
        }
    }

    private void OnDisable()
    {
        if (totalTimer != null)
        {
            totalTimer.TotalTimerExpired -=
                OnTotalTimerExpired;
        }
    }

    private void Start()
    {
        // Винаги започваме с нормално време.
        Time.timeScale = 1f;

        gameOverShown = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // =========================================================
    // TIME EXPIRED
    // =========================================================

    private void OnTotalTimerExpired()
    {
        if (gameOverShown)
            return;

        gameOverShown = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Уверяваме се, че панелът е най-отгоре.
            gameOverPanel.transform.SetAsLastSibling();
        }

        // Замразява цялата текуща миниигра:
        // indicator, marker, task timers, анимации и т.н.
        Time.timeScale = 0f;
    }

    // =========================================================
    // TRY AGAIN
    // =========================================================

    public void TryAgain()
    {
        // Много важно:
        // Time.timeScale се запазва и при смяна на сцена.
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    // =========================================================
    // EXIT
    // =========================================================

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            bodyMapSceneName
        );
    }
}