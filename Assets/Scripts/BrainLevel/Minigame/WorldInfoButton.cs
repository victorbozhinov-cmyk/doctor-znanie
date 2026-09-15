using UnityEngine;

public class WorldInfoButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject infoPanel;

    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private WorldPopupAnimation pausePopupAnimation;

    private bool isOpeningInfo;

    private void Awake()
    {
        if (
            pausePopupAnimation == null &&
            pausePanel != null
        )
        {
            pausePopupAnimation =
                pausePanel.GetComponent<
                    WorldPopupAnimation
                >();
        }
    }

    private void OnMouseUpAsButton()
    {
        if (infoPanel == null)
            return;

        if (isOpeningInfo)
            return;

        if (pausePanel == null)
        {
            infoPanel.SetActive(true);
            return;
        }

        isOpeningInfo = true;

        // Ако PausePanel има WorldPopupAnimation,
        // първо изчакваме close анимацията.
        if (pausePopupAnimation != null)
        {
            pausePopupAnimation.PlayClose(
                () =>
                {
                    pausePanel.SetActive(false);

                    infoPanel.SetActive(true);

                    isOpeningInfo = false;
                }
            );
        }
        else
        {
            // Fallback ако липсва анимация.
            pausePanel.SetActive(false);

            infoPanel.SetActive(true);

            isOpeningInfo = false;
        }
    }
}