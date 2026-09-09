using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartButton : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private OrganPurchaseController organPurchaseController;

    public void OpenHeartLevel()
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

        SceneManager.LoadScene("HeartLevel");
    }
}
