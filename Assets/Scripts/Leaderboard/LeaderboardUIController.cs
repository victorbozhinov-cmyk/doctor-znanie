using System.Collections;
using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    // =========================================================
    // ROWS
    // =========================================================

    [Header("Top 5 Rows")]
    [SerializeField] private LeaderboardRowUI[] topFiveRows;

    [Header("Current Player Extra Row")]
    [SerializeField] private LeaderboardRowUI currentPlayerRow;

    // =========================================================
    // ROWS CONTAINER
    // =========================================================

    [Header("Rows Container")]
    [SerializeField] private RectTransform rowsContainer;
    [SerializeField] private CanvasGroup rowsCanvasGroup;

    // =========================================================
    // INITIAL ANIMATION
    // =========================================================

    [Header("Initial Animation")]
    [SerializeField]
    [Min(0.05f)]
    private float initialRevealDuration = 0.25f;

    [SerializeField]
    [Min(0f)]
    private float initialSlideDistance = 12f;

    // =========================================================
    // AUTO REFRESH
    // =========================================================

    [Header("Auto Refresh")]
    [SerializeField]
    [Min(1f)]
    private float refreshInterval = 10f;

    [SerializeField]
    [Min(0.5f)]
    private float ownScoreRefreshDelay = 2f;

    [SerializeField]
    [Min(0.5f)]
    private float startupFollowUpDelay = 2f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private bool isLoading = false;
    private bool firstSuccessfulLoad = true;

    private int lastKnownOwnScore = -1;

    private bool ownScoreRefreshPending = false;
    private float ownScoreRefreshTimer = 0f;

    private Coroutine autoRefreshCoroutine;
    private Coroutine initialRevealCoroutine;
    private Coroutine startupFollowUpCoroutine;

    private bool initialRevealDone = false;

    private Vector2 rowsTargetPosition;

    // =========================================================
    // PREVIOUS LEADERBOARD STATE
    // =========================================================

    private string[] previousTopPlayerIds =
        new string[5];

    private bool previousExtraRowVisible = false;
    private int previousExtraPlayerRank = -1;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (rowsContainer != null)
        {
            rowsTargetPosition =
                rowsContainer.anchoredPosition;

            rowsContainer.anchoredPosition =
                rowsTargetPosition +
                Vector2.down *
                initialSlideDistance;
        }

        if (rowsCanvasGroup != null)
        {
            rowsCanvasGroup.alpha = 0f;

            rowsCanvasGroup.interactable =
                false;

            rowsCanvasGroup.blocksRaycasts =
                false;
        }

        initialRevealDone = false;
        firstSuccessfulLoad = true;
    }

    private void Start()
    {
        lastKnownOwnScore =
            CalculateOwnTotalScore();

        LoadLeaderboard();

        startupFollowUpCoroutine =
            StartCoroutine(
                StartupFollowUpRefreshRoutine()
            );

        autoRefreshCoroutine =
            StartCoroutine(
                AutoRefreshRoutine()
            );
    }

    private void Update()
    {
        CheckOwnScoreChange();
    }

    private void OnDestroy()
    {
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(
                autoRefreshCoroutine
            );

            autoRefreshCoroutine = null;
        }

        if (initialRevealCoroutine != null)
        {
            StopCoroutine(
                initialRevealCoroutine
            );

            initialRevealCoroutine = null;
        }

        if (startupFollowUpCoroutine != null)
        {
            StopCoroutine(
                startupFollowUpCoroutine
            );

            startupFollowUpCoroutine = null;
        }
    }

    // =========================================================
    // STARTUP FOLLOW-UP REFRESH
    // =========================================================

    private IEnumerator StartupFollowUpRefreshRoutine()
    {
        yield return new WaitForSecondsRealtime(
            startupFollowUpDelay
        );

        while (isLoading)
        {
            yield return null;
        }

        Debug.Log(
            "Startup leaderboard follow-up refresh."
        );

        LoadLeaderboard();

        startupFollowUpCoroutine = null;
    }

    // =========================================================
    // OWN SCORE CHANGE
    // =========================================================

    private void CheckOwnScoreChange()
    {
        int currentOwnScore =
            CalculateOwnTotalScore();

        if (currentOwnScore !=
            lastKnownOwnScore)
        {
            lastKnownOwnScore =
                currentOwnScore;

            ownScoreRefreshPending = true;

            ownScoreRefreshTimer =
                ownScoreRefreshDelay;

            Debug.Log(
                "Own leaderboard score changed: " +
                currentOwnScore +
                ". Leaderboard refresh scheduled."
            );
        }

        if (ownScoreRefreshPending)
        {
            UpdateOwnScoreRefreshTimer();
        }
    }

    // =========================================================
    // OWN SCORE TIMER
    // =========================================================

    private void UpdateOwnScoreRefreshTimer()
    {
        if (!ownScoreRefreshPending)
            return;

        ownScoreRefreshTimer -=
            Time.unscaledDeltaTime;

        if (ownScoreRefreshTimer > 0f)
            return;

        ownScoreRefreshPending = false;

        LoadLeaderboard();
    }

    // =========================================================
    // OWN TOTAL SCORE
    // =========================================================

    private int CalculateOwnTotalScore()
    {
        return
            PlayerPrefs.GetInt(
                "HeartBestAntibodies",
                0
            )
            +
            PlayerPrefs.GetInt(
                "StomachBestAntibodies",
                0
            )
            +
            PlayerPrefs.GetInt(
                "LiverBestAntibodies",
                0
            )
            +
            PlayerPrefs.GetInt(
                "BrainBestAntibodies",
                0
            )
            +
            PlayerPrefs.GetInt(
                "LungsBestAntibodies",
                0
            );
    }

    // =========================================================
    // AUTO REFRESH
    // =========================================================

    private IEnumerator AutoRefreshRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                refreshInterval
            );

            LoadLeaderboard();
        }
    }

    // =========================================================
    // LOAD
    // =========================================================

    public void LoadLeaderboard()
    {
        if (isLoading)
            return;

        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError(
                "LeaderboardUIController: " +
                "LeaderboardManager.Instance is null."
            );

            return;
        }

        if (SupabaseManager.Instance == null)
        {
            Debug.LogError(
                "LeaderboardUIController: " +
                "SupabaseManager.Instance is null."
            );

            return;
        }

        if (!SupabaseManager.Instance.IsLoggedIn)
        {
            return;
        }

        isLoading = true;

        LeaderboardManager.Instance.LoadLeaderboard(
            result =>
            {
                isLoading = false;

                HandleLeaderboardResult(
                    result
                );
            }
        );
    }

    // =========================================================
    // RESULT
    // =========================================================

    private void HandleLeaderboardResult(
        LeaderboardManager.LeaderboardResult result
    )
    {
        if (result == null)
            return;

        if (!result.success)
        {
            Debug.LogError(
                "Could not load leaderboard:\n" +
                result.debugMessage
            );

            return;
        }

        if (firstSuccessfulLoad)
        {
            ApplyFirstLeaderboardLoad(
                result
            );

            SaveCurrentState(
                result
            );

            firstSuccessfulLoad = false;

            StartInitialReveal();

            return;
        }

        ApplyLeaderboardRefresh(
            result
        );

        SaveCurrentState(
            result
        );
    }

    // =========================================================
    // FIRST LOAD
    // =========================================================

    private void ApplyFirstLeaderboardLoad(
        LeaderboardManager.LeaderboardResult result
    )
    {
        for (int i = 0;
             i < topFiveRows.Length;
             i++)
        {
            LeaderboardRowUI row =
                topFiveRows[i];

            if (row == null)
                continue;

            if (result.topPlayers == null ||
                i >= result.topPlayers.Length)
            {
                row.ClearRow();
                continue;
            }

            LeaderboardManager.LeaderboardEntry entry =
                result.topPlayers[i];

            bool isCurrentPlayer =
                entry.id ==
                SupabaseManager.Instance.UserId;

            row.SetData(
                i + 1,
                entry.username,
                entry.total_score,
                isCurrentPlayer
            );
        }

        if (currentPlayerRow == null)
            return;

        if (result.currentPlayerInTop5 ||
            result.currentPlayer == null)
        {
            currentPlayerRow.ClearRow();

            return;
        }

        currentPlayerRow.SetData(
            result.currentPlayerRank,
            result.currentPlayer.username,
            result.currentPlayer.total_score,
            true
        );
    }

    // =========================================================
    // REFRESH EXISTING LEADERBOARD
    // =========================================================

    private void ApplyLeaderboardRefresh(
        LeaderboardManager.LeaderboardResult result
    )
    {
        for (int i = 0;
             i < topFiveRows.Length;
             i++)
        {
            LeaderboardRowUI row =
                topFiveRows[i];

            if (row == null)
                continue;

            bool hasNewPlayer =
                result.topPlayers != null &&
                i < result.topPlayers.Length;

            string previousId =
                i < previousTopPlayerIds.Length
                    ? previousTopPlayerIds[i]
                    : "";

            if (!hasNewPlayer)
            {
                if (!string.IsNullOrEmpty(
                    previousId))
                {
                    row.AnimateHide();
                }

                continue;
            }

            LeaderboardManager.LeaderboardEntry entry =
                result.topPlayers[i];

            bool isCurrentPlayer =
                entry.id ==
                SupabaseManager.Instance.UserId;

            if (previousId !=
                entry.id)
            {
                if (string.IsNullOrEmpty(
                    previousId))
                {
                    row.AnimateShow(
                        i + 1,
                        entry.username,
                        entry.total_score,
                        isCurrentPlayer
                    );
                }
                else
                {
                    row.AnimateToData(
                        i + 1,
                        entry.username,
                        entry.total_score,
                        isCurrentPlayer
                    );
                }
            }
            else
            {
                row.SetData(
                    i + 1,
                    entry.username,
                    entry.total_score,
                    isCurrentPlayer
                );
            }
        }

        if (currentPlayerRow == null)
            return;

        bool shouldShowExtraRow =
            !result.currentPlayerInTop5 &&
            result.currentPlayer != null;

        if (shouldShowExtraRow &&
            !previousExtraRowVisible)
        {
            currentPlayerRow.AnimateShow(
                result.currentPlayerRank,
                result.currentPlayer.username,
                result.currentPlayer.total_score,
                true
            );

            return;
        }

        if (!shouldShowExtraRow &&
            previousExtraRowVisible)
        {
            currentPlayerRow.AnimateHide();

            return;
        }

        if (shouldShowExtraRow)
        {
            if (result.currentPlayerRank !=
                previousExtraPlayerRank)
            {
                currentPlayerRow.AnimateToData(
                    result.currentPlayerRank,
                    result.currentPlayer.username,
                    result.currentPlayer.total_score,
                    true
                );
            }
            else
            {
                currentPlayerRow.SetData(
                    result.currentPlayerRank,
                    result.currentPlayer.username,
                    result.currentPlayer.total_score,
                    true
                );
            }
        }
    }

    // =========================================================
    // SAVE STATE FOR NEXT REFRESH
    // =========================================================

    private void SaveCurrentState(
        LeaderboardManager.LeaderboardResult result
    )
    {
        for (int i = 0;
             i < previousTopPlayerIds.Length;
             i++)
        {
            if (result.topPlayers != null &&
                i < result.topPlayers.Length)
            {
                previousTopPlayerIds[i] =
                    result.topPlayers[i].id;
            }
            else
            {
                previousTopPlayerIds[i] =
                    "";
            }
        }

        previousExtraRowVisible =
            !result.currentPlayerInTop5 &&
            result.currentPlayer != null;

        if (previousExtraRowVisible)
        {
            previousExtraPlayerRank =
                result.currentPlayerRank;
        }
        else
        {
            previousExtraPlayerRank =
                -1;
        }
    }

    // =========================================================
    // INITIAL REVEAL
    // =========================================================

    private void StartInitialReveal()
    {
        if (initialRevealDone)
            return;

        initialRevealDone = true;

        if (initialRevealCoroutine != null)
        {
            StopCoroutine(
                initialRevealCoroutine
            );
        }

        initialRevealCoroutine =
            StartCoroutine(
                InitialRevealRoutine()
            );
    }

    private IEnumerator InitialRevealRoutine()
    {
        if (rowsCanvasGroup == null)
            yield break;

        float elapsed = 0f;

        Vector2 startPosition =
            rowsTargetPosition +
            Vector2.down *
            initialSlideDistance;

        while (elapsed <
               initialRevealDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    initialRevealDuration
                );

            float smooth =
                progress *
                progress *
                (3f - 2f * progress);

            rowsCanvasGroup.alpha =
                smooth;

            if (rowsContainer != null)
            {
                rowsContainer.anchoredPosition =
                    Vector2.Lerp(
                        startPosition,
                        rowsTargetPosition,
                        smooth
                    );
            }

            yield return null;
        }

        rowsCanvasGroup.alpha = 1f;

        if (rowsContainer != null)
        {
            rowsContainer.anchoredPosition =
                rowsTargetPosition;
        }

        rowsCanvasGroup.interactable =
            true;

        rowsCanvasGroup.blocksRaycasts =
            true;

        initialRevealCoroutine = null;
    }

    // =========================================================
    // MANUAL REFRESH
    // =========================================================

    [ContextMenu("Refresh Leaderboard")]
    public void RefreshLeaderboard()
    {
        LoadLeaderboard();
    }
}