using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameFeedbackSoundManager : MonoBehaviour
{
    public static GameFeedbackSoundManager Instance
    {
        get;
        private set;
    }

    [Header("Success / Game Over")]
    [SerializeField]
    private AudioClip successClip;

    [SerializeField]
    private AudioClip gameOverClip;

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
    private float correctVolume = 0.75f;

    [Range(0f, 1f)]
    [SerializeField]
    private float wrongVolume = 0.75f;

    private AudioSource audioSource;

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
    }

    public void PlaySuccess()
    {
        PlayFeedback(
            successClip,
            successVolume
        );
    }

    public void PlayGameOver()
    {
        PlayFeedback(
            gameOverClip,
            gameOverVolume
        );
    }

    public void PlayCorrect()
    {
        PlayFeedback(
            correctClip,
            correctVolume
        );
    }

    public void PlayWrong()
    {
        PlayFeedback(
            wrongClip,
            wrongVolume
        );
    }

    private void PlayFeedback(
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

        // Леко намаляваме музиката
        // за времетраенето на SFX-а.
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