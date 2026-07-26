using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingsNavigation
{
    private static string previousSceneName = "MainMenu";

    public static void OpenSettings()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Не позволяваме SettingsMenu да запише себе си като предишна сцена.
        if (currentSceneName != "SettingsMenu")
        {
            previousSceneName = currentSceneName;
        }

        SceneManager.LoadScene("SettingsMenu");
    }

    public static void GoBack()
    {
        // Резервна защита, ако името е празно.
        if (string.IsNullOrEmpty(previousSceneName))
        {
            previousSceneName = "MainMenu";
        }

        SceneManager.LoadScene(previousSceneName);
    }
}
