using UnityEngine;

public class BrainTokenCarrier : MonoBehaviour
{
    [Header("Carried Token")]
    [SerializeField]
    private SpriteRenderer carriedTokenRenderer;

    [Header("Audio")]
    [SerializeField]
    private BrainMinigameAudio minigameAudio;

    public bool IsCarryingToken
    {
        get;
        private set;
    }

    public BrainTokenType CurrentTokenType
    {
        get;
        private set;
    } = BrainTokenType.None;

    private void Awake()
    {
        if (minigameAudio == null)
        {
            minigameAudio =
                FindFirstObjectByType<
                    BrainMinigameAudio>();
        }

        if (carriedTokenRenderer != null)
        {
            carriedTokenRenderer.sprite = null;
            carriedTokenRenderer.enabled = false;
        }

        IsCarryingToken = false;

        CurrentTokenType =
            BrainTokenType.None;
    }

    // =========================================================
    // PICK UP
    // =========================================================

    public void PickUpToken(
        Sprite tokenSprite,
        BrainTokenType tokenType)
    {
        if (carriedTokenRenderer == null ||
            tokenSprite == null)
        {
            return;
        }

        carriedTokenRenderer.sprite =
            tokenSprite;

        carriedTokenRenderer.enabled =
            true;

        CurrentTokenType =
            tokenType;

        IsCarryingToken =
            true;

        // Лек POP при реално взимане на токен.
        if (minigameAudio != null)
        {
            minigameAudio
                .PlayTokenPop();
        }
    }

    // =========================================================
    // DROP
    // =========================================================

    public void DropToken(
        bool playSound = true)
    {
        if (carriedTokenRenderer == null)
        {
            return;
        }

        bool hadToken =
            IsCarryingToken;

        carriedTokenRenderer.sprite =
            null;

        carriedTokenRenderer.enabled =
            false;

        CurrentTokenType =
            BrainTokenType.None;

        IsCarryingToken =
            false;

        // POP има само при истинско
        // поставяне от играча.
        //
        // При автоматично махане на токен
        // използваме DropToken(false).
        if (playSound &&
            hadToken &&
            minigameAudio != null)
        {
            minigameAudio
                .PlayTokenPop();
        }
    }
}