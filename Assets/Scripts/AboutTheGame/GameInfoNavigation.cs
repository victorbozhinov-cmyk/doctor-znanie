using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInfoNavigation : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}