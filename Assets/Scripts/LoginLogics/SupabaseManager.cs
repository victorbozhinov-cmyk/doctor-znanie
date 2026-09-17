using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance { get; private set; }

    private const int CURRENT_SAVE_VERSION = 5;

    // =========================================================
    // SUPABASE CONFIG
    // =========================================================

    [Header("Supabase")]
    [SerializeField] private string projectUrl;

    [SerializeField]
    [TextArea(2, 4)]
    private string publishableKey;

    // =========================================================
    // CURRENT SESSION
    // =========================================================

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string UserId { get; private set; }
    public string Username { get; private set; }

    public int TotalScore { get; private set; }

    public bool IsPlayerDataLoaded { get; private set; }

    public string ProjectUrl => projectUrl;
    public string PublishableKey => publishableKey;

    public bool IsLoggedIn =>
        !string.IsNullOrEmpty(AccessToken) &&
        !string.IsNullOrEmpty(UserId);

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        projectUrl = projectUrl.Trim().TrimEnd('/');
        publishableKey = publishableKey.Trim();

        IsPlayerDataLoaded = false;
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public void RegisterUser(
        string username,
        string password,
        Action<RegisterResult> onComplete
    )
    {
        StartCoroutine(
            RegisterUserRoutine(
                username,
                password,
                onComplete
            )
        );
    }

    private IEnumerator RegisterUserRoutine(
        string username,
        string password,
        Action<RegisterResult> onComplete
    )
    {
        if (!HasValidConfiguration())
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,
                    errorType =
                        RegisterErrorType.Configuration,

                    debugMessage =
                        "Supabase URL or Publishable Key is missing."
                }
            );

            yield break;
        }

        string internalEmail =
            CreateInternalEmail(username);

        SignupRequest signupRequest =
            new SignupRequest
            {
                email = internalEmail,
                password = password,

                data = new SignupMetadata
                {
                    username = username
                }
            };

        string json =
            JsonUtility.ToJson(signupRequest);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            projectUrl + "/auth/v1/signup";

        using UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        request.SetRequestHeader(
            "apikey",
            publishableKey
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Supabase register error: " +
                request.responseCode +
                "\n" +
                response
            );

            if (IsConnectionError(request))
            {
                onComplete?.Invoke(
                    new RegisterResult
                    {
                        success = false,
                        errorType =
                            RegisterErrorType.Network,

                        debugMessage = response
                    }
                );
            }
            else if (ContainsUserAlreadyRegistered(response))
            {
                onComplete?.Invoke(
                    new RegisterResult
                    {
                        success = false,
                        errorType =
                            RegisterErrorType.UsernameTaken,

                        debugMessage = response
                    }
                );
            }
            else
            {
                onComplete?.Invoke(
                    new RegisterResult
                    {
                        success = false,
                        errorType =
                            RegisterErrorType.Server,

                        debugMessage = response
                    }
                );
            }

            yield break;
        }

        AuthResponse authResponse;

        try
        {
            authResponse =
                JsonUtility.FromJson<AuthResponse>(
                    request.downloadHandler.text
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,
                    errorType =
                        RegisterErrorType.InvalidResponse,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        if (authResponse == null ||
            authResponse.user == null ||
            string.IsNullOrEmpty(authResponse.user.id))
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,
                    errorType =
                        RegisterErrorType.InvalidResponse,

                    debugMessage =
                        "Supabase did not return a valid user."
                }
            );

            yield break;
        }

        if (string.IsNullOrEmpty(
            authResponse.access_token))
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,
                    errorType =
                        RegisterErrorType.MissingSession,

                    debugMessage =
                        "Registration succeeded, but no access token was returned."
                }
            );

            yield break;
        }

        SetSession(
            authResponse.access_token,
            authResponse.refresh_token,
            authResponse.user.id,
            username
        );

        bool profileCreated = false;
        bool profileNetworkError = false;
        string profileError = "";

        yield return StartCoroutine(
            CreateProfileRoutine(
                authResponse.user.id,
                username,
                result =>
                {
                    profileCreated =
                        result.success;

                    profileNetworkError =
                        result.networkError;

                    profileError =
                        result.debugMessage;
                }
            )
        );

        if (!profileCreated)
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,

                    errorType =
                        profileNetworkError
                            ? RegisterErrorType.Network
                            : RegisterErrorType.ProfileCreation,

                    debugMessage =
                        profileError
                }
            );

            yield break;
        }

        bool playerDataReady = false;
        bool playerDataNetworkError = false;
        string playerDataError = "";

        yield return StartCoroutine(
            EnsurePlayerDataRoutine(
                authResponse.user.id,
                result =>
                {
                    playerDataReady =
                        result.success;

                    playerDataNetworkError =
                        result.networkError;

                    playerDataError =
                        result.debugMessage;
                }
            )
        );

        if (!playerDataReady)
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,

                    errorType =
                        playerDataNetworkError
                            ? RegisterErrorType.Network
                            : RegisterErrorType.PlayerDataCreation,

                    debugMessage =
                        playerDataError
                }
            );

            yield break;
        }

        bool playerDataLoaded = false;
        bool playerDataLoadNetworkError = false;
        string playerDataLoadError = "";

        yield return StartCoroutine(
            LoadPlayerDataRoutine(
                result =>
                {
                    playerDataLoaded =
                        result.success;

                    playerDataLoadNetworkError =
                        result.networkError;

                    playerDataLoadError =
                        result.debugMessage;
                }
            )
        );

        if (!playerDataLoaded)
        {
            onComplete?.Invoke(
                new RegisterResult
                {
                    success = false,

                    errorType =
                        playerDataLoadNetworkError
                            ? RegisterErrorType.Network
                            : RegisterErrorType.PlayerDataLoad,

                    debugMessage =
                        playerDataLoadError
                }
            );

            yield break;
        }

        TotalScore = 0;

        Debug.Log(
            "Supabase registration successful."
        );

        onComplete?.Invoke(
            new RegisterResult
            {
                success = true,
                errorType =
                    RegisterErrorType.None,

                debugMessage = ""
            }
        );
    }

    // =========================================================
    // LOGIN
    // =========================================================

    public void LoginUser(
        string username,
        string password,
        Action<LoginResult> onComplete
    )
    {
        StartCoroutine(
            LoginUserRoutine(
                username,
                password,
                onComplete
            )
        );
    }

    private IEnumerator LoginUserRoutine(
        string username,
        string password,
        Action<LoginResult> onComplete
    )
    {
        if (!HasValidConfiguration())
        {
            onComplete?.Invoke(
                new LoginResult
                {
                    success = false,
                    errorType =
                        LoginErrorType.Configuration,

                    debugMessage =
                        "Supabase URL or Publishable Key is missing."
                }
            );

            yield break;
        }

        string internalEmail =
            CreateInternalEmail(username);

        LoginRequest loginRequest =
            new LoginRequest
            {
                email = internalEmail,
                password = password
            };

        string json =
            JsonUtility.ToJson(loginRequest);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            projectUrl +
            "/auth/v1/token?grant_type=password";

        using UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        request.SetRequestHeader(
            "apikey",
            publishableKey
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogWarning(
                "Supabase login failed: " +
                request.responseCode +
                "\n" +
                response
            );

            if (IsConnectionError(request))
            {
                onComplete?.Invoke(
                    new LoginResult
                    {
                        success = false,
                        errorType =
                            LoginErrorType.Network,

                        debugMessage = response
                    }
                );
            }
            else
            {
                onComplete?.Invoke(
                    new LoginResult
                    {
                        success = false,
                        errorType =
                            LoginErrorType.InvalidCredentials,

                        debugMessage = response
                    }
                );
            }

            yield break;
        }

        AuthResponse authResponse;

        try
        {
            authResponse =
                JsonUtility.FromJson<AuthResponse>(
                    request.downloadHandler.text
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new LoginResult
                {
                    success = false,
                    errorType =
                        LoginErrorType.InvalidResponse,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        if (authResponse == null ||
            authResponse.user == null ||
            string.IsNullOrEmpty(authResponse.user.id) ||
            string.IsNullOrEmpty(authResponse.access_token))
        {
            onComplete?.Invoke(
                new LoginResult
                {
                    success = false,
                    errorType =
                        LoginErrorType.InvalidResponse,

                    debugMessage =
                        "Supabase did not return a valid login session."
                }
            );

            yield break;
        }

        SetSession(
            authResponse.access_token,
            authResponse.refresh_token,
            authResponse.user.id,
            username
        );

        bool playerDataReady = false;
        bool playerDataNetworkError = false;
        string playerDataError = "";

        yield return StartCoroutine(
            EnsurePlayerDataRoutine(
                authResponse.user.id,
                result =>
                {
                    playerDataReady =
                        result.success;

                    playerDataNetworkError =
                        result.networkError;

                    playerDataError =
                        result.debugMessage;
                }
            )
        );

        if (!playerDataReady)
        {
            onComplete?.Invoke(
                new LoginResult
                {
                    success = false,

                    errorType =
                        playerDataNetworkError
                            ? LoginErrorType.Network
                            : LoginErrorType.PlayerDataSetup,

                    debugMessage =
                        playerDataError
                }
            );

            yield break;
        }

        bool playerDataLoaded = false;
        bool playerDataLoadNetworkError = false;
        string playerDataLoadError = "";

        yield return StartCoroutine(
            LoadPlayerDataRoutine(
                result =>
                {
                    playerDataLoaded =
                        result.success;

                    playerDataLoadNetworkError =
                        result.networkError;

                    playerDataLoadError =
                        result.debugMessage;
                }
            )
        );

        if (!playerDataLoaded)
        {
            onComplete?.Invoke(
                new LoginResult
                {
                    success = false,

                    errorType =
                        playerDataLoadNetworkError
                            ? LoginErrorType.Network
                            : LoginErrorType.PlayerDataLoad,

                    debugMessage =
                        playerDataLoadError
                }
            );

            yield break;
        }

        Debug.Log(
            "Supabase login successful."
        );

        onComplete?.Invoke(
            new LoginResult
            {
                success = true,
                errorType =
                    LoginErrorType.None,

                debugMessage = ""
            }
        );
    }

    // =========================================================
    // PROFILE
    // =========================================================

    public void LoadCurrentProfile(
        Action<ProfileLoadResult> onComplete
    )
    {
        StartCoroutine(
            LoadCurrentProfileRoutine(
                onComplete
            )
        );
    }

    private IEnumerator LoadCurrentProfileRoutine(
        Action<ProfileLoadResult> onComplete
    )
    {
        if (!IsLoggedIn)
        {
            onComplete?.Invoke(
                new ProfileLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        "No logged in user."
                }
            );

            yield break;
        }

        string url =
            projectUrl +
            "/rest/v1/profiles" +
            "?id=eq." +
            UnityWebRequest.EscapeURL(UserId) +
            "&select=id,username,total_score";

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        AddAuthenticatedHeaders(request);

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Profile load error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new ProfileLoadResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        string wrappedJson =
            "{\"items\":" +
            request.downloadHandler.text +
            "}";

        ProfileArrayWrapper wrapper;

        try
        {
            wrapper =
                JsonUtility.FromJson<ProfileArrayWrapper>(
                    wrappedJson
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new ProfileLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        if (wrapper == null ||
            wrapper.items == null ||
            wrapper.items.Length == 0)
        {
            onComplete?.Invoke(
                new ProfileLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        "Profile was not found."
                }
            );

            yield break;
        }

        ProfileData profile =
            wrapper.items[0];

        Username =
            profile.username;

        TotalScore =
            profile.total_score;

        Debug.Log(
            "Profile loaded successfully."
        );

        Debug.Log(
            "Profile username: " +
            Username
        );

        Debug.Log(
            "Profile total score: " +
            TotalScore
        );

        onComplete?.Invoke(
            new ProfileLoadResult
            {
                success = true,
                networkError = false,
                debugMessage = ""
            }
        );
    }

    // =========================================================
    // SAVE PLAYER DATA
    // =========================================================

    public void SavePlayerData(
        Action<PlayerDataSaveResult> onComplete = null
    )
    {
        StartCoroutine(
            SavePlayerDataRoutine(
                onComplete
            )
        );
    }

    private IEnumerator SavePlayerDataRoutine(
        Action<PlayerDataSaveResult> onComplete
    )
    {
        if (!IsLoggedIn)
        {
            onComplete?.Invoke(
                new PlayerDataSaveResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        "No logged in user."
                }
            );

            yield break;
        }

        PlayerSaveData saveData =
            ReadCurrentPlayerPrefs();

        PlayerDataUpdateRequest updateRequest =
            new PlayerDataUpdateRequest
            {
                save_data = saveData,

                updated_at =
                    DateTime.UtcNow.ToString("o")
            };

        string json =
            JsonUtility.ToJson(updateRequest);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            projectUrl +
            "/rest/v1/player_data" +
            "?user_id=eq." +
            UnityWebRequest.EscapeURL(UserId);

        using UnityWebRequest request =
            new UnityWebRequest(
                url,
                "PATCH"
            );

        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        AddAuthenticatedHeaders(request);

        request.SetRequestHeader(
            "Prefer",
            "return=minimal"
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Player data save error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new PlayerDataSaveResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        Debug.Log(
            "Player data saved successfully."
        );

        onComplete?.Invoke(
            new PlayerDataSaveResult
            {
                success = true,
                networkError = false,
                debugMessage = ""
            }
        );
    }

    // =========================================================
    // LOAD PLAYER DATA
    // =========================================================

    public void LoadPlayerData(
        Action<PlayerDataLoadResult> onComplete = null
    )
    {
        StartCoroutine(
            LoadPlayerDataRoutine(
                onComplete
            )
        );
    }

    private IEnumerator LoadPlayerDataRoutine(
        Action<PlayerDataLoadResult> onComplete
    )
    {
        if (!IsLoggedIn)
        {
            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        "No logged in user."
                }
            );

            yield break;
        }

        IsPlayerDataLoaded = false;

        string url =
            projectUrl +
            "/rest/v1/player_data" +
            "?user_id=eq." +
            UnityWebRequest.EscapeURL(UserId) +
            "&select=user_id,save_data,updated_at";

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        AddAuthenticatedHeaders(request);

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Player data load error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        string wrappedJson =
            "{\"items\":" +
            request.downloadHandler.text +
            "}";

        PlayerDataRecordWrapper wrapper;

        try
        {
            wrapper =
                JsonUtility.FromJson<PlayerDataRecordWrapper>(
                    wrappedJson
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        if (wrapper == null ||
            wrapper.items == null ||
            wrapper.items.Length == 0)
        {
            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        "Player data row was not found."
                }
            );

            yield break;
        }

        PlayerDataRecord record =
            wrapper.items[0];

        // =====================================================
        // NEW EMPTY ACCOUNT
        // =====================================================

        if (record.save_data == null ||
            record.save_data.version <= 0)
        {
            PlayerSaveData defaultData =
                CreateDefaultPlayerSaveData();

            ApplyPlayerSaveData(
                defaultData
            );

            bool saved = false;
            bool saveNetworkError = false;
            string saveError = "";

            yield return StartCoroutine(
                SavePlayerDataRoutine(
                    result =>
                    {
                        saved =
                            result.success;

                        saveNetworkError =
                            result.networkError;

                        saveError =
                            result.debugMessage;
                    }
                )
            );

            if (!saved)
            {
                onComplete?.Invoke(
                    new PlayerDataLoadResult
                    {
                        success = false,
                        networkError =
                            saveNetworkError,

                        debugMessage =
                            saveError
                    }
                );

                yield break;
            }

            IsPlayerDataLoaded = true;

            Debug.Log(
                "Default account save created."
            );

            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = true,
                    networkError = false,
                    debugMessage = ""
                }
            );

            yield break;
        }

        // =====================================================
        // MIGRATION
        // =====================================================

        if (record.save_data.version <
            CURRENT_SAVE_VERSION)
        {
            Debug.Log(
                "Migrating save version " +
                record.save_data.version +
                " -> " +
                CURRENT_SAVE_VERSION
            );

            PlayerSaveData migrated =
                MigratePlayerSaveData(
                    record.save_data
                );

            ApplyPlayerSaveData(
                migrated
            );

            bool saved = false;
            bool saveNetworkError = false;
            string saveError = "";

            yield return StartCoroutine(
                SavePlayerDataRoutine(
                    result =>
                    {
                        saved =
                            result.success;

                        saveNetworkError =
                            result.networkError;

                        saveError =
                            result.debugMessage;
                    }
                )
            );

            if (!saved)
            {
                onComplete?.Invoke(
                    new PlayerDataLoadResult
                    {
                        success = false,
                        networkError =
                            saveNetworkError,

                        debugMessage =
                            saveError
                    }
                );

                yield break;
            }

            IsPlayerDataLoaded = true;

            Debug.Log(
                "Player data migration completed."
            );

            onComplete?.Invoke(
                new PlayerDataLoadResult
                {
                    success = true,
                    networkError = false,
                    debugMessage = ""
                }
            );

            yield break;
        }

        // =====================================================
        // NORMAL LOAD
        // =====================================================

        ApplyPlayerSaveData(
            record.save_data
        );

        IsPlayerDataLoaded = true;

        Debug.Log(
            "Player data loaded successfully."
        );

        onComplete?.Invoke(
            new PlayerDataLoadResult
            {
                success = true,
                networkError = false,
                debugMessage = ""
            }
        );
    }

    // =========================================================
    // READ PLAYER PREFS
    // =========================================================

    private PlayerSaveData ReadCurrentPlayerPrefs()
    {
        return new PlayerSaveData
        {
            version = CURRENT_SAVE_VERSION,

            difficulty =
                PlayerPrefs.GetInt(
                    "Difficulty",
                    1
                ),

            colorTheme =
                PlayerPrefs.GetInt(
                    "ColorTheme",
                    0
                ),

            masterVolume =
                PlayerPrefs.GetFloat(
                    "MasterVolume",
                    1f
                ),

            brightness =
                PlayerPrefs.GetFloat(
                    "Brightness",
                    1f
                ),

            vitamins =
                PlayerPrefs.GetInt(
                    "Vitamins",
                    150
                ),

            heartUnlocked =
                GetInt("HeartUnlocked"),

            liverUnlocked =
                GetInt("LiverUnlocked"),

            lungsUnlocked =
                GetInt("LungsUnlocked"),

            stomachUnlocked =
                GetInt("StomachUnlocked"),

            brainUnlocked =
                GetInt("BrainUnlocked"),

            serumHeart =
                GetInt("Serum_Heart"),

            serumStomach =
                GetInt("Serum_Stomach"),

            serumLiver =
                GetInt("Serum_Liver"),

            serumBrain =
                GetInt("Serum_Brain"),

            serumLungs =
                GetInt("Serum_Lungs"),

            patientCuredPanelShown =
                GetInt(
                    "PatientCuredPanelShown"
                ),

            heartBestAntibodies =
                GetInt("HeartBestAntibodies"),

            stomachBestAntibodies =
                GetInt("StomachBestAntibodies"),

            liverBestAntibodies =
                GetInt("LiverBestAntibodies"),

            brainBestAntibodies =
                GetInt("BrainBestAntibodies"),

            lungsBestAntibodies =
                GetInt("LungsBestAntibodies"),

            heartEasyReward =
                GetInt(
                    "Heart_Easy_VitaminRewardClaimed"
                ),

            heartMediumReward =
                GetInt(
                    "Heart_Medium_VitaminRewardClaimed"
                ),

            heartHardReward =
                GetInt(
                    "Heart_Hard_VitaminRewardClaimed"
                ),

            liverEasyReward =
                GetInt(
                    "Liver_Easy_VitaminRewardClaimed"
                ),

            liverMediumReward =
                GetInt(
                    "Liver_Medium_VitaminRewardClaimed"
                ),

            liverHardReward =
                GetInt(
                    "Liver_Hard_VitaminRewardClaimed"
                ),

            lungsEasyReward =
                GetInt(
                    "Lungs_Easy_VitaminRewardClaimed"
                ),

            lungsMediumReward =
                GetInt(
                    "Lungs_Medium_VitaminRewardClaimed"
                ),

            lungsHardReward =
                GetInt(
                    "Lungs_Hard_VitaminRewardClaimed"
                ),

            stomachEasyReward =
                GetInt(
                    "Stomach_Easy_VitaminRewardClaimed"
                ),

            stomachMediumReward =
                GetInt(
                    "Stomach_Medium_VitaminRewardClaimed"
                ),

            stomachHardReward =
                GetInt(
                    "Stomach_Hard_VitaminRewardClaimed"
                ),

            brainEasyReward =
                GetInt(
                    "Brain_Easy_VitaminRewardClaimed"
                ),

            brainMediumReward =
                GetInt(
                    "Brain_Medium_VitaminRewardClaimed"
                ),

            brainHardReward =
                GetInt(
                    "Brain_Hard_VitaminRewardClaimed"
                )
        };
    }

    // =========================================================
    // DEFAULT SAVE
    // =========================================================

    private PlayerSaveData CreateDefaultPlayerSaveData()
    {
        return new PlayerSaveData
        {
            version = CURRENT_SAVE_VERSION,

            difficulty = 1,
            colorTheme = 0,
            masterVolume = 1f,
            brightness = 1f,

            vitamins = 150,

            patientCuredPanelShown = 0
        };
    }

    // =========================================================
    // MIGRATION
    // =========================================================

    private PlayerSaveData MigratePlayerSaveData(
        PlayerSaveData oldData
    )
    {
        PlayerSaveData data =
            CreateDefaultPlayerSaveData();

        if (oldData == null)
            return data;

        data.difficulty =
            oldData.difficulty;

        data.colorTheme =
            oldData.colorTheme;

        data.masterVolume =
            oldData.masterVolume;

        if (oldData.version >= 2)
        {
            data.brightness =
                oldData.brightness;
        }

        if (oldData.version >= 3)
        {
            data.vitamins =
                oldData.vitamins;

            data.heartUnlocked =
                oldData.heartUnlocked;

            data.liverUnlocked =
                oldData.liverUnlocked;

            data.lungsUnlocked =
                oldData.lungsUnlocked;

            data.stomachUnlocked =
                oldData.stomachUnlocked;

            data.brainUnlocked =
                oldData.brainUnlocked;

            data.serumHeart =
                oldData.serumHeart;

            data.serumStomach =
                oldData.serumStomach;

            data.serumLiver =
                oldData.serumLiver;

            data.serumBrain =
                oldData.serumBrain;

            data.serumLungs =
                oldData.serumLungs;

            data.heartBestAntibodies =
                oldData.heartBestAntibodies;

            data.stomachBestAntibodies =
                oldData.stomachBestAntibodies;

            data.liverBestAntibodies =
                oldData.liverBestAntibodies;

            data.brainBestAntibodies =
                oldData.brainBestAntibodies;

            data.lungsBestAntibodies =
                oldData.lungsBestAntibodies;
        }

        if (oldData.version >= 4)
        {
            CopyRewardFlags(
                oldData,
                data
            );
        }

        // Version 5:
        // Patient Cured panel one-time flag.
        //
        // Стар save version 4 няма тази стойност,
        // затова остава default 0.
        if (oldData.version >= 5)
        {
            data.patientCuredPanelShown =
                oldData.patientCuredPanelShown;
        }

        data.version =
            CURRENT_SAVE_VERSION;

        return data;
    }

    private void CopyRewardFlags(
        PlayerSaveData from,
        PlayerSaveData to
    )
    {
        to.heartEasyReward =
            from.heartEasyReward;

        to.heartMediumReward =
            from.heartMediumReward;

        to.heartHardReward =
            from.heartHardReward;

        to.liverEasyReward =
            from.liverEasyReward;

        to.liverMediumReward =
            from.liverMediumReward;

        to.liverHardReward =
            from.liverHardReward;

        to.lungsEasyReward =
            from.lungsEasyReward;

        to.lungsMediumReward =
            from.lungsMediumReward;

        to.lungsHardReward =
            from.lungsHardReward;

        to.stomachEasyReward =
            from.stomachEasyReward;

        to.stomachMediumReward =
            from.stomachMediumReward;

        to.stomachHardReward =
            from.stomachHardReward;

        to.brainEasyReward =
            from.brainEasyReward;

        to.brainMediumReward =
            from.brainMediumReward;

        to.brainHardReward =
            from.brainHardReward;
    }

    // =========================================================
    // APPLY SAVE
    // =========================================================

    private void ApplyPlayerSaveData(
        PlayerSaveData data
    )
    {
        if (data == null)
            return;

        PlayerPrefs.SetInt(
            "Difficulty",
            data.difficulty
        );

        PlayerPrefs.SetInt(
            "ColorTheme",
            data.colorTheme
        );

        PlayerPrefs.SetFloat(
            "MasterVolume",
            data.masterVolume
        );

        PlayerPrefs.SetFloat(
            "Brightness",
            data.brightness
        );

        PlayerPrefs.SetInt(
            "Vitamins",
            data.vitamins
        );

        SetInt(
            "HeartUnlocked",
            data.heartUnlocked
        );

        SetInt(
            "LiverUnlocked",
            data.liverUnlocked
        );

        SetInt(
            "LungsUnlocked",
            data.lungsUnlocked
        );

        SetInt(
            "StomachUnlocked",
            data.stomachUnlocked
        );

        SetInt(
            "BrainUnlocked",
            data.brainUnlocked
        );

        SetInt(
            "Serum_Heart",
            data.serumHeart
        );

        SetInt(
            "Serum_Stomach",
            data.serumStomach
        );

        SetInt(
            "Serum_Liver",
            data.serumLiver
        );

        SetInt(
            "Serum_Brain",
            data.serumBrain
        );

        SetInt(
            "Serum_Lungs",
            data.serumLungs
        );

        SetInt(
            "PatientCuredPanelShown",
            data.patientCuredPanelShown
        );

        SetInt(
            "HeartBestAntibodies",
            data.heartBestAntibodies
        );

        SetInt(
            "StomachBestAntibodies",
            data.stomachBestAntibodies
        );

        SetInt(
            "LiverBestAntibodies",
            data.liverBestAntibodies
        );

        SetInt(
            "BrainBestAntibodies",
            data.brainBestAntibodies
        );

        SetInt(
            "LungsBestAntibodies",
            data.lungsBestAntibodies
        );

        SetInt(
            "Heart_Easy_VitaminRewardClaimed",
            data.heartEasyReward
        );

        SetInt(
            "Heart_Medium_VitaminRewardClaimed",
            data.heartMediumReward
        );

        SetInt(
            "Heart_Hard_VitaminRewardClaimed",
            data.heartHardReward
        );

        SetInt(
            "Liver_Easy_VitaminRewardClaimed",
            data.liverEasyReward
        );

        SetInt(
            "Liver_Medium_VitaminRewardClaimed",
            data.liverMediumReward
        );

        SetInt(
            "Liver_Hard_VitaminRewardClaimed",
            data.liverHardReward
        );

        SetInt(
            "Lungs_Easy_VitaminRewardClaimed",
            data.lungsEasyReward
        );

        SetInt(
            "Lungs_Medium_VitaminRewardClaimed",
            data.lungsMediumReward
        );

        SetInt(
            "Lungs_Hard_VitaminRewardClaimed",
            data.lungsHardReward
        );

        SetInt(
            "Stomach_Easy_VitaminRewardClaimed",
            data.stomachEasyReward
        );

        SetInt(
            "Stomach_Medium_VitaminRewardClaimed",
            data.stomachMediumReward
        );

        SetInt(
            "Stomach_Hard_VitaminRewardClaimed",
            data.stomachHardReward
        );

        SetInt(
            "Brain_Easy_VitaminRewardClaimed",
            data.brainEasyReward
        );

        SetInt(
            "Brain_Medium_VitaminRewardClaimed",
            data.brainMediumReward
        );

        SetInt(
            "Brain_Hard_VitaminRewardClaimed",
            data.brainHardReward
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // PLAYER PREFS HELPERS
    // =========================================================

    private int GetInt(
        string key
    )
    {
        return PlayerPrefs.GetInt(
            key,
            0
        );
    }

    private void SetInt(
        string key,
        int value
    )
    {
        PlayerPrefs.SetInt(
            key,
            value
        );
    }

    // =========================================================
    // ENSURE PLAYER DATA
    // =========================================================

    private IEnumerator EnsurePlayerDataRoutine(
        string userId,
        Action<PlayerDataResult> onComplete
    )
    {
        string url =
            projectUrl +
            "/rest/v1/player_data" +
            "?user_id=eq." +
            UnityWebRequest.EscapeURL(userId) +
            "&select=user_id";

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        AddAuthenticatedHeaders(request);

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Player data lookup error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new PlayerDataResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        string wrappedJson =
            "{\"items\":" +
            request.downloadHandler.text +
            "}";

        PlayerDataLookupWrapper wrapper;

        try
        {
            wrapper =
                JsonUtility.FromJson<PlayerDataLookupWrapper>(
                    wrappedJson
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new PlayerDataResult
                {
                    success = false,
                    networkError = false,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        if (wrapper != null &&
            wrapper.items != null &&
            wrapper.items.Length > 0)
        {
            onComplete?.Invoke(
                new PlayerDataResult
                {
                    success = true,
                    networkError = false,
                    debugMessage = ""
                }
            );

            yield break;
        }

        yield return StartCoroutine(
            CreatePlayerDataRoutine(
                userId,
                onComplete
            )
        );
    }

    // =========================================================
    // CREATE PLAYER DATA
    // =========================================================

    private IEnumerator CreatePlayerDataRoutine(
        string userId,
        Action<PlayerDataResult> onComplete
    )
    {
        PlayerDataInsertRequest data =
            new PlayerDataInsertRequest
            {
                user_id = userId
            };

        string json =
            JsonUtility.ToJson(data);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            projectUrl +
            "/rest/v1/player_data";

        using UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        AddAuthenticatedHeaders(request);

        request.SetRequestHeader(
            "Prefer",
            "return=minimal"
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Player data creation error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new PlayerDataResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        Debug.Log(
            "Player data row created successfully."
        );

        onComplete?.Invoke(
            new PlayerDataResult
            {
                success = true,
                networkError = false,
                debugMessage = ""
            }
        );
    }

    // =========================================================
    // CREATE PROFILE
    // =========================================================

    private IEnumerator CreateProfileRoutine(
        string userId,
        string username,
        Action<ProfileResult> onComplete
    )
    {
        ProfileInsertRequest profile =
            new ProfileInsertRequest
            {
                id = userId,
                username = username,
                total_score = 0
            };

        string json =
            JsonUtility.ToJson(profile);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            projectUrl +
            "/rest/v1/profiles";

        using UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        AddAuthenticatedHeaders(request);

        request.SetRequestHeader(
            "Prefer",
            "return=minimal"
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                GetRequestErrorMessage(request);

            Debug.LogError(
                "Profile creation error: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new ProfileResult
                {
                    success = false,

                    networkError =
                        IsConnectionError(request),

                    debugMessage = response
                }
            );

            yield break;
        }

        onComplete?.Invoke(
            new ProfileResult
            {
                success = true,
                networkError = false,
                debugMessage = ""
            }
        );
    }

    // =========================================================
    // NETWORK HELPERS
    // =========================================================

    private bool IsConnectionError(
        UnityWebRequest request
    )
    {
        return
            request != null &&
            request.result ==
            UnityWebRequest.Result.ConnectionError;
    }

    private string GetRequestErrorMessage(
        UnityWebRequest request
    )
    {
        if (request == null)
        {
            return "Unknown request error.";
        }

        if (request.downloadHandler != null &&
            !string.IsNullOrWhiteSpace(
                request.downloadHandler.text
            ))
        {
            return request.downloadHandler.text;
        }

        if (!string.IsNullOrWhiteSpace(
            request.error
        ))
        {
            return request.error;
        }

        return "Request failed.";
    }

    // =========================================================
    // HEADERS
    // =========================================================

    private void AddAuthenticatedHeaders(
        UnityWebRequest request
    )
    {
        request.SetRequestHeader(
            "apikey",
            publishableKey
        );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + AccessToken
        );
    }

    // =========================================================
    // SESSION
    // =========================================================

    public void SetSession(
        string accessToken,
        string refreshToken,
        string userId,
        string username
    )
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        Username = username;

        TotalScore = 0;

        IsPlayerDataLoaded = false;
    }

    public void ClearSession()
    {
        AccessToken = "";
        RefreshToken = "";
        UserId = "";
        Username = "";

        TotalScore = 0;

        IsPlayerDataLoaded = false;
    }

    // =========================================================
    // CONFIG
    // =========================================================

    private bool HasValidConfiguration()
    {
        return
            !string.IsNullOrEmpty(projectUrl) &&
            !string.IsNullOrEmpty(publishableKey);
    }

    // =========================================================
    // USERNAME -> INTERNAL EMAIL
    // =========================================================

    public string CreateInternalEmail(
        string username
    )
    {
        if (string.IsNullOrWhiteSpace(username))
            return "";

        string normalizedUsername =
            username
                .Trim()
                .Normalize(
                    NormalizationForm.FormC
                )
                .ToLowerInvariant();

        byte[] bytes =
            Encoding.UTF8.GetBytes(
                normalizedUsername
            );

        string encoded =
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        return encoded + "@example.com";
    }

    private bool ContainsUserAlreadyRegistered(
        string response
    )
    {
        if (string.IsNullOrEmpty(response))
            return false;

        return response.IndexOf(
            "already registered",
            StringComparison.OrdinalIgnoreCase
        ) >= 0;
    }

    // =========================================================
    // JSON CLASSES
    // =========================================================

    [Serializable]
    private class SignupRequest
    {
        public string email;
        public string password;
        public SignupMetadata data;
    }

    [Serializable]
    private class SignupMetadata
    {
        public string username;
    }

    [Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class AuthResponse
    {
        public string access_token;
        public string refresh_token;
        public int expires_in;
        public string token_type;
        public AuthUser user;
    }

    [Serializable]
    private class AuthUser
    {
        public string id;
        public string email;
    }

    [Serializable]
    private class ProfileInsertRequest
    {
        public string id;
        public string username;
        public int total_score;
    }

    [Serializable]
    private class ProfileData
    {
        public string id;
        public string username;
        public int total_score;
    }

    [Serializable]
    private class ProfileArrayWrapper
    {
        public ProfileData[] items;
    }

    // =========================================================
    // PLAYER SAVE DATA
    // =========================================================

    [Serializable]
    private class PlayerSaveData
    {
        public int version;

        public int difficulty;
        public int colorTheme;
        public float masterVolume;
        public float brightness;

        public int vitamins;

        public int heartUnlocked;
        public int liverUnlocked;
        public int lungsUnlocked;
        public int stomachUnlocked;
        public int brainUnlocked;

        public int serumHeart;
        public int serumStomach;
        public int serumLiver;
        public int serumBrain;
        public int serumLungs;

        public int patientCuredPanelShown;

        public int heartBestAntibodies;
        public int stomachBestAntibodies;
        public int liverBestAntibodies;
        public int brainBestAntibodies;
        public int lungsBestAntibodies;

        public int heartEasyReward;
        public int heartMediumReward;
        public int heartHardReward;

        public int liverEasyReward;
        public int liverMediumReward;
        public int liverHardReward;

        public int lungsEasyReward;
        public int lungsMediumReward;
        public int lungsHardReward;

        public int stomachEasyReward;
        public int stomachMediumReward;
        public int stomachHardReward;

        public int brainEasyReward;
        public int brainMediumReward;
        public int brainHardReward;
    }

    [Serializable]
    private class PlayerDataInsertRequest
    {
        public string user_id;
    }

    [Serializable]
    private class PlayerDataUpdateRequest
    {
        public PlayerSaveData save_data;
        public string updated_at;
    }

    [Serializable]
    private class PlayerDataLookup
    {
        public string user_id;
    }

    [Serializable]
    private class PlayerDataLookupWrapper
    {
        public PlayerDataLookup[] items;
    }

    [Serializable]
    private class PlayerDataRecord
    {
        public string user_id;
        public PlayerSaveData save_data;
        public string updated_at;
    }

    [Serializable]
    private class PlayerDataRecordWrapper
    {
        public PlayerDataRecord[] items;
    }

    // =========================================================
    // RESULTS
    // =========================================================

    public enum RegisterErrorType
    {
        None,
        UsernameTaken,
        Network,
        Configuration,
        Server,
        InvalidResponse,
        MissingSession,
        ProfileCreation,
        PlayerDataCreation,
        PlayerDataLoad
    }

    public class RegisterResult
    {
        public bool success;
        public RegisterErrorType errorType;
        public string debugMessage;
    }

    public enum LoginErrorType
    {
        None,
        InvalidCredentials,
        Network,
        Configuration,
        InvalidResponse,
        PlayerDataSetup,
        PlayerDataLoad
    }

    public class LoginResult
    {
        public bool success;
        public LoginErrorType errorType;
        public string debugMessage;
    }

    public class ProfileLoadResult
    {
        public bool success;
        public bool networkError;
        public string debugMessage;
    }

    public class PlayerDataSaveResult
    {
        public bool success;
        public bool networkError;
        public string debugMessage;
    }

    public class PlayerDataLoadResult
    {
        public bool success;
        public bool networkError;
        public string debugMessage;
    }

    private class ProfileResult
    {
        public bool success;
        public bool networkError;
        public string debugMessage;
    }

    private class PlayerDataResult
    {
        public bool success;
        public bool networkError;
        public string debugMessage;
    }
}