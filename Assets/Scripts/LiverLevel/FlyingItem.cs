using UnityEngine;
using UnityEngine.UI;

public class FlyingItem : MonoBehaviour
{
    public enum ItemType
    {
        Helpful,
        Harmful
    }

    [Header("Item")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private Image itemIcon;

    [Header("Speed by Difficulty")]
    [SerializeField] private float easySpeed = 140f;
    [SerializeField] private float mediumSpeed = 180f;
    [SerializeField] private float hardSpeed = 230f;

    private RectTransform rectTransform;
    private float moveSpeed;

    public ItemType Type => itemType;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        SetSpeedFromDifficulty();
    }

    private void Update()
    {
        MoveLeft();
    }

    private void SetSpeedFromDifficulty()
    {
        string difficultyString =
            PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            moveSpeed = easySpeed;
            return;
        }

        if (difficultyString == "medium")
        {
            moveSpeed = mediumSpeed;
            return;
        }

        if (difficultyString == "hard")
        {
            moveSpeed = hardSpeed;
            return;
        }

        // Ако Difficulty се пази като int:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard
        int difficultyInt =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficultyInt)
        {
            case 0:
                moveSpeed = easySpeed;
                break;

            case 2:
                moveSpeed = hardSpeed;
                break;

            default:
                moveSpeed = mediumSpeed;
                break;
        }
    }

    private void MoveLeft()
    {
        rectTransform.anchoredPosition +=
            Vector2.left * moveSpeed * Time.deltaTime;
    }

    public void Setup(Sprite iconSprite, ItemType newType)
    {
        itemType = newType;

        if (itemIcon != null)
        {
            itemIcon.sprite = iconSprite;
            itemIcon.enabled = iconSprite != null;
        }
    }

    public bool IsHelpful()
    {
        return itemType == ItemType.Helpful;
    }

    public bool IsHarmful()
    {
        return itemType == ItemType.Harmful;
    }
}