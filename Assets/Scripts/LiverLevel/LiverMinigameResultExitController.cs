using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverMinigameResultExitController : MonoBehaviour
{
    [Header("Exit Confirmation")]
    [SerializeField]
    private GameObject exitConfirmationPanel;

    // =========================================================
    // OPEN EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (exitConfirmationPanel == null)
        {
            Debug.LogError(
                "Exit Confirmation Panel не е зададен!"
            );

            return;
        }

        exitConfirmationPanel.SetActive(true);
    }

    // =========================================================
    // CLOSE EXIT CONFIRMATION
    // =========================================================

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }
    }

    // =========================================================
    // CONFIRM EXIT
    // =========================================================

    public void ConfirmExitToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }
}