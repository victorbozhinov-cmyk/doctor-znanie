using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainButton : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private OrganPurchaseController organPurchaseController;

    public void OpenBrainLevel()
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

        SceneManager.LoadScene("BrainLevel");
    }
}
