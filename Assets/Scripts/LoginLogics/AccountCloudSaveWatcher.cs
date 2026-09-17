using System.Globalization;
using System.Text;
using UnityEngine;

public class AccountCloudSaveWatcher : MonoBehaviour
{
    public static AccountCloudSaveWatcher Instance { get; private set; }

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Cloud Save")]
    [SerializeField]
    [Min(0.1f)]
    private float saveDelay = 1f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private string lastSnapshot = "";

    private bool watchingStarted = false;

    private bool hasPendingSave = false;
    private bool saveInProgress = false;

    private float saveTimer = 0f;

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
    }

    private void Update()
    {
        SupabaseManager supabase =
            SupabaseManager.Instance;

        if (supabase == null)
        {
            ResetWatcher();
            return;
        }

        if (!supabase.IsLoggedIn)
        {
            ResetWatcher();
            return;
        }

        if (!supabase.IsPlayerDataLoaded)
        {
            ResetWatcher();
            return;
        }

        // =====================================================
        // START WATCHING
        // =====================================================

        if (!watchingStarted)
        {
            lastSnapshot =
                BuildCurrentSnapshot();

            watchingStarted = true;

            Debug.Log(
                "AccountCloudSaveWatcher started."
            );

            return;
        }

        // =====================================================
        // DETECT PLAYER PREFS CHANGE
        // =====================================================

        string currentSnapshot =
            BuildCurrentSnapshot();

        if (currentSnapshot != lastSnapshot)
        {
            lastSnapshot =
                currentSnapshot;

            hasPendingSave = true;

            saveTimer =
                saveDelay;

            Debug.Log(
                "Account PlayerPrefs change detected."
            );
        }

        // =====================================================
        // SAVE DELAY
        // =====================================================

        if (!hasPendingSave)
            return;

        if (saveInProgress)
            return;

        saveTimer -=
            Time.unscaledDeltaTime;

        if (saveTimer > 0f)
            return;

        StartCloudSave();
    }

    // =========================================================
    // BUILD SNAPSHOT
    // =========================================================

    private string BuildCurrentSnapshot()
    {
        StringBuilder builder =
            new StringBuilder();

        // =====================================================
        // SETTINGS
        // =====================================================

        AppendInt(
            builder,
            "Difficulty",
            1
        );

        AppendInt(
            builder,
            "ColorTheme",
            0
        );

        AppendFloat(
            builder,
            "MasterVolume",
            1f
        );

        AppendFloat(
            builder,
            "Brightness",
            1f
        );

        // =====================================================
        // VITAMINS
        // =====================================================

        AppendInt(
            builder,
            "Vitamins",
            150
        );

        // =====================================================
        // ORGAN UNLOCKS
        // =====================================================

        AppendInt(
            builder,
            "HeartUnlocked"
        );

        AppendInt(
            builder,
            "LiverUnlocked"
        );

        AppendInt(
            builder,
            "LungsUnlocked"
        );

        AppendInt(
            builder,
            "StomachUnlocked"
        );

        AppendInt(
            builder,
            "BrainUnlocked"
        );

        // =====================================================
        // SERUM
        // =====================================================

        AppendInt(
            builder,
            "Serum_Heart"
        );

        AppendInt(
            builder,
            "Serum_Stomach"
        );

        AppendInt(
            builder,
            "Serum_Liver"
        );

        AppendInt(
            builder,
            "Serum_Brain"
        );

        AppendInt(
            builder,
            "Serum_Lungs"
        );

        // =====================================================
        // PATIENT CURED PANEL
        // =====================================================

        AppendInt(
            builder,
            "PatientCuredPanelShown"
        );

        // =====================================================
        // ANTIBODIES
        // =====================================================

        AppendInt(
            builder,
            "HeartBestAntibodies"
        );

        AppendInt(
            builder,
            "StomachBestAntibodies"
        );

        AppendInt(
            builder,
            "LiverBestAntibodies"
        );

        AppendInt(
            builder,
            "BrainBestAntibodies"
        );

        AppendInt(
            builder,
            "LungsBestAntibodies"
        );

        // =====================================================
        // HEART REWARDS
        // =====================================================

        AppendInt(
            builder,
            "Heart_Easy_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Heart_Medium_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Heart_Hard_VitaminRewardClaimed"
        );

        // =====================================================
        // LIVER REWARDS
        // =====================================================

        AppendInt(
            builder,
            "Liver_Easy_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Liver_Medium_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Liver_Hard_VitaminRewardClaimed"
        );

        // =====================================================
        // LUNGS REWARDS
        // =====================================================

        AppendInt(
            builder,
            "Lungs_Easy_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Lungs_Medium_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Lungs_Hard_VitaminRewardClaimed"
        );

        // =====================================================
        // STOMACH REWARDS
        // =====================================================

        AppendInt(
            builder,
            "Stomach_Easy_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Stomach_Medium_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Stomach_Hard_VitaminRewardClaimed"
        );

        // =====================================================
        // BRAIN REWARDS
        // =====================================================

        AppendInt(
            builder,
            "Brain_Easy_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Brain_Medium_VitaminRewardClaimed"
        );

        AppendInt(
            builder,
            "Brain_Hard_VitaminRewardClaimed"
        );

        return builder.ToString();
    }

    // =========================================================
    // SNAPSHOT HELPERS
    // =========================================================

    private void AppendInt(
        StringBuilder builder,
        string key,
        int defaultValue = 0
    )
    {
        builder.Append(key);
        builder.Append('=');

        builder.Append(
            PlayerPrefs.GetInt(
                key,
                defaultValue
            )
        );

        builder.Append('|');
    }

    private void AppendFloat(
        StringBuilder builder,
        string key,
        float defaultValue
    )
    {
        builder.Append(key);
        builder.Append('=');

        builder.Append(
            PlayerPrefs
                .GetFloat(
                    key,
                    defaultValue
                )
                .ToString(
                    "R",
                    CultureInfo.InvariantCulture
                )
        );

        builder.Append('|');
    }

    // =========================================================
    // CLOUD SAVE
    // =========================================================

    private void StartCloudSave()
    {
        if (SupabaseManager.Instance == null)
            return;

        if (!SupabaseManager.Instance.IsLoggedIn)
            return;

        if (!SupabaseManager.Instance.IsPlayerDataLoaded)
            return;

        hasPendingSave = false;
        saveInProgress = true;

        Debug.Log(
            "Starting automatic account cloud save..."
        );

        SupabaseManager.Instance.SavePlayerData(
            result =>
            {
                saveInProgress = false;

                if (result == null)
                {
                    Debug.LogError(
                        "Cloud save returned null."
                    );

                    ScheduleRetry();

                    return;
                }

                if (!result.success)
                {
                    Debug.LogError(
                        "Automatic cloud save failed:\n" +
                        result.debugMessage
                    );

                    ScheduleRetry();

                    return;
                }

                lastSnapshot =
                    BuildCurrentSnapshot();

                Debug.Log(
                    "Automatic account cloud save completed."
                );
            }
        );
    }

    // =========================================================
    // RETRY
    // =========================================================

    private void ScheduleRetry()
    {
        hasPendingSave = true;
        saveTimer = 2f;
    }

    // =========================================================
    // RESET
    // =========================================================

    private void ResetWatcher()
    {
        watchingStarted = false;

        lastSnapshot = "";

        hasPendingSave = false;
        saveInProgress = false;

        saveTimer = 0f;
    }
}