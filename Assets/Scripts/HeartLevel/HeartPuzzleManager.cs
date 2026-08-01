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
    [SerializeField] private Image currentLabelImage;

    [SerializeField]
    private HeartPuzzleDraggableLabel currentLabel;

    [Header("Label Data")]
    [SerializeField]
    private List<LabelData> labels =
        new List<LabelData>();

    private readonly List<LabelData>
        remainingLabels =
            new List<LabelData>();

    private int correctlyPlacedCount;

    private void Start()
    {
        PreparePuzzle();
        ShowNextRandomLabel();
    }

    private void PreparePuzzle()
    {
        correctlyPlacedCount = 0;

        remainingLabels.Clear();
        remainingLabels.AddRange(labels);
    }

    private void ShowNextRandomLabel()
    {
        if (remainingLabels.Count == 0)
        {
            CompletePuzzle();
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

        currentLabelImage.sprite =
            selectedLabel.labelSprite;

        currentLabelImage.preserveAspect = true;

        currentLabel.SetPartType(
            selectedLabel.partType
        );

        currentLabel.ResetForNextLabel();
    }

    public void HandleCorrectPlacement(
        HeartPuzzleDropZone correctZone)
    {
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

    private void CreatePlacedLabel(
        HeartPuzzleDropZone correctZone)
    {
        RectTransform zoneRect =
            correctZone.GetComponent<RectTransform>();

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

        // Поставеното изображение не трябва
        // да прихваща курсора.
        placedImage.raycastTarget = false;
    }

    private void CompletePuzzle()
    {
        currentLabel.gameObject.SetActive(false);

        Debug.Log(
            "Пъзелът е завършен!"
        );

        // По-късно тук ще покажем:
        // - брояч 7/7;
        // - панел „Браво!“;
        // - бутон „Продължи“.
    }
}