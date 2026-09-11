using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsLevelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("SettingsPanel не е зададен в LungsLevelManager.");
        }
    }

    public void BackToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void GoToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }
}