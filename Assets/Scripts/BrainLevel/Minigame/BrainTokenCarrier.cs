using UnityEngine;

public class BrainTokenCarrier : MonoBehaviour
{
    [Header("Carried Token")]
    [SerializeField] private SpriteRenderer carriedTokenRenderer;

    public bool IsCarryingToken { get; private set; }

    public BrainTokenType CurrentTokenType { get; private set; }
        = BrainTokenType.None;

    private void Awake()
    {
        if (carriedTokenRenderer != null)
        {
            carriedTokenRenderer.sprite = null;
            carriedTokenRenderer.enabled = false;
        }

        IsCarryingToken = false;
        CurrentTokenType = BrainTokenType.None;
    }

    public void PickUpToken(
        Sprite tokenSprite,
        BrainTokenType tokenType)
    {
        if (carriedTokenRenderer == null ||
            tokenSprite == null)
            return;

        carriedTokenRenderer.sprite = tokenSprite;
        carriedTokenRenderer.enabled = true;

        CurrentTokenType = tokenType;
        IsCarryingToken = true;
    }

    public void DropToken()
    {
        if (carriedTokenRenderer == null)
            return;

        carriedTokenRenderer.sprite = null;
        carriedTokenRenderer.enabled = false;

        CurrentTokenType = BrainTokenType.None;
        IsCarryingToken = false;
    }
}