using UnityEngine;

public class OrganPurchaseController : MonoBehaviour
{
    public enum OrganType
    {
        Heart,
        Liver,
        Lungs,
        Stomach,
        Brain
    }

    [Header("Organ")]
    [SerializeField] private OrganType organType;

    [Header("Price")]
    [SerializeField] private int price = 100;

    [Header("UI")]
    [SerializeField] private GameObject purchasePanel;
    [SerializeField] private UIPopupCloseAnimation purchasePanelCloseAnimation;

    [Header("Visual")]
    [SerializeField] private OrganLockVisual organLockVisual;
    [SerializeField] private OrganUnlockAnimation organUnlockAnimation;

    private bool isClosingPanel;

    public void OpenPanel()
    {
        if (IsUnlocked())
        {
            return;
        }

        if (purchasePanel == null)
        {
            return;
        }

        if (isClosingPanel)
        {
            return;
        }

        purchasePanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (purchasePanel == null)
        {
            return;
        }

        if (!purchasePanel.activeSelf)
        {
            return;
        }

        if (isClosingPanel)
        {
            return;
        }

        if (purchasePanelCloseAnimation != null)
        {
            isClosingPanel = true;

            purchasePanelCloseAnimation.PlayClose(() =>
            {
                purchasePanel.SetActive(false);
                isClosingPanel = false;
            });
        }
        else
        {
            purchasePanel.SetActive(false);
        }
    }

    public void TryUnlock()
    {
        if (IsUnlocked())
        {
            ClosePanel();
            return;
        }

        if (VitaminManager.Instance == null)
        {
            Debug.LogWarning("VitaminManager не е намерен.");
            return;
        }

        if (!VitaminManager.Instance.SpendVitamins(price))
        {
            Debug.Log("Няма достатъчно витамини.");
            return;
        }

        UnlockOrgan();

        ClosePanel();

        if (organUnlockAnimation != null)
        {
            organUnlockAnimation.PlayUnlockAnimation();
        }
        else if (organLockVisual != null)
        {
            organLockVisual.RefreshVisual();
        }
    }

    public bool CanEnterLevel()
    {
        return IsUnlocked();
    }

    private bool IsUnlocked()
    {
        if (OrganUnlockManager.Instance == null)
        {
            return false;
        }

        switch (organType)
        {
            case OrganType.Heart:
                return OrganUnlockManager.Instance.IsHeartUnlocked();

            case OrganType.Liver:
                return OrganUnlockManager.Instance.IsLiverUnlocked();

            case OrganType.Lungs:
                return OrganUnlockManager.Instance.IsLungsUnlocked();

            case OrganType.Stomach:
                return OrganUnlockManager.Instance.IsStomachUnlocked();

            case OrganType.Brain:
                return OrganUnlockManager.Instance.IsBrainUnlocked();

            default:
                return false;
        }
    }

    private void UnlockOrgan()
    {
        if (OrganUnlockManager.Instance == null)
        {
            return;
        }

        switch (organType)
        {
            case OrganType.Heart:
                OrganUnlockManager.Instance.UnlockHeart();
                break;

            case OrganType.Liver:
                OrganUnlockManager.Instance.UnlockLiver();
                break;

            case OrganType.Lungs:
                OrganUnlockManager.Instance.UnlockLungs();
                break;

            case OrganType.Stomach:
                OrganUnlockManager.Instance.UnlockStomach();
                break;

            case OrganType.Brain:
                OrganUnlockManager.Instance.UnlockBrain();
                break;
        }
    }
}