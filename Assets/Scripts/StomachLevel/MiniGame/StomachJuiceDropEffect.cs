using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StomachJuiceDropEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform effectArea;
    [SerializeField] private GameObject juiceDropPrefab;

    [Header("Drop Amount")]
    [SerializeField] private int minDropCount = 8;
    [SerializeField] private int maxDropCount = 12;

    [Header("Spawn Area")]
    [Range(0f, 1f)]
    [SerializeField] private float minSpawnX = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float maxSpawnX = 0.85f;

    [Range(0f, 1f)]
    [SerializeField] private float minSpawnY = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float maxSpawnY = 0.85f;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 0.02f;
    [SerializeField] private float maxSpawnDelay = 0.08f;

    [Header("Drop Movement")]
    [SerializeField] private float minFallDistance = 140f;
    [SerializeField] private float maxFallDistance = 260f;

    [SerializeField] private float minHorizontalDrift = -35f;
    [SerializeField] private float maxHorizontalDrift = 35f;

    [SerializeField] private float minFallDuration = 0.55f;
    [SerializeField] private float maxFallDuration = 0.95f;

    [Header("Random Size")]
    [SerializeField] private float minScale = 0.65f;
    [SerializeField] private float maxScale = 1.15f;

    [Header("Fade")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeStartPoint = 0.55f;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = false;

    private Coroutine effectCoroutine;

    private int activeDropCount;

    // =========================================================
    // PUBLIC INFO
    // =========================================================

    public bool IsPlaying
    {
        get;
        private set;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (effectArea == null)
        {
            effectArea =
                transform as RectTransform;
        }

        if (juiceDropPrefab != null)
        {
            juiceDropPrefab.SetActive(false);
        }
    }

    private void Update()
    {
        if (!enableTestKey ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            PlayJuiceDropEffect();
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void PlayJuiceDropEffect()
    {
        if (effectArea == null ||
            juiceDropPrefab == null)
        {
            return;
        }

        // Не позволяваме два ефекта
        // да вървят едновременно.
        if (IsPlaying)
        {
            return;
        }

        effectCoroutine =
            StartCoroutine(
                PlayEffectRoutine()
            );
    }

    // =========================================================
    // FULL EFFECT
    // =========================================================

    private IEnumerator PlayEffectRoutine()
    {
        IsPlaying = true;
        activeDropCount = 0;

        int amount =
            Random.Range(
                minDropCount,
                maxDropCount + 1
            );

        // ---------------------------------------------
        // SPAWN
        // ---------------------------------------------

        for (int i = 0; i < amount; i++)
        {
            CreateDrop();

            float delay =
                Random.Range(
                    minSpawnDelay,
                    maxSpawnDelay
                );

            yield return new WaitForSeconds(
                delay
            );
        }

        // ---------------------------------------------
        // WAIT FOR EVERY DROP
        // ---------------------------------------------

        while (activeDropCount > 0)
        {
            yield return null;
        }

        IsPlaying = false;
        effectCoroutine = null;
    }

    // =========================================================
    // CREATE DROP
    // =========================================================

    private void CreateDrop()
    {
        GameObject newDrop =
            Instantiate(
                juiceDropPrefab,
                effectArea
            );

        newDrop.SetActive(true);

        RectTransform dropRect =
            newDrop.transform as RectTransform;

        if (dropRect == null)
        {
            Destroy(newDrop);
            return;
        }

        CanvasGroup canvasGroup =
            newDrop.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                newDrop.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // ---------------------------------------------
        // RANDOM POSITION
        // ---------------------------------------------

        Rect areaRect =
            effectArea.rect;

        float normalizedX =
            Random.Range(
                minSpawnX,
                maxSpawnX
            );

        float normalizedY =
            Random.Range(
                minSpawnY,
                maxSpawnY
            );

        float x =
            Mathf.Lerp(
                areaRect.xMin,
                areaRect.xMax,
                normalizedX
            );

        float y =
            Mathf.Lerp(
                areaRect.yMin,
                areaRect.yMax,
                normalizedY
            );

        dropRect.anchoredPosition =
            new Vector2(x, y);

        // ---------------------------------------------
        // RANDOM SCALE
        // ---------------------------------------------

        float randomScale =
            Random.Range(
                minScale,
                maxScale
            );

        dropRect.localScale =
            Vector3.one *
            randomScale;

        // ---------------------------------------------
        // RANDOM ROTATION
        // ---------------------------------------------

        dropRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                Random.Range(
                    -12f,
                    12f
                )
            );

        activeDropCount++;

        StartCoroutine(
            AnimateDrop(
                newDrop,
                dropRect,
                canvasGroup
            )
        );
    }

    // =========================================================
    // DROP ANIMATION
    // =========================================================

    private IEnumerator AnimateDrop(
        GameObject dropObject,
        RectTransform dropRect,
        CanvasGroup canvasGroup)
    {
        Vector2 startPosition =
            dropRect.anchoredPosition;

        float fallDistance =
            Random.Range(
                minFallDistance,
                maxFallDistance
            );

        float horizontalDrift =
            Random.Range(
                minHorizontalDrift,
                maxHorizontalDrift
            );

        Vector2 endPosition =
            startPosition +
            new Vector2(
                horizontalDrift,
                -fallDistance
            );

        float duration =
            Random.Range(
                minFallDuration,
                maxFallDuration
            );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            // Леко ускоряване надолу.
            float moveT =
                t * t;

            dropRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    moveT
                );

            // Fade във втората част.
            if (t >= fadeStartPoint)
            {
                float fadeT =
                    Mathf.InverseLerp(
                        fadeStartPoint,
                        1f,
                        t
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        fadeT
                    );
            }

            yield return null;
        }

        activeDropCount--;

        Destroy(dropObject);
    }
}