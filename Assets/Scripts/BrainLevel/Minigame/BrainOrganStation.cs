using System;
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
    private bool orderActive = false;
    private bool orderCompleted = false;
    private bool orderFailed = false;

    public event Action<BrainOrganStation>
        OrderCompleted;

    public event Action<BrainOrganStation>
        OrderFailed;

    public bool CanStartOrder =>
        !orderActive;

    public BrainMinigameDropZone BrainDropZone =>
        brainDropZone;

    private void OnEnable()
    {
        if (orderUI != null)
        {
            orderUI.OrderFailed +=
                HandleOrderFailed;
        }
    }

    private void OnDisable()
    {
        if (orderUI != null)
        {
            orderUI.OrderFailed -=
                HandleOrderFailed;
        }
    }

    private void Update()
    {
        if (!orderActive ||
            orderCompleted ||
            orderFailed)
        {
            return;
        }

        if (!playerInside)
            return;

        if (playerCarrier == null)
            return;

        if (Keyboard.current == null)
            return;

        bool interactPressed =
            Keyboard.current.eKey
                .wasPressedThisFrame ||
            Keyboard.current.spaceKey
                .wasPressedThisFrame;

        if (!interactPressed)
            return;

        TryInteract();
    }

    public void PrepareForManager()
    {
        orderActive = false;
        orderCompleted = false;
        orderFailed = false;

        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        if (orderUI != null)
        {
            orderUI.CancelOrder();
        }
    }

    public void StartManagedOrder(
        float orderDuration)
    {
        if (orderActive)
            return;

        orderActive = true;
        orderCompleted = false;
        orderFailed = false;

        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(true);
        }

        if (orderUI != null)
        {
            orderUI.StartOrder(
                problemTokenType,
                orderDuration);
        }

        Debug.Log(
            gameObject.name +
            " started managed order: " +
            problemTokenType);
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
        if (!orderActive ||
            orderCompleted ||
            orderFailed)
        {
            return;
        }

        if (problemTokenVisual == null ||
            !problemTokenVisual.activeSelf)
        {
            return;
        }

        if (problemTokenSprite == null)
            return;

        playerCarrier.PickUpToken(
            problemTokenSprite,
            problemTokenType);

        problemTokenVisual.SetActive(false);

        if (orderUI != null)
        {
            orderUI.NotifyProblemPickedUp();
        }
    }

    private void TryReceiveCommandToken()
    {
        if (!playerCarrier.IsCarryingToken)
            return;

        if (playerCarrier.CurrentTokenType !=
            acceptedCommandTokenType)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();
            }

            return;
        }

        playerCarrier.DropToken();

        orderCompleted = true;
        orderActive = false;

        if (orderUI != null)
        {
            orderUI.CompleteOrder();
        }

        OrderCompleted?.Invoke(this);
    }

    private void HandleOrderFailed()
    {
        if (!orderActive ||
            orderCompleted ||
            orderFailed)
        {
            return;
        }

        orderFailed = true;
        orderActive = false;

        ClearOrderTokens();

        OrderFailed?.Invoke(this);
    }

    public void CancelOrder()
    {
        if (!orderActive &&
            !orderCompleted &&
            !orderFailed)
        {
            return;
        }

        orderActive = false;
        orderCompleted = false;
        orderFailed = false;

        ClearOrderTokens();

        if (orderUI != null)
        {
            orderUI.CancelOrder();
        }
    }

    private void ClearOrderTokens()
    {
        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        if (brainDropZone != null)
        {
            brainDropZone.ClearCommandToken();
        }

        BrainTokenCarrier carrier =
            FindFirstObjectByType<
                BrainTokenCarrier>();

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
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<
                BrainTokenCarrier>();

        if (carrier == null)
            return;

        playerCarrier = carrier;
        playerInside = true;

        if (orderUI != null)
        {
            orderUI.SetPlayerNearby(true);
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<
                BrainTokenCarrier>();

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