using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float speed = 12f;

    [Header("Interactable Check")]
    [SerializeField] private bool disableWhenNotInteractable = false;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerInside;

    private Button button;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        originalScale =
            transform.localScale;

        targetScale =
            originalScale;

        button =
            GetComponent<Button>();
    }

    private void Update()
    {
        // Само ако тази настройка е включена
        // за конкретния бутон.
        if (ShouldBlockEffect())
        {
            targetScale =
                originalScale;
        }
        else
        {
            // Ако бутонът отново стане активен,
            // докато курсорът вече е върху него,
            // hover ефектът отново се позволява.
            if (isPointerInside &&
                targetScale == originalScale)
            {
                targetScale =
                    originalScale *
                    hoverScale;
            }
        }

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

    // =========================================================
    // POINTER ENTER
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        isPointerInside = true;

        if (ShouldBlockEffect())
        {
            targetScale =
                originalScale;

            return;
        }

        targetScale =
            originalScale *
            hoverScale;

        UISoundManager.Instance
            ?.PlayHover();
    }

    // =========================================================
    // POINTER EXIT
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData)
    {
        isPointerInside = false;

        targetScale =
            originalScale;
    }

    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (ShouldBlockEffect())
        {
            targetScale =
                originalScale;

            return;
        }

        targetScale =
            originalScale *
            pressedScale;

        UISoundManager.Instance
            ?.PlayClick();
    }

    // =========================================================
    // POINTER UP
    // =========================================================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if (ShouldBlockEffect())
        {
            targetScale =
                originalScale;

            return;
        }

        targetScale =
            isPointerInside
                ? originalScale * hoverScale
                : originalScale;
    }

    // =========================================================
    // CHECK
    // =========================================================

    private bool ShouldBlockEffect()
    {
        // Ако настройката не е включена,
        // работим точно както старият script.
        if (!disableWhenNotInteractable)
        {
            return false;
        }

        // Ако по някаква причина няма Button,
        // също не блокираме ефекта.
        if (button == null)
        {
            return false;
        }

        return !button.interactable;
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        transform.localScale =
            originalScale;

        targetScale =
            originalScale;

        isPointerInside =
            false;
    }
}