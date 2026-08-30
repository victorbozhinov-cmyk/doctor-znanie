using System.Collections;
using UnityEngine;

public class LiverMinigameInfoController : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject puzzleSuccessPanel;

    [Header("Minigame")]
    [SerializeField] private GameObject liverMinigameRoot;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject pausePanel;

    private bool openedFromPause = false;

    // =========================================================
    // ОТ ПЪЗЕЛА
    // =========================================================

    public void OpenInfoFromPuzzle()
    {
        openedFromPause = false;

        // Скриваме Success прозореца на пъзела.
        if (puzzleSuccessPanel != null)
            puzzleSuccessPanel.SetActive(false);

        // Скриваме самия пъзел.
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Включваме минииграта, за да я виждаме отзад.
        if (liverMinigameRoot != null)
            liverMinigameRoot.SetActive(true);

        // Показваме InfoPanel-а.
        ShowInfoPanel();

        // Замразяваме веднага.
        Time.timeScale = 0f;

        /*
         * LiverMinigameRoot току-що е активиран.
         * Някои негови Start() методи могат да се изпълнят
         * на следващия frame и да върнат timeScale на 1
         * или да скрият панела.
         *
         * Затова го подсигуряваме още веднъж.
         */
        StartCoroutine(EnsureInfoAfterMinigameStart());
    }

    private IEnumerator EnsureInfoAfterMinigameStart()
    {
        yield return null;

        if (openedFromPause)
            yield break;

        ShowInfoPanel();

        Time.timeScale = 0f;
    }

    // =========================================================
    // ОТ PAUSE МЕНЮТО
    // =========================================================

    public void OpenInfoFromPause()
    {
        openedFromPause = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        ShowInfoPanel();
    }

    // =========================================================
    // ПОКАЗВАНЕ НА INFO
    // =========================================================

    private void ShowInfoPanel()
    {
        if (infoPanel == null)
            return;

        infoPanel.SetActive(true);

        /*
         * Слагаме InfoPanel последен в Canvas,
         * за да се рисува НАД LiverMinigameRoot.
         */
        infoPanel.transform.SetAsLastSibling();
    }

    // =========================================================
    // БУТОН "НАПРЕД"
    // =========================================================

    public void ContinueFromInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        // Ако сме дошли от Pause менюто:
        if (openedFromPause)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.SetAsLastSibling();
            }

            // Играта остава паузирана.
            Time.timeScale = 0f;

            return;
        }

        // Ако сме дошли след пъзела:
        // стартираме минииграта.
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