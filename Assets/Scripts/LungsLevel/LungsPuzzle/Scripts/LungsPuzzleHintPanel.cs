using UnityEngine;
using UnityEngine.UI;

public class LungsPuzzleHintPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image hintContentImage;

    [Header("Hint Sprites")]
    [SerializeField] private Sprite[] hintSprites;

    [Header("Settings")]
    [SerializeField] private bool showFirstHintOnStart = true;

    private int currentHintIndex = 0;

    private void Start()
    {
        if (showFirstHintOnStart)
        {
            ShowHint(currentHintIndex);
        }
        else if (hintContentImage != null)
        {
            hintContentImage.enabled = false;
        }
    }

    public void NextHint()
    {
        if (hintSprites == null || hintSprites.Length == 0)
        {
            Debug.LogWarning("Няма зададени hint sprites.");
            return;
        }

        currentHintIndex++;

        if (currentHintIndex >= hintSprites.Length)
        {
            currentHintIndex = 0;
        }

        ShowHint(currentHintIndex);
    }

    public void ShowHint(int index)
    {
        if (hintContentImage == null)
        {
            Debug.LogWarning("Hint Content Image не е зададен.");
            return;
        }

        if (hintSprites == null || hintSprites.Length == 0)
        {
            Debug.LogWarning("Няма зададени hint sprites.");
            return;
        }

        if (index < 0 || index >= hintSprites.Length)
        {
            Debug.LogWarning("Невалиден hint index: " + index);
            return;
        }

        currentHintIndex = index;
        hintContentImage.enabled = true;
        hintContentImage.sprite = hintSprites[currentHintIndex];
        
    }
}