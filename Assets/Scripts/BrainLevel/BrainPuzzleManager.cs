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

    [Header("Drop Zones")]
    [SerializeField] private BrainPuzzleDropZone forebrainDropZone;
    [SerializeField] private BrainPuzzleDropZone diencephalonDropZone;
    [SerializeField] private BrainPuzzleDropZone midbrainDropZone;
    [SerializeField] private BrainPuzzleDropZone cerebellumDropZone;
    [SerializeField] private BrainPuzzleDropZone ponsDropZone;
    [SerializeField] private BrainPuzzleDropZone medullaDropZone;

    [Header("Lives")]
    [SerializeField] private GameObject lifeHeart1;
    [SerializeField] private GameObject lifeHeart2;
    [SerializeField] private GameObject lifeHeart3;

    [Header("Puzzle UI")]
    [SerializeField] private GameObject infoOverlay;

    [Header("Order")]
    [SerializeField] private bool shuffleOrder = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onWrongPlacement;
    [SerializeField] private UnityEvent onPuzzleCompleted;
    [SerializeField] private UnityEvent onGameOver;

    private readonly List<BrainPartType> labelOrder =
        new List<BrainPartType>();

    private int currentIndex;
    private int currentLives;

    private bool puzzleCompleted;
    private bool gameOver;

    public bool IsPuzzleCompleted => puzzleCompleted;
    public bool IsGameOver => gameOver;

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

    // Стартира реално пъзела.
    public void StartPuzzle()
    {
        puzzleCompleted = false;
        gameOver = false;

        currentIndex = 0;

        CloseInfo();

        ResetDropZones();
        SetupLives();
        CreateLabelOrder();

        ResetCurrentLabel();

        ShowCurrentLabel();
    }

    // Подготвя пъзела за нов опит,
    // но НЕ стартира новия рунд.
    public void PreparePuzzle()
    {
        puzzleCompleted = false;
        gameOver = false;

        currentIndex = 0;

        CloseInfo();

        ResetDropZones();
        SetupLives();

        labelOrder.Clear();

        HideAllLabelImages();
        ResetCurrentLabel();

        if (currentLabel != null)
        {
            currentLabel.SetActive(true);
        }
    }

    // Използва се от бутона "Опитай пак".
    // След това Navigation връща играча на StartPanel.
    public void ResetPuzzle()
    {
        PreparePuzzle();
    }

    // -------------------------
    // INFO PANEL
    // -------------------------

    public void OpenInfo()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(true);
        }
    }

    public void CloseInfo()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }
    }

    private void ResetCurrentLabel()
    {
        if (currentLabel == null)
        {
            return;
        }

        CanvasGroup canvasGroup =
            currentLabel.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        RectTransform rectTransform =
            currentLabel.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }
    }

    private void ResetDropZones()
    {
        ResetDropZone(forebrainDropZone);
        ResetDropZone(diencephalonDropZone);
        ResetDropZone(midbrainDropZone);
        ResetDropZone(cerebellumDropZone);
        ResetDropZone(ponsDropZone);
        ResetDropZone(medullaDropZone);
    }

    private void ResetDropZone(
        BrainPuzzleDropZone dropZone)
    {
        if (dropZone != null)
        {
            dropZone.ResetZone();
        }
    }

    private void SetupLives()
    {
        string difficulty =
            PlayerPrefs.GetString(
                "Difficulty",
                "Medium"
            );

        switch (difficulty)
        {
            case "Easy":
                currentLives = 3;
                break;

            case "Hard":
                currentLives = 1;
                break;

            case "Medium":
            default:
                currentLives = 2;
                break;
        }

        SetHeartActive(
            lifeHeart1,
            currentLives >= 1
        );

        SetHeartActive(
            lifeHeart2,
            currentLives >= 2
        );

        SetHeartActive(
            lifeHeart3,
            currentLives >= 3
        );
    }

    private void SetHeartActive(
        GameObject heart,
        bool active)
    {
        if (heart == null)
        {
            return;
        }

        heart.SetActive(active);

        if (!active)
        {
            return;
        }

        CanvasGroup canvasGroup =
            heart.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        RectTransform rect =
            heart.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
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
                Random.Range(
                    i,
                    labelOrder.Count
                );

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
            gameOver ||
            currentIndex >= labelOrder.Count)
        {
            return;
        }

        if (currentLabel != null)
        {
            currentLabel.SetActive(true);
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
        if (puzzleCompleted ||
            gameOver)
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
        if (puzzleCompleted ||
            gameOver)
        {
            return;
        }

        LoseLife();

        onWrongPlacement?.Invoke();
    }

    private void LoseLife()
    {
        if (currentLives <= 0)
        {
            return;
        }

        GameObject heartToLose =
            GetHeartForLife(currentLives);

        currentLives--;

        if (heartToLose != null)
        {
            UILifeHeartAnimation heartAnimation =
                heartToLose.GetComponent<
                    UILifeHeartAnimation
                >();

            if (heartAnimation != null)
            {
                heartAnimation.PlayLoseAnimation();
            }
            else
            {
                heartToLose.SetActive(false);
            }
        }

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }

    private GameObject GetHeartForLife(
        int lifeNumber)
    {
        switch (lifeNumber)
        {
            case 3:
                return lifeHeart3;

            case 2:
                return lifeHeart2;

            case 1:
                return lifeHeart1;

            default:
                return null;
        }
    }

    private void TriggerGameOver()
    {
        gameOver = true;

        CloseInfo();
        HideAllLabelImages();

        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }

        Debug.Log(
            "Brain Puzzle - Game Over!"
        );

        onGameOver?.Invoke();
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        CloseInfo();
        HideAllLabelImages();

        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }

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
}