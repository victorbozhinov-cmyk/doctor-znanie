using UnityEngine;

public class LiverIdleFloat : MonoBehaviour
{
    [Header("Idle движение")]
    [SerializeField] private float moveDistance = 8f;
    [SerializeField] private float moveSpeed = 1.5f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float timer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime * moveSpeed;

        float offsetY = Mathf.Sin(timer) * moveDistance;

        rectTransform.anchoredPosition =
            startPosition + new Vector2(0f, offsetY);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
            rectTransform.anchoredPosition = startPosition;
    }
}