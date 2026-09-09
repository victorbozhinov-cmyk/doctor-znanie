using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartPuzzleManager : MonoBehaviour
{
    [Serializable]
    public class LabelData
    {
        public HeartPartType partType;
        public Sprite labelSprite;
    }

    // =========================================================
    // CURRENT LABEL
    // =========================================================

    [Header("Current Label")]
    [SerializeField] private Image currentLabelImage;
    [SerializeField] private HeartPuzzleDraggableLabel currentLabel;
    [SerializeField] private UILabelChangeAnimation labelChangeAnimation;

    // =========================================================
    // LABEL DATA
    // =========================================================

    [Header("Label Data")]
    [SerializeField]
    private List<LabelData> labels =
        new List<LabelData>();

    // =========================================================
    // LIVES
    // =========================================================

    [Header("Lives")]
    [SerializeField] private HeartPuzzleLives puzzleLives;

    // =========================================================
    // SUCCESS
    // =========================================================

    [Header("Puzzle Success")]
    [SerializeField] private GameObject puzzleSuccessOverlay;

    // =========================================================
    // INTERNAL
    // =========================================================

    private readonly List<LabelData> remainingLabels =
        new List<LabelData>();

    private readonly List<GameObject> placedLabels =
        new List<GameObject>();

    private int correctlyPlacedCount;

    private bool puzzleStarted;
    private bool puzzleStopped = true;
    private bool isChangingLabel;

    // =========================================================
    // PREPARE FOR WELCOME
    // =========================================================

    public void PrepareForWelcome()
    {
        puzzleStarted = false;
        puzzleStopped = true;
        isChangingLabel = false;

        correctlyPlacedCount = 0;

        HideSuccessOverlay();
        ClearPlacedLabels();

        remainingLabels.Clear();

        if (puzzleLives != null)
        {
            puzzleLives.ResetLives();
        }

        if (currentLabel != null)
        {
            currentLabel.enabled = false;
            currentLabel.gameObject.SetActive(false);
        }

        Debug.Log(
            "Heart Puzzle е подготвен и чака Welcome."
        );
    }

    // =========================================================
    // START PUZZLE
    // =========================================================

    public void StartPuzzle()
    {
        puzzleStarted = true;
        puzzleStopped = false;
        isChangingLabel = false;

        correctlyPlacedCount = 0;

        HideSuccessOverlay();
        ClearPlacedLabels();

        remainingLabels.Clear();
        remainingLabels.AddRange(labels);

        if (puzzleLives != null)
        {
            puzzleLives.ResetLives();
        }

        if (currentLabel != null)
        {
            currentLabel.gameObject.SetActive(true);
            currentLabel.enabled = true;
        }

        ShowNextRandomLabel();

        Debug.Log("Heart Puzzle започна.");
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartPuzzle()
    {
        StartPuzzle();
    }

    // =========================================================
    // NEXT LABEL
    // =========================================================

    private void ShowNextRandomLabel()
    {
        if (!puzzleStarted ||
            puzzleStopped ||
            isChangingLabel)
        {
            return;
        }

        if (remainingLabels.Count == 0)
        {
            CompletePuzzle();
            return;
        }

        if (currentLabelImage == null ||
            currentLabel == null)
        {
            Debug.LogError(
                "Current Label Image или Current Label " +
                "не е свързан."
            );

            return;
        }

        int randomIndex =
            UnityEngine.Random.Range(
                0,
                remainingLabels.Count
            );

        LabelData selectedLabel =
            remainingLabels[randomIndex];

        remainingLabels.RemoveAt(randomIndex);

        isChangingLabel = true;

        currentLabel.ResetForNextLabel();

        currentLabel.SetPartType(
            selectedLabel.partType
        );

        currentLabel.enabled = false;

        if (labelChangeAnimation != null)
        {
            labelChangeAnimation.ChangeSprite(
                currentLabelImage,
                selectedLabel.labelSprite,
                FinishLabelChange
            );
        }
        else
        {
            currentLabelImage.sprite =
                selectedLabel.labelSprite;

            currentLabelImage.preserveAspect = true;

            FinishLabelChange();
        }
    }

    private void FinishLabelChange()
    {
        if (!puzzleStarted ||
            puzzleStopped ||
            currentLabel == null)
        {
            isChangingLabel = false;
            return;
        }

        currentLabel.enabled = true;
        isChangingLabel = false;
    }

    // =========================================================
    // CORRECT PLACEMENT
    // =========================================================

    public void HandleCorrectPlacement(
        HeartPuzzleDropZone correctZone)
    {
        if (!puzzleStarted ||
            puzzleStopped ||
            isChangingLabel)
        {
            return;
        }

        if (correctZone == null)
        {
            Debug.LogError(
                "Правилната drop зона липсва."
            );

            return;
        }

        CreatePlacedLabel(correctZone);

        correctlyPlacedCount++;

        Debug.Log(
            $"Поставени: {correctlyPlacedCount} / {labels.Count}"
        );

        ShowNextRandomLabel();
    }

    // =========================================================
    // WRONG PLACEMENT
    // =========================================================

    public void HandleWrongPlacement()
    {
        if (!puzzleStarted ||
            puzzleStopped ||
            isChangingLabel)
        {
            return;
        }

        if (puzzleLives == null)
        {
            Debug.LogError(
                "HeartPuzzleLives не е свързан " +
                "в HeartPuzzleManager!"
            );

            return;
        }

        puzzleLives.LoseLife();

        if (puzzleLives.IsGameOver)
        {
            puzzleStarted = false;
            puzzleStopped = true;
        }
    }

    // =========================================================
    // CREATE PLACED LABEL
    // =========================================================

    private void CreatePlacedLabel(
        HeartPuzzleDropZone correctZone)
    {
        RectTransform zoneRect =
            correctZone.GetComponent<RectTransform>();

        if (zoneRect == null)
        {
            Debug.LogError(
                "Drop зоната няма RectTransform."
            );

            return;
        }

        GameObject placedLabel =
            new GameObject(
                $"Placed_{currentLabel.PartType}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform placedRect =
            placedLabel.GetComponent<RectTransform>();

        Image placedImage =
            placedLabel.GetComponent<Image>();

        placedRect.SetParent(zoneRect, false);

        placedRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        placedRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        placedRect.pivot =
            new Vector2(0.5f, 0.5f);

        placedRect.anchoredPosition =
            Vector2.zero;

        placedRect.sizeDelta =
            currentLabelImage.rectTransform.sizeDelta;

        placedRect.localScale =
            currentLabelImage.rectTransform.localScale;

        placedRect.localRotation =
            Quaternion.identity;

        placedImage.sprite =
            currentLabelImage.sprite;

        placedImage.color =
            currentLabelImage.color;

        placedImage.material =
            currentLabelImage.material;

        placedImage.preserveAspect = true;
        placedImage.raycastTarget = false;

        placedLabels.Add(placedLabel);
    }

    // =========================================================
    // CLEAR LABELS
    // =========================================================

    private void ClearPlacedLabels()
    {
        foreach (GameObject placedLabel in placedLabels)
        {
            if (placedLabel != null)
            {
                Destroy(placedLabel);
            }
        }

        placedLabels.Clear();
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompletePuzzle()
    {
        puzzleStarted = false;
        puzzleStopped = true;
        isChangingLabel = false;

        if (currentLabel != null)
        {
            currentLabel.enabled = false;
            currentLabel.gameObject.SetActive(false);
        }

        Debug.Log(
            "Пъзелът е завършен!"
        );

        if (puzzleSuccessOverlay == null)
        {
            Debug.LogError(
                "PuzzleSuccessOverlay не е свързан " +
                "в HeartPuzzleManager!"
            );

            return;
        }

        puzzleSuccessOverlay.SetActive(true);
        puzzleSuccessOverlay.transform.SetAsLastSibling();
    }

    // =========================================================
    // SUCCESS OVERLAY
    // =========================================================

    private void HideSuccessOverlay()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }
    }
}