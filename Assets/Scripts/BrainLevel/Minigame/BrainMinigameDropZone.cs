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

    private BrainTokenCarrier playerCarrier;
    private bool playerInside = false;

    private void Awake()
    {
        // Зеленият Command токен
        // не трябва да се вижда в началото.
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
        // =====================================================
        // 1. Героят носи токен.
        // Опитваме се да предадем Problem токена.
        // =====================================================

        if (playerCarrier.IsCarryingToken)
        {
            TryDeliverProblemToken();
            return;
        }

        // =====================================================
        // 2. Героят НЕ носи токен.
        // Ако Command токенът е готов,
        // може да го вземе.
        // =====================================================

        TryPickUpCommandToken();
    }

    private void TryDeliverProblemToken()
    {
        if (playerCarrier.CurrentTokenType !=
            acceptedProblemTokenType)
        {
            // Грешен токен за тази мозъчна зона.
            return;
        }

        // Премахваме червения Problem токен
        // от ръцете.
        playerCarrier.DropToken();

        // Появява се зеленият Command токен.
        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(true);
        }

        Debug.Log(
            "Problem token delivered. Command token created at: "
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