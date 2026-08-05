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

        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }
        else
        {
            Debug.LogError(
                "Difficulty Selector не е свързан."
            );
        }
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
}