
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class LungsLabelsLockAnimator : MonoBehaviour
{
    [Header("Lock Icon")]
    [SerializeField] private RectTransform lockIcon;

    [Header("Unlock Animation")]
    [SerializeField, Min(0.1f)]
    private float unlockDuration = 0.65f;

    [SerializeField] private float lockJumpHeight = 18f;
    [SerializeField] private float lockScaleBoost = 1.18f;

    private CanvasGroup canvasGroup;
    private Coroutine unlockCoroutine;

    private Vector2 initialIconPosition;
    private Vector3 initialIconScale;
    private Quaternion initialIconRotation;

    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        foreach (Graphic graphic in
                 GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        if (lockIcon != null)
        {
            initialIconPosition = lockIcon.anchoredPosition;
            initialIconScale = lockIcon.localScale;
            initialIconRotation = lockIcon.localRotation;
        }

        initialized = true;
    }

    // ==============================================
    // LOCKED STATE
    // ==============================================

    public void ShowLocked()
    {
        Initialize();
        StopUnlock();

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        ResetLockIcon();
    }

    // ==============================================
    // START UNLOCK ANIMATION
    // ==============================================

    public void PlayUnlock()
    {
        Initialize();
        StopUnlock();

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        unlockCoroutine = StartCoroutine(
            UnlockRoutine()
        );
    }

    // ==============================================
    // UNLOCK ANIMATION
    // ==============================================

    private IEnumerator UnlockRoutine()
    {
        float elapsed = 0f;

        while (elapsed < unlockDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / unlockDuration
            );

            float smooth =
                t * t * (3f - 2f * t);

            canvasGroup.alpha = 1f - smooth;

            if (lockIcon != null)
            {
                float pulse = Mathf.Sin(
                    t * Mathf.PI
                );

                lockIcon.localScale =
                    initialIconScale *
                    Mathf.Lerp(
                        1f,
                        lockScaleBoost,
                        pulse
                    );

                lockIcon.anchoredPosition =
                    initialIconPosition +
                    Vector2.up *
                    (lockJumpHeight * pulse);

                lockIcon.localRotation =
                    initialIconRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        8f * pulse
                    );
            }

            yield return null;
        }

        canvasGroup.alpha = 0f;

        ResetLockIcon();

        unlockCoroutine = null;

        gameObject.SetActive(false);
    }

    // ==============================================
    // STOP ANIMATION
    // ==============================================

    private void StopUnlock()
    {
        if (unlockCoroutine == null)
            return;

        StopCoroutine(unlockCoroutine);

        unlockCoroutine = null;
    }

    // ==============================================
    // RESET ICON
    // ==============================================

    private void ResetLockIcon()
    {
        if (lockIcon == null)
            return;

        lockIcon.anchoredPosition =
            initialIconPosition;

        lockIcon.localScale =
            initialIconScale;

        lockIcon.localRotation =
            initialIconRotation;
    }
}
