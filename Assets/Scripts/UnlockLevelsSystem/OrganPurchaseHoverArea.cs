using UnityEngine;
using UnityEngine.EventSystems;

public class OrganPurchaseHoverArea : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public enum AreaType
    {
        Organ,
        PurchasePanel
    }

    [Header("Hover")]
    [SerializeField] private AreaType areaType;

    [SerializeField] private OrganPurchaseHover hoverManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverManager == null)
        {
            return;
        }

        if (areaType == AreaType.Organ)
        {
            hoverManager.EnterOrgan();
        }
        else
        {
            hoverManager.EnterPanel();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverManager == null)
        {
            return;
        }

        if (areaType == AreaType.Organ)
        {
            hoverManager.ExitOrgan();
        }
        else
        {
            hoverManager.ExitPanel();
        }
    }
}
