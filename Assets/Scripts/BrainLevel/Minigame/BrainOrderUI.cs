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

    private float remainingTime;

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
        if (!orderActive ||
            orderCompleted)
        {
            return;
        }

        UpdateTimer();
        UpdateVisualState();
    }

    public void StartOrder(
        BrainTokenType problemTokenType,
        float duration)
    {
        activeProblemTokenType =
            problemTokenType;

        activeProblemUI =
            FindProblemUI(
                problemTokenType);

        orderDuration =
            Mathf.Max(
                1f,
                duration);

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
    }

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

    public void CompleteOrder()
    {
        if (!orderActive)
            return;

        orderCompleted = true;
        orderActive = false;

        HideAllOrderUI();
    }

    public void CancelOrder()
    {
        orderActive = false;
        orderCompleted = false;
        problemPickedUp = false;
        remainingTime = 0f;

        HideAllOrderUI();
    }

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

        if (screenFlash != null)
        {
            screenFlash.PlayRedFlash();
        }

        OrderFailed?.Invoke();
    }

    public void SetPlayerNearby(
        bool nearby)
    {
        playerNearby = nearby;

        UpdateVisualState();
    }

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

    private void UpdateTimeDisplay()
    {
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
