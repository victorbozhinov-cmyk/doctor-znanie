using UnityEngine;
using UnityEngine.InputSystem;

public class BrainOrganStation : MonoBehaviour
{
    [Header("Problem Token")]
    [SerializeField]
    private BrainTokenType problemTokenType =
        BrainTokenType.None;

    [SerializeField]
    private Sprite problemTokenSprite;

    [SerializeField]
    private GameObject problemTokenVisual;

    [Header("Command Token")]
    [SerializeField]
    private BrainTokenType acceptedCommandTokenType =
        BrainTokenType.None;

    [Header("Brain Drop Zone")]
    [SerializeField]
    private BrainMinigameDropZone brainDropZone;

    [Header("Order UI")]
    [SerializeField]
    private BrainOrderUI orderUI;

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

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
        // Засега поръчката стартира веднага.
        // По-късно това ще се управлява от Order Manager.
        if (orderUI != null)
        {
            orderUI.StartOrder(problemTokenType);
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
        if (playerCarrier.IsCarryingToken)
        {
            TryReceiveCommandToken();
            return;
        }

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

        if (problemTokenSprite == null)
            return;

        if (problemTokenType == BrainTokenType.None)
            return;

        playerCarrier.PickUpToken(
            problemTokenSprite,
            problemTokenType);

        problemTokenVisual.SetActive(false);

        if (orderUI != null)
        {
            orderUI.NotifyProblemPickedUp();
        }

        Debug.Log(
            gameObject.name +
            " Problem token picked up: " +
            problemTokenType);
    }

    private void TryReceiveCommandToken()
    {
        if (playerCarrier == null)
            return;

        if (!playerCarrier.IsCarryingToken)
            return;

        // =====================================================
        // ГРЕШЕН COMMAND TOKEN
        // =====================================================

        if (playerCarrier.CurrentTokenType !=
            acceptedCommandTokenType)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();
            }

            Debug.Log(
                gameObject.name +
                " rejected Command token: " +
                playerCarrier.CurrentTokenType);

            return;
        }

        // =====================================================
        // ПРАВИЛЕН COMMAND TOKEN
        // =====================================================

        playerCarrier.DropToken();

        orderCompleted = true;

        if (orderUI != null)
        {
            orderUI.CompleteOrder();
        }

        Debug.Log(
            gameObject.name +
            " order completed successfully.");
    }

    private void HandleOrderFailed()
    {
        if (orderCompleted || orderFailed)
            return;

        orderFailed = true;

        // Ако Problem токенът още е на станцията,
        // го махаме.
        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        // Ако Command токенът е останал върху
        // правилната мозъчна зона,
        // го махаме.
        if (brainDropZone != null)
        {
            brainDropZone.ClearCommandToken();
        }

        // Ако играчът носи токен,
        // принадлежащ на тази поръчка,
        // го махаме.
        BrainTokenCarrier carrier =
            FindFirstObjectByType<BrainTokenCarrier>();

        if (carrier != null &&
            carrier.IsCarryingToken)
        {
            if (carrier.CurrentTokenType ==
                    problemTokenType ||
                carrier.CurrentTokenType ==
                    acceptedCommandTokenType)
            {
                carrier.DropToken();
            }
        }

        Debug.Log(
            gameObject.name +
            " order failed and its tokens were cleared.");
    }

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