using UnityEngine;

public class StartPanelSettingsController : MonoBehaviour
{
    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    [SerializeField] private GameObject settingsOverlay;

    [SerializeField]
    private UIPopupCloseAnimation settingsCloseAnimation;

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool isClosing;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        isClosing = false;

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }
    }

    // =========================================================
    // OPEN SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (settingsOverlay == null)
        {
            Debug.LogWarning(
                "SettingsOverlay не е свързан."
            );

            return;
        }

        if (isClosing)
        {
            return;
        }

        settingsOverlay.SetActive(true);

        settingsOverlay
            .transform
            .SetAsLastSibling();
    }

    // =========================================================
    // CLOSE SETTINGS
    // =========================================================

    public void CloseSettings()
    {
        if (settingsOverlay == null)
        {
            return;
        }

        if (!settingsOverlay.activeSelf)
        {
            return;
        }

        if (isClosing)
        {
            return;
        }

        // Ако има close animation,
        // първо я изиграваме.
        if (settingsCloseAnimation != null)
        {
            isClosing = true;

            settingsCloseAnimation.PlayClose(() =>
            {
                settingsOverlay.SetActive(false);

                isClosing = false;
            });
        }
        else
        {
            // Fallback ако няма свързана анимация.
            settingsOverlay.SetActive(false);
        }
    }
}