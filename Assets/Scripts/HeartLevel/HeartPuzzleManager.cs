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

    [Header("Current Label")]
    [SerializeField]
    private Image currentLabelImage;

    [SerializeField]
    private HeartPuzzleDraggableLabel currentLabel;

    [SerializeField]
    private UILabelChangeAnimation labelChangeAnimation;

    [Header("Label Data")]
    [SerializeField]
    private List<LabelData> labels =
        new List<LabelData>();

    [Header("Lives")]
    [SerializeField]
    private HeartPuzzleLives puzzleLives;

    [Header("Puzzle Success")]
    [SerializeField]
    private GameObject puzzleSuccessOverlay;

    private readonly List<LabelData> remainingLabels =
        new List<LabelData>();

    private readonly List<GameObject> placedLabels =
        new List<GameObject>();

    private int correctlyPlacedCount;

    private bool puzzleStopped;
    private bool isChangingLabel;

    private void Start()
    {
        StartPuzzle();
    }

    public void StartPuzzle()
    {
        puzzleStopped = false;
        isChangingLabel = false;
        correctlyPlacedCount = 0;

        HideSuccessOverlay();
        ClearPlacedLabels();

        remainingLabels.Clear();
        remainingLabels.AddRange(labels);

        if (currentLabel != null)
        {
            currentLabel.gameObject.SetActive(true);
            currentLabel.enabled = true;
        }

        ShowNextRandomLabel();
    }

    public void RestartPuzzle()
    {
        if (puzzleLives != null)
        {
            puzzleLives.ResetLives();
        }

        StartPuzzle();
    }

    private void ShowNextRandomLabel()
    {
        if (puzzleStopped || isChangingLabel)
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
                "Current Label Image или Current Label не е свързан."
            );

            return;
        }

        int randomIndex = UnityEngine.Random.Range(
            0,
            remainingLabels.Count
        );

        LabelData selectedLabel =
            remainingLabels[randomIndex];

        remainingLabels.RemoveAt(randomIndex);

        isChangingLabel = true;

        // Връщаме етикета в началната му позиция,
        // преди да покажем следващото изображение.
        currentLabel.ResetForNextLabel();

        currentLabel.SetPartType(
            selectedLabel.partType
        );

        // Не може да се влачи по време на анимацията.
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
        if (puzzleStopped || currentLabel == null)
        {
            isChangingLabel = false;
            return;
        }

        currentLabel.enabled = true;
        isChangingLabel = false;
    }

    public void HandleCorrectPlacement(
        HeartPuzzleDropZone correctZone)
    {
        if (puzzleStopped || isChangingLabel)
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

    public void HandleWrongPlacement()
    {
        if (puzzleStopped || isChangingLabel)
        {
            return;
        }

        if (puzzleLives == null)
        {
            Debug.LogError(
                "HeartPuzzleLives не е свързан в HeartPuzzleManager!"
            );

            return;
        }

        puzzleLives.LoseLife();

        if (puzzleLives.IsGameOver)
        {
            puzzleStopped = true;

            if (currentLabel != null)
            {
                currentLabel.enabled = false;
                currentLabel.gameObject.SetActive(false);
            }
        }
    }

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

    private void CompletePuzzle()
    {
        puzzleStopped = true;
        isChangingLabel = false;

        if (currentLabel != null)
        {
            currentLabel.enabled = false;
            currentLabel.gameObject.SetActive(false);
        }

        Debug.Log("Пъзелът е завършен!");

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

    private void HideSuccessOverlay()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }
    }
}