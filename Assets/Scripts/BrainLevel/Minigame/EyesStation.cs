using UnityEngine;
using UnityEngine.InputSystem;

public class EyesStation : MonoBehaviour
{
    [Header("Problem Token")]
    [SerializeField] private Sprite eyeProblemTokenSprite;
    [SerializeField] private GameObject problemTokenVisual;

    private BrainTokenCarrier playerCarrier;
    private bool playerInside = false;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        TryGiveProblemToken();
    }

    private void TryGiveProblemToken()
    {
        if (playerCarrier == null)
            return;

        // Героят вече носи нещо.
        if (playerCarrier.IsCarryingToken)
            return;

        // Проблемът вече е взет.
        if (problemTokenVisual == null || !problemTokenVisual.activeSelf)
            return;

        playerCarrier.PickUpToken(eyeProblemTokenSprite);

        problemTokenVisual.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BrainTokenCarrier carrier = other.GetComponent<BrainTokenCarrier>();

        if (carrier == null)
            return;

        playerCarrier = carrier;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BrainTokenCarrier carrier = other.GetComponent<BrainTokenCarrier>();

        if (carrier == null)
            return;

        if (carrier == playerCarrier)
        {
            playerCarrier = null;
            playerInside = false;
        }
    }
}