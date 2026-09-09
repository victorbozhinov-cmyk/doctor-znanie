using System.Collections;
using UnityEngine;

public class OrganUnlockAnimation : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject lockedObject;
    [SerializeField] private GameObject unlockedObject;

    [Header("Timing")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float revealDuration = 0.3f;

    [Header("Shake")]
    [SerializeField] private float shakeStrength = 8f;

    [Header("Reveal Scale")]
    [SerializeField] private float revealStartScale = 0.85f;

    private RectTransform lockedRect;
    private RectTransform unlockedRect;

    private CanvasGroup lockedCanvasGroup;
    private CanvasGroup unlockedCanvasGroup;

    private Vector2 lockedStartPosition;
    private Vector3 unlockedOriginalScale;

    private bool isPlaying;

    private void Awake()
    {
        if (lockedObject != null)
        {
            lockedRect = lockedObject.GetComponent<RectTransform>();

            lockedCanvasGroup = lockedObject.GetComponent<CanvasGroup>();

            if (lockedCanvasGroup == null)
            {
                lockedCanvasGroup =
                    lockedObject.AddComponent<CanvasGroup>();
            }
        }

        if (unlockedObject != null)
        {
            unlockedRect =
                unlockedObject.GetComponent<RectTransform>();

            unlockedCanvasGroup =
                unlockedObject.GetComponent<CanvasGroup>();

            if (unlockedCanvasGroup == null)
            {
                unlockedCanvasGroup =
                    unlockedObject.AddComponent<CanvasGroup>();
            }

            unlockedOriginalScale =
                unlockedRect.localScale;
        }
    }

    public void PlayUnlockAnimation()
    {
        if (isPlaying)
        {
            return;
        }

        StartCoroutine(UnlockRoutine());
    }

    private IEnumerator UnlockRoutine()
    {
        isPlaying = true;

        if (lockedObject == null ||
            unlockedObject == null)
        {
            isPlaying = false;
            yield break;
        }

        lockedObject.SetActive(true);

        unlockedObject.SetActive(false);

        lockedCanvasGroup.alpha = 1f;

        if (lockedRect != null)
        {
            lockedStartPosition =
                lockedRect.anchoredPosition;
        }

        // =========================
        // SHAKE
        // =========================

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (lockedRect != null)
            {
                Vector2 offset =
                    Random.insideUnitCircle *
                    shakeStrength;

                lockedRect.anchoredPosition =
                    lockedStartPosition + offset;
            }

            yield return null;
        }

        if (lockedRect != null)
        {
            lockedRect.anchoredPosition =
                lockedStartPosition;
        }

        // =========================
        // LOCKED FADE OUT
        // =========================

        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            lockedCanvasGroup.alpha =
                Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        lockedCanvasGroup.alpha = 0f;

        lockedObject.SetActive(false);

        // =========================
        // UNLOCKED REVEAL
        // =========================

        unlockedObject.SetActive(true);

        unlockedCanvasGroup.alpha = 0f;

        if (unlockedRect != null)
        {
            unlockedRect.localScale =
                unlockedOriginalScale *
                revealStartScale;
        }

        timer = 0f;

        while (timer < revealDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / revealDuration
                );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            unlockedCanvasGroup.alpha =
                smoothT;

            if (unlockedRect != null)
            {
                unlockedRect.localScale =
                    Vector3.Lerp(
                        unlockedOriginalScale *
                        revealStartScale,
                        unlockedOriginalScale,
                        smoothT
                    );
            }

            yield return null;
        }

        unlockedCanvasGroup.alpha = 1f;

        if (unlockedRect != null)
        {
            unlockedRect.localScale =
                unlockedOriginalScale;
        }

        isPlaying = false;
    }
}
