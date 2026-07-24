using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsButton : MonoBehaviour
{
    public void OpenLungsLevel()
    {
        SceneManager.LoadScene("LungsLevel");
    }
}
