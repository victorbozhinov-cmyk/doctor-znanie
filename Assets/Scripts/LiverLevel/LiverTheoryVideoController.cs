using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class LiverTheoryVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Next Panel")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Shared Buttons")]
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject settingsButton;

    [Header("StreamingAssets Video")]
    [SerializeField] private string videoFileName = "LiverTheory.mp4";

    private bool isFinishing;
    private bool theoryIsActive;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoAudioSource == null)
        {
            videoAudioSource = GetComponent<AudioSource>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError(
                "Liver Theory: няма намерен VideoPlayer."
            );

            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        videoPlayer.audioOutputMode =
            VideoAudioOutputMode.AudioSource;

        if (videoAudioSource != null)
        {
            videoPlayer.EnableAudioTrack(0, true);

            videoPlayer.SetTargetAudioSource(
                0,
                videoAudioSource
            );
        }

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void OnEnable()
    {
        theoryIsActive = true;
        isFinishing = false;

        HideSharedButtons();

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (videoPlayer == null)
        {
            return;
        }

        string relativePath =
            "Videos/Liver/" + videoFileName;

        string finalUrl;

#if UNITY_WEBGL && !UNITY_EDITOR

        finalUrl =
            Application.streamingAssetsPath +
            "/" +
            relativePath;

#else

        string localPath = Path.Combine(
            Application.streamingAssetsPath,
            "Videos",
            "Liver",
            videoFileName
        );

        localPath = localPath.Replace("\\", "/");

        if (!File.Exists(localPath))
        {
            Debug.LogError(
                "Liver Theory: видеото НЕ е намерено тук:\n" +
                localPath
            );

            return;
        }

        finalUrl = new Uri(localPath).AbsoluteUri;

#endif

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = finalUrl;

        videoPlayer.Prepare();
    }

    private void LateUpdate()
    {
        // Докато TheoryPanel е активен,
        // тези два бутона винаги стоят скрити.
        if (theoryIsActive)
        {
            HideSharedButtons();
        }
    }

    private void HideSharedButtons()
    {
        if (backButton != null &&
            backButton.activeSelf)
        {
            backButton.SetActive(false);
        }

        if (settingsButton != null &&
            settingsButton.activeSelf)
        {
            settingsButton.SetActive(false);
        }
    }

    private void ShowSharedButtons()
    {
        if (backButton != null)
        {
            backButton.SetActive(true);
        }

        if (settingsButton != null)
        {
            settingsButton.SetActive(true);
        }
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        Debug.Log("Liver Theory: видеото е готово.");

        player.Play();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        if (isFinishing)
        {
            return;
        }

        isFinishing = true;
        theoryIsActive = false;

        player.Stop();

        // Първо връщаме бутоните за PuzzlePanel.
        ShowSharedButtons();

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void OnVideoError(
        VideoPlayer player,
        string message
    )
    {
        Debug.LogError(
            "Грешка при пускане на Liver Theory видеото: " +
            message
        );
    }

    private void OnDisable()
    {
        theoryIsActive = false;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        ShowSharedButtons();
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}