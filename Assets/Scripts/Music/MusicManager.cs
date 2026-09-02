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

    [Header("Lobby Scenes")]
    [SerializeField] private List<string> lobbyScenes = new List<string>
    {
        "MainMenu",
        "SettingsMenu",
        "GameInfo",
        "BodyMap"
    };

    private AudioSource musicSource;

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
        if (lobbyScenes.Contains(sceneName))
        {
            PlayLobbyMusic();
        }
        else
        {
            StopMusic();
        }
    }

    public void PlayLobbyMusic()
    {
        if (lobbyMusic == null)
        {
            Debug.LogWarning("MusicManager: Lobby Music не е зададена.");
            return;
        }

        // Ако вече свири lobby музиката, не я рестартираме.
        if (musicSource.isPlaying && musicSource.clip == lobbyMusic)
        {
            return;
        }

        musicSource.clip = lobbyMusic;
        musicSource.volume = lobbyMusicVolume;
        musicSource.loop = true;

        musicSource.Play();
    }

    public void StopMusic()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
    }
}