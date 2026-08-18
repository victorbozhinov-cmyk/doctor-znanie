using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LiverHelperAttention : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1f;

    private CanvasGroup canvasGroup;
    private float timer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime * blinkSpeed;

        float wave = (Mathf.Sin(timer) + 1f) / 2f;

        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
    }

    public void ShowAttention()
    {
        gameObject.SetActive(true);
        timer = 0f;
    }

    public void HideAttention()
    {
        gameObject.SetActive(false);
    }
}