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

    [Header("Pop Animation")]
    [SerializeField]
    private LungsBubblePopAnimation popAnimation;

    private Button button;
    private bool wasClicked;

    private void Awake()
    {
        button =
            GetComponent<Button>();

        if (popAnimation == null)
        {
            popAnimation =
                GetComponent<LungsBubblePopAnimation>();
        }

        if (button != null)
        {
            button.onClick.AddListener(
                OnBubbleClicked
            );
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                OnBubbleClicked
            );
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

        if (
            LungsMinigameManager.Instance
                .IsClickLocked
        )
        {
            return;
        }

        wasClicked = true;

        if (button != null)
        {
            button.interactable = false;
        }

        bool isCorrect =
            IsCorrectForCurrentPhase();

        LungsMinigameManager.Instance
            .HandleBubbleClicked(
                gasType
            );

        if (popAnimation != null)
        {
            popAnimation.PlayPop(
                isCorrect
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // CORRECT CHECK
    // =========================================================

    private bool IsCorrectForCurrentPhase()
    {
        if (
            LungsMinigameManager.Instance.CurrentPhase ==
            LungsBreathingPhase.Inhale
        )
        {
            return gasType ==
                LungGasType.O2;
        }

        return gasType ==
            LungGasType.CO2;
    }
}