using System.Collections;
using TMPro;
using UnityEngine;

public class AuthPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Login Inputs")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;

    [Header("Register Inputs")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;

    [Header("Active Tab")]
    [SerializeField] private RectTransform activeTab;

    [Header("Tab Positions / Widths")]
    [SerializeField] private RectTransform loginTabPosition;
    [SerializeField] private RectTransform registerTabPosition;

    [Header("Tab Texts")]
    [SerializeField] private TMP_Text loginTabText;
    [SerializeField] private TMP_Text registerTabText;

    [Header("Colors")]
    [SerializeField] private Color activeTextColor = Color.white;

    [SerializeField] private Color inactiveTextColor =
        new Color(0.15f, 0.35f, 0.65f, 1f);

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.18f;

    private Coroutine moveCoroutine;

    private float activeTabHeight;

    private void Awake()
    {
        if (activeTab != null)
        {
            activeTabHeight = activeTab.sizeDelta.y;
        }
    }

    private void Start()
    {
        ShowLoginImmediate();
    }

    // =========================================================
    // LOGIN
    // =========================================================

    public void ShowLogin()
    {
        ClearAllFields();

        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        SetTextColors(true);

        MoveActiveTab(loginTabPosition);
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public void ShowRegister()
    {
        ClearAllFields();

        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (registerPanel != null)
            registerPanel.SetActive(true);

        SetTextColors(false);

        MoveActiveTab(registerTabPosition);
    }

    // =========================================================
    // INITIAL STATE
    // =========================================================

    private void ShowLoginImmediate()
    {
        ClearAllFields();

        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        SetTextColors(true);

        if (activeTab == null || loginTabPosition == null)
            return;

        Vector2 position = activeTab.anchoredPosition;

        position.x =
            loginTabPosition.anchoredPosition.x;

        activeTab.anchoredPosition = position;

        activeTab.sizeDelta = new Vector2(
            loginTabPosition.sizeDelta.x,
            activeTabHeight
        );
    }

    // =========================================================
    // CLEAR INPUT FIELDS
    // =========================================================

    private void ClearAllFields()
    {
        ClearInput(loginUsernameInput);
        ClearInput(loginPasswordInput);

        ClearInput(registerUsernameInput);
        ClearInput(registerPasswordInput);
    }

    private void ClearInput(TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        inputField.SetTextWithoutNotify("");
        inputField.DeactivateInputField();
        inputField.ForceLabelUpdate();
    }

    // =========================================================
    // TEXT COLORS
    // =========================================================

    private void SetTextColors(bool loginActive)
    {
        if (loginTabText != null)
        {
            loginTabText.color =
                loginActive
                    ? activeTextColor
                    : inactiveTextColor;
        }

        if (registerTabText != null)
        {
            registerTabText.color =
                loginActive
                    ? inactiveTextColor
                    : activeTextColor;
        }
    }

    // =========================================================
    // ACTIVE TAB
    // =========================================================

    private void MoveActiveTab(RectTransform target)
    {
        if (activeTab == null || target == null)
            return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine =
            StartCoroutine(
                MoveRoutine(target)
            );
    }

    private IEnumerator MoveRoutine(RectTransform target)
    {
        Vector2 startPosition =
            activeTab.anchoredPosition;

        Vector2 targetPosition =
            startPosition;

        targetPosition.x =
            target.anchoredPosition.x;

        float startWidth =
            activeTab.sizeDelta.x;

        float targetWidth =
            target.sizeDelta.x;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / moveDuration
                );

            t = t * t * (3f - 2f * t);

            activeTab.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            float currentWidth =
                Mathf.Lerp(
                    startWidth,
                    targetWidth,
                    t
                );

            activeTab.sizeDelta =
                new Vector2(
                    currentWidth,
                    activeTabHeight
                );

            yield return null;
        }

        activeTab.anchoredPosition =
            targetPosition;

        activeTab.sizeDelta =
            new Vector2(
                targetWidth,
                activeTabHeight
            );

        moveCoroutine = null;
    }
}