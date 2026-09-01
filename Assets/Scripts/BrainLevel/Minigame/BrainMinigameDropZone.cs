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

    [Header("Accepted Problems")]
    [SerializeField]
    private TokenResponse[] tokenResponses;

    [Header("Command Token Visual")]
    [SerializeField]
    private GameObject commandTokenVisual;

    [Header("Brain Processing")]
    [SerializeField]
    private float processingDuration = 3f;

    [Header("Failure Feedback")]
    [SerializeField]
    private ScreenFlash screenFlash;

    private BrainTokenCarrier playerCarrier;

    private bool playerInside = false;
    private bool processing = false;

    private BrainTokenType activeCommandTokenType =
        BrainTokenType.None;

    private Sprite activeCommandTokenSprite;

    private Coroutine processingCoroutine;

    public event Action WrongProblemDelivered;

    public bool IsBusy =>
        processing ||
        activeCommandTokenType !=
            BrainTokenType.None;

    private void Awake()
    {
        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
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

    public void SetProcessingDuration(
        float duration)
    {
        processingDuration =
            Mathf.Max(
                0f,
                duration);
    }

    private void TryInteract()
    {
        if (playerCarrier.IsCarryingToken)
        {
            TryDeliverProblemToken();
            return;
        }

        TryPickUpCommandToken();
    }

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
                playerCarrier.CurrentTokenType);

        if (response == null)
        {
            if (screenFlash != null)
            {
                screenFlash.PlayRedFlash();
            }

            WrongProblemDelivered?.Invoke();

            return;
        }

        playerCarrier.DropToken();

        activeCommandTokenType =
            response.commandTokenType;

        activeCommandTokenSprite =
            response.commandTokenSprite;

        processingCoroutine =
            StartCoroutine(
                ProcessProblemRoutine());
    }

    private IEnumerator
        ProcessProblemRoutine()
    {
        processing = true;

        if (processingDuration > 0f)
        {
            yield return new WaitForSeconds(
                processingDuration);
        }

        processing = false;
        processingCoroutine = null;

        if (activeCommandTokenType ==
            BrainTokenType.None)
        {
            yield break;
        }

        if (activeCommandTokenSprite == null)
            yield break;

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
        }
    }

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

        playerCarrier.PickUpToken(
            activeCommandTokenSprite,
            activeCommandTokenType);

        commandTokenVisual.SetActive(false);

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite = null;
    }

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

    public void ClearCommandToken()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(
                processingCoroutine);

            processingCoroutine = null;
        }

        processing = false;

        if (commandTokenVisual != null)
        {
            commandTokenVisual.SetActive(false);
        }

        activeCommandTokenType =
            BrainTokenType.None;

        activeCommandTokenSprite = null;
    }

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