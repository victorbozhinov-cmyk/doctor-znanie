using UnityEngine;

public class LungsMinigameWelcomeController : MonoBehaviour
{
    // =========================================================
    // WELCOME FLOW
    // =========================================================

    [Header("Welcome Flow")]
    [SerializeField] private GameObject welcomeFlowRoot;
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject infoOverlay;

    // =========================================================
    // MINIGAME
    // =========================================================

    [Header("Minigame")]
    [SerializeField] private LungsMinigameManager minigameManager;

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool hasStarted;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        ResetWelcomeFlow();
    }

    // =========================================================
    // RESET / SHOW WELCOME
    // =========================================================

    public void ResetWelcomeFlow()
    {
        hasStarted = false;

        if (welcomeFlowRoot != null)
        {
            welcomeFlowRoot.SetActive(true);
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }
    }

    // =========================================================
    // OPEN INFO
    // =========================================================

    public void OpenInfo()
    {
        if (hasStarted)
        {
            return;
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(true);
            infoOverlay.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning(
                "Welcome Info Overlay не е свързан."
            );
        }
    }

    // =========================================================
    // CLOSE INFO
    // =========================================================

    public void CloseInfo()
    {
        if (hasStarted)
        {
            return;
        }

        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }
    }

    // =========================================================
    // START MINIGAME
    // =========================================================

    public void StartMinigame()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;

        // Първо стартираме играта.
        if (minigameManager != null)
        {
            minigameManager.StartMinigame();
        }
        else
        {
            Debug.LogWarning(
                "LungsMinigameManager не е свързан."
            );
        }

        // После премахваме ЦЕЛИЯ Welcome flow.
        // Така няма как Info Overlay да остане активен.
        if (welcomeFlowRoot != null)
        {
            welcomeFlowRoot.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Welcome Flow Root не е свързан."
            );

            if (infoOverlay != null)
            {
                infoOverlay.SetActive(false);
            }

            if (welcomePanel != null)
            {
                welcomePanel.SetActive(false);
            }
        }
    }
}