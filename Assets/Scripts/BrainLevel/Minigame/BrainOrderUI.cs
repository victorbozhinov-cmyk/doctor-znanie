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
    }

    [Header("Order Time")]
    [SerializeField]
    private float orderDuration = 90f;

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

    private float remainingTime;

    private bool orderActive = false;
    private bool problemPickedUp = false;
    private bool orderCompleted = false;
    private bool playerNearby = false;

    private BrainTokenType activeProblemTokenType =
        BrainTokenType.None;

    private ProblemUIEntry activeProblemUI;

    public event Action OrderFailed;

    public bool IsOrderActive => orderActive;
    public bool HasTimeExpired => remainingTime <= 0f;
    public float RemainingTime => remainingTime;

    public BrainTokenType ActiveProblemTokenType =>
        activeProblemTokenType;

    private void Awake()
    {
        HideAllOrderUI();

        if (timeFill != null)
        {
            timeFill.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (!orderActive || orderCompleted)
            return;

        UpdateTimer();
        UpdateVisualState();
    }

    // =====================================================
    // ORDER STATE
    // =====================================================

    public void StartOrder(
        BrainTokenType problemTokenType)
    {
        activeProblemTokenType =
            problemTokenType;

        activeProblemUI =
            FindProblemUI(problemTokenType);

        remainingTime =
            orderDuration;

        orderActive = true;
        problemPickedUp = false;
        orderCompleted = false;

        if (timeFill != null)
        {
            timeFill.fillAmount = 1f;
        }

        UpdateTimeDisplay();
        UpdateVisualState();

        Debug.Log(
            gameObject.name +
            " started order: " +
            activeProblemTokenType);
    }

    public void NotifyProblemPickedUp()
    {
        if (!orderActive || orderCompleted)
            return;

        problemPickedUp = true;

        UpdateVisualState();
    }

    public void CompleteOrder()
    {
        if (!orderActive)
            return;

        orderCompleted = true;
        orderActive = false;

        HideAllOrderUI();

        Debug.Log(
            gameObject.name +
            " order completed. Timer stopped.");
    }

    // =====================================================
    // FAILURE
    // =====================================================

    private void FailOrder()
    {
        if (!orderActive || orderCompleted)
            return;

        remainingTime = 0f;
        orderActive = false;

        UpdateTimeDisplay();
        HideAllOrderUI();

        if (screenFlash != null)
        {
            screenFlash.PlayRedFlash();
        }

        Debug.Log(
            gameObject.name +
            " order failed - time expired.");

        OrderFailed?.Invoke();
    }

    // =====================================================
    // PLAYER PROXIMITY
    // =====================================================

    public void SetPlayerNearby(bool nearby)
    {
        playerNearby = nearby;

        UpdateVisualState();
    }

    // =====================================================
    // TIMER
    // =====================================================

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimeDisplay();
            FailOrder();

            return;
        }

        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeFill != null)
        {
            if (orderDuration > 0f)
            {
                timeFill.fillAmount =
                    Mathf.Clamp01(
                        remainingTime /
                        orderDuration);
            }
            else
            {
                timeFill.fillAmount = 0f;
            }
        }

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

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

    // =====================================================
    // VISUAL STATE
    // =====================================================

    private void UpdateVisualState()
    {
        if (!orderActive ||
            orderCompleted)
        {
            HideAllOrderUI();
            return;
        }

        // След взимане на Problem токена
        // Info panel изчезва,
        // но Alert + timer остават.
        if (problemPickedUp)
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(true);
            }

            HideAllInfoPanels();

            return;
        }

        // Преди взимане:
        // ако сме близо до станцията,
        // показваме правилния Info panel.
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
                activeProblemUI.infoPanelRoot
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

    // =====================================================
    // PROBLEM UI LOOKUP
    // =====================================================

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
            " has no UI entry for problem: " +
            problemTokenType);

        return null;
    }

    // =====================================================
    // HIDE UI
    // =====================================================

    private void HideAllOrderUI()
    {
        if (orderAlert != null)
        {
            orderAlert.SetActive(false);
        }

        HideAllInfoPanels();
    }

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
