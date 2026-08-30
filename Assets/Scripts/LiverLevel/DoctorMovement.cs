using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DoctorMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 350f;

    [Header("Movement Limits")]
    [SerializeField] private float leftPadding = 10f;
    [SerializeField] private float rightPadding = 10f;
    [SerializeField] private float bottomPadding = 10f;
    [SerializeField] private float topPadding = 230f;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite runLeftSprite;
    [SerializeField] private Sprite runRightSprite;

    [Header("Visual")]
    [SerializeField] private float runningRotation = 5f;

    [Header("Running Animation")]
    [SerializeField] private float runAnimationSpeed = 12f;
    [SerializeField] private float runSquashAmount = 0.04f;

    [Header("Liver Boundary")]
    [SerializeField] private RectTransform liverCharacter;
    [SerializeField] private float liverGap = 20f;

    

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Image doctorImage;

    private int lastHorizontalDirection = 1;

    private Vector3 normalScale;
    private float runAnimationTimer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        doctorImage = GetComponent<Image>();
        parentRect = rectTransform.parent as RectTransform;

        normalScale = rectTransform.localScale;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void AnimateRunning()
    {
        runAnimationTimer += Time.deltaTime * runAnimationSpeed;

        float bounce = Mathf.Sin(runAnimationTimer);

        float scaleX = 1f + bounce * runSquashAmount;
        float scaleY = 1f - bounce * runSquashAmount;

        rectTransform.localScale = new Vector3(
            normalScale.x * scaleX,
            normalScale.y * scaleY,
            normalScale.z
        );
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        // НАЛЯВО
        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        // НАДЯСНО
        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        // НАГОРЕ
        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        // НАДОЛУ
        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        Vector2 direction = new Vector2(horizontal, vertical);

        // За да не се движи по-бързо по диагонал
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        if (direction != Vector2.zero)
        {
            Move(direction);
            UpdateRunningVisual(horizontal);
            AnimateRunning();
        }
        else
        {
            ShowIdle();
        }
    }

    private void Move(Vector2 direction)
    {
        Vector2 newPosition =
            rectTransform.anchoredPosition +
            direction * moveSpeed * Time.deltaTime;

        ClampToGameArea(ref newPosition);

        rectTransform.anchoredPosition = newPosition;
    }

    private void ClampToGameArea(ref Vector2 position)
    {
        if (parentRect == null)
            return;

        Rect area = parentRect.rect;

        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        // Нормалните граници на игровото поле
        float minX = area.xMin + halfWidth + leftPadding;
        float maxX = area.xMax - halfWidth - rightPadding;

        float minY = area.yMin + halfHeight + bottomPadding;
        float maxY = area.yMax - halfHeight - topPadding;

        // Допълнителна граница вдясно от черния дроб
        if (liverCharacter != null)
        {
            float liverRightEdge =
                liverCharacter.anchoredPosition.x +
                liverCharacter.rect.width * 0.5f;

            float liverBoundary =
                liverRightEdge +
                halfWidth +
                liverGap;

            minX = Mathf.Max(minX, liverBoundary);
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
    }

    private void UpdateRunningVisual(float horizontal)
    {
        if (horizontal < 0f)
        {
            lastHorizontalDirection = -1;

            if (runLeftSprite != null)
                doctorImage.sprite = runLeftSprite;

            rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, runningRotation);
        }
        else if (horizontal > 0f)
        {
            lastHorizontalDirection = 1;

            if (runRightSprite != null)
                doctorImage.sprite = runRightSprite;

            rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, -runningRotation);
        }
        else
        {
            // Ако върви само нагоре или надолу,
            // запазва последната посока.
            if (lastHorizontalDirection < 0)
            {
                if (runLeftSprite != null)
                    doctorImage.sprite = runLeftSprite;

                rectTransform.localRotation =
                    Quaternion.Euler(0f, 0f, runningRotation);
            }
            else
            {
                if (runRightSprite != null)
                    doctorImage.sprite = runRightSprite;

                rectTransform.localRotation =
                    Quaternion.Euler(0f, 0f, -runningRotation);
            }
        }
    }

    private void ShowIdle()
    {
        if (idleSprite != null)
            doctorImage.sprite = idleSprite;

        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = normalScale;

        runAnimationTimer = 0f;
    }
}