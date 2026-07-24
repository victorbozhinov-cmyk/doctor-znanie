using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverButton : MonoBehaviour
{
    public void OpenLiverLevel()
    {
        SceneManager.LoadScene("LiverLevel");
    }
}
