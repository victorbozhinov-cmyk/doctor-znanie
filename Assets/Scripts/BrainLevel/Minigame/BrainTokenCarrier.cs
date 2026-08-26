using UnityEngine;

public class BrainTokenCarrier : MonoBehaviour
{
    [Header("Carried Token")]
    [SerializeField] private SpriteRenderer carriedTokenRenderer;

    public bool IsCarryingToken { get; private set; }

    private void Awake()
    {
        if (carriedTokenRenderer != null)
        {
            carriedTokenRenderer.sprite = null;
            carriedTokenRenderer.enabled = false;
        }

        IsCarryingToken = false;
    }

    public void PickUpToken(Sprite tokenSprite)
    {
        if (carriedTokenRenderer == null || tokenSprite == null)
            return;

        carriedTokenRenderer.sprite = tokenSprite;
        carriedTokenRenderer.enabled = true;

        IsCarryingToken = true;
    }

    public void DropToken()
    {
        if (carriedTokenRenderer == null)
            return;

        carriedTokenRenderer.sprite = null;
        carriedTokenRenderer.enabled = false;

        IsCarryingToken = false;
    }
}