using UnityEngine;

public class LungsBubbleFloat : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float horizontalAmount = 8f;
    [SerializeField] private float verticalAmount = 10f;
    [SerializeField] private float floatSpeed = 1.5f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    private float randomOffsetX;
    private float randomOffsetY;

    private bool initialized;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            enabled = false;
            return;
        }

        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(0f, 100f);
    }

    private void Start()
    {
        /*
         * ВАЖНО:
         * Вземаме началната позиция в Start(),
         * а не в Awake().
         *
         * Awake се изпълнява още при Instantiate,
         * преди LungsBubbleSpawner да постави
         * балончето на random spawn позицията му.
         */

        startPosition = rectTransform.anchoredPosition;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        float x =
            Mathf.Sin(
                Time.time * floatSpeed +
                randomOffsetX
            ) * horizontalAmount;

        float y =
            Mathf.Sin(
                Time.time * floatSpeed * 0.8f +
                randomOffsetY
            ) * verticalAmount;

        rectTransform.anchoredPosition =
            startPosition +
            new Vector2(x, y);
    }
}