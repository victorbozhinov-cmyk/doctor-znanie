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

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateNewWaveLayout();
        }
        else
        {
            ResetMarker();
        }
    }

    private void Update()
    {
        if (!enableTestKey || Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            GenerateNewWaveLayout();
        }
    }

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

        Debug.Log(
            "Генерирани са нови позиции за перисталтичната вълна."
        );
    }

    public void ResetMarker()
    {
        if (draggableMarker == null ||
            startPoint == null)
        {
            return;
        }

        RectTransform markerParent =
            draggableMarker.parent as RectTransform;

        if (markerParent == null)
        {
            return;
        }

        // Вземаме реалната позиция на StartPoint в света.
        Vector3 startWorldPosition =
            startPoint.TransformPoint(Vector3.zero);

        // Преобразуваме я към координатите
        // на текущия parent на marker-а.
        Vector3 localPosition =
            markerParent.InverseTransformPoint(
                startWorldPosition
            );

        draggableMarker.anchoredPosition =
            new Vector2(
                localPosition.x,
                localPosition.y
            );
    }

    private void PlaceCheckpointInsideArea(
        RectTransform checkpoint,
        RectTransform spawnArea)
    {
        if (checkpoint == null ||
            spawnArea == null)
        {
            return;
        }

        Rect areaRect = spawnArea.rect;

        float checkpointHalfWidth =
            checkpoint.rect.width * 0.5f;

        float checkpointHalfHeight =
            checkpoint.rect.height * 0.5f;

        float paddingX =
            checkpointHalfWidth + edgePadding;

        float paddingY =
            checkpointHalfHeight + edgePadding;

        float minX =
            areaRect.xMin + paddingX;

        float maxX =
            areaRect.xMax - paddingX;

        float minY =
            areaRect.yMin + paddingY;

        float maxY =
            areaRect.yMax - paddingY;

        float randomX =
            minX <= maxX
                ? Random.Range(minX, maxX)
                : 0f;

        float randomY =
            minY <= maxY
                ? Random.Range(minY, maxY)
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
            gameplayArea.InverseTransformPoint(
                worldPoint
            );

        checkpoint.anchoredPosition =
            new Vector2(
                gameplayLocalPoint.x,
                gameplayLocalPoint.y
            );
    }
}