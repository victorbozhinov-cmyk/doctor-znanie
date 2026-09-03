using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Lobby Music")]
    [SerializeField] private AudioClip lobbyMusic;

    [Range(0f, 1f)]
    [SerializeField] private float lobbyMusicVolume = 0.35f;

    [Header("Intro / Lesson Music")]
    [SerializeField] private AudioClip introMusic;

    [Range(0f, 1f)]
    [SerializeField] private float introMusicVolume = 0.30f;

    [Header("Puzzle Music")]
    [SerializeField] private AudioClip puzzleMusic;

    [Range(0f, 1f)]
    [SerializeField] private float puzzleMusicVolume = 0.30f;

    [Header("Quiz Music")]
    [SerializeField] private AudioClip quizMusic;

    [Range(0f, 1f)]
    [SerializeField] private float quizMusicVolume = 0.30f;

    [Header("Video Ducking")]
    [Range(0f, 1f)]
    [SerializeField] private float videoDuckedVolumeMultiplier = 0.25f;

    [SerializeField] private float volumeFadeDuration = 0.4f;

    [Header("Lobby Scenes")]
    [SerializeField] private List<string> lobbyScenes = new List<string>
    {
        "MainMenu",
        "SettingsMenu",
        "GameInfo",
        "BodyMap"
    };

    [Header("Organ Level Scenes")]
    [SerializeField] private List<string> organLevelScenes = new List<string>
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

    private Coroutine volumeFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HandleSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene.name);
    }

    private void HandleSceneMusic(string sceneName)
    {
        isVideoDucked = false;

        if (lobbyScenes.Contains(sceneName))
        {
            PlayLobbyMusic();
        }
        else if (organLevelScenes.Contains(sceneName))
        {
            // Всяко органно ниво започва със Start Panel.
            PlayIntroMusic();
        }
        else
        {
            StopMusic();
        }
    }

    public void PlayLobbyMusic()
    {
        PlayMusic(lobbyMusic, lobbyMusicVolume);
    }

    public void PlayIntroMusic()
    {
        PlayMusic(introMusic, introMusicVolume);
    }

    public void PlayPuzzleMusic()
    {
        PlayMusic(puzzleMusic, puzzleMusicVolume);
    }

    public void PlayQuizMusic()
    {
        PlayMusic(quizMusic, quizMusicVolume);
    }

    private void PlayMusic(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning("MusicManager: Няма зададен AudioClip.");
            return;
        }

        isVideoDucked = false;
        currentBaseVolume = volume;

        StopVolumeFade();

        // Ако същата музика вече свири,
        // не я рестартираме.
        if (musicSource.isPlaying && musicSource.clip == clip)
        {
            musicSource.volume = currentBaseVolume;
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = currentBaseVolume;
        musicSource.loop = true;

        musicSource.Play();
    }

    // Ще го използваме по-късно,
    // когато видеото започне да се възпроизвежда.
    public void DuckMusicForVideo()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        isVideoDucked = true;

        float targetVolume =
            currentBaseVolume * videoDuckedVolumeMultiplier;

        FadeToVolume(targetVolume);
    }

    // Ще го използваме при Pause или край на видеото.
    public void RestoreMusicAfterVideo()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        isVideoDucked = false;

        FadeToVolume(currentBaseVolume);
    }

    private void FadeToVolume(float targetVolume)
    {
        StopVolumeFade();

        volumeFadeCoroutine =
            StartCoroutine(
                FadeVolumeCoroutine(targetVolume)
            );
    }

    private IEnumerator FadeVolumeCoroutine(float targetVolume)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        if (volumeFadeDuration <= 0f)
        {
            musicSource.volume = targetVolume;
            volumeFadeCoroutine = null;
            yield break;
        }

        while (elapsed < volumeFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / volumeFadeDuration
                );

            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    targetVolume,
                    t
                );

            yield return null;
        }

        musicSource.volume = targetVolume;
        volumeFadeCoroutine = null;
    }

    private void StopVolumeFade()
    {
        if (volumeFadeCoroutine == null)
        {
            return;
        }

        StopCoroutine(volumeFadeCoroutine);
        volumeFadeCoroutine = null;
    }

    public void StopMusic()
    {
        StopVolumeFade();

        isVideoDucked = false;

        if (!musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
    }
}