using UnityEngine;

public class WorldButtonHoverEffect : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float speed = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerInside;
    private bool isPressed;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetScale,
                1f - Mathf.Exp(
                    -speed *
                    Time.unscaledDeltaTime
                )
            );
    }

    private void OnMouseEnter()
    {
        isPointerInside = true;

        if (isPressed)
            return;

        targetScale =
            originalScale *
            hoverScale;

        UISoundManager.Instance
            ?.PlayHover();
    }

    private void OnMouseExit()
    {
        isPointerInside = false;

        if (isPressed)
            return;

        targetScale =
            originalScale;
    }

    private void OnMouseDown()
    {
        isPressed = true;

        targetScale =
            originalScale *
            pressedScale;

        UISoundManager.Instance
            ?.PlayClick();
    }

    private void OnMouseUp()
    {
        isPressed = false;

        targetScale =
            isPointerInside
                ? originalScale * hoverScale
                : originalScale;
    }

    private void OnDisable()
    {
        transform.localScale =
            originalScale;

        targetScale =
            originalScale;

        isPointerInside = false;
        isPressed = false;
    }
}
