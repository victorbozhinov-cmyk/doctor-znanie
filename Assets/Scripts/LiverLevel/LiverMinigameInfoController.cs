using System.Collections;
using UnityEngine;

public class LiverMinigameInfoController : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject puzzleSuccessPanel;

    [Header("Minigame")]
    [SerializeField] private GameObject liverMinigameRoot;
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject pausePanel;

    private enum InfoOpenedFrom
    {
        None,
        Welcome,
        Puzzle,
        Pause
    }

    private InfoOpenedFrom openedFrom =
        InfoOpenedFrom.None;

    // =========================================================
    // WELCOME PANEL - СЛЕД ПЪЗЕЛА
    // =========================================================

    public void OpenWelcomeFromPuzzle()
    {
        openedFrom = InfoOpenedFrom.None;

        // Замразяваме ПРЕДИ да включим минииграта.
        Time.timeScale = 0f;

        // Скриваме Success панела на пъзела.
        if (puzzleSuccessPanel != null)
        {
            puzzleSuccessPanel.SetActive(false);
        }

        // Скриваме самия пъзел.
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Включваме минииграта,
        // за да се вижда като фон.
        if (liverMinigameRoot != null)
        {
            liverMinigameRoot.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        ShowWelcomePanel();

        /*
         * Някой Start() в LiverMiniGameRoot
         * може да върне Time.timeScale на 1.
         *
         * Затова след един frame подсигуряваме
         * Welcome панела и паузата отново.
         */
        StartCoroutine(
            EnsureWelcomeAfterMinigameStart()
        );
    }

    private IEnumerator EnsureWelcomeAfterMinigameStart()
    {
        yield return null;

        if (welcomePanel == null ||
            !welcomePanel.activeSelf)
        {
            yield break;
        }

        ShowWelcomePanel();

        Time.timeScale = 0f;
    }

    private void ShowWelcomePanel()
    {
        if (welcomePanel == null)
        {
            return;
        }

        welcomePanel.SetActive(true);

        // Welcome панелът винаги стои
        // над минииграта.
        welcomePanel.transform.SetAsLastSibling();
    }

    // =========================================================
    // БУТОН "ПРОДЪЛЖИ" НА WELCOME PANEL
    // =========================================================

    public void ContinueFromWelcome()
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }

        openedFrom = InfoOpenedFrom.None;

        // Тук реално започва минииграта.
        Time.timeScale = 1f;
    }

    // =========================================================
    // "КАК СЕ ИГРАЕ" ОТ WELCOME PANEL
    // =========================================================

    public void OpenInfoFromWelcome()
    {
        openedFrom = InfoOpenedFrom.Welcome;

        // Минииграта остава замразена.
        Time.timeScale = 0f;

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }

        ShowInfoPanel();
    }

    // =========================================================
    // INFO ДИРЕКТНО СЛЕД ПЪЗЕЛА
    // Оставяме го за съвместимост.
    // =========================================================

    public void OpenInfoFromPuzzle()
    {
        openedFrom = InfoOpenedFrom.Puzzle;

        Time.timeScale = 0f;

        if (puzzleSuccessPanel != null)
        {
            puzzleSuccessPanel.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (liverMinigameRoot != null)
        {
            liverMinigameRoot.SetActive(true);
        }

        ShowInfoPanel();

        StartCoroutine(
            EnsureInfoAfterMinigameStart()
        );
    }

    private IEnumerator EnsureInfoAfterMinigameStart()
    {
        yield return null;

        if (openedFrom != InfoOpenedFrom.Puzzle)
        {
            yield break;
        }

        ShowInfoPanel();

        Time.timeScale = 0f;
    }

    // =========================================================
    // INFO ОТ PAUSE МЕНЮТО
    // =========================================================

    public void OpenInfoFromPause()
    {
        openedFrom = InfoOpenedFrom.Pause;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        ShowInfoPanel();
    }

    // =========================================================
    // ПОКАЗВАНЕ НА INFO PANEL
    // =========================================================

    private void ShowInfoPanel()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.SetActive(true);

        // InfoPanel винаги е над минииграта.
        infoPanel.transform.SetAsLastSibling();
    }

    // =========================================================
    // БУТОН "НАПРЕД" НА INFO PANEL
    // =========================================================

    public void ContinueFromInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        // Ако сме дошли от Pause:
        if (openedFrom == InfoOpenedFrom.Pause)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.SetAsLastSibling();
            }

            // Играта остава паузирана.
            Time.timeScale = 0f;

            openedFrom = InfoOpenedFrom.None;

            return;
        }

        // Ако сме дошли от Welcome или след пъзела,
        // започваме минииграта.
        openedFrom = InfoOpenedFrom.None;

        Time.timeScale = 1f;
    }

    // =========================================================
    // SAFETY
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}