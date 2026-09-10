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
        public Sprite sprite;
        public RectTransform correctTarget;
        public GameObject placedVisual;

        public PuzzleItem(
            string itemName,
            ItemType itemType,
            Sprite sprite,
            RectTransform correctTarget,
            GameObject placedVisual)
        {
            this.itemName = itemName;
            this.itemType = itemType;
            this.sprite = sprite;
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
    // LIVES
    // =====================================================

    [Header("Lives")]
    [SerializeField] private LungsPuzzleLives puzzleLives;

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
    [SerializeField] private UnityEvent onGameOver;

    // =====================================================
    // INTERNAL
    // =====================================================

    private readonly List<PuzzleItem> puzzleItems =
        new List<PuzzleItem>();

    private readonly List<RectTransform> allTargets =
        new List<RectTransform>();

    private PuzzleItem currentItem;

    private int currentIndex = 0;

    private bool puzzleActive = false;

    // =====================================================
    // PUBLIC PROPERTIES
    // =====================================================

    public bool PuzzleActive => puzzleActive;

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
        StartPuzzle();
    }

    // =====================================================
    // START / RESET PUZZLE
    // =====================================================

    public void StartPuzzle()
    {
        Debug.Log("=== LUNGS PUZZLE START ===");

        puzzleActive = false;

        HideCurrentElement();
        HideAllPlacedVisuals();

        if (!CheckImportantReferences())
        {
            Debug.LogError(
                "LungsPuzzleManager: липсват важни връзки в Inspector."
            );

            return;
        }

        if (puzzleLives != null)
        {
            puzzleLives.ResetLives();
        }

        BuildPuzzleItems();
        BuildTargetList();

        ShufflePuzzleItems();

        currentIndex = 0;
        currentItem = null;

        puzzleActive = true;

        Debug.Log(
            "Puzzle created with " +
            puzzleItems.Count +
            " random items."
        );

        ShowCurrentItem();
    }

    // =====================================================
    // BUILD ITEMS
    // =====================================================

    private void BuildPuzzleItems()
    {
        puzzleItems.Clear();

        // PARTS

        AddPuzzleItem(
            "Nose Part",
            ItemType.Part,
            noseCard,
            noseTarget,
            nosePlacedVisual
        );

        AddPuzzleItem(
            "Trachea Part",
            ItemType.Part,
            tracheaCard,
            tracheaTarget,
            tracheaPlacedVisual
        );

        AddPuzzleItem(
            "Left Lung Part",
            ItemType.Part,
            leftLungCard,
            leftLungTarget,
            leftLungPlacedVisual
        );

        AddPuzzleItem(
            "Right Lung Part",
            ItemType.Part,
            rightLungCard,
            rightLungTarget,
            rightLungPlacedVisual
        );

        AddPuzzleItem(
            "Main Bronchi Part",
            ItemType.Part,
            mainBronchiCard,
            mainBronchiTarget,
            mainBronchiPlacedVisual
        );

        AddPuzzleItem(
            "Diaphragm Part",
            ItemType.Part,
            diaphragmCard,
            diaphragmTarget,
            diaphragmPlacedVisual
        );

        // LABELS

        AddPuzzleItem(
            "Nose Label",
            ItemType.Label,
            noseLabel,
            noseLabelTarget,
            noseLabelPlacedVisual
        );

        AddPuzzleItem(
            "Trachea Label",
            ItemType.Label,
            tracheaLabel,
            tracheaLabelTarget,
            tracheaLabelPlacedVisual
        );

        AddPuzzleItem(
            "Left Lung Label",
            ItemType.Label,
            leftLungLabel,
            leftLungLabelTarget,
            leftLungLabelPlacedVisual
        );

        AddPuzzleItem(
            "Right Lung Label",
            ItemType.Label,
            rightLungLabel,
            rightLungLabelTarget,
            rightLungLabelPlacedVisual
        );

        AddPuzzleItem(
            "Main Bronchi Label",
            ItemType.Label,
            mainBronchiLabel,
            mainBronchiLabelTarget,
            mainBronchiLabelPlacedVisual
        );

        AddPuzzleItem(
            "Diaphragm Label",
            ItemType.Label,
            diaphragmLabel,
            diaphragmLabelTarget,
            diaphragmLabelPlacedVisual
        );
    }

    private void AddPuzzleItem(
        string itemName,
        ItemType itemType,
        Sprite sprite,
        RectTransform target,
        GameObject placedVisual)
    {
        PuzzleItem item =
            new PuzzleItem(
                itemName,
                itemType,
                sprite,
                target,
                placedVisual
            );

        puzzleItems.Add(item);
    }

    // =====================================================
    // TARGET LIST
    // =====================================================

    private void BuildTargetList()
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
    // RANDOM SHUFFLE
    // =====================================================

    private void ShufflePuzzleItems()
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

        Debug.Log("Random order:");

        for (int i = 0; i < puzzleItems.Count; i++)
        {
            Debug.Log(
                (i + 1) +
                ". " +
                puzzleItems[i].itemName
            );
        }
    }

    // =====================================================
    // SHOW CURRENT ITEM
    // =====================================================

    private void ShowCurrentItem()
    {
        if (!puzzleActive)
            return;

        if (currentIndex >= puzzleItems.Count)
        {
            CompletePuzzle();
            return;
        }

        currentItem =
            puzzleItems[currentIndex];

        HideCurrentElement();

        Debug.Log(
            "CURRENT ITEM: " +
            currentItem.itemName +
            " (" +
            (currentIndex + 1) +
            "/12)"
        );

        if (currentItem.itemType == ItemType.Part)
        {
            ShowPart();
        }
        else
        {
            ShowLabel();
        }
    }

    private void ShowPart()
    {
        if (partImage == null)
        {
            Debug.LogError(
                "PartImage is missing."
            );

            return;
        }

        partImage.sprite =
            currentItem.sprite;

        partImage.gameObject.SetActive(true);

        ResetDraggable(partImage);
    }

    private void ShowLabel()
    {
        if (labelImage == null)
        {
            Debug.LogError(
                "LabelImage is missing."
            );

            return;
        }

        labelImage.sprite =
            currentItem.sprite;

        labelImage.gameObject.SetActive(true);

        ResetDraggable(labelImage);
    }

    private void ResetDraggable(
        Image image)
    {
        LungsPuzzleDraggable draggable =
            image.GetComponent<LungsPuzzleDraggable>();

        if (draggable != null)
        {
            draggable.ResetToStart();
        }
        else
        {
            Debug.LogError(
                image.name +
                " няма LungsPuzzleDraggable."
            );
        }
    }

    // =====================================================
    // DROP CHECK
    // =====================================================

    public bool IsCorrectTarget(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        if (!puzzleActive)
            return false;

        if (currentItem == null)
            return false;

        if (currentItem.correctTarget == null)
            return false;

        bool result =
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    currentItem.correctTarget,
                    screenPosition,
                    eventCamera
                );

        Debug.Log(
            "Correct target check: " +
            result
        );

        return result;
    }

    public bool IsOverAnyTarget(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        if (!puzzleActive)
            return false;

        for (int i = 0; i < allTargets.Count; i++)
        {
            RectTransform target =
                allTargets[i];

            if (target == null)
                continue;

            if (RectTransformUtility
                .RectangleContainsScreenPoint(
                    target,
                    screenPosition,
                    eventCamera
                ))
            {
                Debug.Log(
                    "Dropped over target: " +
                    target.name
                );

                return true;
            }
        }

        Debug.Log(
            "Dropped on empty space."
        );

        return false;
    }

    // =====================================================
    // CORRECT
    // =====================================================

    public void CorrectPlacement()
    {
        if (!puzzleActive)
            return;

        if (currentItem == null)
            return;

        Debug.Log(
            "CORRECT: " +
            currentItem.itemName
        );

        if (currentItem.placedVisual != null)
        {
            currentItem.placedVisual.SetActive(true);
        }

        onCorrectPlacement?.Invoke();

        currentIndex++;

        ShowCurrentItem();
    }

    // =====================================================
    // WRONG
    // =====================================================

    public void WrongPlacement()
    {
        if (!puzzleActive)
            return;

        Debug.Log(
            "WRONG TARGET: " +
            currentItem.itemName
        );

        onWrongPlacement?.Invoke();

        if (puzzleLives == null)
        {
            Debug.LogWarning(
                "PuzzleLives is not connected."
            );

            return;
        }

        puzzleLives.LoseLife();

        Debug.Log(
            "Lives left: " +
            puzzleLives.CurrentLives
        );

        if (puzzleLives.CurrentLives <= 0)
        {
            GameOver();
        }
    }

    // =====================================================
    // EMPTY SPACE
    // =====================================================

    public void DroppedOnEmptySpace()
    {
        if (!puzzleActive)
            return;

        Debug.Log(
            "Empty space - no life lost."
        );
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void GameOver()
    {
        puzzleActive = false;

        currentItem = null;

        HideCurrentElement();

        Debug.Log(
            "=== LUNGS PUZZLE GAME OVER ==="
        );

        onGameOver?.Invoke();
    }

    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompletePuzzle()
    {
        puzzleActive = false;

        currentItem = null;

        HideCurrentElement();

        Debug.Log(
            "=== LUNGS PUZZLE COMPLETE ==="
        );

        onPuzzleCompleted?.Invoke();
    }

    // =====================================================
    // HIDE CURRENT
    // =====================================================

    private void HideCurrentElement()
    {
        if (partImage != null)
        {
            partImage.gameObject.SetActive(false);
        }

        if (labelImage != null)
        {
            labelImage.gameObject.SetActive(false);
        }
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

    // =====================================================
    // VALIDATION
    // =====================================================

    private bool CheckImportantReferences()
    {
        bool valid = true;

        if (partImage == null)
        {
            Debug.LogError("Part Image = None");
            valid = false;
        }

        if (labelImage == null)
        {
            Debug.LogError("Label Image = None");
            valid = false;
        }

        if (noseTarget == null)
        {
            Debug.LogError("Nose Target = None");
            valid = false;
        }

        if (tracheaTarget == null)
        {
            Debug.LogError("Trachea Target = None");
            valid = false;
        }

        if (leftLungTarget == null)
        {
            Debug.LogError("Left Lung Target = None");
            valid = false;
        }

        if (rightLungTarget == null)
        {
            Debug.LogError("Right Lung Target = None");
            valid = false;
        }

        if (mainBronchiTarget == null)
        {
            Debug.LogError("Main Bronchi Target = None");
            valid = false;
        }

        if (diaphragmTarget == null)
        {
            Debug.LogError("Diaphragm Target = None");
            valid = false;
        }

        if (noseLabelTarget == null)
        {
            Debug.LogError("Nose Label Target = None");
            valid = false;
        }

        if (tracheaLabelTarget == null)
        {
            Debug.LogError("Trachea Label Target = None");
            valid = false;
        }

        if (leftLungLabelTarget == null)
        {
            Debug.LogError("Left Lung Label Target = None");
            valid = false;
        }

        if (rightLungLabelTarget == null)
        {
            Debug.LogError("Right Lung Label Target = None");
            valid = false;
        }

        if (mainBronchiLabelTarget == null)
        {
            Debug.LogError("Main Bronchi Label Target = None");
            valid = false;
        }

        if (diaphragmLabelTarget == null)
        {
            Debug.LogError("Diaphragm Label Target = None");
            valid = false;
        }

        return valid;
    }
}