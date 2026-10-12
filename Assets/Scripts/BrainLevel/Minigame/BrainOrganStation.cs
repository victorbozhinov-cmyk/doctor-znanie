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

    [Header("Order Spawn Animation")]
    [SerializeField]
    private BrainOrderSpawnAnimation
        orderSpawnAnimation;

    [Header("Failure / Success Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    [Header("Audio")]
    [SerializeField]
    private BrainMinigameAudio minigameAudio;

    // =========================================================
    // INTERNAL
    // =========================================================

    private BrainTokenCarrier playerCarrier;

    private bool playerInside = false;
    private bool orderActive = false;
    private bool orderCompleted = false;
    private bool orderFailed = false;

    // =========================================================
    // TUTORIAL
    // =========================================================

    private bool isTutorialOrder = false;

    public bool IsTutorialOrder =>
        isTutorialOrder;

    // =========================================================
    // SELECTED VARIANT
    // =========================================================

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

    // =========================================================
    // EVENTS
    // =========================================================

    public event Action<BrainOrganStation>
        OrderCompleted;

    public event Action<BrainOrganStation>
        OrderFailed;

    public event Action
        WrongCommandDelivered;

    // NEW: Notifies manager when problem is picked up.
    public event Action<BrainOrganStation>
        ProblemPickedUp;

    // =========================================================
    // PROPERTIES
    // =========================================================

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

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (minigameAudio == null)
        {
            minigameAudio =
                FindFirstObjectByType<
                    BrainMinigameAudio>();
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

    // =========================================================
    // PREPARE
    // =========================================================

    public void PrepareForManager()
    {
        orderActive = false;
        orderCompleted = false;
        orderFailed = false;

        // Reset tutorial state.
        isTutorialOrder = false;

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
                totalVariants
            );

        if (selectedIndex == 0)
        {
            SelectMainVariant();
        }
        else
        {
            AdditionalProblemVariant variant =
                additionalProblemVariants[
                    selectedIndex - 1
                ];

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

        selectedProblemTokenSprite =
            null;

        selectedCommandTokenType =
            BrainTokenType.None;

        selectedBrainDropZone =
            null;
    }

    // =========================================================
    // ORDER
    // =========================================================

    public void StartManagedOrder(
        float orderDuration,
        bool tutorial = false)
    {
        if (orderActive)
            return;

        if (!variantPrepared)
        {
            PrepareNextVariant();
        }

        // NEW: Store whether this order is a tutorial.
        isTutorialOrder = tutorial;

        orderActive = true;
        orderCompleted = false;
        orderFailed = false;

        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(true);
        }

        if (orderSpawnAnimation != null)
        {
            orderSpawnAnimation.Play();
        }

        if (orderUI != null)
        {
            // Tutorial orders do not have a timer.
            // Normal orders keep their original timer.
            orderUI.StartOrder(
                selectedProblemTokenType,
                orderDuration,
                !isTutorialOrder
            );
        }

        Debug.Log(
            gameObject.name +
            " started managed order: " +
            selectedProblemTokenType +
            " | Tutorial: " +
            isTutorialOrder
        );
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    private void TryInteract()
    {
        if (playerCarrier.IsCarryingToken)
        {
            TryReceiveCommandToken();

            return;
        }

        TryGiveProblemToken();
    }

    // =========================================================
    // GIVE PROBLEM
    // =========================================================

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
        {
            return;
        }

        // PickUpToken() automatically plays POP.
        playerCarrier.PickUpToken(
            selectedProblemTokenSprite,
            selectedProblemTokenType
        );

        problemTokenVisual.SetActive(false);

        if (orderUI != null)
        {
            orderUI.NotifyProblemPickedUp();
        }

        // =====================================================
        // NEW: PROBLEM PICKED UP EVENT
        // =====================================================

        ProblemPickedUp?.Invoke(this);

        Debug.Log(
            gameObject.name +
            " problem picked up. Tutorial: " +
            isTutorialOrder
        );
    }

    // =========================================================
    // RECEIVE COMMAND
    // =========================================================

    private void TryReceiveCommandToken()
    {
        if (!playerCarrier.IsCarryingToken)
        {
            return;
        }

        // =====================================================
        // WRONG ORGAN STATION
        // =====================================================

        if (playerCarrier.CurrentTokenType !=
            selectedCommandTokenType)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();

                if (minigameAudio != null)
                {
                    minigameAudio
                        .PlayWrongStation();
                }
            }

            WrongCommandDelivered?.Invoke();

            return;
        }

        // =====================================================
        // CORRECT ORGAN STATION
        // =====================================================

        // DropToken() automatically plays POP.
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

            // Global Correct sound.
            if (minigameAudio != null)
            {
                minigameAudio
                    .PlayCorrect();
            }
        }

        variantPrepared = false;

        // Notify manager before resetting tutorial state.
        OrderCompleted?.Invoke(this);

        // NEW: Reset tutorial state.
        isTutorialOrder = false;
    }

    // =========================================================
    // ORDER FAILURE
    // =========================================================

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

        // Reset tutorial state.
        isTutorialOrder = false;
    }

    public void CancelOrder()
    {
        // Always reset tutorial state.
        isTutorialOrder = false;

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

    // =========================================================
    // CLEAR TOKENS
    // =========================================================

    private void ClearOrderTokens()
    {
        if (problemTokenVisual != null)
        {
            problemTokenVisual.SetActive(false);
        }

        if (selectedBrainDropZone != null)
        {
            selectedBrainDropZone
                .ClearCommandToken();
        }

        BrainTokenCarrier carrier =
            FindFirstObjectByType<
                BrainTokenCarrier>();

        if (carrier != null &&
            carrier.IsCarryingToken)
        {
            if (
                carrier.CurrentTokenType ==
                    selectedProblemTokenType ||
                carrier.CurrentTokenType ==
                    selectedCommandTokenType
            )
            {
                /*
                 * Automatic cleanup.
                 * No token POP sound.
                 */
                carrier.DropToken(false);
            }
        }
    }

    // =========================================================
    // TRIGGERS
    // =========================================================

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