using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachButton : MonoBehaviour
{
    public void OpenStomachLevel()
    {
        SceneManager.LoadScene("StomachLevel");
    }
}