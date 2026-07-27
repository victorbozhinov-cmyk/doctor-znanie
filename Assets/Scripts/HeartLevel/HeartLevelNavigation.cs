using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartLevelNavigation : MonoBehaviour
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