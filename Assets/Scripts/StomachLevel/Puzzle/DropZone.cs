using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Correct Card")]
    [SerializeField] private CardType requiredCardType;

    public DraggableCard CurrentCard { get; private set; }

    public bool IsOccupied => CurrentCard != null;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        DraggableCard card =
            eventData.pointerDrag.GetComponent<DraggableCard>();

        if (card == null)
        {
            return;
        }

        card.SetHoveredDropZone(this);
    }

    public bool CanAcceptCard(DraggableCard card)
    {
        return card != null &&
               (!IsOccupied || CurrentCard == card);
    }

    public void SetCurrentCard(DraggableCard card)
    {
        CurrentCard = card;
    }

    public void Clear()
    {
        CurrentCard = null;
    }

    public bool IsCorrect()
    {
        return CurrentCard != null &&
               CurrentCard.CardType == requiredCardType;
    }
}