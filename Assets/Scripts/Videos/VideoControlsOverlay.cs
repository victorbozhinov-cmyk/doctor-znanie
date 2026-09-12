using UnityEngine;
using UnityEngine.Video;

public class VideoControlsOverlay : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Controls")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("Skip")]
    [SerializeField] private GameObject theoryPanel;
    [SerializeField] private GameObject nextPanel;

    [Header("Behaviour")]
    [SerializeField] private float hideDelay = 2.5f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float mouseMoveThreshold = 1f;

    private Vector3 lastMousePosition;
    private float hideTimer;

    private bool controlsVisible;
    private bool isPaused;

    private void Start()
    {
        lastMousePosition = Input.mousePosition;

        HideControlsImmediate();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        DetectMouseMovement();
        UpdateControlsVisibility();
    }

    // =========================
    // Mouse Detection
    // =========================

    private void DetectMouseMovement()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        float movement = Vector3.Distance(
            currentMousePosition,
            lastMousePosition
        );

        if (movement >= mouseMoveThreshold)
        {
            ShowControls();

            lastMousePosition = currentMousePosition;
        }
    }

    // =========================
    // Controls Visibility
    // =========================

    private void ShowControls()
    {
        controlsVisible = true;
        hideTimer = hideDelay;

        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
    }

    private void UpdateControlsVisibility()
    {
        if (controlsCanvasGroup == null)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        if (controlsVisible)
        {
            hideTimer -= Time.unscaledDeltaTime;

            controlsCanvasGroup.alpha = Mathf.MoveTowards(
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
            controlsCanvasGroup.alpha = Mathf.MoveTowards(
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

        controlsCanvasGroup.alpha = 0f;
        controlsCanvasGroup.interactable = false;
        controlsCanvasGroup.blocksRaycasts = false;
    }

    // =========================
    // -5 Seconds
    // =========================

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

    // =========================
    // +5 Seconds
    // =========================

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

    // =========================
    // Pause
    // =========================

    public void PauseVideo()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.Pause();

        isPaused = true;

        HideControlsImmediate();

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    // =========================
    // Resume
    // =========================

    public void ResumeVideo()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        ShowControls();
    }

    // =========================
    // Skip
    // =========================

    public void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
        }

        if (theoryPanel != null)
        {
            theoryPanel.SetActive(false);
        }
    }
}