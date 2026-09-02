using UnityEngine;

public class WorldInfoButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject infoPanel;

    [SerializeField]
    private GameObject pausePanel;

    private void OnMouseUpAsButton()
    {
        if (infoPanel == null)
            return;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        infoPanel.SetActive(true);
    }
}
