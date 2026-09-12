using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class StomachTheoryVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Next Panel")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Puzzle Welcome")]
    [SerializeField] private GameObject puzzleWelcomePanel;

    [Header("StreamingAssets Video")]
    [SerializeField] private string videoFileName = "StomachTheory.mp4";

    private bool isFinishing;

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
                "Stomach Theory: няма намерен VideoPlayer."
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
        isFinishing = false;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (videoPlayer == null)
        {
            return;
        }

        string relativePath =
            "Videos/Stomach/" + videoFileName;

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
            "Stomach",
            videoFileName
        );

        localPath = localPath.Replace("\\", "/");

        if (!File.Exists(localPath))
        {
            Debug.LogError(
                "Stomach Theory: видеото НЕ е намерено тук:\n" +
                localPath
            );

            return;
        }

        finalUrl = new Uri(localPath).AbsoluteUri;

#endif

        Debug.Log(
            "Stomach Theory Video URL: " +
            finalUrl
        );

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = finalUrl;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        Debug.Log(
            "Stomach Theory: видеото е готово."
        );

        player.Play();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        if (isFinishing)
        {
            return;
        }

        isFinishing = true;

        player.Stop();

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        if (puzzleWelcomePanel != null)
        {
            puzzleWelcomePanel.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void OnVideoError(
        VideoPlayer player,
        string message
    )
    {
        Debug.LogError(
            "Грешка при пускане на Stomach Theory видеото: " +
            message
        );
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
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