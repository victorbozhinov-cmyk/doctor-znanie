using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRowUI : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Images")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image badgeImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;

    // =========================================================
    // BACKGROUND SPRITES
    // =========================================================

    [Header("Background Sprites")]
    [SerializeField] private Sprite normalRowSprite;
    [SerializeField] private Sprite currentPlayerRowSprite;

    // =========================================================
    // BADGE SPRITES
    // =========================================================

    [Header("Badge Sprites")]
    [SerializeField] private Sprite normalBadgeSprite;
    [SerializeField] private Sprite topThreeBadgeSprite;

    // =========================================================
    // BADGE COLORS
    // =========================================================

    [Header("Badge Colors")]
    [SerializeField] private Color firstPlaceColor =
        new Color(1f, 0.72f, 0.08f, 1f);

    [SerializeField] private Color secondPlaceColor =
        new Color(0.75f, 0.85f, 1f, 1f);

    [SerializeField] private Color thirdPlaceColor =
        new Color(0.80f, 0.45f, 0.20f, 1f);

    [SerializeField] private Color normalBadgeColor =
        Color.white;

    // =========================================================
    // CHANGE ANIMATION
    // =========================================================

    [Header("Change Animation")]
    [SerializeField]
    [Min(0.05f)]
    private float fadeOutDuration = 0.08f;

    [SerializeField]
    [Min(0.05f)]
    private float fadeInDuration = 0.12f;

    [SerializeField]
    [Min(0f)]
    private float slideDistance = 12f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private Vector2 normalPosition;

    private Coroutine animationCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        if (rectTransform != null)
        {
            normalPosition =
                rectTransform.anchoredPosition;
        }

        canvasGroup.alpha = 1f;
    }

    // =========================================================
    // SET DATA IMMEDIATELY
    // =========================================================

    public void SetData(
        int rank,
        string username,
        int score,
        bool isCurrentPlayer
    )
    {
        StopCurrentAnimation();

        gameObject.SetActive(true);

        ResetVisualTransform();

        ApplyData(
            rank,
            username,
            score,
            isCurrentPlayer
        );
    }

    // =========================================================
    // ANIMATED DATA CHANGE
    // =========================================================

    public void AnimateToData(
        int rank,
        string username,
        int score,
        bool isCurrentPlayer
    )
    {
        StopCurrentAnimation();

        gameObject.SetActive(true);

        animationCoroutine =
            StartCoroutine(
                AnimateToDataRoutine(
                    rank,
                    username,
                    score,
                    isCurrentPlayer
                )
            );
    }

    private IEnumerator AnimateToDataRoutine(
        int rank,
        string username,
        int score,
        bool isCurrentPlayer
    )
    {
        // =====================================================
        // FADE OLD PLAYER OUT
        // =====================================================

        float elapsed = 0f;

        Vector2 startPosition =
            rectTransform != null
                ? rectTransform.anchoredPosition
                : normalPosition;

        Vector2 fadeOutTarget =
            normalPosition +
            Vector2.left * slideDistance;

        while (elapsed < fadeOutDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeOutDuration
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    Vector2.Lerp(
                        startPosition,
                        fadeOutTarget,
                        progress
                    );
            }

            yield return null;
        }

        canvasGroup.alpha = 0f;

        // =====================================================
        // CHANGE CONTENT WHILE INVISIBLE
        // =====================================================

        ApplyData(
            rank,
            username,
            score,
            isCurrentPlayer
        );

        Vector2 fadeInStart =
            normalPosition +
            Vector2.right * slideDistance;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                fadeInStart;
        }

        // =====================================================
        // FADE NEW PLAYER IN
        // =====================================================

        elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeInDuration
                );

            float smoothProgress =
                progress *
                progress *
                (3f - 2f * progress);

            canvasGroup.alpha =
                smoothProgress;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    Vector2.Lerp(
                        fadeInStart,
                        normalPosition,
                        smoothProgress
                    );
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;

        ResetVisualTransform();

        animationCoroutine = null;
    }

    // =========================================================
    // ANIMATED SHOW
    // =========================================================

    public void AnimateShow(
        int rank,
        string username,
        int score,
        bool isCurrentPlayer
    )
    {
        StopCurrentAnimation();

        gameObject.SetActive(true);

        ApplyData(
            rank,
            username,
            score,
            isCurrentPlayer
        );

        animationCoroutine =
            StartCoroutine(
                AnimateShowRoutine()
            );
    }

    private IEnumerator AnimateShowRoutine()
    {
        canvasGroup.alpha = 0f;

        Vector2 startPosition =
            normalPosition +
            Vector2.right * slideDistance;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                startPosition;
        }

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeInDuration
                );

            float smoothProgress =
                progress *
                progress *
                (3f - 2f * progress);

            canvasGroup.alpha =
                smoothProgress;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    Vector2.Lerp(
                        startPosition,
                        normalPosition,
                        smoothProgress
                    );
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;

        ResetVisualTransform();

        animationCoroutine = null;
    }

    // =========================================================
    // ANIMATED HIDE
    // =========================================================

    public void AnimateHide()
    {
        if (!gameObject.activeSelf)
            return;

        StopCurrentAnimation();

        animationCoroutine =
            StartCoroutine(
                AnimateHideRoutine()
            );
    }

    private IEnumerator AnimateHideRoutine()
    {
        float elapsed = 0f;

        Vector2 startPosition =
            normalPosition;

        Vector2 targetPosition =
            normalPosition +
            Vector2.left * slideDistance;

        while (elapsed < fadeOutDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeOutDuration
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    Vector2.Lerp(
                        startPosition,
                        targetPosition,
                        progress
                    );
            }

            yield return null;
        }

        gameObject.SetActive(false);

        canvasGroup.alpha = 1f;

        ResetVisualTransform();

        animationCoroutine = null;
    }

    // =========================================================
    // APPLY DATA
    // =========================================================

    private void ApplyData(
        int rank,
        string username,
        int score,
        bool isCurrentPlayer
    )
    {
        // =====================================================
        // TEXT
        // =====================================================

        if (rankText != null)
        {
            rankText.text =
                rank.ToString();
        }

        if (usernameText != null)
        {
            usernameText.text =
                username;
        }

        if (scoreText != null)
        {
            scoreText.text =
                score.ToString();
        }

        // =====================================================
        // BACKGROUND
        // =====================================================

        if (backgroundImage != null)
        {
            if (isCurrentPlayer &&
                currentPlayerRowSprite != null)
            {
                backgroundImage.sprite =
                    currentPlayerRowSprite;
            }
            else if (normalRowSprite != null)
            {
                backgroundImage.sprite =
                    normalRowSprite;
            }

            backgroundImage.color =
                Color.white;
        }

        // =====================================================
        // BADGE
        // =====================================================

        UpdateBadge(rank);
    }

    // =========================================================
    // BADGE
    // =========================================================

    private void UpdateBadge(
        int rank
    )
    {
        if (badgeImage == null)
            return;

        switch (rank)
        {
            case 1:

                if (topThreeBadgeSprite != null)
                {
                    badgeImage.sprite =
                        topThreeBadgeSprite;
                }

                badgeImage.color =
                    firstPlaceColor;

                break;

            case 2:

                if (topThreeBadgeSprite != null)
                {
                    badgeImage.sprite =
                        topThreeBadgeSprite;
                }

                badgeImage.color =
                    secondPlaceColor;

                break;

            case 3:

                if (topThreeBadgeSprite != null)
                {
                    badgeImage.sprite =
                        topThreeBadgeSprite;
                }

                badgeImage.color =
                    thirdPlaceColor;

                break;

            default:

                if (normalBadgeSprite != null)
                {
                    badgeImage.sprite =
                        normalBadgeSprite;
                }

                badgeImage.color =
                    normalBadgeColor;

                break;
        }
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearRow()
    {
        StopCurrentAnimation();

        if (rankText != null)
        {
            rankText.text = "";
        }

        if (usernameText != null)
        {
            usernameText.text = "";
        }

        if (scoreText != null)
        {
            scoreText.text = "";
        }

        ResetVisualTransform();

        gameObject.SetActive(false);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void StopCurrentAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }
    }

    private void ResetVisualTransform()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                normalPosition;
        }
    }
}