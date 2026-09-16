using System.Collections;
using UnityEngine;

public class VideoSettingsCloseController : MonoBehaviour
{
    // =========================================================
    // SETTINGS PANEL
    // =========================================================

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    // =========================================================
    // CLOSE ANIMATION
    // =========================================================

    [Header("Close Animation")]
    [SerializeField] private float closeDuration = 0.20f;

    [Range(0.5f, 1f)]
    [SerializeField] private float closeScale = 0.85f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private RectTransform panelRectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 normalScale;

    private bool isClosing;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }

        panelRectTransform =
            settingsPanel.GetComponent<RectTransform>();

        canvasGroup =
            settingsPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                settingsPanel.AddComponent<CanvasGroup>();
        }

        if (panelRectTransform != null)
        {
            normalScale =
                panelRectTransform.localScale;
        }

        isClosing = false;
    }

    private void OnEnable()
    {
        isClosing = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // =========================================================
    // CLOSE SETTINGS
    // =========================================================

    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        if (!settingsPanel.activeSelf)
        {
            return;
        }

        if (isClosing)
        {
            return;
        }

        StartCoroutine(
            PlayCloseAnimation()
        );
    }

    // =========================================================
    // CLOSE ANIMATION
    // =========================================================

    private IEnumerator PlayCloseAnimation()
    {
        isClosing = true;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panelRectTransform == null)
        {
            FinishClose();
            yield break;
        }

        Vector3 startScale =
            panelRectTransform.localScale;

        Vector3 targetScale =
            normalScale * closeScale;

        float startAlpha =
            canvasGroup != null
                ? canvasGroup.alpha
                : 1f;

        float timer = 0f;

        while (timer < closeDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / closeDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            panelRectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    smoothT
                );

            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        0f,
                        smoothT
                    );
            }

            yield return null;
        }

        FinishClose();
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void FinishClose()
    {
        if (panelRectTransform != null)
        {
            panelRectTransform.localScale =
                normalScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        isClosing = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}