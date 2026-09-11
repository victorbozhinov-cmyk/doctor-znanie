using UnityEngine;

public class LungsPuzzlePopupController : MonoBehaviour
{
    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmationPanel;

    public void OpenExitConfirmation()
    {
        if (exitConfirmationPanel == null)
            return;

        exitConfirmationPanel.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel == null)
            return;

        exitConfirmationPanel.SetActive(false);
    }
}