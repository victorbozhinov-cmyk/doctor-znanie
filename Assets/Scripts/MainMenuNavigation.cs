using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField] private UIPopupAnimation popupAnimation;

    public void BackWithAnimation()
    {
        popupAnimation.PlayClose(() =>
        {
            SettingsNavigation.GoBack();
        });
    }

    public void OpenBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    public void OpenSettings()
    {
        SettingsNavigation.OpenSettings();
    }

    public void OpenGameInfo()
    {
        SceneManager.LoadScene("GameInfo");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}