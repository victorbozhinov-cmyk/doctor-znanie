using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LungsPuzzleManager : MonoBehaviour
{
    private enum ItemType
    {
        Part,
        Label
    }

    private class PuzzleItem
    {
        public string itemName;
        public ItemType itemType;
        public Sprite displaySprite;
        public RectTransform correctTarget;
        public GameObject placedVisual;

        public PuzzleItem(
            string itemName,
            ItemType itemType,
            Sprite displaySprite,
            RectTransform correctTarget,
            GameObject placedVisual)
        {
            this.itemName = itemName;
            this.itemType = itemType;
            this.displaySprite = displaySprite;
            this.correctTarget = correctTarget;
            this.placedVisual = placedVisual;
        }
    }

    // =====================================================
    // CURRENT ELEMENT
    // =====================================================

    [Header("Current Element")]
    [SerializeField] private Image partImage;
    [SerializeField] private Image labelImage;

    // =====================================================
    // PART CARDS
    // =====================================================

    [Header("Part Cards")]
    [SerializeField] private Sprite noseCard;
    [SerializeField] private Sprite tracheaCard;
    [SerializeField] private Sprite leftLungCard;
    [SerializeField] private Sprite rightLungCard;
    [SerializeField] private Sprite mainBronchiCard;
    [SerializeField] private Sprite diaphragmCard;

    // =====================================================
    // PART TARGETS
    // =====================================================

    [Header("Part Targets")]
    [SerializeField] private RectTransform noseTarget;
    [SerializeField] private RectTransform tracheaTarget;
    [SerializeField] private RectTransform leftLungTarget;
    [SerializeField] private RectTransform rightLungTarget;
    [SerializeField] private RectTransform mainBronchiTarget;
    [SerializeField] private RectTransform diaphragmTarget;

    // =====================================================
    // PART PLACED VISUALS
    // =====================================================

    [Header("Part Placed Visuals")]
    [SerializeField] private GameObject nosePlacedVisual;
    [SerializeField] private GameObject tracheaPlacedVisual;
    [SerializeField] private GameObject leftLungPlacedVisual;
    [SerializeField] private GameObject rightLungPlacedVisual;
    [SerializeField] private GameObject mainBronchiPlacedVisual;
    [SerializeField] private GameObject diaphragmPlacedVisual;

    // =====================================================
    // LABEL IMAGES
    // =====================================================

    [Header("Label Images")]
    [SerializeField] private Sprite noseLabel;
    [SerializeField] private Sprite tracheaLabel;
    [SerializeField] private Sprite leftLungLabel;
    [SerializeField] private Sprite rightLungLabel;
    [SerializeField] private Sprite mainBronchiLabel;
    [SerializeField] private Sprite diaphragmLabel;

    // =====================================================
    // LABEL TARGETS
    // =====================================================

    [Header("Label Targets")]
    [SerializeField] private RectTransform noseLabelTarget;
    [SerializeField] private RectTransform tracheaLabelTarget;
    [SerializeField] private RectTransform leftLungLabelTarget;
    [SerializeField] private RectTransform rightLungLabelTarget;
    [SerializeField] private RectTransform mainBronchiLabelTarget;
    [SerializeField] private RectTransform diaphragmLabelTarget;

    // =====================================================
    // LABEL PLACED VISUALS
    // =====================================================

    [Header("Label Placed Visuals")]
    [SerializeField] private GameObject noseLabelPlacedVisual;
    [SerializeField] private GameObject tracheaLabelPlacedVisual;
    [SerializeField] private GameObject leftLungLabelPlacedVisual;
    [SerializeField] private GameObject rightLungLabelPlacedVisual;
    [SerializeField] private GameObject mainBronchiLabelPlacedVisual;
    [SerializeField] private GameObject diaphragmLabelPlacedVisual;

    // =====================================================
    // EVENTS
    // =====================================================

    [Header("Events")]
    [SerializeField] private UnityEvent onCorrectPlacement;
    [SerializeField] private UnityEvent onWrongPlacement;
    [SerializeField] private UnityEvent onPuzzleCompleted;

    // =====================================================

    private readonly List<PuzzleItem> puzzleItems =
        new List<PuzzleItem>();

    private readonly List<RectTransform> allTargets =
        new List<RectTransform>();

    private PuzzleItem currentItem;

    private int currentIndex;

    // =====================================================
    // PUBLIC
    // =====================================================

    public RectTransform CurrentTarget
    {
        get
        {
            if (currentItem == null)
                return null;

            return currentItem.correctTarget;
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        HideAllPlacedVisuals();

        CreatePuzzleItems();

        CreateTargetList();

        ShuffleItems();

        currentIndex = 0;

        ShowCurrentItem();
    }

    // =====================================================
    // CREATE PUZZLE ITEMS
    // =====================================================

    private void CreatePuzzleItems()
    {
        puzzleItems.Clear();

        // =========================
        // PARTS
        // =========================

        puzzleItems.Add(
            new PuzzleItem(
                "Nose Part",
                ItemType.Part,
                noseCard,
                noseTarget,
                nosePlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Trachea Part",
                ItemType.Part,
                tracheaCard,
                tracheaTarget,
                tracheaPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Left Lung Part",
                ItemType.Part,
                leftLungCard,
                leftLungTarget,
                leftLungPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Right Lung Part",
                ItemType.Part,
                rightLungCard,
                rightLungTarget,
                rightLungPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Main Bronchi Part",
                ItemType.Part,
                mainBronchiCard,
                mainBronchiTarget,
                mainBronchiPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Diaphragm Part",
                ItemType.Part,
                diaphragmCard,
                diaphragmTarget,
                diaphragmPlacedVisual
            )
        );

        // =========================
        // LABELS
        // =========================

        puzzleItems.Add(
            new PuzzleItem(
                "Nose Label",
                ItemType.Label,
                noseLabel,
                noseLabelTarget,
                noseLabelPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Trachea Label",
                ItemType.Label,
                tracheaLabel,
                tracheaLabelTarget,
                tracheaLabelPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Left Lung Label",
                ItemType.Label,
                leftLungLabel,
                leftLungLabelTarget,
                leftLungLabelPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Right Lung Label",
                ItemType.Label,
                rightLungLabel,
                rightLungLabelTarget,
                rightLungLabelPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Main Bronchi Label",
                ItemType.Label,
                mainBronchiLabel,
                mainBronchiLabelTarget,
                mainBronchiLabelPlacedVisual
            )
        );

        puzzleItems.Add(
            new PuzzleItem(
                "Diaphragm Label",
                ItemType.Label,
                diaphragmLabel,
                diaphragmLabelTarget,
                diaphragmLabelPlacedVisual
            )
        );
    }

    // =====================================================
    // TARGET LIST
    // =====================================================

    private void CreateTargetList()
    {
        allTargets.Clear();

        AddTarget(noseTarget);
        AddTarget(tracheaTarget);
        AddTarget(leftLungTarget);
        AddTarget(rightLungTarget);
        AddTarget(mainBronchiTarget);
        AddTarget(diaphragmTarget);

        AddTarget(noseLabelTarget);
        AddTarget(tracheaLabelTarget);
        AddTarget(leftLungLabelTarget);
        AddTarget(rightLungLabelTarget);
        AddTarget(mainBronchiLabelTarget);
        AddTarget(diaphragmLabelTarget);
    }

    private void AddTarget(RectTransform target)
    {
        if (target != null)
        {
            allTargets.Add(target);
        }
    }

    // =====================================================
    // SHUFFLE
    // =====================================================

    private void ShuffleItems()
    {
        for (int i = puzzleItems.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            PuzzleItem temp =
                puzzleItems[i];

            puzzleItems[i] =
                puzzleItems[randomIndex];

            puzzleItems[randomIndex] =
                temp;
        }
    }

    // =====================================================
    // SHOW CURRENT ITEM
    // =====================================================

    private void ShowCurrentItem()
    {
        if (currentIndex >= puzzleItems.Count)
        {
            CompletePuzzle();
            return;
        }

        currentItem =
            puzzleItems[currentIndex];

        partImage.gameObject.SetActive(false);
        labelImage.gameObject.SetActive(false);

        if (currentItem.itemType == ItemType.Part)
        {
            partImage.sprite =
                currentItem.displaySprite;

            partImage.gameObject.SetActive(true);

            ResetDraggable(partImage);
        }
        else
        {
            labelImage.sprite =
                currentItem.displaySprite;

            labelImage.gameObject.SetActive(true);

            ResetDraggable(labelImage);
        }
    }

    private void ResetDraggable(Image image)
    {
        if (image == null)
            return;

        LungsPuzzleDraggable draggable =
            image.GetComponent<LungsPuzzleDraggable>();

        if (draggable != null)
        {
            draggable.ResetToStart();
        }
    }

    // =====================================================
    // DROP CHECK
    // =====================================================

    public bool IsCorrectTarget(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        if (currentItem == null ||
            currentItem.correctTarget == null)
        {
            return false;
        }

        return RectTransformUtility
            .RectangleContainsScreenPoint(
                currentItem.correctTarget,
                screenPosition,
                eventCamera
            );
    }

    public bool IsOverAnyTarget(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        for (int i = 0; i < allTargets.Count; i++)
        {
            RectTransform target =
                allTargets[i];

            if (target == null)
                continue;

            bool inside =
                RectTransformUtility
                    .RectangleContainsScreenPoint(
                        target,
                        screenPosition,
                        eventCamera
                    );

            if (inside)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // CORRECT
    // =====================================================

    public void CorrectPlacement()
    {
        if (currentItem == null)
            return;

        if (currentItem.placedVisual != null)
        {
            currentItem.placedVisual
                .SetActive(true);
        }

        onCorrectPlacement?.Invoke();

        currentIndex++;

        ShowCurrentItem();
    }

    // =====================================================
    // WRONG TARGET
    // =====================================================

    public void WrongPlacement()
    {
        Debug.Log(
            "Wrong target."
        );

        /*
         * По-късно точно тук
         * ще взимаме един живот.
         */

        onWrongPlacement?.Invoke();
    }

    // =====================================================
    // EMPTY SPACE
    // =====================================================

    public void DroppedOnEmptySpace()
    {
        /*
         * Умишлено няма наказание.
         *
         * Елементът просто се връща
         * на началното си място.
         */
    }

    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompletePuzzle()
    {
        currentItem = null;

        if (partImage != null)
        {
            partImage.gameObject.SetActive(false);
        }

        if (labelImage != null)
        {
            labelImage.gameObject.SetActive(false);
        }

        Debug.Log(
            "Lungs Puzzle completed."
        );

        onPuzzleCompleted?.Invoke();
    }

    // =====================================================
    // HIDE PLACED VISUALS
    // =====================================================

    private void HideAllPlacedVisuals()
    {
        SetInactive(nosePlacedVisual);
        SetInactive(tracheaPlacedVisual);
        SetInactive(leftLungPlacedVisual);
        SetInactive(rightLungPlacedVisual);
        SetInactive(mainBronchiPlacedVisual);
        SetInactive(diaphragmPlacedVisual);

        SetInactive(noseLabelPlacedVisual);
        SetInactive(tracheaLabelPlacedVisual);
        SetInactive(leftLungLabelPlacedVisual);
        SetInactive(rightLungLabelPlacedVisual);
        SetInactive(mainBronchiLabelPlacedVisual);
        SetInactive(diaphragmLabelPlacedVisual);
    }

    private void SetInactive(
        GameObject target)
    {
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}