using UnityEngine;

public class LevelSettingsNavigation : MonoBehaviour
{
    private const string LockDifficultyKey =
        "LockDifficultyInSettings";

    public void OpenSettingsFromLevel()
    {
        PlayerPrefs.SetInt(LockDifficultyKey, 1);
        PlayerPrefs.Save();

        SettingsNavigation.OpenSettings();
    }
}
