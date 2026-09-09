using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BrainMinigameAudio : MonoBehaviour
{
    [Header("Token")]
    [SerializeField] private AudioClip tokenPopClip;

    [Range(0f, 1f)]
    [SerializeField] private float tokenPopVolume = 0.40f;

    [Header("Wrong Station")]
    [SerializeField] private AudioClip wrongStationClip;

    [Range(0f, 1f)]
    [SerializeField] private float wrongStationVolume = 0.55f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    // =========================================================
    // TOKEN PICKUP / DROP
    // =========================================================

    public void PlayTokenPop()
    {
        PlayOneShot(
            tokenPopClip,
            tokenPopVolume
        );
    }

    // =========================================================
    // WRONG STATION
    // =========================================================

    public void PlayWrongStation()
    {
        PlayOneShot(
            wrongStationClip,
            wrongStationVolume
        );
    }

    // =========================================================
    // CORRECT DELIVERY
    // =========================================================

    public void PlayCorrect()
    {
        if (GameFeedbackSoundManager.Instance != null)
        {
            GameFeedbackSoundManager
                .Instance
                .PlayCorrect();
        }
        else
        {
            Debug.LogWarning(
                "GameFeedbackSoundManager не е намерен. " +
                "Стартирай играта през Bootstrap."
            );
        }
    }

    // =========================================================
    // ONE SHOT
    // =========================================================

    private void PlayOneShot(
        AudioClip clip,
        float volume)
    {
        if (audioSource == null ||
            clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            clip,
            volume
        );
    }
}