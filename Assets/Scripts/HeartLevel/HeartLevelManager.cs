using UnityEngine;

public class HeartLevelManager : MonoBehaviour
{
    [Header("Level Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject finishPanel;

    [Header("Puzzle")]
    [SerializeField] private HeartPuzzleManager puzzleManager;

    [Header("Minigame")]
    [SerializeField] private HeartMinigameManager minigameManager;

    private OrganVitaminRewardManager vitaminRewardManager;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        vitaminRewardManager =
            GetComponent<OrganVitaminRewardManager>();

        if (vitaminRewardManager == null)
        {
            Debug.LogWarning(
                "OrganVitaminRewardManager не е намерен на HeartLevelManager."
            );
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ShowStartPanel();
    }

    // =========================================================
    // START PANEL
    // =========================================================

    public void ShowStartPanel()
    {
        HideAllPanels();

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    // =========================================================
    // VIDEO PANEL
    // =========================================================

    public void ShowVideoPanel()
    {
        HideAllPanels();

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
        }
    }

    // =========================================================
    // PUZZLE PANEL
    // =========================================================

    public void ShowPuzzlePanel()
    {
        HideAllPanels();

        if (puzzlePanel == null)
        {
            Debug.LogError(
                "Puzzle Panel не е свързан."
            );

            return;
        }

        puzzlePanel.SetActive(true);

        if (puzzleManager != null)
        {
            puzzleManager.PrepareForWelcome();
        }
        else
        {
            Debug.LogError(
                "HeartPuzzleManager не е свързан."
            );
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPuzzleMusic();
        }
    }

    // =========================================================
    // MINIGAME PANEL
    // =========================================================

    public void ShowMiniGamePanel()
    {
        HideAllPanels();

        if (miniGamePanel == null)
        {
            Debug.LogError(
                "Minigame Panel не е свързан."
            );

            return;
        }

        miniGamePanel.SetActive(true);

        if (minigameManager != null)
        {
            minigameManager.PrepareForWelcome();
        }
        else
        {
            Debug.LogError(
                "HeartMinigameManager не е свързан."
            );
        }
    }

    // =========================================================
    // QUIZ PANEL
    // =========================================================

    public void ShowQuizPanel()
    {
        HideAllPanels();

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
    }

    // =========================================================
    // FINISH PANEL
    // =========================================================

    public void ShowFinishPanel()
    {
        HideAllPanels();

        if (vitaminRewardManager != null)
        {
            int earnedVitamins =
                vitaminRewardManager.TryGiveReward();

            Debug.Log(
                "HeartLevel reward: "
                + earnedVitamins
                + " витамина."
            );
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // =========================================================
    // HIDE ALL
    // =========================================================

    private void HideAllPanels()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (videoPanel != null)
            videoPanel.SetActive(false);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(false);
    }
}