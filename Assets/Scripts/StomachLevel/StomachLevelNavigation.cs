using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachLevelNavigation : MonoBehaviour
{
    [Header("Level Panels")]
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private GameObject finishPanel;

    // =========================================================
    // BODY MAP
    // =========================================================

    public void BackToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "BodyMap"
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        SettingsNavigation.OpenSettings();
    }

    // =========================================================
    // FINISH PANEL
    // =========================================================

    public void OpenFinishPanel()
    {
        Time.timeScale = 1f;

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);

            finishPanel.transform
                .SetAsLastSibling();
        }
    }
}