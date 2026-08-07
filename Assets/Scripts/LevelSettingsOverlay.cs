using UnityEngine;

public class LevelSettingsOverlay : MonoBehaviour
{
    [Header("Settings Overlay")]
    [SerializeField] private GameObject settingsOverlay;

    [SerializeField]
    private UIPopupAnimation settingsPanelAnimation;

    [Header("Difficulty")]
    [SerializeField]
    private DifficultySelector difficultySelector;

    private void Awake()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }
    }

    /*
     * Изпълнява се винаги, когато LevelSettingsWindow
     * стане активен, включително когато е отворен
     * чрез LevelPopupController.
     */
    private void OnEnable()
    {
        LockDifficulty();
    }

    public void OpenSettings()
    {
        if (settingsOverlay == null)
        {
            Debug.LogError(
                "Settings Overlay не е свързан."
            );

            return;
        }

        settingsOverlay.SetActive(true);
        LockDifficulty();
    }

    public void CloseSettings()
    {
        if (settingsOverlay == null)
        {
            return;
        }

        if (settingsPanelAnimation != null)
        {
            settingsPanelAnimation.PlayClose(
                () => settingsOverlay.SetActive(false)
            );
        }
        else
        {
            settingsOverlay.SetActive(false);
        }
    }

    private void LockDifficulty()
    {
        if (difficultySelector == null)
        {
            Debug.LogError(
                "Difficulty Selector не е свързан."
            );

            return;
        }

        difficultySelector.SetLocked(true);
    }
}