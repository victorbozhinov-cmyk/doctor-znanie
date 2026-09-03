using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BrainOrganStation : MonoBehaviour
{
    [System.Serializable]
    public class AdditionalProblemVariant
    {
        [Header("Problem")]
        public BrainTokenType problemTokenType =
            BrainTokenType.None;

        public Sprite problemTokenSprite;

        [Header("Command")]
        public BrainTokenType acceptedCommandTokenType =
            BrainTokenType.None;

        [Header("Brain Drop Zone")]
        public BrainMinigameDropZone brainDropZone;
    }

    [Header("Main Problem Token")]
    [SerializeField]
    private BrainTokenType problemTokenType =
        BrainTokenType.None;

    [SerializeField]
    private Sprite problemTokenSprite;

    [SerializeField]
    private GameObject problemTokenVisual;

    [Header("Main Command Token")]
    [SerializeField]
    private BrainTokenType acceptedCommandTokenType =
        BrainTokenType.None;

    [Header("Main Brain Drop Zone")]
    [SerializeField]
    private BrainMinigameDropZone brainDropZone;

    [Header("Additional Problem Variants")]
    [SerializeField]
    private AdditionalProblemVariant[]
        additionalProblemVariants;

    [Header("Order UI")]
    [SerializeField]
    private BrainOrderUI orderUI;

    [Header("Failure / Success Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    private BrainTokenCarrier playerCarrier;

    private bool playerInside = false;
    private bool orderActive = false;
    private bool orderCompleted = false;
    private bool orderFailed = false;

    private BrainTokenType
        selectedProblemTokenType =
            BrainTokenType.None;

    private Sprite selectedProblemTokenSprite;

    private BrainTokenType
        selectedCommandTokenType =
            BrainTokenType.None;

    private BrainMinigameDropZone
        selectedBrainDropZone;

    private bool variantPrepared = false;

    public event Action<BrainOrganStation>
        OrderCompleted;

    public event Action<BrainOrganStation>
        OrderFailed;

    public bool CanStartOrder =>
        !orderActive;

    public BrainMinigameDropZone BrainDropZone
    {
        get
        {
            if (variantPrepared &&
                selectedBrainDropZone != null)
            {
                return selectedBrainDropZone;
            }

            return brainDropZone;
        }
    }

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

        variantPrepared = false;

        ResetSelectedVariant();

        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        if (orderUI != null)
        {
            orderUI.CancelOrder();
        }
    }

    public void PrepareNextVariant()
    {
        if (orderActive)
            return;

        int additionalCount = 0;

        if (additionalProblemVariants != null)
        {
            additionalCount =
                additionalProblemVariants.Length;
        }

        int totalVariants =
            1 + additionalCount;

        int selectedIndex =
            UnityEngine.Random.Range(
                0,
                totalVariants);

        if (selectedIndex == 0)
        {
            SelectMainVariant();
        }
        else
        {
            AdditionalProblemVariant variant =
                additionalProblemVariants[
                    selectedIndex - 1];

            if (variant == null)
            {
                SelectMainVariant();
            }
            else
            {
                selectedProblemTokenType =
                    variant.problemTokenType;

                selectedProblemTokenSprite =
                    variant.problemTokenSprite;

                selectedCommandTokenType =
                    variant.acceptedCommandTokenType;

                selectedBrainDropZone =
                    variant.brainDropZone;
            }
        }

        variantPrepared = true;
    }

    private void SelectMainVariant()
    {
        selectedProblemTokenType =
            problemTokenType;

        selectedProblemTokenSprite =
            problemTokenSprite;

        selectedCommandTokenType =
            acceptedCommandTokenType;

        selectedBrainDropZone =
            brainDropZone;
    }

    private void ResetSelectedVariant()
    {
        selectedProblemTokenType =
            BrainTokenType.None;

        selectedProblemTokenSprite = null;

        selectedCommandTokenType =
            BrainTokenType.None;

        selectedBrainDropZone = null;
    }

    public void StartManagedOrder(
        float orderDuration)
    {
        if (orderActive)
            return;

        if (!variantPrepared)
        {
            PrepareNextVariant();
        }

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
                selectedProblemTokenType,
                orderDuration);
        }

        Debug.Log(
            gameObject.name +
            " started managed order: " +
            selectedProblemTokenType);
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

        if (selectedProblemTokenSprite == null)
            return;

        playerCarrier.PickUpToken(
            selectedProblemTokenSprite,
            selectedProblemTokenType);

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
            selectedCommandTokenType)
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

        if (screenFlash != null)
        {
            screenFlash.PlayGreenFlash();
        }

        variantPrepared = false;

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

        variantPrepared = false;

        OrderFailed?.Invoke(this);
    }

    public void CancelOrder()
    {
        if (!orderActive &&
            !orderCompleted &&
            !orderFailed)
        {
            variantPrepared = false;
            return;
        }

        orderActive = false;
        orderCompleted = false;
        orderFailed = false;

        ClearOrderTokens();

        variantPrepared = false;

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

        if (selectedBrainDropZone != null)
        {
            selectedBrainDropZone.ClearCommandToken();
        }

        BrainTokenCarrier carrier =
            FindFirstObjectByType<
                BrainTokenCarrier>();

        if (carrier != null &&
            carrier.IsCarryingToken)
        {
            if (carrier.CurrentTokenType ==
                    selectedProblemTokenType ||
                carrier.CurrentTokenType ==
                    selectedCommandTokenType)
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