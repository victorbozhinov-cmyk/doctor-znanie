using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

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
    // PROGRESS
    // =====================================================

    [Header("Progress")]
    [SerializeField] private TMP_Text progressText;

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

    private int currentIndex;

    private bool puzzleActive;
    private bool inputLocked;

    // =====================================================
    // PUBLIC
    // =====================================================

    public bool PuzzleActive =>
        puzzleActive &&
        !inputLocked;

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
    // START PUZZLE
    // =====================================================

    public void StartPuzzle()
    {
        puzzleActive = false;
        inputLocked = false;

        HideCurrentElement();
        HideAllPlacedVisuals();

        if (puzzleLives != null)
        {
            puzzleLives.ResetLives();
        }

        BuildPuzzleItems();
        BuildTargetList();
        ShufflePuzzleItems();

        currentIndex = 0;
        currentItem = null;

        UpdateProgressUI();

        puzzleActive = true;

        ShowCurrentItem();
    }

    // =====================================================
    // BUILD ITEMS
    // =====================================================

    private void BuildPuzzleItems()
    {
        puzzleItems.Clear();

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
        puzzleItems.Add(
            new PuzzleItem(
                itemName,
                itemType,
                sprite,
                target,
                placedVisual
            )
        );
    }

    // =====================================================
    // TARGETS
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

    private void AddTarget(
        RectTransform target)
    {
        if (target != null)
        {
            allTargets.Add(target);
        }
    }

    // =====================================================
    // RANDOM
    // =====================================================

    private void ShufflePuzzleItems()
    {
        for (int i = puzzleItems.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            PuzzleItem temp =
                puzzleItems[i];

            puzzleItems[i] =
                puzzleItems[randomIndex];

            puzzleItems[randomIndex] =
                temp;
        }
    }

    // =====================================================
    // PROGRESS
    // =====================================================

    private void UpdateProgressUI()
    {
        if (progressText == null)
            return;

        progressText.text =
            currentIndex +
            "/" +
            puzzleItems.Count;
    }

    // =====================================================
    // CURRENT ITEM
    // =====================================================

    private void ShowCurrentItem()
    {
        if (!puzzleActive)
            return;

        if (currentIndex >=
            puzzleItems.Count)
        {
            CompletePuzzle();
            return;
        }

        currentItem =
            puzzleItems[currentIndex];

        HideCurrentElement();

        if (currentItem.itemType ==
            ItemType.Part)
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
            return;

        partImage.sprite =
            currentItem.sprite;

        partImage.gameObject
            .SetActive(true);

        ResetDraggable(
            partImage
        );
    }

    private void ShowLabel()
    {
        if (labelImage == null)
            return;

        labelImage.sprite =
            currentItem.sprite;

        labelImage.gameObject
            .SetActive(true);

        ResetDraggable(
            labelImage
        );
    }

    private void ResetDraggable(
        Image image)
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
        if (!PuzzleActive)
            return false;

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
        if (!PuzzleActive)
            return false;

        for (int i = 0;
             i < allTargets.Count;
             i++)
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
        if (!PuzzleActive)
            return;

        if (currentItem == null)
            return;

        if (currentItem.placedVisual != null)
        {
            currentItem
                .placedVisual
                .SetActive(true);

            LungsPuzzleFeedbackFlash correctFlash =
                currentItem
                    .placedVisual
                    .GetComponent<
                        LungsPuzzleFeedbackFlash>();

            if (correctFlash != null)
            {
                correctFlash
                    .FlashCorrect();
            }
        }

        onCorrectPlacement?.Invoke();

        currentIndex++;

        UpdateProgressUI();

        ShowCurrentItem();
    }

    // =====================================================
    // WRONG
    // =====================================================

    public void WrongPlacement()
    {
        if (!PuzzleActive)
            return;

        LungsPuzzleFeedbackFlash wrongFlash =
            null;

        if (partImage != null &&
            partImage.gameObject.activeSelf)
        {
            wrongFlash =
                partImage.GetComponent<
                    LungsPuzzleFeedbackFlash>();
        }
        else if (labelImage != null &&
                 labelImage.gameObject.activeSelf)
        {
            wrongFlash =
                labelImage.GetComponent<
                    LungsPuzzleFeedbackFlash>();
        }

        if (wrongFlash != null)
        {
            wrongFlash.FlashWrong();
        }

        onWrongPlacement?.Invoke();

        if (puzzleLives == null)
        {
            Debug.LogWarning(
                "Puzzle Lives не е свързан."
            );

            return;
        }

        inputLocked = true;

        puzzleLives.LoseLife(
            () =>
            {
                if (puzzleLives == null)
                    return;

                if (puzzleLives.CurrentLives <= 0)
                {
                    GameOver();
                }
                else
                {
                    inputLocked = false;
                }
            }
        );
    }

    // =====================================================
    // EMPTY SPACE
    // =====================================================

    public void DroppedOnEmptySpace()
    {
        // Няма наказание.
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void GameOver()
    {
        if (!puzzleActive)
            return;

        puzzleActive = false;
        inputLocked = true;

        currentItem = null;

        HideCurrentElement();

        onGameOver?.Invoke();
    }

    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompletePuzzle()
    {
        if (!puzzleActive)
            return;

        puzzleActive = false;
        inputLocked = true;

        currentItem = null;

        HideCurrentElement();

        // =================================================
        // SCORE
        // =================================================

        if (LungsScoreManager.Instance != null &&
            puzzleLives != null)
        {
            LungsScoreManager.Instance
                .SubmitPuzzleResult(
                    puzzleLives.CurrentLives,
                    puzzleLives.MaxLives
                );

            Debug.Log(
                $"Lungs Puzzle Score submitted | " +
                $"Lives: " +
                $"{puzzleLives.CurrentLives}/" +
                $"{puzzleLives.MaxLives}"
            );
        }
        else
        {
            if (LungsScoreManager.Instance == null)
            {
                Debug.LogWarning(
                    "LungsScoreManager.Instance липсва. " +
                    "Резултатът от Lungs Puzzle " +
                    "не беше записан."
                );
            }

            if (puzzleLives == null)
            {
                Debug.LogWarning(
                    "LungsPuzzleLives не е свързан. " +
                    "Резултатът от Lungs Puzzle " +
                    "не беше записан."
                );
            }
        }

        onPuzzleCompleted?.Invoke();
    }

    // =====================================================
    // HIDE CURRENT
    // =====================================================

    private void HideCurrentElement()
    {
        if (partImage != null)
        {
            partImage
                .gameObject
                .SetActive(false);
        }

        if (labelImage != null)
        {
            labelImage
                .gameObject
                .SetActive(false);
        }
    }

    // =====================================================
    // RESET VISUALS
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