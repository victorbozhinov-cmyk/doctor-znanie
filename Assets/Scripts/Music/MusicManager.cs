using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance
    {
        get;
        private set;
    }

    [Header("Lobby Music")]
    [SerializeField]
    private AudioClip lobbyMusic;

    [Range(0f, 1f)]
    [SerializeField]
    private float lobbyMusicVolume = 0.35f;

    [Header("Intro / Lesson Music")]
    [SerializeField]
    private AudioClip introMusic;

    [Range(0f, 1f)]
    [SerializeField]
    private float introMusicVolume = 0.30f;

    [Header("Puzzle Music")]
    [SerializeField]
    private AudioClip puzzleMusic;

    [Range(0f, 1f)]
    [SerializeField]
    private float puzzleMusicVolume = 0.30f;

    [Header("Quiz Music")]
    [SerializeField]
    private AudioClip quizMusic;

    [Range(0f, 1f)]
    [SerializeField]
    private float quizMusicVolume = 0.30f;

    [Header("Video Ducking")]
    [Range(0f, 1f)]
    [SerializeField]
    private float videoDuckedVolumeMultiplier = 0.25f;

    [Header("Feedback SFX Ducking")]
    [Range(0f, 1f)]
    [SerializeField]
    private float feedbackDuckedVolumeMultiplier = 0.60f;

    [Header("Volume Fade")]
    [SerializeField]
    private float volumeFadeDuration = 0.25f;

    [Header("Lobby Scenes")]
    [SerializeField]
    private List<string> lobbyScenes =
        new List<string>
        {
            "MainMenu",
            "SettingsMenu",
            "GameInfo",
            "BodyMap"
        };

    [Header("Organ Level Scenes")]
    [SerializeField]
    private List<string> organLevelScenes =
        new List<string>
        {
            "HeartLevel",
            "StomachLevel",
            "LiverLevel",
            "BrainLevel",
            "LungsLevel"
        };

    private AudioSource musicSource;

    private float currentBaseVolume;

    private bool isVideoDucked;
    private bool isFeedbackDucked;

    private float feedbackDuckUntil;

    private Coroutine volumeFadeCoroutine;
    private Coroutine feedbackDuckCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        musicSource =
            GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void Start()
    {
        HandleSceneMusic(
            SceneManager
                .GetActiveScene()
                .name
        );
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        HandleSceneMusic(
            scene.name
        );
    }

    private void HandleSceneMusic(
        string sceneName)
    {
        ResetDucking();

        if (lobbyScenes.Contains(sceneName))
        {
            PlayLobbyMusic();
        }
        else if (
            organLevelScenes.Contains(sceneName))
        {
            PlayIntroMusic();
        }
        else
        {
            StopMusic();
        }
    }

    public void PlayLobbyMusic()
    {
        PlayMusic(
            lobbyMusic,
            lobbyMusicVolume
        );
    }

    public void PlayIntroMusic()
    {
        PlayMusic(
            introMusic,
            introMusicVolume
        );
    }

    public void PlayPuzzleMusic()
    {
        PlayMusic(
            puzzleMusic,
            puzzleMusicVolume
        );
    }

    public void PlayQuizMusic()
    {
        PlayMusic(
            quizMusic,
            quizMusicVolume
        );
    }

    private void PlayMusic(
        AudioClip clip,
        float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "MusicManager: " +
                "Няма зададен AudioClip."
            );

            return;
        }

        ResetDucking();

        currentBaseVolume = volume;

        StopVolumeFade();

        if (musicSource.isPlaying &&
            musicSource.clip == clip)
        {
            musicSource.volume =
                currentBaseVolume;

            return;
        }

        musicSource.clip = clip;
        musicSource.volume =
            currentBaseVolume;

        musicSource.loop = true;

        musicSource.Play();
    }

    // =========================
    // Video Ducking
    // =========================

    public void DuckMusicForVideo()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        isVideoDucked = true;

        UpdateTargetVolume();
    }

    public void RestoreMusicAfterVideo()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        isVideoDucked = false;

        UpdateTargetVolume();
    }

    // =========================
    // Feedback SFX Ducking
    // =========================

    public void DuckMusicForFeedback(
        float duration)
    {
        if (!musicSource.isPlaying ||
            duration <= 0f)
        {
            return;
        }

        float newEndTime =
            Time.unscaledTime +
            duration;

        feedbackDuckUntil =
            Mathf.Max(
                feedbackDuckUntil,
                newEndTime
            );

        if (!isFeedbackDucked)
        {
            isFeedbackDucked = true;

            UpdateTargetVolume();
        }

        if (feedbackDuckCoroutine == null)
        {
            feedbackDuckCoroutine =
                StartCoroutine(
                    FeedbackDuckRoutine()
                );
        }
    }

    private IEnumerator FeedbackDuckRoutine()
    {
        while (
            Time.unscaledTime <
            feedbackDuckUntil)
        {
            yield return null;
        }

        isFeedbackDucked = false;

        feedbackDuckCoroutine = null;

        UpdateTargetVolume();
    }

    // =========================
    // Volume
    // =========================

    private void UpdateTargetVolume()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        float multiplier = 1f;

        if (isFeedbackDucked)
        {
            multiplier =
                Mathf.Min(
                    multiplier,
                    feedbackDuckedVolumeMultiplier
                );
        }

        if (isVideoDucked)
        {
            multiplier =
                Mathf.Min(
                    multiplier,
                    videoDuckedVolumeMultiplier
                );
        }

        float targetVolume =
            currentBaseVolume *
            multiplier;

        FadeToVolume(
            targetVolume
        );
    }

    private void FadeToVolume(
        float targetVolume)
    {
        StopVolumeFade();

        volumeFadeCoroutine =
            StartCoroutine(
                FadeVolumeCoroutine(
                    targetVolume
                )
            );
    }

    private IEnumerator FadeVolumeCoroutine(
        float targetVolume)
    {
        float startVolume =
            musicSource.volume;

        float elapsed = 0f;

        if (volumeFadeDuration <= 0f)
        {
            musicSource.volume =
                targetVolume;

            volumeFadeCoroutine = null;

            yield break;
        }

        while (
            elapsed <
            volumeFadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    volumeFadeDuration
                );

            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    targetVolume,
                    t
                );

            yield return null;
        }

        musicSource.volume =
            targetVolume;

        volumeFadeCoroutine = null;
    }

    private void StopVolumeFade()
    {
        if (volumeFadeCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            volumeFadeCoroutine
        );

        volumeFadeCoroutine = null;
    }

    private void ResetDucking()
    {
        isVideoDucked = false;
        isFeedbackDucked = false;

        feedbackDuckUntil = 0f;

        if (feedbackDuckCoroutine != null)
        {
            StopCoroutine(
                feedbackDuckCoroutine
            );

            feedbackDuckCoroutine = null;
        }
    }

    public void StopMusic()
    {
        StopVolumeFade();
        ResetDucking();

        if (!musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
    }
}