using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class BrainTutorialSkipButton : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField]
    private BrainMinigameManager minigameManager;

    [SerializeField]
    private Camera targetCamera;

    // =========================================================
    // HOVER EFFECT
    // =========================================================

    [Header("Hover Effect")]
    [SerializeField]
    [Range(1f, 1.3f)]
    private float hoverScale = 1.08f;

    [SerializeField]
    [Min(0f)]
    private float hoverAnimationSpeed = 10f;

    // =========================================================
    // INTERNAL
    // =========================================================

    private SpriteRenderer spriteRenderer;

    private Vector3 originalScale;

    private bool isHovered = false;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        originalScale =
            transform.localScale;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetCamera == null ||
            Mouse.current == null ||
            spriteRenderer == null)
        {
            return;
        }

        // =====================================================
        // MOUSE POSITION
        // =====================================================

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        float distance =
            Vector3.Dot(
                transform.position -
                targetCamera.transform.position,
                targetCamera.transform.forward
            );

        if (distance < 0f)
        {
            return;
        }

        Vector3 worldPosition =
            targetCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    distance
                )
            );

        // =====================================================
        // CHECK HOVER
        // =====================================================

        Bounds bounds =
            spriteRenderer.bounds;

        isHovered =
            spriteRenderer.enabled &&
            worldPosition.x >= bounds.min.x &&
            worldPosition.x <= bounds.max.x &&
            worldPosition.y >= bounds.min.y &&
            worldPosition.y <= bounds.max.y;

        // =====================================================
        // HOVER SCALE ANIMATION
        // =====================================================

        Vector3 targetScale =
            isHovered
                ? originalScale * hoverScale
                : originalScale;

        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetScale,
                hoverAnimationSpeed *
                Time.unscaledDeltaTime
            );

        // =====================================================
        // BUTTON CLICK
        // =====================================================

        if (!isHovered)
        {
            return;
        }

        if (minigameManager == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // Skip tutorial
        minigameManager.SkipTutorial();
    }

    // =========================================================
    // RESET
    // =========================================================

    private void OnDisable()
    {
        transform.localScale =
            originalScale;

        isHovered = false;
    }
}
