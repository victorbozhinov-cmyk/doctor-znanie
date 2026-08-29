using UnityEngine;
using UnityEngine.InputSystem;

public class BrainMinigameDropZone : MonoBehaviour
{
    [Header("Problem Token")]
    [SerializeField]
    private BrainTokenType acceptedProblemTokenType =
        BrainTokenType.EyesProblem;

    [Header("Command Token")]
    [SerializeField]
    private BrainTokenType commandTokenType =
        BrainTokenType.EyesCommand;

    [SerializeField]
    private Sprite commandTokenSprite;

    [SerializeField]
    private GameObject commandTokenVisual;

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    private BrainTokenCarrier playerCarrier;
    private bool playerInside = false;

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
        // Ако носим токен, винаги правим опит за drop.
        if (playerCarrier.IsCarryingToken)
        {
            TryDeliverProblemToken();
            return;
        }

        TryPickUpCommandToken();
    }

    private void TryDeliverProblemToken()
    {
        // =====================================================
        // ГРЕШНА ЗОНА
        // =====================================================

        if (playerCarrier.CurrentTokenType !=
            acceptedProblemTokenType)
        {
            // Реален drop НЕ се случва.
            // Токенът остава в ръцете.

            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();
            }

            Debug.Log(
                "Wrong brain zone. Drop rejected. Token stays carried: "
                + playerCarrier.CurrentTokenType);

            return;
        }

        // =====================================================
        // ПРАВИЛНА ЗОНА
        // =====================================================

        playerCarrier.DropToken();

        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(true);
        }

        Debug.Log(
            "Problem token delivered successfully at: "
            + gameObject.name);
    }

    private void TryPickUpCommandToken()
    {
        if (playerCarrier.IsCarryingToken)
            return;

        if (commandTokenVisual == null)
            return;

        if (!commandTokenVisual.activeSelf)
            return;

        if (commandTokenSprite == null)
            return;

        playerCarrier.PickUpToken(
            commandTokenSprite,
            commandTokenType);

        commandTokenVisual.SetActive(false);

        Debug.Log(
            "Command token picked up from: "
            + gameObject.name);
    }

    public void ClearCommandToken()
    {
        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }

        Debug.Log(
            "Command token cleared from: "
            + gameObject.name);
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