using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class LiverTheoryVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Puzzle")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject welcomeToPuzzlePanel;

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

        if (welcomeToPuzzlePanel != null)
        {
            welcomeToPuzzlePanel.SetActive(false);
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
            "Liver Theory: видеото е готово."
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
        theoryIsActive = false;

        player.Stop();

        RestoreBackgroundMusic();

        ShowSharedButtons();

        // Първо показваме самия PuzzlePanel.
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "Liver Theory: PuzzlePanel не е свързан."
            );
        }

        // След това Welcome панела върху него.
        if (welcomeToPuzzlePanel != null)
        {
            welcomeToPuzzlePanel.SetActive(true);

            // Понеже е отделен sibling под Canvas,
            // го слагаме най-отгоре.
            welcomeToPuzzlePanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError(
                "Liver Theory: WelcomeToPuzzlePanel не е свързан."
            );
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

        RestoreBackgroundMusic();

        ShowSharedButtons();
    }

    private void OnDestroy()
    {
        RestoreBackgroundMusic();

        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -=
            OnVideoPrepared;

        videoPlayer.loopPointReached -=
            OnVideoFinished;

        videoPlayer.errorReceived -=
            OnVideoError;
    }
}