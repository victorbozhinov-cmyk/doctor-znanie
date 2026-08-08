using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeartMinigameManager : MonoBehaviour
{
    public enum PulseState
    {
        Normal,
        DangerLow,
        DangerHigh,
        CriticalLow,
        CriticalHigh
    }

    [Header("Pulse")]
    [SerializeField] private int currentBPM = 80;

    [Header("UI")]
    [SerializeField] private TMP_Text pulseValueText;

    [Header("ECG")]
    [SerializeField] private ECGLineGraphic ecgLine;

    [Header("Heart States")]
    [SerializeField] private GameObject heartNormal;
    [SerializeField] private GameObject heartLow;
    [SerializeField] private GameObject heartHigh;
    [SerializeField] private GameObject heartDead;

    [Header("Keyboard Button Animation")]
    [SerializeField] private RectTransform decreaseButtonHitbox;
    [SerializeField] private RectTransform increaseButtonHitbox;

    [SerializeField] private float keyboardPressedScale = 0.96f;
    [SerializeField] private float keyboardPressDuration = 0.08f;

    private Coroutine decreaseAnimation;
    private Coroutine increaseAnimation;

    private PulseState currentPulseState;

    private void Start()
    {
        UpdatePulseUI();
        UpdatePulseState();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Намаляване
        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            DecreasePulse();

            if (decreaseButtonHitbox != null)
            {
                if (decreaseAnimation != null)
                    StopCoroutine(decreaseAnimation);

                decreaseAnimation = StartCoroutine(
                    PlayKeyboardPressAnimation(decreaseButtonHitbox)
                );
            }
        }

        // Увеличаване
        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            IncreasePulse();

            if (increaseButtonHitbox != null)
            {
                if (increaseAnimation != null)
                    StopCoroutine(increaseAnimation);

                increaseAnimation = StartCoroutine(
                    PlayKeyboardPressAnimation(increaseButtonHitbox)
                );
            }
        }
    }

    private IEnumerator PlayKeyboardPressAnimation(RectTransform button)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 pressedScale = originalScale * keyboardPressedScale;

        float timer = 0f;

        // Свиване
        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale = Vector3.Lerp(
                originalScale,
                pressedScale,
                timer / keyboardPressDuration
            );

            yield return null;
        }

        timer = 0f;

        // Връщане
        while (timer < keyboardPressDuration)
        {
            timer += Time.unscaledDeltaTime;

            button.localScale = Vector3.Lerp(
                pressedScale,
                originalScale,
                timer / keyboardPressDuration
            );

            yield return null;
        }

        button.localScale = originalScale;
    }

    private void UpdatePulseUI()
    {
        if (pulseValueText != null)
        {
            pulseValueText.text = currentBPM.ToString();
        }

        if (ecgLine != null)
        {
            ecgLine.SetBPM(currentBPM);
        }
    }

    private void UpdatePulseState()
    {
        // NORMAL: 70 - 100
        if (currentBPM >= 70 && currentBPM <= 100)
        {
            currentPulseState = PulseState.Normal;
        }

        // DANGER LOW: 50 - 69
        else if (currentBPM >= 50 && currentBPM < 70)
        {
            currentPulseState = PulseState.DangerLow;
        }

        // DANGER HIGH: 101 - 120
        else if (currentBPM > 100 && currentBPM <= 120)
        {
            currentPulseState = PulseState.DangerHigh;
        }

        // CRITICAL LOW: под 50
        else if (currentBPM < 50)
        {
            currentPulseState = PulseState.CriticalLow;
        }

        // CRITICAL HIGH: над 120
        else
        {
            currentPulseState = PulseState.CriticalHigh;
        }

        UpdateHeartVisual();
    }

    private void UpdateHeartVisual()
    {
        if (heartNormal != null)
            heartNormal.SetActive(false);

        if (heartLow != null)
            heartLow.SetActive(false);

        if (heartHigh != null)
            heartHigh.SetActive(false);

        if (heartDead != null)
            heartDead.SetActive(false);

        switch (currentPulseState)
        {
            case PulseState.Normal:

                if (heartNormal != null)
                    heartNormal.SetActive(true);

                break;

            case PulseState.DangerLow:
            case PulseState.CriticalLow:

                if (heartLow != null)
                    heartLow.SetActive(true);

                break;

            case PulseState.DangerHigh:
            case PulseState.CriticalHigh:

                if (heartHigh != null)
                    heartHigh.SetActive(true);

                break;
        }
    }

    public void DecreasePulse()
    {
        currentBPM -= 5;

        UpdatePulseUI();
        UpdatePulseState();
    }

    public void IncreasePulse()
    {
        currentBPM += 5;

        UpdatePulseUI();
        UpdatePulseState();
    }
}