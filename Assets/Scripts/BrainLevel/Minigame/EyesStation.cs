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

    [Header("Brain Drop Zone")]
    [SerializeField]
    private BrainMinigameDropZone brainDropZone;

    [Header("Order UI")]
    [SerializeField]
    private EyesOrderUI orderUI;

    private BrainTokenCarrier playerCarrier;

    private bool playerInside = false;
    private bool orderCompleted = false;
    private bool orderFailed = false;

    private void OnEnable()
    {
        if (orderUI != null)
        {
            orderUI.OrderFailed += HandleOrderFailed;
        }
    }

    private void OnDisable()
    {
        if (orderUI != null)
        {
            orderUI.OrderFailed -= HandleOrderFailed;
        }
    }

    private void Start()
    {
        // Засега поръчката стартира веднага,
        // за да тестваме UI системата.
        if (orderUI != null)
        {
            orderUI.StartOrder();
        }
    }

    private void Update()
    {
        if (orderCompleted || orderFailed)
            return;

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
        if (orderCompleted || orderFailed)
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

        if (orderUI != null)
        {
            orderUI.NotifyProblemPickedUp();
        }

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
            return;
        }

        playerCarrier.DropToken();

        orderCompleted = true;

        if (orderUI != null)
        {
            orderUI.CompleteOrder();
        }

        Debug.Log(
            "Eyes order completed successfully.");
    }

    // =====================================================
    // ORDER FAILURE
    // =====================================================

    private void HandleOrderFailed()
    {
        if (orderCompleted || orderFailed)
            return;

        orderFailed = true;

        // 1. Ако червеният Problem токен
        // още е при EyesStation, го махаме.
        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        // 2. Ако зеленият Command токен
        // е останал върху мозъчната зона, го махаме.
        if (brainDropZone != null)
        {
            brainDropZone.ClearCommandToken();
        }

        // 3. Ако играчът в момента носи
        // EyesProblem или EyesCommand,
        // махаме токена от ръцете му.
        BrainTokenCarrier carrier =
            FindFirstObjectByType<BrainTokenCarrier>();

        if (carrier != null &&
            carrier.IsCarryingToken)
        {
            if (carrier.CurrentTokenType ==
                    BrainTokenType.EyesProblem ||
                carrier.CurrentTokenType ==
                    BrainTokenType.EyesCommand)
            {
                carrier.DropToken();
            }
        }

        Debug.Log(
            "Eyes order failed and all Eyes tokens were cleared.");
    }

    // =====================================================
    // TRIGGER
    // =====================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<BrainTokenCarrier>();

        if (carrier == null)
            return;

        playerCarrier = carrier;
        playerInside = true;

        if (orderUI != null)
        {
            orderUI.SetPlayerNearby(true);
        }
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

            if (orderUI != null)
            {
                orderUI.SetPlayerNearby(false);
            }
        }
    }
}