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

    private void OnEnable()
    {
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

    private void Update()
    {
        if (!isPaused)
        {
            DetectMouseMovement();
            UpdateControlsVisibility();
        }
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
        if (videoPlayer == null)
        {
            return;
        }

        double newTime = videoPlayer.time - 5.0;

        if (newTime < 0.0)
        {
            newTime = 0.0;
        }

        videoPlayer.time = newTime;

        ShowControls();
    }

    // =====================================================
    // +5 Seconds
    // =====================================================

    public void ForwardFiveSeconds()
    {
        if (videoPlayer == null)
        {
            return;
        }

        double newTime = videoPlayer.time + 5.0;

        if (videoPlayer.length > 0 &&
            newTime >= videoPlayer.length)
        {
            newTime = videoPlayer.length - 0.1;
        }

        videoPlayer.time = newTime;

        ShowControls();
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

        if (videoPlayer != null && videoPlayer.isPlaying)
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