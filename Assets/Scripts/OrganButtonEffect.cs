using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrganButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Visual Organ")]
    [SerializeField] private RectTransform visualTarget;

    [Header("Scale")]
    [SerializeField] private float hoverMultiplier = 1.1f;
    [SerializeField] private float pressedMultiplier = 0.95f;
    [SerializeField] private float scaleSpeed = 15f;

    [Header("White Glow")]
    [SerializeField] private Color glowColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float glowAlpha = 0.7f;

    [SerializeField]
    private float glowSize = 4f;

    [Header("Glow Animation")]
    [SerializeField]
    private float glowFadeSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerOver;

    private Outline glowOutline;

    private float currentGlowAlpha;
    private float targetGlowAlpha;


    private void Awake()
    {
        if (visualTarget == null)
        {
            visualTarget = transform as RectTransform;
        }

        if (visualTarget == null)
        {
            Debug.LogError(
                "OrganButtonEffect: Няма зададен Visual Target.",
                this
            );

            enabled = false;
            return;
        }

        originalScale = visualTarget.localScale;
        targetScale = originalScale;

        CreateGlow();
    }


    private void CreateGlow()
    {
        Graphic graphic =
            visualTarget.GetComponent<Graphic>();

        if (graphic == null)
        {
            Debug.LogWarning(
                "OrganButtonEffect: Visual Target няма Image/Graphic компонент.",
                visualTarget
            );

            return;
        }

        glowOutline =
            visualTarget.GetComponent<Outline>();

        if (glowOutline == null)
        {
            glowOutline =
                visualTarget.gameObject.AddComponent<Outline>();
        }

        glowOutline.effectDistance =
            new Vector2(glowSize, glowSize);

        glowOutline.useGraphicAlpha = true;

        currentGlowAlpha = 0f;
        targetGlowAlpha = 0f;

        ApplyGlowColor();
    }


    private void Update()
    {
        float deltaTime =
            Time.unscaledDeltaTime;

        UpdateScale(deltaTime);
        UpdateGlow(deltaTime);
    }


    private void UpdateScale(float deltaTime)
    {
        if (visualTarget == null)
        {
            return;
        }

        float t =
            1f - Mathf.Exp(
                -scaleSpeed * deltaTime
            );

        visualTarget.localScale =
            Vector3.Lerp(
                visualTarget.localScale,
                targetScale,
                t
            );
    }


    private void UpdateGlow(float deltaTime)
    {
        if (glowOutline == null)
        {
            return;
        }

        float t =
            1f - Mathf.Exp(
                -glowFadeSpeed * deltaTime
            );

        currentGlowAlpha =
            Mathf.Lerp(
                currentGlowAlpha,
                targetGlowAlpha,
                t
            );

        ApplyGlowColor();
    }


    private void ApplyGlowColor()
    {
        if (glowOutline == null)
        {
            return;
        }

        Color color = glowColor;
        color.a = currentGlowAlpha;

        glowOutline.effectColor = color;
    }


    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        isPointerOver = true;

        targetScale =
            originalScale *
            hoverMultiplier;

        targetGlowAlpha =
            glowAlpha;
    }


    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        isPointerOver = false;

        targetScale = originalScale;

        targetGlowAlpha = 0f;
    }


    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        targetScale =
            originalScale *
            pressedMultiplier;
    }


    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        targetScale =
            isPointerOver
                ? originalScale *
                  hoverMultiplier
                : originalScale;
    }


    private void OnDisable()
    {
        isPointerOver = false;

        targetScale = originalScale;

        currentGlowAlpha = 0f;
        targetGlowAlpha = 0f;

        if (visualTarget != null)
        {
            visualTarget.localScale =
                originalScale;
        }

        ApplyGlowColor();
    }
}