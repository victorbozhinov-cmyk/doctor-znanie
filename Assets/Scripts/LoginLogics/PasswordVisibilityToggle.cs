using TMPro;
using UnityEngine;

public class PasswordVisibilityToggle : MonoBehaviour
{
    [Header("Password Input")]
    [SerializeField] private TMP_InputField passwordInput;

    private bool isVisible = false;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        ResetToHidden();
    }

    private void OnEnable()
    {
        ResetToHidden();
    }

    private void OnDisable()
    {
        ResetToHidden();
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    public void TogglePasswordVisibility()
    {
        if (isVisible)
        {
            SetPasswordHidden();
        }
        else
        {
            SetPasswordVisible();
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetToHidden()
    {
        isVisible = false;

        if (passwordInput == null)
            return;

        passwordInput.contentType =
            TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();
    }

    // =========================================================
    // VISIBLE
    // =========================================================

    private void SetPasswordVisible()
    {
        if (passwordInput == null)
            return;

        isVisible = true;

        passwordInput.contentType =
            TMP_InputField.ContentType.Standard;

        passwordInput.ForceLabelUpdate();
    }

    // =========================================================
    // HIDDEN
    // =========================================================

    private void SetPasswordHidden()
    {
        if (passwordInput == null)
            return;

        isVisible = false;

        passwordInput.contentType =
            TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();
    }
}