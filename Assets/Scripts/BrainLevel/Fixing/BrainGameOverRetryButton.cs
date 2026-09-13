using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainGameOverRetryButton : MonoBehaviour
{
    private const string ReturnToStartKey =
        "BrainReturnToStartAfterReload";

    private void OnMouseUpAsButton()
    {
        Time.timeScale = 1f;

        // Казваме на сцената, че след reload
        // трябва да отвори StartPanel.
        PlayerPrefs.SetInt(
            ReturnToStartKey,
            1
        );

        PlayerPrefs.Save();

        // Презареждаме цялото BrainLevel.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}