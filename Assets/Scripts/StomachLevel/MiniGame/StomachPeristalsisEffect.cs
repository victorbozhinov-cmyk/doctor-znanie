using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StomachPeristalsisEffect : MonoBehaviour
{
    [Header("Wave References")]
    [SerializeField] private RectTransform waveTop;
    [SerializeField] private RectTransform waveMidLeft;
    [SerializeField] private RectTransform waveMidRight;
    [SerializeField] private RectTransform waveBot;

    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup waveTopCanvasGroup;
    [SerializeField] private CanvasGroup waveMidLeftCanvasGroup;
    [SerializeField] private CanvasGroup waveMidRightCanvasGroup;
    [SerializeField] private CanvasGroup waveBotCanvasGroup;

    [Header("Food")]
    [SerializeField] private RectTransform stomachContents;

    [Header("Animation Timing")]
    [SerializeField] private float pushDuration = 0.28f;
    [SerializeField] private float returnDuration = 0.28f;
    [SerializeField] private float delayBetweenSections = 0.08f;

    [Header("TOP Movement")]
    [SerializeField] private Vector2 topPush = new Vector2(0f, -35f);

    [Header("MIDDLE Movement")]
    [SerializeField] private Vector2 midLeftPush = new Vector2(28f, -4f);
    [SerializeField] private Vector2 midRightPush = new Vector2(-28f, -4f);

    [Header("BOTTOM Movement")]
    [SerializeField] private Vector2 bottomPush = new Vector2(-18f, 30f);

    [Header("Wave Scale")]
    [SerializeField] private Vector2 startWaveScale = new Vector2(0.90f, 0.90f);
    [SerializeField] private Vector2 peakWaveScale = new Vector2(1f, 1f);

    [Header("Food Reaction - TOP")]
    [SerializeField] private Vector2 topFoodMove = new Vector2(0f, -5f);
    [SerializeField] private Vector2 topFoodScale = new Vector2(1.01f, 0.98f);
    [SerializeField] private float topFoodRotation = 0.4f;

    [Header("Food Reaction - MIDDLE")]
    [SerializeField] private Vector2 middleFoodMove = new Vector2(-6f, -5f);
    [SerializeField] private Vector2 middleFoodScale = new Vector2(1.01f, 0.98f);
    [SerializeField] private float middleFoodRotation = -0.5f;

    [Header("Food Reaction - BOTTOM")]
    [SerializeField] private Vector2 bottomFoodMove = new Vector2(-9f, -2f);
    [SerializeField] private Vector2 bottomFoodScale = new Vector2(1.01f, 0.98f);
    [SerializeField] private float bottomFoodRotation = -0.2f;

    [Header("Testing")]
    [SerializeField] private bool enableTestKeys = true;

    // Original wave positions
    private Vector2 topOriginalPosition;
    private Vector2 midLeftOriginalPosition;
    private Vector2 midRightOriginalPosition;
    private Vector2 bottomOriginalPosition;

    // Original wave scales
    private Vector3 topOriginalScale;
    private Vector3 midLeftOriginalScale;
    private Vector3 midRightOriginalScale;
    private Vector3 bottomOriginalScale;

    // Original food values
    private Vector2 originalFoodPosition;
    private Vector3 originalFoodScale;
    private Quaternion originalFoodRotation;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        SaveOriginalValues();
        HideAllWaves();
    }

    private void Update()
    {
        if (!enableTestKeys || Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            PlayTop();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            PlayMiddle();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            PlayBottom();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            PlayFullWave();
        }
    }

    // =========================================================
    // PUBLIC TEST METHODS
    // =========================================================

    public void PlayTop()
    {
        StopCurrentAnimation();

        activeCoroutine = StartCoroutine(
            TopRoutine()
        );
    }

    public void PlayMiddle()
    {
        StopCurrentAnimation();

        activeCoroutine = StartCoroutine(
            MiddleRoutine()
        );
    }

    public void PlayBottom()
    {
        StopCurrentAnimation();

        activeCoroutine = StartCoroutine(
            BottomRoutine()
        );
    }

    public void PlayFullWave()
    {
        StopCurrentAnimation();

        activeCoroutine = StartCoroutine(
            FullWaveRoutine()
        );
    }

    // =========================================================
    // TOP
    // =========================================================

    private IEnumerator TopRoutine()
    {
        yield return StartCoroutine(
            AnimateSingleWave(
                waveTop,
                waveTopCanvasGroup,
                topOriginalPosition,
                topOriginalScale,
                topPush,
                topFoodMove,
                topFoodScale,
                topFoodRotation
            )
        );

        activeCoroutine = null;
    }

    // =========================================================
    // MIDDLE - TWO WAVES AT THE SAME TIME
    // =========================================================

    private IEnumerator MiddleRoutine()
    {
        if (waveMidLeft == null ||
            waveMidRight == null ||
            waveMidLeftCanvasGroup == null ||
            waveMidRightCanvasGroup == null)
        {
            Debug.LogWarning(
                "Middle wave references are missing!"
            );

            yield break;
        }

        ResetAllWaves();
        ResetFood();

        // Start positions
        waveMidLeft.anchoredPosition =
            midLeftOriginalPosition;

        waveMidRight.anchoredPosition =
            midRightOriginalPosition;

        Vector3 leftStartScale =
            MultiplyScale(
                midLeftOriginalScale,
                startWaveScale
            );

        Vector3 rightStartScale =
            MultiplyScale(
                midRightOriginalScale,
                startWaveScale
            );

        waveMidLeft.localScale =
            leftStartScale;

        waveMidRight.localScale =
            rightStartScale;

        waveMidLeftCanvasGroup.alpha = 0f;
        waveMidRightCanvasGroup.alpha = 0f;

        // Where they move
        Vector2 leftTarget =
            midLeftOriginalPosition +
            midLeftPush;

        Vector2 rightTarget =
            midRightOriginalPosition +
            midRightPush;

        Vector3 leftPeakScale =
            MultiplyScale(
                midLeftOriginalScale,
                peakWaveScale
            );

        Vector3 rightPeakScale =
            MultiplyScale(
                midRightOriginalScale,
                peakWaveScale
            );

        // Food target
        Vector2 foodTargetPosition =
            originalFoodPosition +
            middleFoodMove;

        Vector3 foodTargetScale =
            MultiplyFoodScale(
                middleFoodScale
            );

        Quaternion foodTargetRotation =
            originalFoodRotation *
            Quaternion.Euler(
                0f,
                0f,
                middleFoodRotation
            );

        // -------------------------
        // PUSH
        // -------------------------

        float timer = 0f;

        while (timer < pushDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / pushDuration
            );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            waveMidLeftCanvasGroup.alpha =
                smoothT;

            waveMidRightCanvasGroup.alpha =
                smoothT;

            waveMidLeft.anchoredPosition =
                Vector2.Lerp(
                    midLeftOriginalPosition,
                    leftTarget,
                    smoothT
                );

            waveMidRight.anchoredPosition =
                Vector2.Lerp(
                    midRightOriginalPosition,
                    rightTarget,
                    smoothT
                );

            waveMidLeft.localScale =
                Vector3.Lerp(
                    leftStartScale,
                    leftPeakScale,
                    smoothT
                );

            waveMidRight.localScale =
                Vector3.Lerp(
                    rightStartScale,
                    rightPeakScale,
                    smoothT
                );

            AnimateFoodTowards(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                smoothT
            );

            yield return null;
        }

        // -------------------------
        // RETURN
        // -------------------------

        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / returnDuration
            );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            waveMidLeftCanvasGroup.alpha =
                1f - smoothT;

            waveMidRightCanvasGroup.alpha =
                1f - smoothT;

            waveMidLeft.anchoredPosition =
                Vector2.Lerp(
                    leftTarget,
                    midLeftOriginalPosition,
                    smoothT
                );

            waveMidRight.anchoredPosition =
                Vector2.Lerp(
                    rightTarget,
                    midRightOriginalPosition,
                    smoothT
                );

            waveMidLeft.localScale =
                Vector3.Lerp(
                    leftPeakScale,
                    midLeftOriginalScale,
                    smoothT
                );

            waveMidRight.localScale =
                Vector3.Lerp(
                    rightPeakScale,
                    midRightOriginalScale,
                    smoothT
                );

            AnimateFoodBack(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                smoothT
            );

            yield return null;
        }

        ResetAllWaves();
        ResetFood();

        activeCoroutine = null;
    }

    // =========================================================
    // BOTTOM
    // =========================================================

    private IEnumerator BottomRoutine()
    {
        yield return StartCoroutine(
            AnimateSingleWave(
                waveBot,
                waveBotCanvasGroup,
                bottomOriginalPosition,
                bottomOriginalScale,
                bottomPush,
                bottomFoodMove,
                bottomFoodScale,
                bottomFoodRotation
            )
        );

        activeCoroutine = null;
    }

    // =========================================================
    // FULL WAVE
    // =========================================================

    private IEnumerator FullWaveRoutine()
    {
        // TOP
        yield return StartCoroutine(
            AnimateSingleWave(
                waveTop,
                waveTopCanvasGroup,
                topOriginalPosition,
                topOriginalScale,
                topPush,
                topFoodMove,
                topFoodScale,
                topFoodRotation
            )
        );

        yield return new WaitForSeconds(
            delayBetweenSections
        );

        // MIDDLE
        yield return StartCoroutine(
            MiddleAnimationOnly()
        );

        yield return new WaitForSeconds(
            delayBetweenSections
        );

        // BOTTOM
        yield return StartCoroutine(
            AnimateSingleWave(
                waveBot,
                waveBotCanvasGroup,
                bottomOriginalPosition,
                bottomOriginalScale,
                bottomPush,
                bottomFoodMove,
                bottomFoodScale,
                bottomFoodRotation
            )
        );

        ResetAllWaves();
        ResetFood();

        activeCoroutine = null;

        Debug.Log(
            "FULL PERISTALTIC WAVE COMPLETE"
        );
    }

    // =========================================================
    // MIDDLE VERSION USED BY FULL WAVE
    // =========================================================

    private IEnumerator MiddleAnimationOnly()
    {
        if (waveMidLeft == null ||
            waveMidRight == null ||
            waveMidLeftCanvasGroup == null ||
            waveMidRightCanvasGroup == null)
        {
            Debug.LogWarning(
                "Middle references are missing."
            );

            yield break;
        }

        ResetFood();

        waveMidLeft.anchoredPosition =
            midLeftOriginalPosition;

        waveMidRight.anchoredPosition =
            midRightOriginalPosition;

        Vector3 leftStartScale =
            MultiplyScale(
                midLeftOriginalScale,
                startWaveScale
            );

        Vector3 rightStartScale =
            MultiplyScale(
                midRightOriginalScale,
                startWaveScale
            );

        Vector3 leftPeakScale =
            MultiplyScale(
                midLeftOriginalScale,
                peakWaveScale
            );

        Vector3 rightPeakScale =
            MultiplyScale(
                midRightOriginalScale,
                peakWaveScale
            );

        waveMidLeft.localScale =
            leftStartScale;

        waveMidRight.localScale =
            rightStartScale;

        waveMidLeftCanvasGroup.alpha = 0f;
        waveMidRightCanvasGroup.alpha = 0f;

        Vector2 leftTarget =
            midLeftOriginalPosition +
            midLeftPush;

        Vector2 rightTarget =
            midRightOriginalPosition +
            midRightPush;

        Vector2 foodTargetPosition =
            originalFoodPosition +
            middleFoodMove;

        Vector3 foodTargetScale =
            MultiplyFoodScale(
                middleFoodScale
            );

        Quaternion foodTargetRotation =
            originalFoodRotation *
            Quaternion.Euler(
                0f,
                0f,
                middleFoodRotation
            );

        float timer = 0f;

        while (timer < pushDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / pushDuration
            );

            float s =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            waveMidLeftCanvasGroup.alpha = s;
            waveMidRightCanvasGroup.alpha = s;

            waveMidLeft.anchoredPosition =
                Vector2.Lerp(
                    midLeftOriginalPosition,
                    leftTarget,
                    s
                );

            waveMidRight.anchoredPosition =
                Vector2.Lerp(
                    midRightOriginalPosition,
                    rightTarget,
                    s
                );

            waveMidLeft.localScale =
                Vector3.Lerp(
                    leftStartScale,
                    leftPeakScale,
                    s
                );

            waveMidRight.localScale =
                Vector3.Lerp(
                    rightStartScale,
                    rightPeakScale,
                    s
                );

            AnimateFoodTowards(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                s
            );

            yield return null;
        }

        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / returnDuration
            );

            float s =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            waveMidLeftCanvasGroup.alpha =
                1f - s;

            waveMidRightCanvasGroup.alpha =
                1f - s;

            waveMidLeft.anchoredPosition =
                Vector2.Lerp(
                    leftTarget,
                    midLeftOriginalPosition,
                    s
                );

            waveMidRight.anchoredPosition =
                Vector2.Lerp(
                    rightTarget,
                    midRightOriginalPosition,
                    s
                );

            waveMidLeft.localScale =
                Vector3.Lerp(
                    leftPeakScale,
                    midLeftOriginalScale,
                    s
                );

            waveMidRight.localScale =
                Vector3.Lerp(
                    rightPeakScale,
                    midRightOriginalScale,
                    s
                );

            AnimateFoodBack(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                s
            );

            yield return null;
        }

        ResetSingleWave(
            waveMidLeft,
            waveMidLeftCanvasGroup,
            midLeftOriginalPosition,
            midLeftOriginalScale
        );

        ResetSingleWave(
            waveMidRight,
            waveMidRightCanvasGroup,
            midRightOriginalPosition,
            midRightOriginalScale
        );

        ResetFood();
    }

    // =========================================================
    // GENERAL SINGLE WAVE
    // =========================================================

    private IEnumerator AnimateSingleWave(
        RectTransform wave,
        CanvasGroup canvasGroup,
        Vector2 originalPosition,
        Vector3 originalScale,
        Vector2 push,
        Vector2 foodMove,
        Vector2 foodScale,
        float foodRotation)
    {
        if (wave == null ||
            canvasGroup == null)
        {
            yield break;
        }

        ResetFood();

        wave.anchoredPosition =
            originalPosition;

        Vector3 startScale =
            MultiplyScale(
                originalScale,
                startWaveScale
            );

        Vector3 targetScale =
            MultiplyScale(
                originalScale,
                peakWaveScale
            );

        wave.localScale =
            startScale;

        canvasGroup.alpha = 0f;

        Vector2 targetPosition =
            originalPosition +
            push;

        Vector2 foodTargetPosition =
            originalFoodPosition +
            foodMove;

        Vector3 foodTargetScale =
            MultiplyFoodScale(
                foodScale
            );

        Quaternion foodTargetRotation =
            originalFoodRotation *
            Quaternion.Euler(
                0f,
                0f,
                foodRotation
            );

        float timer = 0f;

        // PUSH
        while (timer < pushDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / pushDuration
            );

            float s =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            canvasGroup.alpha = s;

            wave.anchoredPosition =
                Vector2.Lerp(
                    originalPosition,
                    targetPosition,
                    s
                );

            wave.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    s
                );

            AnimateFoodTowards(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                s
            );

            yield return null;
        }

        timer = 0f;

        // RETURN
        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / returnDuration
            );

            float s =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            canvasGroup.alpha =
                1f - s;

            wave.anchoredPosition =
                Vector2.Lerp(
                    targetPosition,
                    originalPosition,
                    s
                );

            wave.localScale =
                Vector3.Lerp(
                    targetScale,
                    originalScale,
                    s
                );

            AnimateFoodBack(
                foodTargetPosition,
                foodTargetScale,
                foodTargetRotation,
                s
            );

            yield return null;
        }

        ResetSingleWave(
            wave,
            canvasGroup,
            originalPosition,
            originalScale
        );

        ResetFood();
    }

    // =========================================================
    // FOOD
    // =========================================================

    private void AnimateFoodTowards(
        Vector2 position,
        Vector3 scale,
        Quaternion rotation,
        float t)
    {
        if (stomachContents == null)
            return;

        stomachContents.anchoredPosition =
            Vector2.Lerp(
                originalFoodPosition,
                position,
                t
            );

        stomachContents.localScale =
            Vector3.Lerp(
                originalFoodScale,
                scale,
                t
            );

        stomachContents.localRotation =
            Quaternion.Lerp(
                originalFoodRotation,
                rotation,
                t
            );
    }

    private void AnimateFoodBack(
        Vector2 position,
        Vector3 scale,
        Quaternion rotation,
        float t)
    {
        if (stomachContents == null)
            return;

        stomachContents.anchoredPosition =
            Vector2.Lerp(
                position,
                originalFoodPosition,
                t
            );

        stomachContents.localScale =
            Vector3.Lerp(
                scale,
                originalFoodScale,
                t
            );

        stomachContents.localRotation =
            Quaternion.Lerp(
                rotation,
                originalFoodRotation,
                t
            );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void SaveOriginalValues()
    {
        SaveWave(
            waveTop,
            out topOriginalPosition,
            out topOriginalScale
        );

        SaveWave(
            waveMidLeft,
            out midLeftOriginalPosition,
            out midLeftOriginalScale
        );

        SaveWave(
            waveMidRight,
            out midRightOriginalPosition,
            out midRightOriginalScale
        );

        SaveWave(
            waveBot,
            out bottomOriginalPosition,
            out bottomOriginalScale
        );

        if (stomachContents != null)
        {
            originalFoodPosition =
                stomachContents.anchoredPosition;

            originalFoodScale =
                stomachContents.localScale;

            originalFoodRotation =
                stomachContents.localRotation;
        }
    }

    private void SaveWave(
        RectTransform wave,
        out Vector2 position,
        out Vector3 scale)
    {
        if (wave != null)
        {
            position = wave.anchoredPosition;
            scale = wave.localScale;
        }
        else
        {
            position = Vector2.zero;
            scale = Vector3.one;
        }
    }

    private Vector3 MultiplyScale(
        Vector3 original,
        Vector2 multiplier)
    {
        return new Vector3(
            original.x * multiplier.x,
            original.y * multiplier.y,
            original.z
        );
    }

    private Vector3 MultiplyFoodScale(
        Vector2 multiplier)
    {
        return new Vector3(
            originalFoodScale.x * multiplier.x,
            originalFoodScale.y * multiplier.y,
            originalFoodScale.z
        );
    }

    private void StopCurrentAnimation()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        ResetAllWaves();
        ResetFood();
    }

    private void HideAllWaves()
    {
        SetAlpha(
            waveTopCanvasGroup,
            0f
        );

        SetAlpha(
            waveMidLeftCanvasGroup,
            0f
        );

        SetAlpha(
            waveMidRightCanvasGroup,
            0f
        );

        SetAlpha(
            waveBotCanvasGroup,
            0f
        );
    }

    private void ResetAllWaves()
    {
        ResetSingleWave(
            waveTop,
            waveTopCanvasGroup,
            topOriginalPosition,
            topOriginalScale
        );

        ResetSingleWave(
            waveMidLeft,
            waveMidLeftCanvasGroup,
            midLeftOriginalPosition,
            midLeftOriginalScale
        );

        ResetSingleWave(
            waveMidRight,
            waveMidRightCanvasGroup,
            midRightOriginalPosition,
            midRightOriginalScale
        );

        ResetSingleWave(
            waveBot,
            waveBotCanvasGroup,
            bottomOriginalPosition,
            bottomOriginalScale
        );
    }

    private void ResetSingleWave(
        RectTransform wave,
        CanvasGroup canvasGroup,
        Vector2 position,
        Vector3 scale)
    {
        if (wave != null)
        {
            wave.anchoredPosition =
                position;

            wave.localScale =
                scale;
        }

        SetAlpha(
            canvasGroup,
            0f
        );
    }

    private void ResetFood()
    {
        if (stomachContents == null)
            return;

        stomachContents.anchoredPosition =
            originalFoodPosition;

        stomachContents.localScale =
            originalFoodScale;

        stomachContents.localRotation =
            originalFoodRotation;
    }

    private void SetAlpha(
        CanvasGroup canvasGroup,
        float value)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                value;
        }
    }
}