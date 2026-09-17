using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ProfileTotalScoreSync : MonoBehaviour
{
    public static ProfileTotalScoreSync Instance { get; private set; }

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Score Sync")]
    [SerializeField]
    [Min(0.1f)]
    private float syncDelay = 1f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private int lastKnownTotalScore = -1;

    private bool watchingStarted = false;
    private bool syncPending = false;
    private bool syncInProgress = false;

    private float syncTimer = 0f;

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

        int currentTotalScore =
            CalculateTotalBestScore();

        // =====================================================
        // FIRST SYNC AFTER LOGIN
        // =====================================================

        if (!watchingStarted)
        {
            lastKnownTotalScore =
                currentTotalScore;

            watchingStarted = true;

            // Винаги правим една синхронизация при вход.
            // Така стар профил с total_score = 0
            // ще бъде поправен автоматично.
            ScheduleSync();

            Debug.Log(
                "ProfileTotalScoreSync started. Total score: " +
                currentTotalScore
            );

            return;
        }

        // =====================================================
        // SCORE CHANGED
        // =====================================================

        if (currentTotalScore !=
            lastKnownTotalScore)
        {
            lastKnownTotalScore =
                currentTotalScore;

            ScheduleSync();

            Debug.Log(
                "Total best score changed: " +
                currentTotalScore
            );
        }

        // =====================================================
        // WAIT BEFORE SYNC
        // =====================================================

        if (!syncPending)
            return;

        if (syncInProgress)
            return;

        syncTimer -=
            Time.unscaledDeltaTime;

        if (syncTimer > 0f)
            return;

        StartCoroutine(
            SyncTotalScoreRoutine()
        );
    }

    // =========================================================
    // CALCULATE TOTAL SCORE
    // =========================================================

    public int CalculateTotalBestScore()
    {
        int heart =
            PlayerPrefs.GetInt(
                "HeartBestAntibodies",
                0
            );

        int stomach =
            PlayerPrefs.GetInt(
                "StomachBestAntibodies",
                0
            );

        int liver =
            PlayerPrefs.GetInt(
                "LiverBestAntibodies",
                0
            );

        int brain =
            PlayerPrefs.GetInt(
                "BrainBestAntibodies",
                0
            );

        int lungs =
            PlayerPrefs.GetInt(
                "LungsBestAntibodies",
                0
            );

        return
            heart +
            stomach +
            liver +
            brain +
            lungs;
    }

    // =========================================================
    // SCHEDULE
    // =========================================================

    private void ScheduleSync()
    {
        syncPending = true;
        syncTimer = syncDelay;
    }

    // =========================================================
    // SYNC TO SUPABASE
    // =========================================================

    private IEnumerator SyncTotalScoreRoutine()
    {
        SupabaseManager supabase =
            SupabaseManager.Instance;

        if (supabase == null)
            yield break;

        if (!supabase.IsLoggedIn)
            yield break;

        if (!supabase.IsPlayerDataLoaded)
            yield break;

        syncPending = false;
        syncInProgress = true;

        int totalScore =
            CalculateTotalBestScore();

        ProfileScoreUpdateRequest data =
            new ProfileScoreUpdateRequest
            {
                total_score = totalScore
            };

        string json =
            JsonUtility.ToJson(data);

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        string url =
            supabase.ProjectUrl +
            "/rest/v1/profiles" +
            "?id=eq." +
            UnityWebRequest.EscapeURL(
                supabase.UserId
            );

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

        request.SetRequestHeader(
            "apikey",
            supabase.PublishableKey
        );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " +
            supabase.AccessToken
        );

        request.SetRequestHeader(
            "Prefer",
            "return=minimal"
        );

        yield return request.SendWebRequest();

        syncInProgress = false;

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Profile total score sync failed: " +
                request.responseCode +
                "\n" +
                request.downloadHandler.text
            );

            // Опитваме пак след малко.
            syncPending = true;
            syncTimer = 2f;

            yield break;
        }

        lastKnownTotalScore =
            totalScore;

        Debug.Log(
            "Profile total score synced successfully: " +
            totalScore
        );
    }

    // =========================================================
    // RESET
    // =========================================================

    private void ResetWatcher()
    {
        watchingStarted = false;

        lastKnownTotalScore = -1;

        syncPending = false;
        syncInProgress = false;

        syncTimer = 0f;
    }

    // =========================================================
    // JSON
    // =========================================================

    [System.Serializable]
    private class ProfileScoreUpdateRequest
    {
        public int total_score;
    }
}
