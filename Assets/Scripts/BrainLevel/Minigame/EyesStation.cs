using UnityEngine;
using UnityEngine.InputSystem;

public class EyesStation : MonoBehaviour
{
    [Header("Problem Token")]
    [SerializeField]
    private Sprite eyeProblemTokenSprite;

    [SerializeField]
    private GameObject problemTokenVisual;

    [Header("Command Token")]
    [SerializeField]
    private BrainTokenType acceptedCommandTokenType =
        BrainTokenType.EyesCommand;

    private BrainTokenCarrier playerCarrier;
    private bool playerInside = false;

    private bool orderCompleted = false;

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
        // Ако героят носи токен,
        // проверяваме дали връща Command токена.
        if (playerCarrier.IsCarryingToken)
        {
            TryReceiveCommandToken();
            return;
        }

        // Ако не носи нищо,
        // опитваме да му дадем Problem токена.
        TryGiveProblemToken();
    }

    private void TryGiveProblemToken()
    {
        if (orderCompleted)
            return;

        if (playerCarrier == null)
            return;

        if (playerCarrier.IsCarryingToken)
            return;

        if (problemTokenVisual == null ||
            !problemTokenVisual.activeSelf)
            return;

        if (eyeProblemTokenSprite == null)
            return;

        playerCarrier.PickUpToken(
            eyeProblemTokenSprite,
            BrainTokenType.EyesProblem);

        problemTokenVisual.SetActive(false);

        Debug.Log(
            "Eyes Problem token picked up.");
    }

    private void TryReceiveCommandToken()
    {
        if (playerCarrier == null)
            return;

        if (!playerCarrier.IsCarryingToken)
            return;

        if (playerCarrier.CurrentTokenType !=
            acceptedCommandTokenType)
        {
            // Играчът носи друг токен.
            return;
        }

        // Зелената команда е върната
        // на правилния орган.
        playerCarrier.DropToken();

        orderCompleted = true;

        Debug.Log(
            "Eyes order completed successfully.");
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