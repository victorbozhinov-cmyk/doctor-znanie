using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    // NAVIGATION
    // =========================================================

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool isLoggingIn = false;
    private bool isRegistering = false;

    private const string NoInternetMessage =
        "Няма връзка с интернет. Проверете връзката си и опитайте отново.";

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        SetupInputFields();
        ClearMessage();
    }

    private void OnDestroy()
    {
        RemoveInputFieldListeners();
    }

    // =========================================================
    // INPUT SETUP
    // =========================================================

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

    private void RemoveInputFieldListeners()
    {
        if (loginUsernameInput != null)
        {
            loginUsernameInput.onValueChanged.RemoveListener(
                OnLoginUsernameChanged
            );
        }

        if (loginPasswordInput != null)
        {
            loginPasswordInput.onValueChanged.RemoveListener(
                OnAnyFieldChanged
            );
        }

        if (registerUsernameInput != null)
        {
            registerUsernameInput.onValueChanged.RemoveListener(
                OnRegisterUsernameChanged
            );
        }

        if (registerPasswordInput != null)
        {
            registerPasswordInput.onValueChanged.RemoveListener(
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

        if (isLoggingIn)
            return;

        if (loginUsernameInput == null ||
            loginPasswordInput == null)
        {
            return;
        }

        string username =
            loginUsernameInput.text.Trim();

        string password =
            loginPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(
                "Въведи потребителско име."
            );

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Въведи парола."
            );

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

        if (SupabaseManager.Instance == null)
        {
            Debug.LogError(
                "SupabaseManager.Instance is null."
            );

            return;
        }

        isLoggingIn = true;

        SupabaseManager.Instance.LoginUser(
            username,
            password,
            result =>
            {
                HandleLoginResult(result);
            }
        );
    }

    // =========================================================
    // LOGIN RESULT
    // =========================================================

    private void HandleLoginResult(
        SupabaseManager.LoginResult result
    )
    {
        if (result == null)
        {
            isLoggingIn = false;

            Debug.LogError(
                "Login result is null."
            );

            return;
        }

        if (!result.success)
        {
            isLoggingIn = false;

            switch (result.errorType)
            {
                case SupabaseManager.LoginErrorType.InvalidCredentials:

                    ShowMessage(
                        "Грешно потребителско име или парола."
                    );

                    break;

                case SupabaseManager.LoginErrorType.Network:

                    ShowMessage(
                        NoInternetMessage
                    );

                    Debug.LogWarning(
                        "Login network error:\n" +
                        result.debugMessage
                    );

                    break;

                case SupabaseManager.LoginErrorType.Configuration:

                    Debug.LogError(
                        "Supabase configuration error:\n" +
                        result.debugMessage
                    );

                    break;

                case SupabaseManager.LoginErrorType.InvalidResponse:

                    Debug.LogError(
                        "Invalid Supabase login response:\n" +
                        result.debugMessage
                    );

                    break;

                case SupabaseManager.LoginErrorType.PlayerDataSetup:

                    Debug.LogError(
                        "Player data setup failed:\n" +
                        result.debugMessage
                    );

                    break;

                case SupabaseManager.LoginErrorType.PlayerDataLoad:

                    Debug.LogError(
                        "Player data loading failed:\n" +
                        result.debugMessage
                    );

                    break;

                default:

                    Debug.LogError(
                        "Unknown login error:\n" +
                        result.debugMessage
                    );

                    break;
            }

            return;
        }

        SupabaseManager.Instance.LoadCurrentProfile(
            profileResult =>
            {
                isLoggingIn = false;

                HandleProfileLoadResult(
                    profileResult
                );
            }
        );
    }

    // =========================================================
    // PROFILE LOAD RESULT
    // =========================================================

    private void HandleProfileLoadResult(
        SupabaseManager.ProfileLoadResult result
    )
    {
        if (result == null)
        {
            Debug.LogError(
                "Profile load result is null."
            );

            return;
        }

        if (!result.success)
        {
            if (result.networkError)
            {
                ShowMessage(
                    NoInternetMessage
                );

                Debug.LogWarning(
                    "Profile load network error:\n" +
                    result.debugMessage
                );

                return;
            }

            Debug.LogError(
                "Could not load player profile:\n" +
                result.debugMessage
            );

            return;
        }

        ClearMessage();

        Debug.Log(
            "Login and profile loading completed successfully."
        );

        Debug.Log(
            "User ID: " +
            SupabaseManager.Instance.UserId
        );

        Debug.Log(
            "Username: " +
            SupabaseManager.Instance.Username
        );

        Debug.Log(
            "Total Score: " +
            SupabaseManager.Instance.TotalScore
        );

        ActivateAccountSystems();

        OpenMainMenu();
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public void ValidateRegister()
    {
        ClearMessage();

        if (isRegistering)
            return;

        if (registerUsernameInput == null ||
            registerPasswordInput == null)
        {
            return;
        }

        string username =
            registerUsernameInput.text.Trim();

        string password =
            registerPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(
                "Въведи потребителско име."
            );

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Въведи парола."
            );

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

        if (SupabaseManager.Instance == null)
        {
            Debug.LogError(
                "SupabaseManager.Instance is null."
            );

            return;
        }

        isRegistering = true;

        SupabaseManager.Instance.RegisterUser(
            username,
            password,
            result =>
            {
                isRegistering = false;

                HandleRegisterResult(result);
            }
        );
    }

    // =========================================================
    // REGISTER RESULT
    // =========================================================

    private void HandleRegisterResult(
        SupabaseManager.RegisterResult result
    )
    {
        if (result == null)
        {
            Debug.LogError(
                "Register result is null."
            );

            return;
        }

        if (result.success)
        {
            ClearMessage();

            Debug.Log(
                "Registration completed successfully."
            );

            Debug.Log(
                "User ID: " +
                SupabaseManager.Instance.UserId
            );

            Debug.Log(
                "Username: " +
                SupabaseManager.Instance.Username
            );

            ActivateAccountSystems();

            OpenMainMenu();

            return;
        }

        switch (result.errorType)
        {
            case SupabaseManager.RegisterErrorType.UsernameTaken:

                ShowMessage(
                    "Това потребителско име вече е заето."
                );

                break;

            case SupabaseManager.RegisterErrorType.Network:

                ShowMessage(
                    NoInternetMessage
                );

                Debug.LogWarning(
                    "Registration network error:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.Configuration:

                Debug.LogError(
                    "Supabase configuration error:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.Server:

                Debug.LogError(
                    "Supabase server error:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.InvalidResponse:

                Debug.LogError(
                    "Invalid Supabase response:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.MissingSession:

                Debug.LogError(
                    "Supabase session missing:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.ProfileCreation:

                Debug.LogError(
                    "Profile creation failed:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.PlayerDataCreation:

                Debug.LogError(
                    "Player data creation failed:\n" +
                    result.debugMessage
                );

                break;

            case SupabaseManager.RegisterErrorType.PlayerDataLoad:

                Debug.LogError(
                    "Player data loading failed:\n" +
                    result.debugMessage
                );

                break;

            default:

                Debug.LogError(
                    "Unknown registration error:\n" +
                    result.debugMessage
                );

                break;
        }
    }

    // =========================================================
    // ACCOUNT SYSTEMS
    // =========================================================

    private void ActivateAccountSystems()
    {
        if (AccountSystemsActivator.Instance == null)
        {
            Debug.LogError(
                "AccountSystemsActivator.Instance is null. " +
                "Account systems will not be activated."
            );

            return;
        }

        AccountSystemsActivator.Instance.ActivateSystems();

        Debug.Log(
            "Account systems activation requested."
        );
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void OpenMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError(
                "Main Menu scene name is empty."
            );

            return;
        }

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    // =========================================================
    // USERNAME FILTER
    // =========================================================

    private void OnLoginUsernameChanged(
        string value
    )
    {
        SanitizeUsername(
            loginUsernameInput,
            value
        );

        ClearMessage();
    }

    private void OnRegisterUsernameChanged(
        string value
    )
    {
        SanitizeUsername(
            registerUsernameInput,
            value
        );

        ClearMessage();
    }

    private void OnAnyFieldChanged(
        string value
    )
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

        StringBuilder result =
            new StringBuilder();

        foreach (char character in value)
        {
            if (IsAllowedUsernameCharacter(
                character
            ))
            {
                result.Append(character);
            }
        }

        string sanitized =
            result.ToString();

        if (sanitized == value)
            return;

        inputField.SetTextWithoutNotify(
            sanitized
        );

        inputField.caretPosition =
            sanitized.Length;

        inputField.ForceLabelUpdate();
    }

    private bool IsAllowedUsernameCharacter(
        char character
    )
    {
        bool latin =
            (character >= 'A' &&
             character <= 'Z') ||
            (character >= 'a' &&
             character <= 'z');

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

    private void ShowMessage(
        string message
    )
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}