using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EyesOrderUI : MonoBehaviour
{
    [Header("Order Time")]
    [SerializeField] private float orderDuration = 90f;

    [Header("Order Alert")]
    [SerializeField] private GameObject orderAlert;
    [SerializeField] private Image timeFill;
    [SerializeField] private TMP_Text timeText;

    [Header("Near Order Info")]
    [SerializeField] private GameObject infoPanelRoot;
    [SerializeField] private TMP_Text infoTimeText;

    [Header("Failure Feedback")]
    [SerializeField] private ScreenFlash screenFlash;

    private float remainingTime;

    private bool orderActive = false;
    private bool problemPickedUp = false;
    private bool orderCompleted = false;
    private bool playerNearby = false;

    public event Action OrderFailed;

    public bool IsOrderActive => orderActive;
    public bool HasTimeExpired => remainingTime <= 0f;
    public float RemainingTime => remainingTime;

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

    public void StartOrder()
    {
        remainingTime = orderDuration;

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
            "Eyes order completed. Timer stopped.");
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
            "Eyes order failed - time expired.");

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
                        remainingTime / orderDuration);
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
            timeText.text = formattedTime;
        }

        if (infoTimeText != null)
        {
            infoTimeText.text = formattedTime;
        }
    }

    // =====================================================
    // VISUAL STATE
    // =====================================================

    private void UpdateVisualState()
    {
        if (!orderActive || orderCompleted)
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

            if (infoPanelRoot != null)
            {
                infoPanelRoot.SetActive(false);
            }

            return;
        }

        if (playerNearby)
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(false);
            }

            if (infoPanelRoot != null)
            {
                infoPanelRoot.SetActive(true);
            }
        }
        else
        {
            if (orderAlert != null)
            {
                orderAlert.SetActive(true);
            }

            if (infoPanelRoot != null)
            {
                infoPanelRoot.SetActive(false);
            }
        }
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

        if (infoPanelRoot != null)
        {
            infoPanelRoot.SetActive(false);
        }
    }
}