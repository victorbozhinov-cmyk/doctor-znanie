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
    [SerializeField] private Sprite carryFrontMoveSprite;
    [SerializeField] private Sprite carryLeftSprite;
    [SerializeField] private Sprite carryRightSprite;
    [SerializeField] private Sprite carryBackSprite;

    [Header("Carry References")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private Transform carriedTokenTransform;

    [Header("Carry Foregrounds")]
    [SerializeField] private GameObject frontCarryForeground;
    [SerializeField] private GameObject frontCarryMoveForeground;
    [SerializeField] private GameObject leftCarryForeground;
    [SerializeField] private GameObject rightCarryForeground;
    [SerializeField] private GameObject backCarryForeground;

    [Header("Carry Masks")]
    [SerializeField] private GameObject frontCarryMask;
    [SerializeField] private GameObject frontCarryMoveMask;
    [SerializeField] private GameObject leftCarryMask;
    [SerializeField] private GameObject rightCarryMask;
    [SerializeField] private GameObject backCarryMask;

    [Header("Carry Point Positions")]
    [SerializeField] private Vector3 carryPointFrontPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointFrontMovePosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointLeftPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointRightPosition = Vector3.zero;
    [SerializeField] private Vector3 carryPointBackPosition = Vector3.zero;

    [Header("Token Local Positions")]
    [SerializeField] private Vector3 tokenFrontPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenFrontMovePosition = Vector3.zero;
    [SerializeField] private Vector3 tokenLeftPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenRightPosition = Vector3.zero;
    [SerializeField] private Vector3 tokenBackPosition = Vector3.zero;

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

    private bool IsMoving
    {
        get
        {
            return moveInput.sqrMagnitude >= 0.01f;
        }
    }

    private bool IsMovingFront
    {
        get
        {
            return
                facingDirection == FacingDirection.Front &&
                IsMoving;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tokenCarrier = GetComponent<BrainTokenCarrier>();

        DisableAllCarryForegrounds();
        DisableAllCarryMasks();
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

        if (keyboard.aKey.isPressed ||
            keyboard.leftArrowKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (keyboard.dKey.isPressed ||
            keyboard.rightArrowKey.isPressed)
        {
            moveInput.x += 1f;
        }

        if (keyboard.wKey.isPressed ||
            keyboard.upArrowKey.isPressed)
        {
            moveInput.y += 1f;
        }

        if (keyboard.sKey.isPressed ||
            keyboard.downArrowKey.isPressed)
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
            moveInput *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void UpdateFacingDirection()
    {
        // Когато героят спре,
        // се връща към неподвижната Front визия.
        if (!IsMoving)
        {
            facingDirection = FacingDirection.Front;
            return;
        }

        if (Mathf.Abs(moveInput.x) >
            Mathf.Abs(moveInput.y))
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
        if (!IsMoving)
        {
            spriteRenderer.sprite =
                idleFrontSprite;

            return;
        }

        if (Mathf.Abs(moveInput.x) >
            Mathf.Abs(moveInput.y))
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

            case FacingDirection.Right:

                spriteRenderer.sprite =
                    carryRightSprite != null
                    ? carryRightSprite
                    : carryFrontSprite;

                break;

            case FacingDirection.Back:

                spriteRenderer.sprite =
                    carryBackSprite != null
                    ? carryBackSprite
                    : walkUpSprite;

                break;

            case FacingDirection.Front:

                if (IsMovingFront &&
                    carryFrontMoveSprite != null)
                {
                    spriteRenderer.sprite =
                        carryFrontMoveSprite;
                }
                else
                {
                    spriteRenderer.sprite =
                        carryFrontSprite;
                }

                break;
        }
    }

    private void UpdateCarryVisuals()
    {
        if (tokenCarrier == null)
            return;

        if (!tokenCarrier.IsCarryingToken)
        {
            DisableAllCarryForegrounds();
            DisableAllCarryMasks();
            return;
        }

        UpdateCarryPointPosition();
        UpdateTokenPosition();
        UpdateCarryForeground();
    }

    private void UpdateCarryPointPosition()
    {
        if (carryPoint == null)
            return;

        switch (facingDirection)
        {
            case FacingDirection.Left:

                carryPoint.localPosition =
                    carryPointLeftPosition;

                break;

            case FacingDirection.Right:

                carryPoint.localPosition =
                    carryPointRightPosition;

                break;

            case FacingDirection.Back:

                carryPoint.localPosition =
                    carryPointBackPosition;

                break;

            case FacingDirection.Front:

                carryPoint.localPosition =
                    IsMovingFront
                    ? carryPointFrontMovePosition
                    : carryPointFrontPosition;

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

                carriedTokenTransform.localPosition =
                    tokenLeftPosition;

                break;

            case FacingDirection.Right:

                carriedTokenTransform.localPosition =
                    tokenRightPosition;

                break;

            case FacingDirection.Back:

                carriedTokenTransform.localPosition =
                    tokenBackPosition;

                break;

            case FacingDirection.Front:

                carriedTokenTransform.localPosition =
                    IsMovingFront
                    ? tokenFrontMovePosition
                    : tokenFrontPosition;

                break;
        }
    }

    private void UpdateCarryForeground()
    {
        DisableAllCarryForegrounds();
        DisableAllCarryMasks();

        switch (facingDirection)
        {
            case FacingDirection.Left:

                SetCarryDirectionActive(
                    leftCarryForeground,
                    leftCarryMask);

                break;

            case FacingDirection.Right:

                SetCarryDirectionActive(
                    rightCarryForeground,
                    rightCarryMask);

                break;

            case FacingDirection.Back:

                SetCarryDirectionActive(
                    backCarryForeground,
                    backCarryMask);

                break;

            case FacingDirection.Front:

                if (IsMovingFront)
                {
                    SetCarryDirectionActive(
                        frontCarryMoveForeground,
                        frontCarryMoveMask);
                }
                else
                {
                    SetCarryDirectionActive(
                        frontCarryForeground,
                        frontCarryMask);
                }

                break;
        }
    }

    private void SetCarryDirectionActive(
        GameObject foreground,
        GameObject mask)
    {
        if (foreground != null)
        {
            foreground.SetActive(true);
        }

        if (mask != null)
        {
            mask.SetActive(true);
        }
    }

    private void DisableAllCarryForegrounds()
    {
        if (frontCarryForeground != null)
            frontCarryForeground.SetActive(false);

        if (frontCarryMoveForeground != null)
            frontCarryMoveForeground.SetActive(false);

        if (leftCarryForeground != null)
            leftCarryForeground.SetActive(false);

        if (rightCarryForeground != null)
            rightCarryForeground.SetActive(false);

        if (backCarryForeground != null)
            backCarryForeground.SetActive(false);
    }

    private void DisableAllCarryMasks()
    {
        if (frontCarryMask != null)
            frontCarryMask.SetActive(false);

        if (frontCarryMoveMask != null)
            frontCarryMoveMask.SetActive(false);

        if (leftCarryMask != null)
            leftCarryMask.SetActive(false);

        if (rightCarryMask != null)
            rightCarryMask.SetActive(false);

        if (backCarryMask != null)
            backCarryMask.SetActive(false);
    }
}