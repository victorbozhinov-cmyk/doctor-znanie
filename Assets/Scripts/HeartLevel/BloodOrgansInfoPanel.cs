using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BloodVesselHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Visual Target")]
    [SerializeField] private RectTransform visualTarget;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;

    [Header("Scale")]
    [SerializeField] private float hoverMultiplier = 1.08f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("Glow")]
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float glowMaxAlpha = 0.55f;
    [SerializeField] private float glowSize = 4f;
    [SerializeField] private float glowSpeed = 8f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.1f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isPointerOver;
    private float currentGlowAlpha;

    private readonly List<Shadow> glowShadows = new List<Shadow>();

    private void Awake()
    {
        if (visualTarget == null)
        {
            visualTarget = transform as RectTransform;
        }

        originalScale = visualTarget.localScale;
        targetScale = originalScale;

        CreateGlow();

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void CreateGlow()
    {
        Graphic graphic = visualTarget.GetComponent<Graphic>();

        if (graphic == null)
        {
            Debug.LogWarning(
                "BloodVesselHoverEffect: Visual Target няма Image/Graphic компонент.",
                visualTarget
            );

            return;
        }

        Vector2[] directions =
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f),
            new Vector2(-0.7f, 0.7f),
            new Vector2(0.7f, 0.7f),
            new Vector2(-0.7f, -0.7f),
            new Vector2(0.7f, -0.7f)
        };

        foreach (Vector2 direction in directions)
        {
            Shadow shadow = visualTarget.gameObject.AddComponent<Shadow>();

            shadow.effectDistance = direction * glowSize;
            shadow.useGraphicAlpha = true;

            Color hiddenGlow = glowColor;
            hiddenGlow.a = 0f;
            shadow.effectColor = hiddenGlow;

            glowShadows.Add(shadow);
        }
    }

    private void Update()
    {
        visualTarget.localScale = Vector3.Lerp(
            visualTarget.localScale,
            targetScale,
            scaleSpeed * Time.deltaTime
        );

        float targetAlpha = 0f;

        if (isPointerOver)
        {
            float pulse =
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

            targetAlpha = Mathf.Lerp(
                glowMaxAlpha - pulseAmount,
                glowMaxAlpha,
                pulse
            );
        }

        currentGlowAlpha = Mathf.Lerp(
            currentGlowAlpha,
            targetAlpha,
            glowSpeed * Time.deltaTime
        );

        UpdateGlowColor();
    }

    private void UpdateGlowColor()
    {
        foreach (Shadow shadow in glowShadows)
        {
            if (shadow == null)
            {
                continue;
            }

            Color color = glowColor;
            color.a = currentGlowAlpha;
            shadow.effectColor = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        targetScale = originalScale * hoverMultiplier;

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        targetScale = originalScale;

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (visualTarget != null)
        {
            visualTarget.localScale = originalScale;
        }

        currentGlowAlpha = 0f;
        UpdateGlowColor();

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
}