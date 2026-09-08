using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StomachMinigameAudio : MonoBehaviour
{
    [Header("Stomach Juices")]
    [SerializeField] private AudioClip stomachAcidsClip;

    [Range(0f, 1f)]
    [SerializeField] private float stomachAcidsVolume = 0.50f;

    [SerializeField] private AudioClip greenZoneHitClip;

    [Range(0f, 1f)]
    [SerializeField] private float greenZoneHitVolume = 0.55f;

    [Header("Stomach Irritation")]
    [SerializeField] private AudioClip stomachGrowlClip;

    [Range(0f, 1f)]
    [SerializeField] private float stomachGrowlVolume = 0.55f;

    [Header("Peristalsis")]
    [SerializeField] private AudioClip checkpointPopClip;

    [Range(0f, 1f)]
    [SerializeField] private float checkpointPopVolume = 0.45f;

    [SerializeField] private AudioClip splat1Clip;

    [Range(0f, 1f)]
    [SerializeField] private float splat1Volume = 0.45f;

    [SerializeField] private AudioClip splat2Clip;

    [Range(0f, 1f)]
    [SerializeField] private float splat2Volume = 0.45f;

    [Header("State Card")]
    [SerializeField] private AudioClip cardPopClip;

    [Range(0f, 1f)]
    [SerializeField] private float cardPopVolume = 0.40f;

    [Header("Task Timer")]
    [SerializeField] private AudioClip fastTickingClip;

    [Range(0f, 1f)]
    [SerializeField] private float fastTickingVolume = 0.35f;

    private AudioSource sfxSource;
    private AudioSource tickingSource;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        sfxSource =
            GetComponent<AudioSource>();

        tickingSource =
            gameObject.AddComponent<AudioSource>();

        ConfigureSource(
            sfxSource,
            false
        );

        ConfigureSource(
            tickingSource,
            true
        );
    }

    private void OnDisable()
    {
        StopFastTicking();
    }

    // =========================================================
    // SOURCE SETUP
    // =========================================================

    private void ConfigureSource(
        AudioSource source,
        bool loop)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = 1f;
    }

    // =========================================================
    // STOMACH JUICES
    // =========================================================

    public void PlayStomachAcids()
    {
        PlayOneShot(
            stomachAcidsClip,
            stomachAcidsVolume
        );
    }

    public void PlayGreenZoneHit()
    {
        PlayOneShot(
            greenZoneHitClip,
            greenZoneHitVolume
        );
    }

    // =========================================================
    // IRRITATION
    // =========================================================

    public void PlayStomachGrowl()
    {
        PlayOneShot(
            stomachGrowlClip,
            stomachGrowlVolume
        );
    }

    // =========================================================
    // PERISTALSIS
    // =========================================================

    public void PlayCheckpointPop()
    {
        PlayOneShot(
            checkpointPopClip,
            checkpointPopVolume
        );
    }

    public void PlaySplat1()
    {
        PlayOneShot(
            splat1Clip,
            splat1Volume
        );
    }

    public void PlaySplat2()
    {
        PlayOneShot(
            splat2Clip,
            splat2Volume
        );
    }

    // =========================================================
    // STATE CARD
    // =========================================================

    public void PlayCardPop()
    {
        PlayOneShot(
            cardPopClip,
            cardPopVolume
        );
    }

    // =========================================================
    // FAST TICKING
    // =========================================================

    public void StartFastTicking()
    {
        if (fastTickingClip == null ||
            tickingSource == null)
        {
            return;
        }

        if (tickingSource.isPlaying &&
            tickingSource.clip ==
            fastTickingClip)
        {
            return;
        }

        tickingSource.Stop();

        tickingSource.clip =
            fastTickingClip;

        tickingSource.volume =
            fastTickingVolume;

        tickingSource.loop = true;

        tickingSource.Play();
    }

    public void StopFastTicking()
    {
        if (tickingSource == null)
        {
            return;
        }

        tickingSource.Stop();
        tickingSource.clip = null;
    }

    // =========================================================
    // ONE SHOT
    // =========================================================

    private void PlayOneShot(
        AudioClip clip,
        float volume)
    {
        if (clip == null ||
            sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            volume
        );
    }
}