using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BrainPuzzleManager : MonoBehaviour
{
    [Header("Current Label")]
    [SerializeField] private GameObject currentLabel;

    [Header("Current Label Images")]
    [SerializeField] private GameObject forebrainText;
    [SerializeField] private GameObject diencephalonText;
    [SerializeField] private GameObject midbrainText;
    [SerializeField] private GameObject cerebellumText;
    [SerializeField] private GameObject ponsText;
    [SerializeField] private GameObject medullaText;

    [Header("Order")]
    [SerializeField] private bool shuffleOrder = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onWrongPlacement;
    [SerializeField] private UnityEvent onPuzzleCompleted;

    private readonly List<BrainPartType> labelOrder =
        new List<BrainPartType>();

    private int currentIndex;
    private bool puzzleCompleted;

    public bool IsPuzzleCompleted => puzzleCompleted;

    public BrainPartType CurrentPart
    {
        get
        {
            if (labelOrder.Count == 0)
            {
                return BrainPartType.Forebrain;
            }

            if (currentIndex >= labelOrder.Count)
            {
                return labelOrder[labelOrder.Count - 1];
            }

            return labelOrder[currentIndex];
        }
    }

    private void Start()
    {
        StartPuzzle();
    }

    public void StartPuzzle()
    {
        puzzleCompleted = false;
        currentIndex = 0;

        CreateLabelOrder();

        if (currentLabel != null)
        {
            currentLabel.SetActive(true);
        }

        ShowCurrentLabel();
    }

    private void CreateLabelOrder()
    {
        labelOrder.Clear();

        labelOrder.Add(BrainPartType.Forebrain);
        labelOrder.Add(BrainPartType.Diencephalon);
        labelOrder.Add(BrainPartType.Midbrain);
        labelOrder.Add(BrainPartType.Cerebellum);
        labelOrder.Add(BrainPartType.Pons);
        labelOrder.Add(BrainPartType.Medulla);

        if (shuffleOrder)
        {
            ShuffleLabels();
        }
    }

    private void ShuffleLabels()
    {
        for (int i = 0; i < labelOrder.Count; i++)
        {
            int randomIndex =
                Random.Range(i, labelOrder.Count);

            BrainPartType temporary =
                labelOrder[i];

            labelOrder[i] =
                labelOrder[randomIndex];

            labelOrder[randomIndex] =
                temporary;
        }
    }

    private void ShowCurrentLabel()
    {
        HideAllLabelImages();

        if (puzzleCompleted ||
            currentIndex >= labelOrder.Count)
        {
            return;
        }

        GameObject labelImage =
            GetLabelImage(CurrentPart);

        if (labelImage != null)
        {
            labelImage.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "Липсва изображение за: " +
                CurrentPart,
                this
            );
        }
    }

    private void HideAllLabelImages()
    {
        SetLabelActive(forebrainText, false);
        SetLabelActive(diencephalonText, false);
        SetLabelActive(midbrainText, false);
        SetLabelActive(cerebellumText, false);
        SetLabelActive(ponsText, false);
        SetLabelActive(medullaText, false);
    }

    private void SetLabelActive(
        GameObject label,
        bool active)
    {
        if (label != null)
        {
            label.SetActive(active);
        }
    }

    private GameObject GetLabelImage(
        BrainPartType part)
    {
        switch (part)
        {
            case BrainPartType.Forebrain:
                return forebrainText;

            case BrainPartType.Diencephalon:
                return diencephalonText;

            case BrainPartType.Midbrain:
                return midbrainText;

            case BrainPartType.Cerebellum:
                return cerebellumText;

            case BrainPartType.Pons:
                return ponsText;

            case BrainPartType.Medulla:
                return medullaText;

            default:
                return null;
        }
    }

    public void HandleCorrectPlacement(
        BrainPuzzleDropZone dropZone)
    {
        if (puzzleCompleted)
        {
            return;
        }

        if (dropZone == null)
        {
            Debug.LogError(
                "BrainPuzzleDropZone липсва.",
                this
            );

            return;
        }

        if (dropZone.AcceptedPart != CurrentPart)
        {
            Debug.LogWarning(
                "HandleCorrectPlacement е извикан " +
                "за неправилна зона.",
                this
            );

            return;
        }

        currentIndex++;

        if (currentIndex >= labelOrder.Count)
        {
            CompletePuzzle();
            return;
        }

        ShowCurrentLabel();
    }

    public void HandleWrongPlacement()
    {
        if (puzzleCompleted)
        {
            return;
        }

        Debug.Log(
            "Грешно поставяне на: " +
            CurrentPart
        );

        onWrongPlacement?.Invoke();
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        HideAllLabelImages();

        Debug.Log(
            "Brain Puzzle е завършен успешно!"
        );

        onPuzzleCompleted?.Invoke();
    }

    public void HideCurrentLabel()
    {
        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }
    }

    public void RestartPuzzle()
    {
        StartPuzzle();
    }
}