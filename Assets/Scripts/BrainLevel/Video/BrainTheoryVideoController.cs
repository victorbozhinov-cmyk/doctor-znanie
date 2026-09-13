using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class BrainTheoryVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Next Panel")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Puzzle Welcome")]
    [SerializeField] private GameObject puzzleWelcomePanel;

    [Header("StreamingAssets Video")]
    [SerializeField] private string videoFileName = "BrainTheory.mp4";

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
                "Brain Theory: няма намерен VideoPlayer."
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
            "Videos/Brain/" + videoFileName;

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
            "Brain",
            videoFileName
        );

        localPath = localPath.Replace("\\", "/");

        if (!File.Exists(localPath))
        {
            Debug.LogError(
                "Brain Theory: видеото НЕ е намерено тук:\n" +
                localPath
            );

            return;
        }

        finalUrl = new Uri(localPath).AbsoluteUri;

#endif

        Debug.Log(
            "Brain Theory Video URL: " +
            finalUrl
        );

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = finalUrl;

        videoPlayer.Prepare();
    }

    // =========================================================
    // VIDEO MUSIC DUCKING
    // =========================================================

    private void DuckBackgroundMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .DuckMusicForVideo();
        }
        else
        {
            Debug.LogWarning(
                "MusicManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }
    }

    private void RestoreBackgroundMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .RestoreMusicAfterVideo();
        }
    }

    // =========================================================
    // VIDEO EVENTS
    // =========================================================

    private void OnVideoPrepared(VideoPlayer player)
    {
        Debug.Log(
            "Brain Theory: видеото е готово."
        );

        DuckBackgroundMusic();

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

        RestoreBackgroundMusic();

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
        RestoreBackgroundMusic();

        Debug.LogError(
            "Грешка при пускане на Brain Theory видеото: " +
            message
        );
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        RestoreBackgroundMusic();
    }

    private void OnDestroy()
    {
        RestoreBackgroundMusic();

        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}