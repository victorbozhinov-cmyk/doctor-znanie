using UnityEngine;
using UnityEngine.UI;

public enum LungGasType
{
    O2,
    CO2
}

[RequireComponent(typeof(Button))]
public class LungsGasBubble : MonoBehaviour
{
    [Header("Gas Type")]
    [SerializeField] private LungGasType gasType;

    private Button button;
    private bool wasClicked;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnBubbleClicked);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnBubbleClicked);
        }
    }

    private void OnBubbleClicked()
    {
        if (wasClicked)
        {
            return;
        }

        if (LungsMinigameManager.Instance == null)
        {
            Debug.LogWarning(
                "LungsMinigameManager липсва в сцената."
            );

            return;
        }

        // Ако играта е приключила,
        // има transition,
        // или сме в lock след грешен клик,
        // балончето НЕ се пука.
        if (LungsMinigameManager.Instance.IsClickLocked)
        {
            return;
        }

        wasClicked = true;

        LungsMinigameManager.Instance.HandleBubbleClicked(
            gasType
        );

        Destroy(gameObject);
    }
}