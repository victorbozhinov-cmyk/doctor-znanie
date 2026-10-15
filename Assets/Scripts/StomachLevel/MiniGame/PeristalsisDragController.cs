using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PeristalsisDragController :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private enum WaveStep
    {
        NeedTop,
        NeedMiddle,
        NeedBottom
    }

    // =========================================================
    // EVENTS
    // =========================================================

    public event Action WaveFailedEarlyRelease;
    public event Action MarkerGrabbed;

    // =========================================================
    // MARKER
    // =========================================================

    [Header("Marker")]
    [SerializeField]
    private RectTransform draggableMarker;

    [SerializeField]
    private MarkerAttentionAnimation markerAttentionAnimation;

    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement Area")]
    [SerializeField]
    private RectTransform gameplayArea;

    [SerializeField]
    private Canvas canvas;

    // =========================================================
    // CHECKPOINTS
    // =========================================================

    [Header("Checkpoints")]
    [SerializeField]
    private RectTransform topCheckpoint;

    [SerializeField]
    private RectTransform middleCheckpoint;

    [SerializeField]
    private RectTransform bottomCheckpoint;

    // =========================================================
    // SYSTEMS
    // =========================================================

    [Header("Systems")]
    [SerializeField]
    private StomachPeristalsisEffect peristalsisEffect;

    [SerializeField]
    private StomachFoodStageController foodStageController;

    [SerializeField]
    private PeristalsisCheckpointLayout checkpointLayout;

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Audio")]
    [SerializeField]
    private StomachMinigameAudio minigameAudio;

    // =========================================================
    // INTERNAL
    // =========================================================

    private WaveStep currentStep =
        WaveStep.NeedTop;

    private bool isDragging;
    private bool waveCompleted;

    private Camera uiCamera;
    private Vector3 dragOffset;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (draggableMarker == null)
        {
            draggableMarker =
                transform as RectTransform;
        }

        if (markerAttentionAnimation == null &&
            draggableMarker != null)
        {
            markerAttentionAnimation =
                draggableMarker.GetComponent<
                    MarkerAttentionAnimation
                >();
        }

        if (canvas != null)
        {
            uiCamera =
                canvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera;
        }

        HideAllCheckpoints();
    }

    private void OnEnable()
    {
        isDragging = false;
        waveCompleted = false;

        currentStep =
            WaveStep.NeedTop;

        HideAllCheckpoints();
    }

    // =========================================================
    // DRAG START
    // =========================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (waveCompleted ||
            draggableMarker == null ||
            gameplayArea == null)
        {
            return;
        }

        isDragging = true;

        // Играчът е открил маркера.
        if (markerAttentionAnimation != null)
        {
            markerAttentionAnimation
                .StopAttention();
        }

        // Current Task Timer започва
        // при първото хващане.
        MarkerGrabbed?.Invoke();

        if (
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    gameplayArea,
                    eventData.position,
                    uiCamera,
                    out Vector3 pointerWorldPosition
                )
        )
        {
            dragOffset =
                draggableMarker.position -
                pointerWorldPosition;
        }
        else
        {
            dragOffset =
                Vector3.zero;
        }

        if (currentStep ==
            WaveStep.NeedTop)
        {
            HideAllCheckpoints();

            ShowCheckpoint(
                topCheckpoint
            );
        }
    }

    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!isDragging ||
            waveCompleted ||
            draggableMarker == null ||
            gameplayArea == null)
        {
            return;
        }

        if (
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    gameplayArea,
                    eventData.position,
                    uiCamera,
                    out Vector3 pointerWorldPosition
                )
        )
        {
            draggableMarker.position =
                pointerWorldPosition +
                dragOffset;
        }

        CheckCurrentCheckpoint();
    }

    // =========================================================
    // DRAG END
    // =========================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (waveCompleted)
            return;

        FailCurrentWave();
    }

    // =========================================================
    // CHECKPOINT DETECTION
    // =========================================================

    private void CheckCurrentCheckpoint()
    {
        if (draggableMarker == null ||
            waveCompleted)
        {
            return;
        }

        Vector3 markerWorldPosition =
            draggableMarker.TransformPoint(
                draggableMarker.rect.center
            );

        switch (currentStep)
        {
            case WaveStep.NeedTop:

                if (
                    IsPointInsideCheckpoint(
                        markerWorldPosition,
                        topCheckpoint
                    )
                )
                {
                    ReachTop();
                }

                break;

            case WaveStep.NeedMiddle:

                if (
                    IsPointInsideCheckpoint(
                        markerWorldPosition,
                        middleCheckpoint
                    )
                )
                {
                    ReachMiddle();
                }

                break;

            case WaveStep.NeedBottom:

                if (
                    IsPointInsideCheckpoint(
                        markerWorldPosition,
                        bottomCheckpoint
                    )
                )
                {
                    ReachBottom();
                }

                break;
        }
    }

    private bool IsPointInsideCheckpoint(
        Vector3 worldPoint,
        RectTransform checkpoint)
    {
        if (checkpoint == null ||
            !checkpoint.gameObject
                .activeInHierarchy)
        {
            return false;
        }

        Vector3 localPoint =
            checkpoint.InverseTransformPoint(
                worldPoint
            );

        return checkpoint.rect.Contains(
            new Vector2(
                localPoint.x,
                localPoint.y
            )
        );
    }

    // =========================================================
    // TOP
    // =========================================================

    private void ReachTop()
    {
        currentStep =
            WaveStep.NeedMiddle;

        // -------------------------
        // AUDIO
        // -------------------------

        if (minigameAudio != null)
        {
            minigameAudio
                .PlayCheckpointPop();

            minigameAudio
                .PlaySplat1();
        }

        // -------------------------
        // VISUAL
        // -------------------------

        PlayCheckpointSuccess(
            topCheckpoint
        );

        ShowCheckpoint(
            middleCheckpoint
        );

        if (peristalsisEffect != null)
        {
            peristalsisEffect
                .PlayTop();
        }
    }

    // =========================================================
    // MIDDLE
    // =========================================================

    private void ReachMiddle()
    {
        currentStep =
            WaveStep.NeedBottom;

        // -------------------------
        // AUDIO
        // -------------------------

        if (minigameAudio != null)
        {
            minigameAudio
                .PlayCheckpointPop();

            minigameAudio
                .PlaySplat2();
        }

        // -------------------------
        // VISUAL
        // -------------------------

        PlayCheckpointSuccess(
            middleCheckpoint
        );

        ShowCheckpoint(
            bottomCheckpoint
        );

        if (peristalsisEffect != null)
        {
            peristalsisEffect
                .PlayMiddle();
        }
    }

    // =========================================================
    // BOTTOM
    // =========================================================

    private void ReachBottom()
    {
        waveCompleted = true;

        // -------------------------
        // AUDIO
        // -------------------------

        if (minigameAudio != null)
        {
            minigameAudio
                .PlayCheckpointPop();

            minigameAudio
                .PlaySplat1();
        }

        // -------------------------
        // VISUAL
        // -------------------------

        if (peristalsisEffect != null)
        {
            peristalsisEffect
                .PlayBottom();
        }

        PlayCheckpointSuccess(
            bottomCheckpoint,
            CompleteWave
        );
    }

    // =========================================================
    // SUCCESS
    // =========================================================

    private void CompleteWave()
    {
        if (foodStageController != null)
        {
            foodStageController
                .RegisterSuccessfulWave();
        }

        if (checkpointLayout != null)
        {
            checkpointLayout
                .GenerateNewWaveLayout();
        }

        HideAllCheckpoints();

        currentStep =
            WaveStep.NeedTop;

        waveCompleted = false;
        isDragging = false;
    }

    // =========================================================
    // FAIL
    // =========================================================

    private void FailCurrentWave()
    {
        currentStep =
            WaveStep.NeedTop;

        waveCompleted = false;
        isDragging = false;

        HideAllCheckpoints();

        if (checkpointLayout != null)
        {
            checkpointLayout
                .ResetMarker();
        }

        WaveFailedEarlyRelease
            ?.Invoke();
    }

    // =========================================================
    // EXTERNAL RESET
    // =========================================================

    public void ResetPeristalsis()
    {
        isDragging = false;
        waveCompleted = false;

        currentStep =
            WaveStep.NeedTop;

        HideAllCheckpoints();

        if (checkpointLayout != null)
        {
            checkpointLayout
                .GenerateNewWaveLayout();
        }
    }

    // =========================================================
    // CHECKPOINT VISUALS
    // =========================================================

    private void PlayCheckpointSuccess(
        RectTransform checkpoint,
        Action onComplete = null)
    {
        if (checkpoint == null)
        {
            onComplete?.Invoke();
            return;
        }

        PeristalsisCheckpointVisual visual =
            checkpoint.GetComponent<
                PeristalsisCheckpointVisual
            >();

        if (visual != null)
        {
            visual.PlaySuccess(
                onComplete
            );
        }
        else
        {
            checkpoint.gameObject
                .SetActive(false);

            onComplete?.Invoke();
        }
    }

    private void ShowCheckpoint(
        RectTransform checkpoint)
    {
        if (checkpoint != null)
        {
            checkpoint.gameObject
                .SetActive(true);
        }
    }

    private void HideCheckpoint(
        RectTransform checkpoint)
    {
        if (checkpoint != null)
        {
            checkpoint.gameObject
                .SetActive(false);
        }
    }

    private void HideAllCheckpoints()
    {
        HideCheckpoint(
            topCheckpoint
        );

        HideCheckpoint(
            middleCheckpoint
        );

        HideCheckpoint(
            bottomCheckpoint
        );
    }
}