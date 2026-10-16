using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrainOrderUI : MonoBehaviour
{
    [System.Serializable]
    public class ProblemUIEntry
    {
        [Header("Problem")]
        public BrainTokenType problemTokenType =
            BrainTokenType.None;

        [Header("Near Order Info")]
        public GameObject infoPanelRoot;

        public TMP_Text infoTimeText;

        [Header("Colored Panel (Easy / Medium)")]
        [Tooltip("Drag the UI Image component on InfoPanelImage. Do not drag the signal image.")]
        public Image infoPanelImage;

        [Tooltip("A pre-generated panel sprite with the border color of the correct brain zone.")]
        public Sprite coloredPanelSprite;

        [Header("Timer Position Offsets")]
        [Tooltip("Easy and Medium. Offset from the original InfoTimeText position. (0, 0) keeps the existing position.")]
        public Vector2 easyTimerOffset = Vector2.zero;

        [Tooltip("Hard only. Offset from the original InfoTimeText position. (0, 0) keeps the existing position.")]
        public Vector2 hardTimerOffset = Vector2.zero;

        // Remember the existing Source Image before swapping sprites.
        [NonSerialized] private Sprite originalPanelSprite;
        [NonSerialized] private bool originalPanelSpriteCached;
        [NonSerialized] private RectTransform timerRect;
        [NonSerialized] private Vector2 originalTimerPosition;
        [NonSerialized] private bool originalTimerPositionCached;

        public void CacheOriginalTimerPosition()
        {
            if (originalTimerPositionCached || infoTimeText == null)
                return;

            timerRect = infoTimeText.GetComponent<RectTransform>();

            if (timerRect == null)
                return;

            originalTimerPosition = timerRect.anchoredPosition;
            originalTimerPositionCached = true;
        }

        public void ApplyTimerPositionForDifficulty(int difficulty)
        {
            CacheOriginalTimerPosition();

            if (!originalTimerPositionCached || timerRect == null)
                return;

            Vector2 offset = Vector2.zero;

            if (difficulty == 0 || difficulty == 1) // Easy and Medium
                offset = easyTimerOffset;
            else if (difficulty == 2) // Hard
                offset = hardTimerOffset;

            timerRect.anchoredPosition = originalTimerPosition + offset;
        }

        public void RestoreOriginalTimerPosition()
        {
            if (originalTimerPositionCached && timerRect != null)
                timerRect.anchoredPosition = originalTimerPosition;
        }

        public void CacheOriginalPanelSprite()
        {
            if (originalPanelSpriteCached || infoPanelImage == null)
                return;

            originalPanelSprite = infoPanelImage.sprite;
            originalPanelSpriteCached = true;
        }

        public void RestoreOriginalPanelSprite()
        {
            if (originalPanelSpriteCached && infoPanelImage != null)
                infoPanelImage.sprite = originalPanelSprite;
        }

        public void ApplyPanelSpriteForDifficulty(int difficulty)
        {
            CacheOriginalPanelSprite();

            if (!originalPanelSpriteCached)
                return;

            bool showColorHint = difficulty == 0 || difficulty == 1;

            infoPanelImage.sprite = showColorHint && coloredPanelSprite != null
                ? coloredPanelSprite
                : originalPanelSprite;
        }
    }

    [Header("Default Order Time")]
    [SerializeField]
    private float orderDuration = 30f;

    [Header("Order Alert")]
    [SerializeField]
    private GameObject orderAlert;

    [SerializeField]
    private Image timeFill;

    [SerializeField]
    private TMP_Text timeText;

    [Header("Problem Info Panels")]
    [SerializeField]
    private ProblemUIEntry[] problemUIEntries;

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    // =========================================================
    // INTERNAL
    // =========================================================

    private float remainingTime;

    // NEW: Tutorial timer support
    private bool timerEnabled = true;

    private bool orderActive = false;
    private bool problemPickedUp = false;
    private bool orderCompleted = false;
    private bool playerNearby = false;

    private BrainTokenType activeProblemTokenType =
        BrainTokenType.None;

    private ProblemUIEntry activeProblemUI;

    public event Action OrderFailed;

    public bool IsOrderActive =>
        orderActive;

    public float RemainingTime =>
        remainingTime;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // Cache the original red-outline sprites before the first order.
        if (problemUIEntries != null)
        {
            foreach (ProblemUIEntry entry in problemUIEntries)
            {
                if (entry != null)
                {
                    entry.CacheOriginalPanelSprite();
                    entry.CacheOriginalTimerPosition();
                }
            }
        }

        HideAllOrderUI();

        if (timeFill != null)
        {
            timeFill.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (!orderActive ||
            orderCompleted)
        {
            return;
        }

        // NEW: Timer runs only when enabled
        if (timerEnabled)
        {
            UpdateTimer();
        }

        UpdateVisualState();
    }

    // =========================================================
    // START ORDER
    // =========================================================

    public void StartOrder(
        BrainTokenType problemTokenType,
        float duration,
        bool enableTimer = true)
    {
        activeProblemTokenType =
            problemTokenType;

        activeProblemUI =
            FindProblemUI(
                problemTokenType);

        // Only the nearby information-panel artwork is swapped.
        // The carried token, zone matching, timers, and tutorial remain unchanged.
        // PlayerPrefs Difficulty: 0 = Easy, 1 = Medium, 2 = Hard.
        if (activeProblemUI != null)
        {
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
            activeProblemUI.ApplyPanelSpriteForDifficulty(difficulty);
            activeProblemUI.ApplyTimerPositionForDifficulty(difficulty);
        }

        orderDuration =
            Mathf.Max(
                1f,
                duration);

        remainingTime =
            orderDuration;

        // NEW: Enable or disable order timer
        timerEnabled =
            enableTimer;

        orderActive = true;
        problemPickedUp = false;
        orderCompleted = false;

        if (timeFill != null)
        {
            timeFill.fillAmount = 1f;
        }

        UpdateTimeDisplay();
        UpdateVisualState();
    }

    // =========================================================
    // PROBLEM PICKED UP
    // =========================================================

    public void NotifyProblemPickedUp()
    {
        if (!orderActive ||
            orderCompleted)
        {
            return;
        }

        problemPickedUp = true;

        UpdateVisualState();
    }

    // =========================================================
    // COMPLETE ORDER
    // =========================================================

    public void CompleteOrder()
    {
        if (!orderActive)
            return;

        orderCompleted = true;
        orderActive = false;

        HideAllOrderUI();
        RestorePanelAndTimer();
    }

    // =========================================================
    // CANCEL ORDER
    // =========================================================

    public void CancelOrder()
    {
        orderActive = false;
        orderCompleted = false;
        problemPickedUp = false;

        remainingTime = 0f;

        // Reset timer mode
        timerEnabled = true;

        HideAllOrderUI();
        RestorePanelAndTimer();
    }

    // =========================================================
    // FAIL ORDER
    // =========================================================

    private void FailOrder()
    {
        if (!orderActive ||
            orderCompleted)
        {
            return;
        }

        remainingTime = 0f;
        orderActive = false;

        UpdateTimeDisplay();
        HideAllOrderUI();
        RestorePanelAndTimer();

        if (screenFlash != null)
        {
            screenFlash.PlayRedFlash();
        }

        OrderFailed?.Invoke();
    }

    // =========================================================
    // RESTORE ORIGINAL PANEL AND TIMER POSITION
    // =========================================================

    private void RestorePanelAndTimer()
    {
        if (problemUIEntries == null)
            return;

        foreach (ProblemUIEntry entry in problemUIEntries)
        {
            if (entry == null)
                continue;

            entry.RestoreOriginalPanelSprite();
            entry.RestoreOriginalTimerPosition();
        }
    }

    // =========================================================
    // PLAYER NEARBY
    // =========================================================

    public void SetPlayerNearby(
        bool nearby)
    {
        playerNearby = nearby;

        UpdateVisualState();
    }

    // =========================================================
    // TIMER
    // =========================================================

    private void UpdateTimer()
    {
        remainingTime -=
            Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimeDisplay();
            FailOrder();

            return;
        }

        UpdateTimeDisplay();
    }

    // =========================================================
    // TIME DISPLAY
    // =========================================================

    private void UpdateTimeDisplay()
    {
        // =====================================================
        // TUTORIAL ORDER - NO TIMER
        // =====================================================

        if (!timerEnabled)
        {
            if (timeFill != null)
            {
                timeFill.fillAmount = 1f;
            }

            if (timeText != null)
            {
                timeText.text = "∞";
            }

            if (activeProblemUI != null &&
                activeProblemUI.infoTimeText != null)
            {
                activeProblemUI.infoTimeText.text = "∞";
            }

            return;
        }

        // =====================================================
        // NORMAL ORDER - TIMER ENABLED
        // =====================================================

        if (timeFill != null)
        {
            timeFill.fillAmount =
                Mathf.Clamp01(
                    remainingTime /
                    orderDuration);
        }

        int totalSeconds =
            Mathf.CeilToInt(
                remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        string formattedTime =
            $"{minutes}:{seconds:00}";

        if (timeText != null)
        {
            timeText.text =
                formattedTime;
        }

        if (activeProblemUI != null &&
            activeProblemUI.infoTimeText != null)
        {
            activeProblemUI.infoTimeText.text =
                formattedTime;
        }
    }

    // =========================================================
    // VISUAL STATE
    // =========================================================

    private void UpdateVisualState()
    {
        if (!orderActive ||
            orderCompleted)
        {
            HideAllOrderUI();
            return;
        }

        if (problemPickedUp)
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(true);
            }

            HideAllInfoPanels();

            return;
        }

        if (playerNearby)
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(false);
            }

            HideAllInfoPanels();

            if (activeProblemUI != null &&
                activeProblemUI.infoPanelRoot != null)
            {
                activeProblemUI
                    .infoPanelRoot
                    .SetActive(true);
            }
        }
        else
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(true);
            }

            HideAllInfoPanels();
        }
    }

    // =========================================================
    // FIND PROBLEM UI
    // =========================================================

    private ProblemUIEntry FindProblemUI(
        BrainTokenType problemTokenType)
    {
        if (problemUIEntries == null)
            return null;

        foreach (ProblemUIEntry entry in
                 problemUIEntries)
        {
            if (entry == null)
                continue;

            if (entry.problemTokenType ==
                problemTokenType)
            {
                return entry;
            }
        }

        Debug.LogWarning(
            gameObject.name +
            " has no UI entry for: " +
            problemTokenType);

        return null;
    }

    // =========================================================
    // HIDE ORDER UI
    // =========================================================

    private void HideAllOrderUI()
    {
        if (orderAlert != null)
        {
            orderAlert.SetActive(false);
        }

        HideAllInfoPanels();
    }

    // =========================================================
    // HIDE INFO PANELS
    // =========================================================

    private void HideAllInfoPanels()
    {
        if (problemUIEntries == null)
            return;

        foreach (ProblemUIEntry entry in
                 problemUIEntries)
        {
            if (entry == null)
                continue;

            if (entry.infoPanelRoot != null)
            {
                entry.infoPanelRoot
                    .SetActive(false);
            }
        }
    }
}