using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private GameObject exitConfirmationOverlay;
    [SerializeField] private GameObject puzzleSuccessOverlay;

    [Header("Puzzle Welcome")]
    [SerializeField] private PuzzleWelcomeController puzzleWelcomeController;

    [Header("Difficulty")]
    [SerializeField] private DifficultySelector difficultySelector;

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

    private bool infoOpenedFromWelcome;
    private bool menuTransitionInProgress;

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

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        PreparePuzzle();
    }

    // =========================================================
    // START PUZZLE
    // =========================================================

    public void StartPuzzle()
    {
        puzzleCompleted = false;
        gameOver = false;

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = false;

        currentIndex = 0;

        CloseAllMenusImmediately();
        HidePuzzleSuccess();

        LockDifficulty();

        ResetDropZones();
        SetupLives();
        CreateLabelOrder();

        ResetCurrentLabel();
        ShowCurrentLabel();

        Debug.Log("Brain Puzzle започна.");
    }

    // =========================================================
    // PREPARE PUZZLE
    // =========================================================

    public void PreparePuzzle()
    {
        puzzleCompleted = false;
        gameOver = false;

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = false;

        currentIndex = 0;

        CloseAllMenusImmediately();
        HidePuzzleSuccess();

        LockDifficulty();

        ResetDropZones();
        SetupLives();

        labelOrder.Clear();

        HideAllLabelImages();
        ResetCurrentLabel();

        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }

        Debug.Log(
            "Brain Puzzle е подготвен и чака Welcome панела."
        );
    }

    // =========================================================
    // RESET PUZZLE
    // =========================================================

    public void ResetPuzzle()
    {
        PreparePuzzle();
    }

    // =========================================================
    // INFO FROM WELCOME
    // =========================================================

    public void OpenInfoFromWelcome()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (puzzleWelcomeController == null)
        {
            Debug.LogError(
                "PuzzleWelcomeController не е свързан " +
                "в BrainPuzzleManager."
            );

            return;
        }

        menuTransitionInProgress = true;
        infoOpenedFromWelcome = true;

        puzzleWelcomeController.HideWelcomeForInfo(
            () =>
            {
                OpenInfoOverlay();

                menuTransitionInProgress = false;
            }
        );
    }

    // =========================================================
    // NORMAL INFO
    // =========================================================

    public void OpenInfo()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        infoOpenedFromWelcome = false;

        OpenInfoOverlay();
    }

    private void OpenInfoOverlay()
    {
        if (infoOverlay == null)
        {
            Debug.LogWarning(
                "InfoOverlay не е свързан."
            );

            return;
        }

        infoOverlay.SetActive(true);
        infoOverlay.transform.SetAsLastSibling();
    }

    public void CloseInfo()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (infoOverlay == null)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlayAnimated(
            infoOverlay,
            () =>
            {
                bool shouldReturnToWelcome =
                    infoOpenedFromWelcome;

                infoOpenedFromWelcome = false;
                menuTransitionInProgress = false;

                if (shouldReturnToWelcome &&
                    puzzleWelcomeController != null)
                {
                    puzzleWelcomeController
                        .ShowWelcomeAfterInfo();
                }
            }
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }
    }

    public void CloseSettings()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (settingsOverlay == null)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlayAnimated(
            settingsOverlay,
            () =>
            {
                menuTransitionInProgress = false;
            }
        );
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(true);
            exitConfirmationOverlay.transform.SetAsLastSibling();
        }
    }

    public void CloseExitConfirmation()
    {
        if (menuTransitionInProgress)
        {
            return;
        }

        if (exitConfirmationOverlay == null)
        {
            return;
        }

        menuTransitionInProgress = true;

        CloseOverlayAnimated(
            exitConfirmationOverlay,
            () =>
            {
                menuTransitionInProgress = false;
            }
        );
    }

    public void ExitToBodyMap()
    {
        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // CLOSE OVERLAY WITH ANIMATION
    // =========================================================

    private void CloseOverlayAnimated(
        GameObject overlay,
        Action onClosed)
    {
        if (overlay == null)
        {
            onClosed?.Invoke();
            return;
        }

        UIPopupAnimation animation =
            overlay.GetComponentInChildren
                <UIPopupAnimation>(true);

        if (animation != null &&
            animation.isActiveAndEnabled)
        {
            animation.PlayClose(
                () =>
                {
                    overlay.SetActive(false);
                    onClosed?.Invoke();
                }
            );
        }
        else
        {
            overlay.SetActive(false);
            onClosed?.Invoke();
        }
    }

    // =========================================================
    // IMMEDIATE MENU CLEANUP
    // =========================================================

    private void CloseAllMenusImmediately()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (exitConfirmationOverlay != null)
        {
            exitConfirmationOverlay.SetActive(false);
        }

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = false;
    }

    // =========================================================
    // PUZZLE SUCCESS
    // =========================================================

    public void ShowPuzzleSuccess()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(true);
            puzzleSuccessOverlay.transform.SetAsLastSibling();
        }
    }

    public void HidePuzzleSuccess()
    {
        if (puzzleSuccessOverlay != null)
        {
            puzzleSuccessOverlay.SetActive(false);
        }
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void LockDifficulty()
    {
        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(true);
        }
    }

    public void UnlockDifficulty()
    {
        if (difficultySelector != null)
        {
            difficultySelector.SetLocked(false);
        }
    }

    // =========================================================
    // CURRENT LABEL RESET
    // =========================================================

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
            rectTransform.localRotation =
                Quaternion.identity;
        }
    }

    // =========================================================
    // DROP ZONES
    // =========================================================

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

    // =========================================================
    // LIVES
    // =========================================================

    private void SetupLives()
    {
        // Difficulty се пази глобално като int:
        // 0 = Easy, 1 = Medium, 2 = Hard.
        int difficulty =
            PlayerPrefs.GetInt(
                "Difficulty",
                1
            );

        switch (difficulty)
        {
            case 0:
                currentLives = 3;
                break;

            case 2:
                currentLives = 1;
                break;

            case 1:
            default:
                currentLives = 2;
                break;
        }

        // Подреждаме сърцата винаги като:
        // LifeHeart1, LifeHeart2, LifeHeart3.
        EnsureHeartOrder();

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

        RebuildLivesLayout();

        Debug.Log(
            "Brain Puzzle Lives: " +
            currentLives +
            " | Difficulty: " +
            difficulty
        );
    }

    private void EnsureHeartOrder()
    {
        if (lifeHeart1 == null ||
            lifeHeart2 == null ||
            lifeHeart3 == null)
        {
            return;
        }

        Transform parent =
            lifeHeart1.transform.parent;

        if (lifeHeart2.transform.parent != parent ||
            lifeHeart3.transform.parent != parent)
        {
            return;
        }

        int firstIndex =
            Mathf.Min(
                lifeHeart1.transform.GetSiblingIndex(),
                lifeHeart2.transform.GetSiblingIndex(),
                lifeHeart3.transform.GetSiblingIndex()
            );

        lifeHeart1.transform.SetSiblingIndex(
            firstIndex
        );

        lifeHeart2.transform.SetSiblingIndex(
            firstIndex + 1
        );

        lifeHeart3.transform.SetSiblingIndex(
            firstIndex + 2
        );
    }

    private void RebuildLivesLayout()
    {
        RectTransform livesParent = null;

        if (lifeHeart1 != null)
        {
            livesParent =
                lifeHeart1.transform.parent
                    as RectTransform;
        }
        else if (lifeHeart2 != null)
        {
            livesParent =
                lifeHeart2.transform.parent
                    as RectTransform;
        }
        else if (lifeHeart3 != null)
        {
            livesParent =
                lifeHeart3.transform.parent
                    as RectTransform;
        }

        if (livesParent == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                livesParent
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
            rect.localRotation =
                Quaternion.identity;
        }
    }

    // =========================================================
    // LABEL ORDER
    // =========================================================

    private void CreateLabelOrder()
    {
        labelOrder.Clear();

        labelOrder.Add(
            BrainPartType.Forebrain
        );

        labelOrder.Add(
            BrainPartType.Diencephalon
        );

        labelOrder.Add(
            BrainPartType.Midbrain
        );

        labelOrder.Add(
            BrainPartType.Cerebellum
        );

        labelOrder.Add(
            BrainPartType.Pons
        );

        labelOrder.Add(
            BrainPartType.Medulla
        );

        if (shuffleOrder)
        {
            ShuffleLabels();
        }
    }

    private void ShuffleLabels()
    {
        for (int i = 0;
             i < labelOrder.Count;
             i++)
        {
            int randomIndex =
                UnityEngine.Random.Range(
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

    // =========================================================
    // SHOW CURRENT LABEL
    // =========================================================

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
        SetLabelActive(
            forebrainText,
            false
        );

        SetLabelActive(
            diencephalonText,
            false
        );

        SetLabelActive(
            midbrainText,
            false
        );

        SetLabelActive(
            cerebellumText,
            false
        );

        SetLabelActive(
            ponsText,
            false
        );

        SetLabelActive(
            medullaText,
            false
        );
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

    // =========================================================
    // CORRECT DROP SOUND
    // =========================================================

    public void PlayCorrectDropSound()
    {
        if (puzzleCompleted ||
            gameOver)
        {
            return;
        }

        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayCorrect();
        }
    }

    // =========================================================
    // CORRECT PLACEMENT
    // =========================================================

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

        if (dropZone.AcceptedPart !=
            CurrentPart)
        {
            Debug.LogWarning(
                "HandleCorrectPlacement е извикан " +
                "за неправилна зона.",
                this
            );

            return;
        }

        currentIndex++;

        if (currentIndex >=
            labelOrder.Count)
        {
            CompletePuzzle();
            return;
        }

        ShowCurrentLabel();
    }

    // =========================================================
    // WRONG PLACEMENT
    // =========================================================

    public void HandleWrongPlacement()
    {
        if (puzzleCompleted ||
            gameOver)
        {
            return;
        }

        PlayWrongSoundIfNotGameOver();

        LoseLife();

        onWrongPlacement?.Invoke();
    }

    // =========================================================
    // WRONG SOUND
    // =========================================================

    private void PlayWrongSoundIfNotGameOver()
    {
        if (currentLives <= 1)
        {
            return;
        }

        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayWrong();
        }
    }

    // =========================================================
    // LOSE LIFE
    // =========================================================

    private void LoseLife()
    {
        if (currentLives <= 0)
        {
            return;
        }

        GameObject heartToLose =
            GetHeartForLife(
                currentLives
            );

        currentLives--;

        if (heartToLose != null)
        {
            UILifeHeartAnimation heartAnimation =
                heartToLose.GetComponent<
                    UILifeHeartAnimation
                >();

            if (heartAnimation != null)
            {
                heartAnimation
                    .PlayLoseAnimation();
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

    // =========================================================
    // GAME OVER
    // =========================================================

    private void TriggerGameOver()
    {
        gameOver = true;

        CloseAllMenusImmediately();
        HidePuzzleSuccess();

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

    // =========================================================
    // COMPLETE PUZZLE
    // =========================================================

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        CloseAllMenusImmediately();

        HideAllLabelImages();

        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }

        ShowPuzzleSuccess();

        Debug.Log(
            "Brain Puzzle е завършен успешно!"
        );

        onPuzzleCompleted?.Invoke();
    }

    // =========================================================
    // HIDE CURRENT LABEL
    // =========================================================

    public void HideCurrentLabel()
    {
        if (currentLabel != null)
        {
            currentLabel.SetActive(false);
        }
    }
}