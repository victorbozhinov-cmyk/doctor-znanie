using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsLevelNavigation : MonoBehaviour
{
    [Header("Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;

    // =========================================================
    // BODY MAP
    // =========================================================

    public void BackToBodyMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        SettingsNavigation.OpenSettings();
    }

    // =========================================================
    // START -> VIDEO
    // =========================================================

    public void OpenVideoPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
            videoPanel.transform.SetAsLastSibling();
        }
    }
}