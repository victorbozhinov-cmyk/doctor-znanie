using UnityEngine;

public class HeartMinigameAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource ecgSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("ECG Monitor")]
    [SerializeField]
    private AudioClip ecgClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float ecgVolume = 0.20f;

    [Header("Event Pop")]
    [SerializeField]
    private AudioClip eventPopClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float eventPopVolume = 0.60f;

    private void Awake()
    {
        ConfigureAudioSources();
    }

    private void OnEnable()
    {
        // Когато MinigamePanel стане активен,
        // сменяме Puzzle Music с Heart Minigame Music.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance
                .PlayHeartMinigameMusic();
        }

        StartECG();
    }

    private void OnDisable()
    {
        StopECG();
    }

    private void ConfigureAudioSources()
    {
        if (ecgSource != null)
        {
            ecgSource.playOnAwake = false;
            ecgSource.loop = true;
            ecgSource.spatialBlend = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void StartECG()
    {
        if (ecgSource == null)
        {
            Debug.LogWarning(
                "HeartMinigameAudio: ECG AudioSource липсва."
            );

            return;
        }

        if (ecgClip == null)
        {
            Debug.LogWarning(
                "HeartMinigameAudio: ECG AudioClip липсва."
            );

            return;
        }

        ecgSource.clip = ecgClip;
        ecgSource.volume = ecgVolume;
        ecgSource.loop = true;

        if (!ecgSource.isPlaying)
        {
            ecgSource.Play();
        }
    }

    public void StopECG()
    {
        if (ecgSource == null)
        {
            return;
        }

        ecgSource.Stop();
    }

    public void PlayEventPop()
    {
        if (sfxSource == null ||
            eventPopClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            eventPopClip,
            eventPopVolume
        );
    }
}