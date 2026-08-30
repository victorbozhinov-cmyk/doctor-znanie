using UnityEngine;
using UnityEngine.InputSystem;

public class BrainMinigameDropZone : MonoBehaviour
{
    [System.Serializable]
    public class TokenResponse
    {
        [Header("Incoming Problem")]
        public BrainTokenType problemTokenType =
            BrainTokenType.None;

        [Header("Returned Command")]
        public BrainTokenType commandTokenType =
            BrainTokenType.None;

        public Sprite commandTokenSprite;
    }

    [Header("Accepted Problems")]
    [SerializeField]
    private TokenResponse[] tokenResponses;

    [Header("Command Token Visual")]
    [SerializeField]
    private GameObject commandTokenVisual;

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    private BrainTokenCarrier playerCarrier;
    private bool playerInside = false;

    private BrainTokenType activeCommandTokenType =
        BrainTokenType.None;

    private Sprite activeCommandTokenSprite;

    private void Awake()
    {
        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (playerCarrier == null)
            return;

        if (Keyboard.current == null)
            return;

        bool interactPressed =
            Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame;

        if (!interactPressed)
            return;

        TryInteract();
    }

    private void TryInteract()
    {
        if (playerCarrier.IsCarryingToken)
        {
            TryDeliverProblemToken();
            return;
        }

        TryPickUpCommandToken();
    }

    private void TryDeliverProblemToken()
    {
        TokenResponse response =
            FindResponseForProblem(
                playerCarrier.CurrentTokenType);

        // =====================================================
        // ГРЕШНА МОЗЪЧНА ЗОНА
        // =====================================================

        if (response == null)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();
            }

            Debug.Log(
                "Wrong brain zone. Drop rejected. " +
                "Token stays carried: " +
                playerCarrier.CurrentTokenType);

            return;
        }

        // =====================================================
        // ПРАВИЛНА МОЗЪЧНА ЗОНА
        // =====================================================

        playerCarrier.DropToken();

        activeCommandTokenType =
            response.commandTokenType;

        activeCommandTokenSprite =
            response.commandTokenSprite;

        if (commandTokenVisual != null)
        {
            SpriteRenderer spriteRenderer =
                commandTokenVisual.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite =
                    activeCommandTokenSprite;
            }

            commandTokenVisual.SetActive(true);
        }

        Debug.Log(
            "Problem token delivered successfully at: " +
            gameObject.name +
            ". Command prepared: " +
            activeCommandTokenType);
    }

    private void TryPickUpCommandToken()
    {
        if (playerCarrier.IsCarryingToken)
            return;

        if (commandTokenVisual == null)
            return;

        if (!commandTokenVisual.activeSelf)
            return;

        if (activeCommandTokenType ==
            BrainTokenType.None)
            return;

        if (activeCommandTokenSprite == null)
            return;

        playerCarrier.PickUpToken(
            activeCommandTokenSprite,
            activeCommandTokenType);

        commandTokenVisual.SetActive(false);

        Debug.Log(
            "Command token picked up from: " +
            gameObject.name +
            " | Type: " +
            activeCommandTokenType);

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite = null;
    }

    private TokenResponse FindResponseForProblem(
        BrainTokenType problemTokenType)
    {
        if (tokenResponses == null)
            return null;

        foreach (TokenResponse response in
                 tokenResponses)
        {
            if (response == null)
                continue;

            if (response.problemTokenType ==
                problemTokenType)
            {
                return response;
            }
        }

        return null;
    }

    public void ClearCommandToken()
    {
        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite = null;

        Debug.Log(
            "Command token cleared from: " +
            gameObject.name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<BrainTokenCarrier>();

        if (carrier == null)
            return;

        playerCarrier = carrier;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<BrainTokenCarrier>();

        if (carrier == null)
            return;

        if (carrier == playerCarrier)
        {
            playerCarrier = null;
            playerInside = false;
        }
    }
}