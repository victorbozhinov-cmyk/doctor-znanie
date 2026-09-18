using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class VideoControlsOverlay : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Controls")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("Settings")]
    [SerializeField] private GameObject settingsOverlay;

    [Header("Navigation")]
    [SerializeField] private GameObject theoryPanel;
    [SerializeField] private GameObject nextPanel;

    [Header("Puzzle Welcome")]
    [SerializeField] private GameObject puzzleWelcomePanel;

    [Header("Behaviour")]
    [SerializeField] private float hideDelay = 2.5f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float mouseMoveThreshold = 2f;

    private Vector2 lastMousePosition;
    private float hideTimer;

    private bool controlsVisible;
    private bool isPaused;

    // =====================================================
    // WEB / SEEK
    // =====================================================

    private bool isSeeking;
    private bool seekCompletedReceived;

    private double lastKnownVideoTime;
    private double pendingSeekTime;
    private double queuedSeekSeconds;

    private Coroutine seekCoroutine;

    private const double SeekStepSeconds = 5.0;
    private const float SeekTimeoutSeconds = 2.0f;

    // =====================================================
    // UNITY
    // =====================================================

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.seekCompleted -= OnSeekCompleted;
            videoPlayer.seekCompleted += OnSeekCompleted;

            if (videoPlayer.isPrepared)
            {
                double currentTime = videoPlayer.time;

                if (IsValidTime(currentTime))
                {
                    lastKnownVideoTime = currentTime;
                }
            }
        }

        isSeeking = false;
        seekCompletedReceived = false;
        queuedSeekSeconds = 0.0;

        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.gameObject.SetActive(true);
        }

        lastMousePosition = GetMousePosition();

        HideControlsImmediate();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.seekCompleted -= OnSeekCompleted;
        }

        if (seekCoroutine != null)
        {
            StopCoroutine(seekCoroutine);
            seekCoroutine = null;
        }

        isSeeking = false;
        seekCompletedReceived = false;
        queuedSeekSeconds = 0.0;
    }

    private void Update()
    {
        UpdateLastKnownVideoTime();

        if (!isPaused)
        {
            DetectMouseMovement();
            UpdateControlsVisibility();
        }
    }

    // =====================================================
    // VIDEO TIME TRACKING
    // =====================================================

    private void UpdateLastKnownVideoTime()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (!videoPlayer.isPrepared)
        {
            return;
        }

        if (isSeeking)
        {
            return;
        }

        double currentTime = videoPlayer.time;

        if (IsValidTime(currentTime))
        {
            lastKnownVideoTime = currentTime;
        }
    }

    private bool IsValidTime(double value)
    {
        return !double.IsNaN(value) &&
               !double.IsInfinity(value) &&
               value >= 0.0;
    }

    private double ClampVideoTime(double targetTime)
    {
        if (targetTime < 0.0)
        {
            targetTime = 0.0;
        }

        if (videoPlayer != null)
        {
            double videoLength = videoPlayer.length;

            if (IsValidTime(videoLength) &&
                videoLength > 0.1)
            {
                double maximumTime = videoLength - 0.1;

                if (targetTime > maximumTime)
                {
                    targetTime = maximumTime;
                }
            }
        }

        return targetTime;
    }

    // =====================================================
    // Mouse
    // =====================================================

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    private void DetectMouseMovement()
    {
        Vector2 currentMousePosition = GetMousePosition();

        float movement = Vector2.Distance(
            currentMousePosition,
            lastMousePosition
        );

        if (movement >= mouseMoveThreshold)
        {
            ShowControls();
            lastMousePosition = currentMousePosition;
        }
    }

    // =====================================================
    // Show / Hide Controls
    // =====================================================

    private void ShowControls()
    {
        controlsVisible = true;
        hideTimer = hideDelay;

        if (controlsCanvasGroup == null)
        {
            return;
        }

        controlsCanvasGroup.gameObject.SetActive(true);
        controlsCanvasGroup.interactable = true;
        controlsCanvasGroup.blocksRaycasts = true;
    }

    private void UpdateControlsVisibility()
    {
        if (controlsCanvasGroup == null)
        {
            return;
        }

        if (controlsVisible)
        {
            hideTimer -= Time.unscaledDeltaTime;

            controlsCanvasGroup.alpha =
                Mathf.MoveTowards(
                    controlsCanvasGroup.alpha,
                    1f,
                    fadeSpeed * Time.unscaledDeltaTime
                );

            if (hideTimer <= 0f)
            {
                controlsVisible = false;

                controlsCanvasGroup.interactable = false;
                controlsCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            controlsCanvasGroup.alpha =
                Mathf.MoveTowards(
                    controlsCanvasGroup.alpha,
                    0f,
                    fadeSpeed * Time.unscaledDeltaTime
                );
        }
    }

    private void HideControlsImmediate()
    {
        controlsVisible = false;

        if (controlsCanvasGroup == null)
        {
            return;
        }

        controlsCanvasGroup.gameObject.SetActive(true);

        controlsCanvasGroup.alpha = 0f;
        controlsCanvasGroup.interactable = false;
        controlsCanvasGroup.blocksRaycasts = false;
    }

    // =====================================================
    // -5 Seconds
    // =====================================================

    public void BackFiveSeconds()
    {
        SeekBySeconds(-SeekStepSeconds);
    }

    // =====================================================
    // +5 Seconds
    // =====================================================

    public void ForwardFiveSeconds()
    {
        SeekBySeconds(SeekStepSeconds);
    }

    // =====================================================
    // SEEK
    // =====================================================

    private void SeekBySeconds(double seconds)
    {
        if (videoPlayer == null)
        {
            return;
        }

        ShowControls();

        // В Web версията не се опитваме да местим времето,
        // преди видеото да е напълно подготвено.
        if (!videoPlayer.isPrepared)
        {
            return;
        }

        // Проверяваме дали текущата платформа / видео
        // позволява промяна на времето.
        if (!videoPlayer.canSetTime)
        {
            return;
        }

        // Ако вече тече seek операция,
        // запазваме следващото +5 / -5 и го изпълняваме след нея.
        if (isSeeking)
        {
            queuedSeekSeconds += seconds;
            return;
        }

        double currentTime = videoPlayer.time;

        if (!IsValidTime(currentTime))
        {
            currentTime = lastKnownVideoTime;
        }

        if (!IsValidTime(currentTime))
        {
            currentTime = 0.0;
        }

        double targetTime =
            ClampVideoTime(currentTime + seconds);

        StartSeek(targetTime);
    }

    private void StartSeek(double targetTime)
    {
        if (seekCoroutine != null)
        {
            StopCoroutine(seekCoroutine);
        }

        seekCoroutine =
            StartCoroutine(SeekRoutine(targetTime));
    }

    private IEnumerator SeekRoutine(double targetTime)
    {
        if (videoPlayer == null)
        {
            yield break;
        }

        if (!videoPlayer.isPrepared ||
            !videoPlayer.canSetTime)
        {
            yield break;
        }

        isSeeking = true;

        bool shouldResumeAfterSeek =
            videoPlayer.isPlaying && !isPaused;

        // Паузираме само временно, докато браузърът
        // завърши seek операцията.
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }

        double currentTarget =
            ClampVideoTime(targetTime);

        while (true)
        {
            pendingSeekTime = currentTarget;
            seekCompletedReceived = false;

            videoPlayer.time = pendingSeekTime;

            float timer = 0f;

            while (!seekCompletedReceived &&
                   timer < SeekTimeoutSeconds)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Пазим целевото време, вместо да разчитаме
            // веднага на videoPlayer.time при Web.
            lastKnownVideoTime = pendingSeekTime;

            // Ако потребителят е натиснал +5 / -5 още веднъж,
            // докато браузърът е местил видеото,
            // изпълняваме и натрупаното преместване.
            if (Math.Abs(queuedSeekSeconds) > 0.001)
            {
                currentTarget =
                    ClampVideoTime(
                        pendingSeekTime +
                        queuedSeekSeconds
                    );

                queuedSeekSeconds = 0.0;

                continue;
            }

            break;
        }

        isSeeking = false;
        seekCoroutine = null;

        if (shouldResumeAfterSeek &&
            !isPaused &&
            videoPlayer != null)
        {
            videoPlayer.Play();
        }

        ShowControls();
    }

    private void OnSeekCompleted(VideoPlayer source)
    {
        if (source != videoPlayer)
        {
            return;
        }

        seekCompletedReceived = true;
    }

    // =====================================================
    // Pause
    // =====================================================

    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        isPaused = true;

        HideControlsImmediate();

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =====================================================
    // Continue
    // =====================================================

    public void ResumeVideo()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        lastMousePosition = GetMousePosition();

        ShowControls();
    }

    // =====================================================
    // Settings
    // =====================================================

    public void OpenSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
        }
    }

    public void CloseSettingsToPause()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        isPaused = true;

        if (videoPlayer != null &&
            videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }

        HideControlsImmediate();
    }

    // =====================================================
    // Restart
    // =====================================================

    public void RestartVideo()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;

        queuedSeekSeconds = 0.0;
        lastKnownVideoTime = 0.0;

        videoPlayer.time = 0.0;
        videoPlayer.Play();

        lastMousePosition = GetMousePosition();

        ShowControls();
    }

    // =====================================================
    // Skip
    // =====================================================

    public void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        isPaused = false;

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Отваряме PuzzlePanel.
        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
        }

        // И задължително показваме Welcome панела на пъзела.
        if (puzzleWelcomePanel != null)
        {
            puzzleWelcomePanel.SetActive(true);
        }

        // Накрая затваряме TheoryPanel.
        if (theoryPanel != null)
        {
            theoryPanel.SetActive(false);
        }
    }
}