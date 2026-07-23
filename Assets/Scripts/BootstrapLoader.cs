using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    private void Start()
    {
        SceneManager.LoadScene(firstScene);
    }
}