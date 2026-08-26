using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BrainTokenCarrier))]
public class BrainMinigamePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Normal Directional Sprites")]
    [SerializeField] private Sprite idleFrontSprite;
    [SerializeField] private Sprite walkUpSprite;
    [SerializeField] private Sprite walkDownSprite;
    [SerializeField] private Sprite walkLeftSprite;
    [SerializeField] private Sprite walkRightSprite;

    [Header("Carry Character Sprites")]
    [SerializeField] private Sprite carryFrontSprite;
    [SerializeField] private Sprite carryLeftSprite;

    [Header("Carry Hands Sprites")]
    [SerializeField] private Sprite carryFrontHandsSprite;
    [SerializeField] private Sprite carryLeftHandsSprite;

    [Header("Carry References")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private Transform carriedTokenTransform;
    [SerializeField] private SpriteRenderer handsOverlayRenderer;

    [Header("Carry Point Positions")]
    [SerializeField] private Vector3 carryPointFrontPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointLeftPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointRightPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointBackPosition = Vector3.zero;

    [Header("Token Local Positions")]
    [SerializeField] private Vector3 tokenFrontPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenLeftPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenRightPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenBackPosition = Vector3.zero;

    [Header("Hands Overlay Local Positions")]
    [SerializeField] private Vector3 handsFrontPosition = Vector3.zero;
    [SerializeField] private Vector3 handsLeftPosition = Vector3.zero;
    [SerializeField] private Vector3 handsRightPosition = Vector3.zero;
    [SerializeField] private Vector3 handsBackPosition = Vector3.zero;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BrainTokenCarrier tokenCarrier;

    private Vector2 moveInput;

    private enum FacingDirection
    {
        Front,
        Left,
        Right,
        Back
    }

    private FacingDirection facingDirection = FacingDirection.Front;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tokenCarrier = GetComponent<BrainTokenCarrier>();

        if (handsOverlayRenderer != null)
        {
            handsOverlayRenderer.enabled = false;
        }
    }

    private void Update()
    {
        ReadMovementInput();
        UpdateFacingDirection();
        UpdateCharacterSprite();
        UpdateCarryVisuals();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            moveInput.x += 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            moveInput.y += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void MovePlayer()
    {
        Vector2 newPosition =
            rb.position +
            moveInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void UpdateFacingDirection()
    {
        // Когато спре, винаги се връща отпред.
        if (moveInput.sqrMagnitude < 0.01f)
        {
            facingDirection = FacingDirection.Front;
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            facingDirection =
                moveInput.x > 0f
                ? FacingDirection.Right
                : FacingDirection.Left;

            return;
        }

        facingDirection =
            moveInput.y > 0f
            ? FacingDirection.Back
            : FacingDirection.Front;
    }

    private void UpdateCharacterSprite()
    {
        bool isCarrying =
            tokenCarrier != null &&
            tokenCarrier.IsCarryingToken;

        if (isCarrying)
        {
            UpdateCarryCharacterSprite();
        }
        else
        {
            UpdateNormalCharacterSprite();
        }
    }

    private void UpdateNormalCharacterSprite()
    {
        if (moveInput.sqrMagnitude < 0.01f)
        {
            spriteRenderer.sprite = idleFrontSprite;
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            spriteRenderer.sprite =
                moveInput.x > 0f
                ? walkRightSprite
                : walkLeftSprite;

            return;
        }

        spriteRenderer.sprite =
            moveInput.y > 0f
            ? walkUpSprite
            : walkDownSprite;
    }

    private void UpdateCarryCharacterSprite()
    {
        switch (facingDirection)
        {
            case FacingDirection.Left:
                spriteRenderer.sprite =
                    carryLeftSprite != null
                    ? carryLeftSprite
                    : carryFrontSprite;
                break;

            case FacingDirection.Back:
                spriteRenderer.sprite = walkUpSprite;
                break;

            case FacingDirection.Right:
                // Засега използваме front,
                // докато добавим right carry sprite.
                spriteRenderer.sprite = carryFrontSprite;
                break;

            default:
                spriteRenderer.sprite = carryFrontSprite;
                break;
        }
    }

    private void UpdateCarryVisuals()
    {
        if (tokenCarrier == null)
            return;

        if (!tokenCarrier.IsCarryingToken)
        {
            if (handsOverlayRenderer != null)
            {
                handsOverlayRenderer.enabled = false;
            }

            return;
        }

        UpdateCarryPointPosition();
        UpdateTokenPosition();
        UpdateHandsOverlay();
    }

    private void UpdateCarryPointPosition()
    {
        if (carryPoint == null)
            return;

        switch (facingDirection)
        {
            case FacingDirection.Left:
                carryPoint.localPosition = carryPointLeftPosition;
                break;

            case FacingDirection.Right:
                carryPoint.localPosition = carryPointRightPosition;
                break;

            case FacingDirection.Back:
                carryPoint.localPosition = carryPointBackPosition;
                break;

            default:
                carryPoint.localPosition = carryPointFrontPosition;
                break;
        }
    }

    private void UpdateTokenPosition()
    {
        if (carriedTokenTransform == null)
            return;

        switch (facingDirection)
        {
            case FacingDirection.Left:
                carriedTokenTransform.localPosition = tokenLeftPosition;
                break;

            case FacingDirection.Right:
                carriedTokenTransform.localPosition = tokenRightPosition;
                break;

            case FacingDirection.Back:
                carriedTokenTransform.localPosition = tokenBackPosition;
                break;

            default:
                carriedTokenTransform.localPosition = tokenFrontPosition;
                break;
        }
    }

    private void UpdateHandsOverlay()
    {
        if (handsOverlayRenderer == null)
            return;

        switch (facingDirection)
        {
            case FacingDirection.Left:
                handsOverlayRenderer.enabled = true;
                handsOverlayRenderer.sprite = carryLeftHandsSprite;
                handsOverlayRenderer.transform.localPosition = handsLeftPosition;
                break;

            case FacingDirection.Right:
                handsOverlayRenderer.enabled = true;

                // Засега front hands,
                // докато добавим right hands.
                handsOverlayRenderer.sprite = carryFrontHandsSprite;
                handsOverlayRenderer.transform.localPosition = handsRightPosition;
                break;

            case FacingDirection.Back:
                // При движение нагоре гледа в гръб.
                handsOverlayRenderer.enabled = false;
                handsOverlayRenderer.transform.localPosition = handsBackPosition;
                break;

            default:
                handsOverlayRenderer.enabled = true;
                handsOverlayRenderer.sprite = carryFrontHandsSprite;
                handsOverlayRenderer.transform.localPosition = handsFrontPosition;
                break;
        }
    }
}