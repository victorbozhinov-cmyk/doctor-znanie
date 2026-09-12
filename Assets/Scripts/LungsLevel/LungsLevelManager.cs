using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsLevelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [Header("Puzzle To Minigame")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject minigameWelcomePanel;

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
    public void GoToMinigame()
    {
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }

        if (minigameWelcomePanel != null)
        {
            minigameWelcomePanel.SetActive(true);
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