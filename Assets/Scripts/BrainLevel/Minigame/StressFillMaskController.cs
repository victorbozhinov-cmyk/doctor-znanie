using UnityEngine;

public class StressFillMaskController : MonoBehaviour
{
    private Vector3 fullScale;
    private Vector3 fullPosition;

    private void Awake()
    {
        fullScale = transform.localScale;
        fullPosition = transform.localPosition;
    }

    public void SetFill(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);

        // Намаляваме ширината на маската
        Vector3 newScale = fullScale;
        newScale.x = fullScale.x * normalizedValue;
        transform.localScale = newScale;

        // Компенсираме позицията така, че левият край
        // винаги да остава на едно и също място.
        float fullHalfWidth = fullScale.x * 0.5f;
        float currentHalfWidth = newScale.x * 0.5f;

        Vector3 newPosition = fullPosition;
        newPosition.x =
            fullPosition.x -
            fullHalfWidth +
            currentHalfWidth;

        transform.localPosition = newPosition;
    }
}
