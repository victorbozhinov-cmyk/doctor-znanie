using UnityEngine;
using UnityEngine.SceneManagement;

public class StomachButton : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private OrganPurchaseController organPurchaseController;

    public void OpenStomachLevel()
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

        SceneManager.LoadScene("StomachLevel");
    }
}