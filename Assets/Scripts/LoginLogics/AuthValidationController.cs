using System.Text;
using TMPro;
using UnityEngine;

public class AuthValidationController : MonoBehaviour
{
    // =========================================================
    // LOGIN
    // =========================================================

    [Header("Login")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;

    // =========================================================
    // REGISTER
    // =========================================================

    [Header("Register")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;

    // =========================================================
    // MESSAGE
    // =========================================================

    [Header("Message")]
    [SerializeField] private TMP_Text messageText;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        SetupInputFields();
        ClearMessage();
    }

    private void SetupInputFields()
    {
        if (loginUsernameInput != null)
        {
            loginUsernameInput.characterLimit = 20;
            loginUsernameInput.onValueChanged.AddListener(
                OnLoginUsernameChanged
            );
        }

        if (loginPasswordInput != null)
        {
            loginPasswordInput.characterLimit = 16;
            loginPasswordInput.onValueChanged.AddListener(
                OnAnyFieldChanged
            );
        }

        if (registerUsernameInput != null)
        {
            registerUsernameInput.characterLimit = 20;
            registerUsernameInput.onValueChanged.AddListener(
                OnRegisterUsernameChanged
            );
        }

        if (registerPasswordInput != null)
        {
            registerPasswordInput.characterLimit = 16;
            registerPasswordInput.onValueChanged.AddListener(
                OnAnyFieldChanged
            );
        }
    }

    // =========================================================
    // LOGIN
    // =========================================================

    public void ValidateLogin()
    {
        ClearMessage();

        if (loginUsernameInput == null ||
            loginPasswordInput == null)
        {
            return;
        }

        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage("Въведи потребителско име.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Въведи парола.");
            return;
        }

        if (username.Length < 3)
        {
            ShowMessage(
                "Потребителското име трябва да е поне 3 символа."
            );
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage(
                "Паролата трябва да е поне 6 символа."
            );
            return;
        }

        if (password.Length > 16)
        {
            ShowMessage(
                "Паролата може да е най-много 16 символа."
            );
            return;
        }

        // Тук по-късно ще извикаме реалния Login.
        Debug.Log("Login validation passed.");
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public void ValidateRegister()
    {
        ClearMessage();

        if (registerUsernameInput == null ||
            registerPasswordInput == null)
        {
            return;
        }

        string username = registerUsernameInput.text.Trim();
        string password = registerPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage("Въведи потребителско име.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Въведи парола.");
            return;
        }

        if (username.Length < 3)
        {
            ShowMessage(
                "Потребителското име трябва да е поне 3 символа."
            );
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage(
                "Паролата трябва да е поне 6 символа."
            );
            return;
        }

        if (password.Length > 16)
        {
            ShowMessage(
                "Паролата може да е най-много 16 символа."
            );
            return;
        }

        // Тук по-късно ще извикаме реалната регистрация.
        Debug.Log("Register validation passed.");
    }

    // =========================================================
    // USERNAME FILTER
    // =========================================================

    private void OnLoginUsernameChanged(string value)
    {
        SanitizeUsername(loginUsernameInput, value);
        ClearMessage();
    }

    private void OnRegisterUsernameChanged(string value)
    {
        SanitizeUsername(registerUsernameInput, value);
        ClearMessage();
    }

    private void OnAnyFieldChanged(string value)
    {
        ClearMessage();
    }

    private void SanitizeUsername(
        TMP_InputField inputField,
        string value
    )
    {
        if (inputField == null)
            return;

        StringBuilder result = new StringBuilder();

        foreach (char character in value)
        {
            if (IsAllowedUsernameCharacter(character))
            {
                result.Append(character);
            }
        }

        string sanitized = result.ToString();

        if (sanitized == value)
            return;

        inputField.SetTextWithoutNotify(sanitized);
        inputField.caretPosition = sanitized.Length;
        inputField.ForceLabelUpdate();
    }

    private bool IsAllowedUsernameCharacter(char character)
    {
        bool latin =
            (character >= 'A' && character <= 'Z') ||
            (character >= 'a' && character <= 'z');

        bool cyrillic =
            character >= '\u0400' &&
            character <= '\u04FF';

        bool digit =
            character >= '0' &&
            character <= '9';

        bool underscore =
            character == '_';

        return latin ||
               cyrillic ||
               digit ||
               underscore;
    }

    // =========================================================
    // MESSAGE
    // =========================================================

    public void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
