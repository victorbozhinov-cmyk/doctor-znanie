using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartButton : MonoBehaviour
{
    public void OpenHeartLevel()
    {
        SceneManager.LoadScene("HeartLevel");
    }
}
