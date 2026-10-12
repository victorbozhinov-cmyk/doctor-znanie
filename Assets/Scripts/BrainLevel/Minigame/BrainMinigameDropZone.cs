using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BrainMinigameDropZone : MonoBehaviour
{
    [System.Serializable]
    public class TokenResponse
    {
        [Header("Incoming Problem")]
        public BrainTokenType problemTokenType =
            BrainTokenType.None;

        [Header("Returned Command")]
        public BrainTokenType commandTokenType =
            BrainTokenType.None;

        public Sprite commandTokenSprite;
    }

    // =========================================================
    // ACCEPTED PROBLEMS
    // =========================================================

    [Header("Accepted Problems")]
    [SerializeField]
    private TokenResponse[] tokenResponses;

    // =========================================================
    // COMMAND TOKEN
    // =========================================================

    [Header("Command Token Visual")]
    [SerializeField]
    private GameObject commandTokenVisual;

    [SerializeField]
    private BrainCommandReadyAnimation
        commandReadyAnimation;

    // =========================================================
    // BRAIN PROCESSING
    // =========================================================

    [Header("Brain Processing")]
    [SerializeField]
    private float processingDuration = 3f;

    [Header("Processing Indicator")]
    [SerializeField]
    private GameObject processingIndicator;

    [SerializeField]
    private Transform processingFill;

    // =========================================================
    // FEEDBACK
    // =========================================================

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    [Header("Audio")]
    [SerializeField]
    private BrainMinigameAudio minigameAudio;

    // =========================================================
    // INTERNAL
    // =========================================================

    private BrainTokenCarrier playerCarrier;

    private bool playerInside = false;
    private bool processing = false;

    private BrainTokenType activeCommandTokenType =
        BrainTokenType.None;

    private Sprite activeCommandTokenSprite;

    private Coroutine processingCoroutine;

    private Vector3 processingFillFullScale =
        Vector3.one;

    private Vector3 processingFillFullPosition =
        Vector3.zero;

    // =========================================================
    // EVENTS
    // =========================================================

    // Existing event
    public event Action WrongProblemDelivered;

    // NEW: Correct problem delivered to brain
    public event Action<BrainMinigameDropZone>
        ProblemDeliveredToBrain;

    // NEW: Brain finished processing
    public event Action<BrainMinigameDropZone>
        ProcessingCompleted;

    // NEW: Player picked up the command
    public event Action<BrainMinigameDropZone>
        CommandPickedUp;

    // =========================================================
    // PROPERTIES
    // =========================================================

    public bool IsBusy =>
        processing ||
        activeCommandTokenType !=
            BrainTokenType.None;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (minigameAudio == null)
        {
            minigameAudio =
                FindFirstObjectByType<
                    BrainMinigameAudio>();
        }

        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }

        if (processingFill != null)
        {
            processingFillFullScale =
                processingFill.localScale;

            processingFillFullPosition =
                processingFill.localPosition;
        }

        if (processingIndicator != null)
        {
            processingIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (playerCarrier == null)
            return;

        if (Keyboard.current == null)
            return;

        bool interactPressed =
            Keyboard.current.eKey
                .wasPressedThisFrame ||
            Keyboard.current.spaceKey
                .wasPressedThisFrame;

        if (!interactPressed)
            return;

        TryInteract();
    }

    // =========================================================
    // PROCESSING DURATION
    // =========================================================

    public void SetProcessingDuration(
        float duration)
    {
        processingDuration =
            Mathf.Max(
                0f,
                duration
            );
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    private void TryInteract()
    {
        if (playerCarrier.IsCarryingToken)
        {
            TryDeliverProblemToken();
            return;
        }

        TryPickUpCommandToken();
    }

    // =========================================================
    // DELIVER PROBLEM
    // =========================================================

    private void TryDeliverProblemToken()
    {
        if (processing)
            return;

        if (activeCommandTokenType !=
            BrainTokenType.None)
        {
            return;
        }

        TokenResponse response =
            FindResponseForProblem(
                playerCarrier.CurrentTokenType
            );

        // =====================================================
        // WRONG BRAIN ZONE
        // =====================================================

        if (response == null)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();

                if (minigameAudio != null)
                {
                    minigameAudio
                        .PlayWrongStation();
                }
            }

            WrongProblemDelivered?.Invoke();

            return;
        }

        // =====================================================
        // CORRECT BRAIN ZONE
        // =====================================================

        // DropToken() automatically plays POP.
        playerCarrier.DropToken();

        activeCommandTokenType =
            response.commandTokenType;

        activeCommandTokenSprite =
            response.commandTokenSprite;

        // =====================================================
        // NEW: NOTIFY TUTORIAL MANAGER
        // =====================================================

        // Notify that the problem was delivered
        // to the correct brain zone.
        ProblemDeliveredToBrain?.Invoke(this);

        // =====================================================
        // START PROCESSING
        // =====================================================

        processingCoroutine =
            StartCoroutine(
                ProcessProblemRoutine()
            );
    }

    // =========================================================
    // PROCESSING
    // =========================================================

    private IEnumerator
        ProcessProblemRoutine()
    {
        processing = true;

        if (processingIndicator != null)
        {
            processingIndicator.SetActive(true);
        }

        SetProcessingFill(0f);

        if (processingDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed <
                   processingDuration)
            {
                elapsed +=
                    Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed /
                        processingDuration
                    );

                SetProcessingFill(
                    progress
                );

                yield return null;
            }
        }

        SetProcessingFill(1f);

        if (processingIndicator != null)
        {
            processingIndicator.SetActive(false);
        }

        processing = false;
        processingCoroutine = null;

        if (activeCommandTokenType ==
            BrainTokenType.None)
        {
            yield break;
        }

        if (activeCommandTokenSprite == null)
        {
            yield break;
        }

        // =====================================================
        // SHOW COMMAND TOKEN
        // =====================================================

        if (commandTokenVisual != null)
        {
            SpriteRenderer spriteRenderer =
                commandTokenVisual
                    .GetComponent<
                        SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite =
                    activeCommandTokenSprite;
            }

            commandTokenVisual.SetActive(true);

            if (commandReadyAnimation != null)
            {
                commandReadyAnimation.Play();
            }

            // =================================================
            // NEW: PROCESSING COMPLETED EVENT
            // =================================================

            // Command is now ready for pickup.
            ProcessingCompleted?.Invoke(this);
        }
    }

    // =========================================================
    // PROCESSING FILL
    // =========================================================

    private void SetProcessingFill(
        float progress)
    {
        if (processingFill == null)
            return;

        progress =
            Mathf.Clamp01(progress);

        SpriteRenderer spriteRenderer =
            processingFill
                .GetComponent<SpriteRenderer>();

        if (spriteRenderer == null ||
            spriteRenderer.sprite == null)
        {
            return;
        }

        Vector3 scale =
            processingFillFullScale;

        scale.x =
            processingFillFullScale.x *
            progress;

        processingFill.localScale =
            scale;

        float fullWidth =
            spriteRenderer.sprite
                .bounds.size.x *
            processingFillFullScale.x;

        float missingWidth =
            fullWidth *
            (1f - progress);

        Vector3 position =
            processingFillFullPosition;

        position.x -=
            missingWidth * 0.5f;

        processingFill.localPosition =
            position;
    }

    // =========================================================
    // PICK UP COMMAND
    // =========================================================

    private void TryPickUpCommandToken()
    {
        if (processing)
            return;

        if (playerCarrier.IsCarryingToken)
            return;

        if (commandTokenVisual == null ||
            !commandTokenVisual.activeSelf)
        {
            return;
        }

        if (activeCommandTokenType ==
            BrainTokenType.None)
        {
            return;
        }

        // =====================================================
        // PICK UP COMMAND
        // =====================================================

        // PickUpToken() automatically plays POP.
        playerCarrier.PickUpToken(
            activeCommandTokenSprite,
            activeCommandTokenType
        );

        commandTokenVisual.SetActive(false);

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite =
            null;

        // =====================================================
        // NEW: COMMAND PICKED UP EVENT
        // =====================================================

        // Notify that the player has picked up
        // the processed command.
        CommandPickedUp?.Invoke(this);
    }

    // =========================================================
    // RESPONSE
    // =========================================================

    private TokenResponse
        FindResponseForProblem(
            BrainTokenType problemTokenType)
    {
        if (tokenResponses == null)
            return null;

        foreach (TokenResponse response in
                 tokenResponses)
        {
            if (response == null)
                continue;

            if (response.problemTokenType ==
                problemTokenType)
            {
                return response;
            }
        }

        return null;
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearCommandToken()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(
                processingCoroutine
            );

            processingCoroutine = null;
        }

        processing = false;

        if (processingIndicator != null)
        {
            processingIndicator.SetActive(false);
        }

        SetProcessingFill(0f);

        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite =
            null;
    }

    // =========================================================
    // TRIGGERS
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<
                BrainTokenCarrier>();

        if (carrier == null)
            return;

        playerCarrier = carrier;
        playerInside = true;
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        BrainTokenCarrier carrier =
            other.GetComponent<
                BrainTokenCarrier>();

        if (carrier == null)
            return;

        if (carrier == playerCarrier)
        {
            playerCarrier = null;
            playerInside = false;
        }
    }
}