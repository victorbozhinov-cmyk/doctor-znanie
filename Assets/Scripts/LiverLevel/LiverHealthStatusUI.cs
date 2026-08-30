using UnityEngine;
using UnityEngine.UI;

public class LiverHealthStatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LiverHealthController liverHealth;
    [SerializeField] private Image statusImage;

    [Header("Status Sprites")]
    [SerializeField] private Sprite verySickSprite;
    [SerializeField] private Sprite sickSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite healthySprite;
    [SerializeField] private Sprite veryHealthySprite;

    private int lastStateIndex = -1;

    private void Awake()
    {
        if (statusImage == null)
            statusImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (liverHealth == null || statusImage == null)
            return;

        int currentState =
            liverHealth.CurrentVisualStateIndex;

        // Не сменяме Sprite всеки frame,
        // ако състоянието не се е променило.
        if (currentState == lastStateIndex)
            return;

        lastStateIndex = currentState;

        UpdateStatusImage(currentState);
    }

    private void UpdateStatusImage(int stateIndex)
    {
        switch (stateIndex)
        {
            case 0:
                statusImage.sprite = verySickSprite;
                break;

            case 1:
                statusImage.sprite = sickSprite;
                break;

            case 2:
                statusImage.sprite = neutralSprite;
                break;

            case 3:
                statusImage.sprite = healthySprite;
                break;

            case 4:
                statusImage.sprite = veryHealthySprite;
                break;
        }
    }
}