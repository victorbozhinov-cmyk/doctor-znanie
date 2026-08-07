using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverIntroController : MonoBehaviour
{
    [Header("Liver Level Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject nextPanel;

    [Header("Scene Names")]
    [SerializeField] private string bodyMapSceneName = "BodyMap";
    [SerializeField] private string settingsSceneName = "SettingsMenu";

    public void StartLiverLevel()
    {
        if (introPanel == null || nextPanel == null)
        {
            Debug.LogError("Intro Panel или Next Panel не е зададен!");
            return;
        }

        introPanel.SetActive(false);
        nextPanel.SetActive(true);
    }

    public void BackToBodyMap()
    {
        SceneManager.LoadScene(bodyMapSceneName);
    }

    public void OpenSettings()
    {
        PlayerPrefs.SetString(
            "SettingsReturnScene",
            SceneManager.GetActiveScene().name
        );

        PlayerPrefs.Save();
        SceneManager.LoadScene(settingsSceneName);
    }
}