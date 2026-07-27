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

    private void Start()
    {
        ShowStartPanel();
    }

    public void ShowStartPanel()
    {
        HideAllPanels();
        startPanel.SetActive(true);
    }

    public void ShowVideoPanel()
    {
        HideAllPanels();
        videoPanel.SetActive(true);
    }

    public void ShowPuzzlePanel()
    {
        HideAllPanels();
        puzzlePanel.SetActive(true);
    }

    public void ShowMiniGamePanel()
    {
        HideAllPanels();
        miniGamePanel.SetActive(true);
    }

    public void ShowQuizPanel()
    {
        HideAllPanels();
        quizPanel.SetActive(true);
    }

    public void ShowFinishPanel()
    {
        HideAllPanels();
        finishPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        startPanel.SetActive(false);
        videoPanel.SetActive(false);
        puzzlePanel.SetActive(false);
        miniGamePanel.SetActive(false);
        quizPanel.SetActive(false);
        finishPanel.SetActive(false);
    }
}