using UnityEngine;
using UnityEngine.SceneManagement;

public class LungsButton : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private OrganPurchaseController organPurchaseController;

    public void OpenLungsLevel()
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

        SceneManager.LoadScene("LungsLevel");
    }
}
