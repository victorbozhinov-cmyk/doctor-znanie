using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameFeedbackSoundManager : MonoBehaviour
{
    public static GameFeedbackSoundManager Instance
    {
        get;
        private set;
    }

    [Header("Success / Game Over / Victory")]
    [SerializeField]
    private AudioClip successClip;

    [SerializeField]
    private AudioClip gameOverClip;

    [SerializeField]
    private AudioClip victoryClip;

    [Header("Correct / Wrong Answer")]
    [SerializeField]
    private AudioClip correctClip;

    [SerializeField]
    private AudioClip wrongClip;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField]
    private float successVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField]
    private float gameOverVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField]
    private float victoryVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField]
    private float correctVolume = 0.75f;

    [Range(0f, 1f)]
    [SerializeField]
    private float wrongVolume = 0.75f;

    [Header("Panel Crossfade")]
    [Tooltip(
        "Колко секунди преди края на Success звука " +
        "той започва да затихва, а Lobby музиката да се усилва."
    )]
    [SerializeField]
    private float successCrossfadeDuration = 1.25f;

    [Tooltip(
        "Колко секунди преди края на Game Over звука " +
        "той започва да затихва, а Lobby музиката да се усилва."
    )]
    [SerializeField]
    private float gameOverCrossfadeDuration = 1.25f;

    [Tooltip(
        "Колко секунди преди края на Victory звука " +
        "той започва да затихва, а Lobby музиката да се усилва."
    )]
    [SerializeField]
    private float victoryCrossfadeDuration = 1.25f;

    private AudioSource audioSource;

    private Coroutine panelFeedbackCoroutine;

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

        audioSource =
            GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
    }

    // =====================================================
    // SUCCESS / GAME OVER / VICTORY
    // =====================================================

    public void PlaySuccess()
    {
        PlayPanelFeedback(
            successClip,
            successVolume,
            successCrossfadeDuration
        );
    }

    public void PlayGameOver()
    {
        PlayPanelFeedback(
            gameOverClip,
            gameOverVolume,
            gameOverCrossfadeDuration
        );
    }

    public void PlayVictory()
    {
        PlayPanelFeedback(
            victoryClip,
            victoryVolume,
            victoryCrossfadeDuration
        );
    }

    private void PlayPanelFeedback(
        AudioClip clip,
        float volume,
        float crossfadeDuration)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager: " +
                "липсва AudioClip."
            );

            return;
        }

        if (panelFeedbackCoroutine != null)
        {
            StopCoroutine(
                panelFeedbackCoroutine
            );

            panelFeedbackCoroutine = null;
        }

        audioSource.Stop();

        // Lobby музиката започва веднага,
        // но е напълно заглушена.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .PlayLobbyMusicMuted();
        }

        panelFeedbackCoroutine =
            StartCoroutine(
                PlayPanelFeedbackRoutine(
                    clip,
                    volume,
                    crossfadeDuration
                )
            );
    }

    private IEnumerator PlayPanelFeedbackRoutine(
        AudioClip clip,
        float volume,
        float crossfadeDuration)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;

        audioSource.Play();

        float fadeDuration =
            Mathf.Clamp(
                crossfadeDuration,
                0f,
                clip.length
            );

        float fullVolumeDuration =
            Mathf.Max(
                0f,
                clip.length - fadeDuration
            );

        // =================================================
        // Feedback звукът свири сам.
        // Lobby музиката е на 0%.
        // =================================================

        float timer = 0f;

        while (timer < fullVolumeDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        // =================================================
        // CROSSFADE
        //
        // Feedback: 100% -> 0%
        // Lobby:       0% -> нормална сила
        // =================================================

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .FadeInLobbyMusic(
                    fadeDuration
                );
        }

        float startFeedbackVolume =
            volume;

        timer = 0f;

        if (fadeDuration > 0f)
        {
            while (timer < fadeDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer /
                        fadeDuration
                    );

                audioSource.volume =
                    Mathf.Lerp(
                        startFeedbackVolume,
                        0f,
                        t
                    );

                yield return null;
            }
        }

        audioSource.Stop();

        audioSource.clip = null;
        audioSource.volume = 1f;

        panelFeedbackCoroutine = null;
    }

    // =====================================================
    // CORRECT / WRONG
    // =====================================================

    public void PlayCorrect()
    {
        PlayShortFeedback(
            correctClip,
            correctVolume
        );
    }

    public void PlayWrong()
    {
        PlayShortFeedback(
            wrongClip,
            wrongVolume
        );
    }

    private void PlayShortFeedback(
        AudioClip clip,
        float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager: " +
                "липсва AudioClip."
            );

            return;
        }

        // Correct / Wrong не сменят музиката.
        // Само я намаляват леко.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .DuckMusicForFeedback(
                    clip.length
                );
        }

        audioSource.PlayOneShot(
            clip,
            volume
        );
    }
}