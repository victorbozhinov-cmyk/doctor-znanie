using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float speed = 12f;

    [SerializeField] private RectTransform animatedPart;

    private Toggle toggle;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerInside;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        if (animatedPart == null)
        {
            Debug.LogError(
                $"Animated Part не е зададен на {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        originalScale = animatedPart.localScale;
        targetScale = originalScale;

        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void Update()
    {
        animatedPart.localScale = Vector3.Lerp(
            animatedPart.localScale,
            targetScale,
            1f - Mathf.Exp(-speed * Time.unscaledDeltaTime)
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toggle.isOn)
            return;

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
        if (toggle.isOn)
            return;

        targetScale = originalScale * pressedScale;

        UISoundManager.Instance?.PlayClick();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (toggle.isOn)
        {
            targetScale = originalScale;
            isPointerInside = false;
            return;
        }

        targetScale = isPointerInside
            ? originalScale * hoverScale
            : originalScale;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        targetScale = originalScale;
        isPointerInside = false;
    }

    private void OnDisable()
    {
        if (animatedPart == null)
            return;

        animatedPart.localScale = originalScale;
        targetScale = originalScale;
        isPointerInside = false;
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}