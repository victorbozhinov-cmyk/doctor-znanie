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

    [Header("Online Video")]
    [SerializeField] private string videoFileName = "StomachTheory.mp4";

    private const string VideoBaseUrl =
        "https://motivate-bg.online/interngames/doktorznanie/StreamingAssets/Videos/Stomach/";

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

        string finalUrl =
            VideoBaseUrl + videoFileName;

        Debug.Log(
            "Stomach Theory Online Video URL: " +
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
            "Stomach Theory: онлайн видеото е готово."
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
            "Грешка при зареждане на Stomach Theory видеото от сървъра: " +
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