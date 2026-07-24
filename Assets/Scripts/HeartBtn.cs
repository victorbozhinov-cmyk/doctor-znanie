using UnityEngine;
using UnityEngine.SceneManagement;

public class OrganButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void OpenOrganLevel()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Не е зададено име на сцена за този орган.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
