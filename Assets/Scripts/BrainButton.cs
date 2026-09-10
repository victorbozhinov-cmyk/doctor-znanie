using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainButton : MonoBehaviour
{
    public void OpenBrainLevel()
    {
        SceneManager.LoadScene("BrainLevel");
    }
}
