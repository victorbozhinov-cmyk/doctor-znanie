using UnityEngine;
using UnityEngine.InputSystem;

public class PeristalsisCheckpointLayout : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private RectTransform gameplayArea;

    [Header("Start")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform draggableMarker;

    [Header("Spawn Areas")]
    [SerializeField] private RectTransform topSpawnArea;
    [SerializeField] private RectTransform middleSpawnArea;
    [SerializeField] private RectTransform bottomSpawnArea;

    [Header("Checkpoints")]
    [SerializeField] private RectTransform topCheckpoint;
    [SerializeField] private RectTransform middleCheckpoint;
    [SerializeField] private RectTransform bottomCheckpoint;

    [Header("Random Position Settings")]
    [SerializeField] private float edgePadding = 15f;

    [Header("Testing")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool enableTestKey = true;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // ВАЖНО:
        // Този script е върху gameplay обект,
        // който се инициализира още при отварянето
        // на MiniGamePanel.
        //
        // Така checkpoint-ите се скриват независимо
        // дали PeristalsisDragController вече е
        // стигнал до Awake/OnEnable.
        HideAllCheckpoints();
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateNewWaveLayout();
        }
        else
        {
            ResetMarker();
            HideAllCheckpoints();
        }
    }

    private void Update()
    {
        if (!enableTestKey ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current
            .rKey
            .wasPressedThisFrame)
        {
            GenerateNewWaveLayout();
        }
    }

    // =========================================================
    // GENERATE WAVE
    // =========================================================

    public void GenerateNewWaveLayout()
    {
        if (gameplayArea == null)
        {
            Debug.LogWarning(
                "PeristalsisCheckpointLayout: Gameplay Area липсва."
            );

            return;
        }

        PlaceCheckpointInsideArea(
            topCheckpoint,
            topSpawnArea
        );

        PlaceCheckpointInsideArea(
            middleCheckpoint,
            middleSpawnArea
        );

        PlaceCheckpointInsideArea(
            bottomCheckpoint,
            bottomSpawnArea
        );

        ResetMarker();

        // ВАЖНО:
        // Генерирането на позиции НЕ означава,
        // че checkpoint-ите трябва да се виждат.
        //
        // Те ще бъдат показани един по един
        // от PeristalsisDragController чак
        // когато започне самото влачене.
        HideAllCheckpoints();

        Debug.Log(
            "Генерирани са нови позиции за перисталтичната вълна."
        );
    }

    // =========================================================
    // RESET MARKER
    // =========================================================

    public void ResetMarker()
    {
        if (draggableMarker == null ||
            startPoint == null)
        {
            return;
        }

        RectTransform markerParent =
            draggableMarker.parent
                as RectTransform;

        if (markerParent == null)
        {
            return;
        }

        Vector3 startWorldPosition =
            startPoint.TransformPoint(
                Vector3.zero
            );

        Vector3 localPosition =
            markerParent
                .InverseTransformPoint(
                    startWorldPosition
                );

        draggableMarker.anchoredPosition =
            new Vector2(
                localPosition.x,
                localPosition.y
            );
    }

    // =========================================================
    // CHECKPOINT POSITION
    // =========================================================

    private void PlaceCheckpointInsideArea(
        RectTransform checkpoint,
        RectTransform spawnArea)
    {
        if (checkpoint == null ||
            spawnArea == null)
        {
            return;
        }

        Rect areaRect =
            spawnArea.rect;

        float checkpointHalfWidth =
            checkpoint.rect.width *
            0.5f;

        float checkpointHalfHeight =
            checkpoint.rect.height *
            0.5f;

        float paddingX =
            checkpointHalfWidth +
            edgePadding;

        float paddingY =
            checkpointHalfHeight +
            edgePadding;

        float minX =
            areaRect.xMin +
            paddingX;

        float maxX =
            areaRect.xMax -
            paddingX;

        float minY =
            areaRect.yMin +
            paddingY;

        float maxY =
            areaRect.yMax -
            paddingY;

        float randomX =
            minX <= maxX
                ? Random.Range(
                    minX,
                    maxX
                )
                : 0f;

        float randomY =
            minY <= maxY
                ? Random.Range(
                    minY,
                    maxY
                )
                : 0f;

        Vector3 localPoint =
            new Vector3(
                randomX,
                randomY,
                0f
            );

        Vector3 worldPoint =
            spawnArea.TransformPoint(
                localPoint
            );

        Vector3 gameplayLocalPoint =
            gameplayArea
                .InverseTransformPoint(
                    worldPoint
                );

        checkpoint.anchoredPosition =
            new Vector2(
                gameplayLocalPoint.x,
                gameplayLocalPoint.y
            );
    }

    // =========================================================
    // CHECKPOINT VISIBILITY
    // =========================================================

    private void HideAllCheckpoints()
    {
        SetCheckpointActive(
            topCheckpoint,
            false
        );

        SetCheckpointActive(
            middleCheckpoint,
            false
        );

        SetCheckpointActive(
            bottomCheckpoint,
            false
        );
    }

    private void SetCheckpointActive(
        RectTransform checkpoint,
        bool active)
    {
        if (checkpoint != null)
        {
            checkpoint.gameObject
                .SetActive(active);
        }
    }
}