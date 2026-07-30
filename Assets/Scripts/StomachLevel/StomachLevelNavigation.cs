using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachLevelNavigation : MonoBehaviour
{
    public void BackToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    public void OpenSettings()
    {
        SettingsNavigation.OpenSettings();
    }
}