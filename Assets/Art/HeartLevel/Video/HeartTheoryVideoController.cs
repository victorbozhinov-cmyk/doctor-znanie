using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class HeartTheoryVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Next Panel")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Puzzle Welcome")]
    [SerializeField] private GameObject puzzleWelcomePanel;

    [Header("StreamingAssets Video")]
    [SerializeField] private string videoFileName = "HeartTheory.mp4";

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
                "Heart Theory: няма намерен VideoPlayer."
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
            "Videos/Heart/" + videoFileName;

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
            "Heart",
            videoFileName
        );

        localPath = localPath.Replace("\\", "/");

        if (!File.Exists(localPath))
        {
            Debug.LogError(
                "Heart Theory: видеото НЕ е намерено тук:\n" +
                localPath
            );

            return;
        }

        finalUrl = new Uri(localPath).AbsoluteUri;

#endif

        Debug.Log(
            "Heart Theory Video URL: " +
            finalUrl
        );

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = finalUrl;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        Debug.Log(
            "Heart Theory: видеото е готово."
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

        // Първо включваме PuzzlePanel.
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        // После показваме Welcome панела.
        if (puzzleWelcomePanel != null)
        {
            puzzleWelcomePanel.SetActive(true);
        }

        // Накрая затваряме VideoPanel.
        gameObject.SetActive(false);
    }

    private void OnVideoError(
        VideoPlayer player,
        string message
    )
    {
        Debug.LogError(
            "Грешка при пускане на Heart Theory видеото: " +
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