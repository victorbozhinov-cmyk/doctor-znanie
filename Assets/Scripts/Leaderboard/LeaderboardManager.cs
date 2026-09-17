using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

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

    // =========================================================
    // PUBLIC LOAD
    // =========================================================

    public void LoadLeaderboard(
        Action<LeaderboardResult> onComplete
    )
    {
        StartCoroutine(
            LoadLeaderboardRoutine(
                onComplete
            )
        );
    }

    // =========================================================
    // TEMPORARY TEST
    // =========================================================

    [ContextMenu("Test Load Leaderboard")]
    public void TestLoadLeaderboard()
    {
        LoadLeaderboard(
            result =>
            {
                if (result == null)
                {
                    Debug.LogError(
                        "Leaderboard result is null."
                    );

                    return;
                }

                if (!result.success)
                {
                    Debug.LogError(
                        "Leaderboard test failed:\n" +
                        result.debugMessage
                    );

                    return;
                }

                Debug.Log(
                    "===== LEADERBOARD TOP 5 ====="
                );

                for (int i = 0;
                     i < result.topPlayers.Length;
                     i++)
                {
                    LeaderboardEntry entry =
                        result.topPlayers[i];

                    Debug.Log(
                        (i + 1) +
                        ". " +
                        entry.username +
                        " - " +
                        entry.total_score
                    );
                }

                Debug.Log(
                    "===== CURRENT PLAYER ====="
                );

                if (result.currentPlayer != null)
                {
                    Debug.Log(
                        "Position: " +
                        result.currentPlayerRank
                    );

                    Debug.Log(
                        "Username: " +
                        result.currentPlayer.username
                    );

                    Debug.Log(
                        "Score: " +
                        result.currentPlayer.total_score
                    );

                    Debug.Log(
                        "In Top 5: " +
                        result.currentPlayerInTop5
                    );
                }

                Debug.Log(
                    "===== END LEADERBOARD ====="
                );
            }
        );
    }

    // =========================================================
    // LOAD ROUTINE
    // =========================================================

    private IEnumerator LoadLeaderboardRoutine(
        Action<LeaderboardResult> onComplete
    )
    {
        // =====================================================
        // SUPABASE CHECK
        // =====================================================

        if (SupabaseManager.Instance == null)
        {
            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,

                    debugMessage =
                        "SupabaseManager.Instance is null."
                }
            );

            yield break;
        }

        // =====================================================
        // LOGIN CHECK
        // =====================================================

        if (!SupabaseManager.Instance.IsLoggedIn)
        {
            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,

                    debugMessage =
                        "No logged in user."
                }
            );

            yield break;
        }

        // =====================================================
        // URL
        // =====================================================

        string url =
            SupabaseManager.Instance.ProjectUrl +
            "/rest/v1/profiles" +
            "?select=id,username,total_score,created_at" +
            "&order=total_score.desc,created_at.asc";

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        // =====================================================
        // HEADERS
        // =====================================================

        request.SetRequestHeader(
            "apikey",
            SupabaseManager.Instance.PublishableKey
        );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " +
            SupabaseManager.Instance.AccessToken
        );

        // =====================================================
        // REQUEST
        // =====================================================

        yield return request.SendWebRequest();

        // =====================================================
        // REQUEST ERROR
        // =====================================================

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            string response =
                request.downloadHandler.text;

            Debug.LogError(
                "Leaderboard load failed: " +
                request.responseCode +
                "\n" +
                response
            );

            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,
                    debugMessage = response
                }
            );

            yield break;
        }

        // =====================================================
        // JSON
        // =====================================================

        string responseJson =
            request.downloadHandler.text;

        string wrappedJson =
            "{\"items\":" +
            responseJson +
            "}";

        LeaderboardWrapper wrapper;

        try
        {
            wrapper =
                JsonUtility.FromJson<LeaderboardWrapper>(
                    wrappedJson
                );
        }
        catch (Exception exception)
        {
            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,

                    debugMessage =
                        exception.Message
                }
            );

            yield break;
        }

        // =====================================================
        // VALIDATE
        // =====================================================

        if (wrapper == null ||
            wrapper.items == null)
        {
            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,

                    debugMessage =
                        "Leaderboard data is empty."
                }
            );

            yield break;
        }

        LeaderboardEntry[] allPlayers =
            wrapper.items;

        // =====================================================
        // TOP 5
        // =====================================================

        int topCount =
            Mathf.Min(
                5,
                allPlayers.Length
            );

        LeaderboardEntry[] topPlayers =
            new LeaderboardEntry[topCount];

        for (int i = 0;
             i < topCount;
             i++)
        {
            topPlayers[i] =
                allPlayers[i];
        }

        // =====================================================
        // CURRENT PLAYER
        // =====================================================

        string currentUserId =
            SupabaseManager.Instance.UserId;

        LeaderboardEntry currentPlayer =
            null;

        int currentPlayerRank =
            -1;

        for (int i = 0;
             i < allPlayers.Length;
             i++)
        {
            if (allPlayers[i].id ==
                currentUserId)
            {
                currentPlayer =
                    allPlayers[i];

                currentPlayerRank =
                    i + 1;

                break;
            }
        }

        // =====================================================
        // CURRENT PLAYER NOT FOUND
        // =====================================================

        if (currentPlayer == null)
        {
            onComplete?.Invoke(
                new LeaderboardResult
                {
                    success = false,

                    debugMessage =
                        "Current player was not found in profiles."
                }
            );

            yield break;
        }

        // =====================================================
        // TOP 5 CHECK
        // =====================================================

        bool currentPlayerInTop5 =
            currentPlayerRank >= 1 &&
            currentPlayerRank <= 5;

        // =====================================================
        // RESULT
        // =====================================================

        onComplete?.Invoke(
            new LeaderboardResult
            {
                success = true,

                topPlayers =
                    topPlayers,

                currentPlayer =
                    currentPlayer,

                currentPlayerRank =
                    currentPlayerRank,

                currentPlayerInTop5 =
                    currentPlayerInTop5,

                debugMessage = ""
            }
        );
    }

    // =========================================================
    // LEADERBOARD ENTRY
    // =========================================================

    [Serializable]
    public class LeaderboardEntry
    {
        public string id;
        public string username;
        public int total_score;
        public string created_at;
    }

    // =========================================================
    // LEADERBOARD RESULT
    // =========================================================

    public class LeaderboardResult
    {
        public bool success;

        public LeaderboardEntry[] topPlayers;

        public LeaderboardEntry currentPlayer;

        public int currentPlayerRank;

        public bool currentPlayerInTop5;

        public string debugMessage;
    }

    // =========================================================
    // JSON WRAPPER
    // =========================================================

    [Serializable]
    private class LeaderboardWrapper
    {
        public LeaderboardEntry[] items;
    }
}