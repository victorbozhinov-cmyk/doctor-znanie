using UnityEngine;

public class StomachLevelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;

    public void StartLevel()
    {
        startPanel.SetActive(false);
        videoPanel.SetActive(true);
    }
}