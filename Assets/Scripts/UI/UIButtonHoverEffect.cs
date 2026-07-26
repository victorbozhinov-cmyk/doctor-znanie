using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float speed = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isPointerInside;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            1f - Mathf.Exp(-speed * Time.unscaledDeltaTime)
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        targetScale = originalScale * hoverScale;

        UISoundManager.Instance?.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressedScale;

        UISoundManager.Instance?.PlayClick();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isPointerInside
            ? originalScale * hoverScale
            : originalScale;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        isPointerInside = false;
    }
}