using UnityEngine;

public class FlyingItem : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 180f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        MoveLeft();
    }

    private void MoveLeft()
    {
        rectTransform.anchoredPosition +=
            Vector2.left * moveSpeed * Time.deltaTime;
    }
}