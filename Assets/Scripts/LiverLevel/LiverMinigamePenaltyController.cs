using System.Collections;
using TMPro;
using UnityEngine;

public class LiverMinigamePenaltyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LiverMinigameTimer timer;
    [SerializeField] private DoctorMovement doctorMovement;

    [Header("Stun Stars")]
    [SerializeField] private GameObject stunStarsRoot;
    [SerializeField] private RectTransform stunStarsTransform;
    [SerializeField] private CanvasGroup stunStarsCanvasGroup;

    [Header("Time Popup")]
    [SerializeField] private GameObject timePenaltyPopupRoot;
    [SerializeField] private RectTransform timePenaltyPopupTransform;
    [SerializeField] private CanvasGroup timePenaltyPopupCanvasGroup;
    [SerializeField] private TMP_Text timePenaltyPopupText;

    [Header("Popup Animation")]
    [SerializeField] private float popupDuration = 0.9f;
    [SerializeField] private float popupRiseDistance = 45f;

    [Header("Stars Animation")]
    [SerializeField] private float starsBobAmount = 8f;
    [SerializeField] private float starsBobSpeed = 6f;
    [SerializeField] private float starsRotationAmount = 8f;
    [SerializeField] private float starsRotationSpeed = 8f;

    [Header("Easy")]
    [SerializeField] private float easyTimePenalty = 2f;
    [SerializeField] private float easyStunDuration = 0.5f;

    [Header("Medium")]
    [SerializeField] private float mediumTimePenalty = 3f;
    [SerializeField] private float mediumStunDuration = 0.75f;

    [Header("Hard")]
    [SerializeField] private float hardTimePenalty = 4f;
    [SerializeField] private float hardStunDuration = 1f;

    private bool isStunned = false;

    private Coroutine stunCoroutine;
    private Coroutine popupCoroutine;
    private Coroutine starsAnimationCoroutine;

    private Vector2 starsStartAnchoredPosition;
    private Vector2 popupStartAnchoredPosition;

    public bool IsStunned => isStunned;

    private void Awake()
    {
        if (stunStarsRoot != null)
        {
            stunStarsRoot.SetActive(false);
        }

        if (timePenaltyPopupRoot != null)
        {
            timePenaltyPopupRoot.SetActive(false);
        }

        if (stunStarsTransform != null)
        {
            starsStartAnchoredPosition =
                stunStarsTransform.anchoredPosition;
        }

        if (timePenaltyPopupTransform != null)
        {
            popupStartAnchoredPosition =
                timePenaltyPopupTransform.anchoredPosition;
        }
    }

    // =========================================================
    // USEFUL BUBBLE PENALTY
    // =========================================================

    public void ApplyUsefulBubblePenalty()
    {
        GetPenaltyForCurrentDifficulty(
            out float timePenalty,
            out float stunDuration
        );

        if (timer != null)
        {
            timer.AddTime(timePenalty);
        }

        ShowTimePenaltyPopup(timePenalty);

        // Stun-овете не се наслагват.
        if (!isStunned)
        {
            stunCoroutine =
                StartCoroutine(StunDoctor(stunDuration));
        }
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void GetPenaltyForCurrentDifficulty(
        out float timePenalty,
        out float stunDuration
    )
    {
        int difficulty =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0: // Easy
                timePenalty = easyTimePenalty;
                stunDuration = easyStunDuration;
                break;

            case 2: // Hard
                timePenalty = hardTimePenalty;
                stunDuration = hardStunDuration;
                break;

            default: // Medium
                timePenalty = mediumTimePenalty;
                stunDuration = mediumStunDuration;
                break;
        }
    }

    // =========================================================
    // STUN
    // =========================================================

    private IEnumerator StunDoctor(float duration)
    {
        isStunned = true;

        if (doctorMovement != null)
        {
            doctorMovement.enabled = false;
        }

        ShowStunStars();

        yield return new WaitForSeconds(duration);

        HideStunStars();

        if (doctorMovement != null)
        {
            doctorMovement.enabled = true;
        }

        isStunned = false;
        stunCoroutine = null;
    }

    private void ShowStunStars()
    {
        if (stunStarsRoot == null)
        {
            return;
        }

        stunStarsRoot.SetActive(true);

        if (stunStarsTransform != null)
        {
            stunStarsTransform.anchoredPosition =
                starsStartAnchoredPosition;

            stunStarsTransform.localRotation =
                Quaternion.identity;
        }

        if (stunStarsCanvasGroup != null)
        {
            stunStarsCanvasGroup.alpha = 1f;
        }

        if (starsAnimationCoroutine != null)
        {
            StopCoroutine(starsAnimationCoroutine);
        }

        starsAnimationCoroutine =
            StartCoroutine(AnimateStunStars());
    }

    private void HideStunStars()
    {
        if (starsAnimationCoroutine != null)
        {
            StopCoroutine(starsAnimationCoroutine);
            starsAnimationCoroutine = null;
        }

        if (stunStarsRoot != null)
        {
            stunStarsRoot.SetActive(false);
        }

        if (stunStarsCanvasGroup != null)
        {
            stunStarsCanvasGroup.alpha = 0f;
        }

        if (stunStarsTransform != null)
        {
            stunStarsTransform.anchoredPosition =
                starsStartAnchoredPosition;

            stunStarsTransform.localRotation =
                Quaternion.identity;
        }
    }

    private IEnumerator AnimateStunStars()
    {
        while (true)
        {
            float bob =
                Mathf.Sin(Time.unscaledTime * starsBobSpeed) *
                starsBobAmount;

            float rotation =
                Mathf.Sin(Time.unscaledTime * starsRotationSpeed) *
                starsRotationAmount;

            if (stunStarsTransform != null)
            {
                stunStarsTransform.anchoredPosition =
                    starsStartAnchoredPosition +
                    new Vector2(0f, bob);

                stunStarsTransform.localRotation =
                    Quaternion.Euler(0f, 0f, rotation);
            }

            yield return null;
        }
    }

    // =========================================================
    // TIME POPUP
    // =========================================================

    private void ShowTimePenaltyPopup(float penaltySeconds)
    {
        if (timePenaltyPopupRoot == null ||
            timePenaltyPopupTransform == null ||
            timePenaltyPopupCanvasGroup == null ||
            timePenaltyPopupText == null)
        {
            return;
        }

        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        string penaltyText =
            $"+{Mathf.RoundToInt(penaltySeconds)} сек.";

        timePenaltyPopupText.text = penaltyText;

        popupCoroutine =
            StartCoroutine(AnimateTimePenaltyPopup());
    }

    private IEnumerator AnimateTimePenaltyPopup()
    {
        timePenaltyPopupRoot.SetActive(true);

        timePenaltyPopupTransform.anchoredPosition =
            popupStartAnchoredPosition;

        timePenaltyPopupTransform.localScale = Vector3.one;

        timePenaltyPopupCanvasGroup.alpha = 1f;

        float timer = 0f;

        Vector2 startPos = popupStartAnchoredPosition;
        Vector2 endPos =
            popupStartAnchoredPosition +
            new Vector2(0f, popupRiseDistance);

        while (timer < popupDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(timer / popupDuration);

            float smoothT =
                1f - Mathf.Pow(1f - t, 3f);

            timePenaltyPopupTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, smoothT);

            float scale =
                1f + Mathf.Sin(t * Mathf.PI) * 0.12f;

            timePenaltyPopupTransform.localScale =
                Vector3.one * scale;

            timePenaltyPopupCanvasGroup.alpha =
                Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        timePenaltyPopupCanvasGroup.alpha = 0f;
        timePenaltyPopupTransform.anchoredPosition =
            popupStartAnchoredPosition;
        timePenaltyPopupTransform.localScale = Vector3.one;

        timePenaltyPopupRoot.SetActive(false);

        popupCoroutine = null;
    }

    // =========================================================
    // SAFETY
    // =========================================================

    private void OnDisable()
    {
        if (doctorMovement != null)
        {
            doctorMovement.enabled = true;
        }

        HideStunStars();

        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }

        if (timePenaltyPopupRoot != null)
        {
            timePenaltyPopupRoot.SetActive(false);
        }

        if (timePenaltyPopupCanvasGroup != null)
        {
            timePenaltyPopupCanvasGroup.alpha = 0f;
        }

        if (timePenaltyPopupTransform != null)
        {
            timePenaltyPopupTransform.anchoredPosition =
                popupStartAnchoredPosition;

            timePenaltyPopupTransform.localScale = Vector3.one;
        }

        isStunned = false;
        stunCoroutine = null;
    }
}