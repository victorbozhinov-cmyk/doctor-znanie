using System.Collections;
using UnityEngine;

public class QRCodeButtonController : MonoBehaviour
{
    [Header("QR Code")]
    [SerializeField] private GameObject qrCodeObject;

    [Header("Button")]
    [SerializeField] private GameObject qrButtonObject;

    [Header("Settings")]
    [SerializeField] private float showDuration = 20f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        // QR кодът започва скрит.
        if (qrCodeObject != null)
        {
            qrCodeObject.SetActive(false);
        }

        // Бутонът започва видим.
        if (qrButtonObject != null)
        {
            qrButtonObject.SetActive(true);
        }
    }

    public void ShowQRCode()
    {
        if (qrCodeObject == null)
        {
            Debug.LogWarning("QR Code Object не е свързан.");
            return;
        }

        // Показваме QR кода.
        qrCodeObject.SetActive(true);

        // Скриваме бутона.
        if (qrButtonObject != null)
        {
            qrButtonObject.SetActive(false);
        }

        // Спираме стар таймер, ако има такъв.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Стартираме 20-секундния таймер.
        hideCoroutine = StartCoroutine(
            HideQRCodeAfterDelay()
        );
    }

    private IEnumerator HideQRCodeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDuration);

        // Скриваме QR кода.
        qrCodeObject.SetActive(false);

        // Връщаме бутона.
        if (qrButtonObject != null)
        {
            qrButtonObject.SetActive(true);
        }

        hideCoroutine = null;
    }
}