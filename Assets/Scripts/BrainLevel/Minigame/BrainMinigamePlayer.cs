using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BrainMinigamePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite idleFrontSprite;
    [SerializeField] private Sprite walkUpSprite;
    [SerializeField] private Sprite walkDownSprite;
    [SerializeField] private Sprite walkLeftSprite;
    [SerializeField] private Sprite walkRightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        ReadMovementInput();
        UpdateCharacterSprite();
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

        // LEFT
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        // RIGHT
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            moveInput.x += 1f;
        }

        // UP
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            moveInput.y += 1f;
        }

        // DOWN
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        // Prevent diagonal movement from being faster.
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

    private void UpdateCharacterSprite()
    {
        // Player is standing still.
        if (moveInput.sqrMagnitude < 0.01f)
        {
            spriteRenderer.sprite = idleFrontSprite;
            return;
        }

        // Horizontal movement.
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            if (moveInput.x > 0f)
            {
                spriteRenderer.sprite = walkRightSprite;
            }
            else
            {
                spriteRenderer.sprite = walkLeftSprite;
            }

            return;
        }

        // Vertical movement.
        if (moveInput.y > 0f)
        {
            spriteRenderer.sprite = walkUpSprite;
        }
        else
        {
            spriteRenderer.sprite = walkDownSprite;
        }
    }
}