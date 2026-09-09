using UnityEngine;
using UnityEngine.SceneManagement;

public class LiverButton : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private OrganPurchaseController organPurchaseController;

    public void OpenLiverLevel()
    {
        if (organPurchaseController == null)
        {
            Debug.LogWarning("OrganPurchaseController не е свързан.");
            return;
        }

        if (!organPurchaseController.CanEnterLevel())
        {
            return;
        }

        SceneManager.LoadScene("LiverLevel");
    }
}
