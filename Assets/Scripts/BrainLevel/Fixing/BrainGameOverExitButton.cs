using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainGameOverExitButton : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("BodyMap");
    }
}