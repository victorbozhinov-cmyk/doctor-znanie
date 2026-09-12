using UnityEngine;
using UnityEngine.EventSystems;

public class LiverDropSlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    [SerializeField]
    private LiverFunctionType acceptedFunction;

    [Header("Puzzle Manager")]
    [SerializeField]
    private LiverPuzzleManager puzzleManager;

    private bool occupied = false;

    public void OnDrop(PointerEventData eventData)
    {
        // Ако мястото вече има правилно
        // поставена карта, не приемаме друга.
        if (occupied)
            return;

        // Проверяваме дали реално
        // се влачи нещо.
        if (eventData.pointerDrag == null)
            return;

        LiverPuzzleCard card =
            eventData.pointerDrag
                .GetComponent<LiverPuzzleCard>();

        // Ако не е карта или вече е
        // поставена правилно, не правим нищо.
        if (card == null ||
            card.IsPlacedCorrectly)
        {
            return;
        }

        // =========================
        // ПРАВИЛНА КАТЕГОРИЯ
        // =========================

        if (card.CorrectFunction ==
            acceptedFunction)
        {
            occupied = true;

            card.PlaceCorrectly(transform);

            if (puzzleManager != null)
            {
                puzzleManager
                    .RegisterCorrectCard();
            }
        }

        // =========================
        // ГРЕШНА КАТЕГОРИЯ
        // =========================

        else
        {
            // Първо картата влиза
            // в грешния слот.
            // Там се пуска червената
            // анимация и чак след това
            // се връща обратно.
            card.PlayWrongFeedback(transform);

            // Отнемаме един живот.
            if (puzzleManager != null)
            {
                puzzleManager.LoseLife();
            }
        }
    }
}