using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainMinigameManager : MonoBehaviour
{
    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Round")]
        [Min(1f)]
        public float roundDuration = 120f;

        [Header("Orders")]
        [Min(1)]
        public int maxActiveOrders = 2;

        [Min(1f)]
        public float orderDuration = 28f;

        [Header("Order Variety")]
        [Min(0)]
        public int recentOrderBlockCount = 3;

        [Header("New Order Delay")]
        [Min(0f)]
        public float minSpawnDelay = 8f;

        [Min(0f)]
        public float maxSpawnDelay = 11f;

        [Header("Brain Processing")]
        [Min(0f)]
        public float brainProcessingTime = 2f;

        [Header("Nervous Tension")]
        [Range(0f, 100f)]
        public float failedOrderTension = 20f;

        [Range(0f, 100f)]
        public float wrongBrainZoneTension = 5f;

        [Min(0f)]
        public float passiveTensionPerActiveOrderPerSecond = 0f;
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    [Header("Difficulty Settings")]
    [SerializeField]
    private DifficultySettings easySettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 2,
            orderDuration = 28f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 8f,
            maxSpawnDelay = 11f,
            brainProcessingTime = 2f,
            failedOrderTension = 20f,
            wrongBrainZoneTension = 5f,
            passiveTensionPerActiveOrderPerSecond = 0f
        };

    [SerializeField]
    private DifficultySettings mediumSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 3,
            orderDuration = 23f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 6f,
            maxSpawnDelay = 9f,
            brainProcessingTime = 3f,
            failedOrderTension = 25f,
            wrongBrainZoneTension = 8f,
            passiveTensionPerActiveOrderPerSecond = 0.15f
        };

    [SerializeField]
    private DifficultySettings hardSettings =
        new DifficultySettings
        {
            roundDuration = 120f,
            maxActiveOrders = 4,
            orderDuration = 19f,
            recentOrderBlockCount = 3,
            minSpawnDelay = 4f,
            maxSpawnDelay = 7f,
            brainProcessingTime = 4f,
            failedOrderTension = 34f,
            wrongBrainZoneTension = 10f,
            passiveTensionPerActiveOrderPerSecond = 0.5f
        };

    // =========================================================
    // STATIONS
    // =========================================================

    [Header("Stations")]
    [SerializeField]
    private BrainOrganStation[] stations;

    // =========================================================
    // ROUND UI
    // =========================================================

    [Header("Optional Round UI")]
    [SerializeField]
    private TMP_Text roundTimeText;

    [Header("Round Timer Pulse")]
    [SerializeField]
    private BrainRoundTimerPulse roundTimerPulse;

    [SerializeField]
    private float roundTimerPulseStartTime = 10f;

    // =========================================================
    // TENSION
    // =========================================================

    [Header("Optional Tension UI")]
    [SerializeField]
    private StressFillMaskController tensionFillMask;

    [SerializeField]
    private TMP_Text tensionText;

    [Header("Tension Brain")]
    [SerializeField]
    private SpriteRenderer tensionBrainRenderer;

    [SerializeField]
    private Sprite calmBrainSprite;

    [SerializeField]
    private Sprite stressedBrainSprite;

    [SerializeField]
    private Sprite criticalBrainSprite;

    [Header("Tension Brain Animation")]
    [SerializeField]
    private BrainTensionStateAnimation tensionStateAnimation;

    // =========================================================
    // WELCOME
    // =========================================================

    [Header("Welcome")]
    [SerializeField]
    private GameObject welcomeOverlay;

    [SerializeField]
    private UIPopupAnimation welcomePanelAnimation;

    // =========================================================
    // TUTORIAL (NEW)
    // =========================================================

    [Header("Tutorial")]
    [SerializeField]
    private bool enableTutorial = true;

    [Tooltip("Тази станция трябва да присъства и в масива Stations.")]
    [SerializeField]
    private BrainOrganStation tutorialStation;

    [Tooltip("TutorialWorldRoot: включва панела, надписа и SkipTutorialButton.")]
    [SerializeField]
    private GameObject tutorialOverlay;

    [SerializeField]
    private TMP_Text tutorialInstructionText;

    [Min(0f)]
    [SerializeField]
    private float tutorialCompletionDelay = 2f;

    [Header("Tutorial Transition - Fade Out")]
    [SerializeField, Min(0f)]
    private float tutorialFadeDuration = 0.5f;

    [Header("Tutorial Transition - Player Reset")]
    [Tooltip("PlayerRoot, който съдържа BrainMinigamePlayer.")]
    [SerializeField] private BrainMinigamePlayer tutorialPlayer;

    [Tooltip("По избор. Ако не е свързан, използва началната позиция на героя при зареждане на сцената.")]
    [SerializeField] private Transform playerStartPoint;

    [Header("Tutorial Transition - Screen Shake")]
    [Tooltip("При успешно завършване и при натискане на Пропусни.")]
    [SerializeField] private Camera tutorialShakeCamera;

    [SerializeField, Min(0f)] private float tutorialShakeDuration = 0.3f;
    [SerializeField, Min(0f)] private float tutorialShakeIntensity = 0.12f;

    // All tutorial messages are editable from the Unity Inspector.
    // Existing scene references and difficulty settings are unchanged.
    [System.Serializable]
    public class TutorialTextSettings
    {
        [Header("Step 1 - Find the problem")]
        [TextArea(2, 5)]
        public string findProblem =
            "СТЪПКА 1/5\nОтиди до органа с появилия се проблем " +
            "и натисни E или Space, за да вземеш сигнала.";

        [Header("Step 2 - Deliver to the brain")]
        [TextArea(2, 5)]
        public string deliverProblem =
            "СТЪПКА 2/5\nЗанеси сигнала до правилната " +
            "област на мозъка и натисни E или Space.";

        [Header("Step 3 - Wait for processing")]
        [TextArea(2, 5)]
        public string waitForProcessing =
            "СТЪПКА 3/5\nЧудесно! Изчакай мозъкът " +
            "да обработи сигнала.";

        [Header("Step 4 - Pick up the command")]
        [TextArea(2, 5)]
        public string pickUpCommand =
            "СТЪПКА 4/5\nКомандата е готова! " +
            "Вземи я с E или Space.";

        [Header("Step 5 - Return the command")]
        [TextArea(2, 5)]
        public string returnCommand =
            "СТЪПКА 5/5\nВърни готовата команда " +
            "до същия орган и натисни E или Space.";

        [Header("Success")]
        [TextArea(2, 5)]
        public string completed =
            "ОТЛИЧНО!\nИзпълни първата поръчка! " +
            "Сега започва истинското предизвикателство.";

        [Header("Wrong brain zone (no penalty)")]
        [TextArea(2, 5)]
        public string wrongBrainZone =
            "Това не е правилната мозъчна зона. " +
            "Опитай друга зона — без наказание.";

        [Header("Wrong station (no penalty)")]
        [TextArea(2, 5)]
        public string wrongStation =
            "Върни командата до органа, " +
            "от който взе сигнала. Опитай отново.";
    }

    [Header("Tutorial Texts (Editable)")]
    [SerializeField]
    private TutorialTextSettings tutorialTexts = new TutorialTextSettings();

    // =========================================================
    // PAUSE
    // =========================================================

    [Header("Pause")]
    [SerializeField]
    private GameObject pauseWorld;

    [Header("Pause Panels")]
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private WorldPopupAnimation pausePopupAnimation;

    [SerializeField]
    private GameObject infoPanel;

    [SerializeField]
    private WorldPopupAnimation infoPopupAnimation;

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    [SerializeField]
    private GameObject settingsOverlay;

    [SerializeField]
    private PauseUIPopupAnimation settingsPopupAnimation;

    [SerializeField]
    private DifficultySelector difficultySelector;

    // =========================================================
    // EXIT
    // =========================================================

    [Header("Exit Confirmation")]
    [SerializeField]
    private GameObject exitConfirmationPanel;

    [SerializeField]
    private PauseUIPopupAnimation exitPopupAnimation;

    // =========================================================
    // END OBJECTS
    // =========================================================

    [Header("Optional End Objects")]
    [SerializeField]
    private GameObject successObject;

    [SerializeField]
    private GameObject gameOverObject;

    // =========================================================
    // INTERNAL
    // =========================================================

    private DifficultySettings currentSettings;

    private readonly List<BrainOrganStation> activeStations =
        new List<BrainOrganStation>();

    private readonly List<BrainOrganStation> recentStations =
        new List<BrainOrganStation>();

    private readonly List<BrainMinigameDropZone> subscribedDropZones =
        new List<BrainMinigameDropZone>();

    private float remainingRoundTime;
    private float nervousTension = 0f;

    private bool roundRunning = false;
    private bool isPaused = false;

    private bool minigameStarted = false;
    private bool welcomeTransitionInProgress = false;
    private bool menuTransitionInProgress = false;
    private bool infoOpenedFromWelcome = false;

    private Coroutine spawnCoroutine;

    private int currentTensionBrainState = 0;
    private int lastRoundTimerPulseSecond = -1;

    // =========================================================
    // TUTORIAL INTERNAL (NEW)
    // =========================================================

    private enum TutorialStep
    {
        None,
        FindProblem,
        DeliverProblem,
        WaitForProcessing,
        PickUpCommand,
        ReturnCommand,
        Completed
    }

    private TutorialStep tutorialStep = TutorialStep.None;
    private bool tutorialActive = false;
    private bool tutorialFinishing = false;

    private BrainOrganStation activeTutorialStation;
    private BrainMinigameDropZone activeTutorialDropZone;
    private Coroutine tutorialCompletionCoroutine;

    // Original UI colors are preserved so the tutorial can run again.
    private SpriteRenderer[] tutorialSpriteRenderers;
    private Color[] tutorialOriginalSpriteColors;
    private TMP_Text[] tutorialTextRenderers;
    private Color[] tutorialOriginalTextColors;

    private Vector3 originalPlayerPosition;
    private bool hasOriginalPlayerPosition;

    private Transform activeShakeTransform;
    private Vector3 originalShakePosition;

    public float NervousTension => nervousTension;
    public float RemainingRoundTime => remainingRoundTime;
    public bool IsPaused => isPaused;
    public bool IsTutorialActive => tutorialActive;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        CacheTutorialVisuals();
        CachePlayerStartPosition();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        PrepareForWelcome();
    }

    private void Update()
    {
        if (!minigameStarted || !roundRunning || isPaused)
            return;

        UpdatePassiveTension();

        if (!roundRunning)
            return;

        UpdateRoundTimer();
    }

    // =========================================================
    // WELCOME PREPARATION
    // =========================================================

    public void PrepareForWelcome()
    {
        Time.timeScale = 1f;

        minigameStarted = false;
        roundRunning = false;
        isPaused = false;

        welcomeTransitionInProgress = false;
        menuTransitionInProgress = false;
        infoOpenedFromWelcome = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopTutorialCompletionCoroutine();
        ResetTutorialState();
        StopAllOrders();

        currentSettings = GetCurrentDifficultySettings();
        remainingRoundTime = currentSettings.roundDuration;
        nervousTension = 0f;

        currentTensionBrainState = 0;
        lastRoundTimerPulseSecond = -1;

        activeStations.Clear();
        recentStations.Clear();

        if (roundTimerPulse != null)
            roundTimerPulse.ResetAnimation();

        HidePauseWorld();

        if (successObject != null)
            successObject.SetActive(false);

        if (gameOverObject != null)
            gameOverObject.SetActive(false);

        UpdateRoundTimeUI();
        UpdateTensionUI();
        ShowWelcome();

        Debug.Log("Brain Minigame чака Welcome панела.");
    }

    private void ShowWelcome()
    {
        if (welcomeOverlay == null)
        {
            Debug.LogError(
                "Minigame Welcome Overlay не е свързан в BrainMinigameManager.");
            return;
        }

        welcomeOverlay.SetActive(true);
        welcomeOverlay.transform.SetAsLastSibling();

        if (welcomePanelAnimation != null)
            welcomePanelAnimation.PlayOpen();
    }

    public void ShowWelcomeAfterInfo()
    {
        if (minigameStarted)
            return;

        welcomeTransitionInProgress = false;
        menuTransitionInProgress = false;
        ShowWelcome();
    }

    // =========================================================
    // START FROM WELCOME
    // =========================================================

    public void StartMinigameFromWelcome()
    {
        if (minigameStarted || welcomeTransitionInProgress)
            return;

        welcomeTransitionInProgress = true;

        if (welcomePanelAnimation != null)
            welcomePanelAnimation.PlayClose(FinishStartingFromWelcome);
        else
            FinishStartingFromWelcome();
    }

    private void FinishStartingFromWelcome()
    {
        if (welcomeOverlay != null)
            welcomeOverlay.SetActive(false);

        welcomeTransitionInProgress = false;
        minigameStarted = true;

        if (enableTutorial)
            StartTutorial();
        else
            StartRound();
    }

    // =========================================================
    // INFO FROM WELCOME
    // =========================================================

    public void OpenInfoFromWelcome()
    {
        if (minigameStarted || welcomeTransitionInProgress ||
            menuTransitionInProgress)
            return;

        infoOpenedFromWelcome = true;
        welcomeTransitionInProgress = true;

        if (welcomePanelAnimation != null)
            welcomePanelAnimation.PlayClose(FinishOpeningInfoFromWelcome);
        else
            FinishOpeningInfoFromWelcome();
    }

    private void FinishOpeningInfoFromWelcome()
    {
        if (welcomeOverlay != null)
            welcomeOverlay.SetActive(false);

        welcomeTransitionInProgress = false;

        if (pauseWorld != null)
            pauseWorld.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    // =========================================================
    // TUTORIAL (NEW)
    // =========================================================

    private void StartTutorial()
    {
        Time.timeScale = 1f;

        currentSettings = GetCurrentDifficultySettings();
        remainingRoundTime = currentSettings.roundDuration;
        nervousTension = 0f;
        currentTensionBrainState = 0;
        lastRoundTimerPulseSecond = -1;

        roundRunning = false;
        minigameStarted = true;
        isPaused = false;

        tutorialActive = true;
        tutorialFinishing = false;
        RestoreTutorialVisuals();
        tutorialStep = TutorialStep.None;
        activeTutorialStation = null;
        activeTutorialDropZone = null;

        activeStations.Clear();
        recentStations.Clear();
        StopAllOrders();

        HidePauseWorld();
        SetupStations();
        SetupBrainDropZones();

        if (roundTimerPulse != null)
            roundTimerPulse.ResetAnimation();

        UpdateRoundTimeUI();
        UpdateTensionUI();

        activeTutorialStation = GetTutorialStation();
        if (activeTutorialStation == null)
        {
            Debug.LogWarning(
                "Brain tutorial: няма валидна станция. Стартира нормален рунд.");
            ResetTutorialState();
            StartRound();
            return;
        }

        activeTutorialStation.PrepareNextVariant();
        activeTutorialDropZone = activeTutorialStation.BrainDropZone;

        if (activeTutorialDropZone == null)
        {
            Debug.LogWarning(
                "Brain tutorial: избраната станция няма мозъчна зона. " +
                "Стартира нормален рунд.");
            ResetTutorialState();
            StartRound();
            return;
        }

        tutorialStep = TutorialStep.FindProblem;
        ShowTutorialInstruction(tutorialTexts.findProblem);

        // No timer for the tutorial order.
        activeTutorialStation.StartManagedOrder(
            currentSettings.orderDuration,
            true);

        Debug.Log("Brain tutorial started: " +
                  activeTutorialStation.gameObject.name);
    }

    private BrainOrganStation GetTutorialStation()
    {
        if (stations == null || stations.Length == 0)
            return null;

        if (tutorialStation != null)
        {
            foreach (BrainOrganStation station in stations)
            {
                if (station == tutorialStation && station.CanStartOrder)
                    return station;
            }

            Debug.LogWarning(
                "Tutorial Station не е налична в Stations. " +
                "Използва се първата налична станция.");
        }

        foreach (BrainOrganStation station in stations)
        {
            if (station != null && station.CanStartOrder)
                return station;
        }

        return null;
    }

    private void ShowTutorialInstruction(string message)
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(true);
            tutorialOverlay.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Tutorial Overlay не е свързан.");
        }

        if (tutorialInstructionText != null)
            tutorialInstructionText.text = message;
        else
            Debug.LogWarning("Tutorial Instruction Text не е свързан.");
    }

    private void HideTutorialUI()
    {
        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);
    }

    private void HandleTutorialProblemPickedUp(BrainOrganStation station)
    {
        if (!tutorialActive || tutorialFinishing ||
            station != activeTutorialStation ||
            tutorialStep != TutorialStep.FindProblem)
            return;

        tutorialStep = TutorialStep.DeliverProblem;
        ShowTutorialInstruction(tutorialTexts.deliverProblem);
    }

    private void HandleTutorialProblemDelivered(BrainMinigameDropZone zone)
    {
        if (!tutorialActive || tutorialFinishing ||
            zone != activeTutorialDropZone ||
            tutorialStep != TutorialStep.DeliverProblem)
            return;

        tutorialStep = TutorialStep.WaitForProcessing;
        ShowTutorialInstruction(tutorialTexts.waitForProcessing);
    }

    private void HandleTutorialProcessingCompleted(BrainMinigameDropZone zone)
    {
        if (!tutorialActive || tutorialFinishing ||
            zone != activeTutorialDropZone ||
            tutorialStep != TutorialStep.WaitForProcessing)
            return;

        tutorialStep = TutorialStep.PickUpCommand;
        ShowTutorialInstruction(tutorialTexts.pickUpCommand);
    }

    private void HandleTutorialCommandPickedUp(BrainMinigameDropZone zone)
    {
        if (!tutorialActive || tutorialFinishing ||
            zone != activeTutorialDropZone ||
            tutorialStep != TutorialStep.PickUpCommand)
            return;

        tutorialStep = TutorialStep.ReturnCommand;
        ShowTutorialInstruction(tutorialTexts.returnCommand);
    }

    private void CompleteTutorial()
    {
        if (!tutorialActive || tutorialFinishing)
            return;

        tutorialFinishing = true;
        tutorialStep = TutorialStep.Completed;
        ShowTutorialInstruction(tutorialTexts.completed);

        StopTutorialCompletionCoroutine();
        tutorialCompletionCoroutine =
            StartCoroutine(TutorialExitRoutine(successfullyCompleted: true));
    }

    // Called by BrainTutorialSkipButton.cs when the player clicks Skip.
    public void SkipTutorial()
    {
        if (!tutorialActive || tutorialFinishing || isPaused)
            return;

        tutorialFinishing = true;
        StopTutorialCompletionCoroutine();
        tutorialCompletionCoroutine =
            StartCoroutine(TutorialExitRoutine(successfullyCompleted: false));
    }

    private IEnumerator TutorialExitRoutine(bool successfullyCompleted)
    {
        // Success message stays readable before the transition starts.
        if (successfullyCompleted && tutorialCompletionDelay > 0f)
            yield return new WaitForSeconds(tutorialCompletionDelay);

        float fadeDuration = Mathf.Max(0f, tutorialFadeDuration);
        float shakeDuration = Mathf.Max(0f, tutorialShakeDuration);

        // Run Fade Out and screen shake simultaneously for both completion and Skip.
        float transitionDuration = Mathf.Max(fadeDuration, shakeDuration);

        Camera cameraToShake = tutorialShakeCamera != null
            ? tutorialShakeCamera
            : Camera.main;

        if (shakeDuration > 0f && tutorialShakeIntensity > 0f &&
            cameraToShake != null)
        {
            activeShakeTransform = cameraToShake.transform;
            originalShakePosition = activeShakeTransform.localPosition;
        }

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Fade includes SpriteRenderers AND TextMeshPro; the skip button
            // is part of the same hierarchy (TutorialWorldRoot).
            float fadeProgress = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);
            float easedFade = Mathf.SmoothStep(0f, 1f, fadeProgress);
            SetTutorialVisualAlpha(1f - easedFade);

            if (activeShakeTransform != null)
            {
                if (elapsed < shakeDuration)
                {
                    float damping = 1f - elapsed / shakeDuration;
                    float strength = tutorialShakeIntensity * damping;
                    float phase = elapsed * 35f;

                    Vector3 offset = new Vector3(
                        Mathf.Sin(phase * 1.29f),
                        Mathf.Cos(phase * 1.71f),
                        0f) * strength;

                    activeShakeTransform.localPosition =
                        originalShakePosition + offset;
                }
                else
                {
                    RestoreCameraPosition();
                }
            }

            yield return null;
        }

        RestoreCameraPosition();
        SetTutorialVisualAlpha(0f);

        // Important: Do not start the clock until the animation has ended.
        tutorialCompletionCoroutine = null;
        if (tutorialActive)
            BeginNormalRoundAfterTutorial();
    }

    private void BeginNormalRoundAfterTutorial()
    {
        StopTutorialCompletionCoroutine();

        // Stop tutorial order and clear its tokens BEFORE the real round.
        StopAllOrders();
        ResetPlayerForNormalRound();
        ResetTutorialState();
        StartRound();
    }

    private void StopTutorialCompletionCoroutine()
    {
        if (tutorialCompletionCoroutine != null)
        {
            StopCoroutine(tutorialCompletionCoroutine);
            tutorialCompletionCoroutine = null;
        }

        RestoreCameraPosition();
    }

    private void RestoreCameraPosition()
    {
        if (activeShakeTransform == null)
            return;

        activeShakeTransform.localPosition = originalShakePosition;
        activeShakeTransform = null;
    }

    private void CacheTutorialVisuals()
    {
        if (tutorialOverlay == null)
            return;

        tutorialSpriteRenderers =
            tutorialOverlay.GetComponentsInChildren<SpriteRenderer>(true);
        tutorialOriginalSpriteColors =
            new Color[tutorialSpriteRenderers.Length];

        for (int i = 0; i < tutorialSpriteRenderers.Length; i++)
            tutorialOriginalSpriteColors[i] = tutorialSpriteRenderers[i].color;

        tutorialTextRenderers =
            tutorialOverlay.GetComponentsInChildren<TMP_Text>(true);
        tutorialOriginalTextColors =
            new Color[tutorialTextRenderers.Length];

        for (int i = 0; i < tutorialTextRenderers.Length; i++)
            tutorialOriginalTextColors[i] = tutorialTextRenderers[i].color;
    }

    private void SetTutorialVisualAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (tutorialSpriteRenderers != null)
        {
            for (int i = 0; i < tutorialSpriteRenderers.Length; i++)
            {
                if (tutorialSpriteRenderers[i] == null) continue;
                Color color = tutorialOriginalSpriteColors[i];
                color.a *= alpha;
                tutorialSpriteRenderers[i].color = color;
            }
        }

        if (tutorialTextRenderers != null)
        {
            for (int i = 0; i < tutorialTextRenderers.Length; i++)
            {
                if (tutorialTextRenderers[i] == null) continue;
                Color color = tutorialOriginalTextColors[i];
                color.a *= alpha;
                tutorialTextRenderers[i].color = color;
            }
        }
    }

    private void RestoreTutorialVisuals()
    {
        // Colors are cached on Awake; restore them after every fade.
        SetTutorialVisualAlpha(1f);
    }

    private void CachePlayerStartPosition()
    {
        if (tutorialPlayer == null)
            tutorialPlayer = FindFirstObjectByType<BrainMinigamePlayer>();

        if (tutorialPlayer == null)
            return;

        originalPlayerPosition = tutorialPlayer.transform.position;
        hasOriginalPlayerPosition = true;
    }

    private void ResetPlayerForNormalRound()
    {
        if (tutorialPlayer == null)
            CachePlayerStartPosition();

        if (tutorialPlayer == null)
        {
            Debug.LogWarning("Brain Tutorial: Player не е открит за връщане в стартовата позиция.");
            return;
        }

        // DropToken(false) clears the carried signal without playing a POP.
        BrainTokenCarrier carrier = tutorialPlayer.GetComponent<BrainTokenCarrier>();
        if (carrier != null && carrier.IsCarryingToken)
            carrier.DropToken(false);

        if (!hasOriginalPlayerPosition && playerStartPoint == null)
            return;

        Vector3 newPosition = playerStartPoint != null
            ? playerStartPoint.position
            : originalPlayerPosition;

        Rigidbody2D rb = tutorialPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = new Vector2(newPosition.x, newPosition.y);
        }

        tutorialPlayer.transform.position = newPosition;
    }

    private void ResetTutorialState()
    {
        tutorialActive = false;
        tutorialFinishing = false;
        tutorialStep = TutorialStep.None;
        activeTutorialStation = null;
        activeTutorialDropZone = null;

        HideTutorialUI();
        RestoreTutorialVisuals();
    }

    // =========================================================
    // ROUND
    // =========================================================

    private void StartRound()
    {
        Time.timeScale = 1f;

        ResetTutorialState();
        StopAllOrders();

        currentSettings = GetCurrentDifficultySettings();
        remainingRoundTime = currentSettings.roundDuration;

        nervousTension = 0f;
        currentTensionBrainState = 0;
        lastRoundTimerPulseSecond = -1;

        if (roundTimerPulse != null)
            roundTimerPulse.ResetAnimation();

        roundRunning = true;
        isPaused = false;
        minigameStarted = true;

        activeStations.Clear();
        recentStations.Clear();
        HidePauseWorld();

        if (successObject != null)
            successObject.SetActive(false);

        if (gameOverObject != null)
            gameOverObject.SetActive(false);

        SetupStations();
        SetupBrainDropZones();
        UpdateRoundTimeUI();
        UpdateTensionUI();

        TryStartRandomOrder();
        spawnCoroutine = StartCoroutine(SpawnOrdersRoutine());

        Debug.Log("Brain minigame started. Difficulty: " +
                  PlayerPrefs.GetInt("Difficulty", 1));
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void PauseGame()
    {
        if (!minigameStarted ||
            (!roundRunning && !tutorialActive) ||
            isPaused || menuTransitionInProgress || tutorialFinishing)
            return;

        isPaused = true;

        if (pauseWorld != null)
            pauseWorld.SetActive(true);

        ShowMainPausePanel();
        Time.timeScale = 0f;

        Debug.Log("Brain minigame paused.");
    }

    public void ResumeGame()
    {
        if ((!roundRunning && !tutorialActive) ||
            !isPaused || menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
            pausePopupAnimation.PlayClose(FinishResumingGame);
        else
            FinishResumingGame();
    }

    private void FinishResumingGame()
    {
        HidePauseWorld();
        isPaused = false;
        menuTransitionInProgress = false;
        Time.timeScale = 1f;

        Debug.Log("Brain minigame resumed.");
    }

    // =========================================================
    // INFO FROM PAUSE
    // =========================================================

    public void OpenInfoPanel()
    {
        if (!isPaused || menuTransitionInProgress)
            return;

        infoOpenedFromWelcome = false;
        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
            pausePopupAnimation.PlayClose(FinishOpeningInfoFromPause);
        else
            FinishOpeningInfoFromPause();
    }

    private void FinishOpeningInfoFromPause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(true);

        menuTransitionInProgress = false;
    }

    public void CloseInfoPanel()
    {
        if (menuTransitionInProgress)
            return;

        if (!isPaused && !infoOpenedFromWelcome)
            return;

        menuTransitionInProgress = true;

        if (infoPopupAnimation != null)
            infoPopupAnimation.PlayClose(FinishClosingInfo);
        else
            FinishClosingInfo();
    }

    private void FinishClosingInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (infoOpenedFromWelcome)
        {
            infoOpenedFromWelcome = false;

            if (pauseWorld != null)
                pauseWorld.SetActive(false);

            menuTransitionInProgress = false;
            ShowWelcomeAfterInfo();
            return;
        }

        if (pauseWorld != null)
            pauseWorld.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        menuTransitionInProgress = false;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettingsPanel()
    {
        if (!isPaused || menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        if (difficultySelector != null)
            difficultySelector.SetLocked(true);

        if (pausePopupAnimation != null)
            pausePopupAnimation.PlayClose(FinishOpeningSettings);
        else
            FinishOpeningSettings();
    }

    private void FinishOpeningSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }

        menuTransitionInProgress = false;
    }

    public void CloseSettingsPanel()
    {
        if (!isPaused || menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        if (settingsPopupAnimation != null)
            settingsPopupAnimation.PlayClose(FinishClosingSettings);
        else
            FinishClosingSettings();
    }

    private void FinishClosingSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (pauseWorld != null)
            pauseWorld.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        menuTransitionInProgress = false;
    }

    // =========================================================
    // EXIT
    // =========================================================

    public void OpenExitConfirmation()
    {
        if (!isPaused || menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        if (pausePopupAnimation != null)
            pausePopupAnimation.PlayClose(FinishOpeningExit);
        else
            FinishOpeningExit();
    }

    private void FinishOpeningExit()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
            exitConfirmationPanel.transform.SetAsLastSibling();
        }

        menuTransitionInProgress = false;
    }

    public void CloseExitConfirmation()
    {
        if (!isPaused || menuTransitionInProgress)
            return;

        menuTransitionInProgress = true;

        if (exitPopupAnimation != null)
            exitPopupAnimation.PlayClose(FinishClosingExitConfirmation);
        else
            FinishClosingExitConfirmation();
    }

    private void FinishClosingExitConfirmation()
    {
        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (pauseWorld != null)
            pauseWorld.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        menuTransitionInProgress = false;
    }

    public void ExitToBodyMap()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("BodyMap");
    }

    // =========================================================
    // PAUSE HELPERS
    // =========================================================

    private void ShowMainPausePanel()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    private void HidePauseWorld()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (exitConfirmationPanel != null)
            exitConfirmationPanel.SetActive(false);

        if (pauseWorld != null)
            pauseWorld.SetActive(false);
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupStations()
    {
        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null)
                continue;

            station.OrderCompleted -= HandleOrderCompleted;
            station.OrderFailed -= HandleOrderFailed;
            station.WrongCommandDelivered -= HandleWrongCommandDelivered;
            station.ProblemPickedUp -= HandleTutorialProblemPickedUp;

            station.OrderCompleted += HandleOrderCompleted;
            station.OrderFailed += HandleOrderFailed;
            station.WrongCommandDelivered += HandleWrongCommandDelivered;
            station.ProblemPickedUp += HandleTutorialProblemPickedUp;

            station.PrepareForManager();
        }
    }

    private void SetupBrainDropZones()
    {
        UnsubscribeFromDropZones();

        BrainMinigameDropZone[] dropZones =
            FindObjectsByType<BrainMinigameDropZone>(
                FindObjectsSortMode.None);

        foreach (BrainMinigameDropZone dropZone in dropZones)
        {
            if (dropZone == null)
                continue;

            dropZone.SetProcessingDuration(
                currentSettings.brainProcessingTime);

            dropZone.WrongProblemDelivered += HandleWrongBrainZone;
            dropZone.ProblemDeliveredToBrain += HandleTutorialProblemDelivered;
            dropZone.ProcessingCompleted += HandleTutorialProcessingCompleted;
            dropZone.CommandPickedUp += HandleTutorialCommandPickedUp;

            subscribedDropZones.Add(dropZone);
        }
    }

    private void UnsubscribeFromDropZones()
    {
        foreach (BrainMinigameDropZone zone in subscribedDropZones)
        {
            if (zone == null)
                continue;

            zone.WrongProblemDelivered -= HandleWrongBrainZone;
            zone.ProblemDeliveredToBrain -= HandleTutorialProblemDelivered;
            zone.ProcessingCompleted -= HandleTutorialProcessingCompleted;
            zone.CommandPickedUp -= HandleTutorialCommandPickedUp;
        }

        subscribedDropZones.Clear();
    }

    // =========================================================
    // ORDER SPAWNING
    // =========================================================

    private IEnumerator SpawnOrdersRoutine()
    {
        while (roundRunning)
        {
            float delay = Random.Range(
                currentSettings.minSpawnDelay,
                currentSettings.maxSpawnDelay);

            yield return new WaitForSeconds(delay);

            if (!roundRunning)
                yield break;

            if (isPaused)
                continue;

            if (activeStations.Count < currentSettings.maxActiveOrders)
                TryStartRandomOrder();
        }
    }

    private void TryStartRandomOrder()
    {
        if (!roundRunning || isPaused ||
            activeStations.Count >= currentSettings.maxActiveOrders)
            return;

        List<BrainOrganStation> available = new List<BrainOrganStation>();

        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null || !station.CanStartOrder ||
                IsRecentlyUsed(station))
                continue;

            station.PrepareNextVariant();

            if (HasActiveOrderForSameBrainZone(station))
                continue;

            available.Add(station);
        }

        if (available.Count == 0)
        {
            foreach (BrainOrganStation station in stations)
            {
                if (station == null || !station.CanStartOrder)
                    continue;

                station.PrepareNextVariant();

                if (HasActiveOrderForSameBrainZone(station))
                    continue;

                available.Add(station);
            }
        }

        if (available.Count == 0)
            return;

        BrainOrganStation selected =
            available[Random.Range(0, available.Count)];

        selected.StartManagedOrder(currentSettings.orderDuration);
        activeStations.Add(selected);
        RegisterRecentStation(selected);

        Debug.Log("New brain order: " + selected.gameObject.name);
    }

    private bool HasActiveOrderForSameBrainZone(BrainOrganStation candidate)
    {
        if (candidate == null)
            return false;

        BrainMinigameDropZone candidateZone = candidate.BrainDropZone;

        if (candidateZone == null)
            return false;

        foreach (BrainOrganStation active in activeStations)
        {
            if (active == null)
                continue;

            if (active.BrainDropZone == candidateZone)
                return true;
        }

        return false;
    }

    // =========================================================
    // ORDER VARIETY
    // =========================================================

    private bool IsRecentlyUsed(BrainOrganStation station)
    {
        return station != null && recentStations.Contains(station);
    }

    private void RegisterRecentStation(BrainOrganStation station)
    {
        if (station == null)
            return;

        int maxRecent = Mathf.Max(0, currentSettings.recentOrderBlockCount);

        if (maxRecent == 0)
        {
            recentStations.Clear();
            return;
        }

        recentStations.Add(station);

        while (recentStations.Count > maxRecent)
            recentStations.RemoveAt(0);
    }

    // =========================================================
    // ORDER EVENTS
    // =========================================================

    private void HandleOrderCompleted(BrainOrganStation station)
    {
        if (tutorialActive)
        {
            if (station == activeTutorialStation)
                CompleteTutorial();

            return;
        }

        activeStations.Remove(station);
        Debug.Log(station.gameObject.name + " completed.");
        TryFillAvailableOrderSlots();
    }

    private void HandleOrderFailed(BrainOrganStation station)
    {
        if (tutorialActive)
        {
            // The tutorial order has no timer and should never fail.
            Debug.LogWarning("Tutorial order unexpectedly failed: " +
                             station.gameObject.name);
            return;
        }

        activeStations.Remove(station);
        AddTension(currentSettings.failedOrderTension);

        Debug.Log(station.gameObject.name + " failed.");
        TryFillAvailableOrderSlots();
    }

    private void HandleWrongBrainZone()
    {
        if (tutorialActive)
        {
            if (!tutorialFinishing &&
                tutorialStep == TutorialStep.DeliverProblem)
            {
                ShowTutorialInstruction(tutorialTexts.wrongBrainZone);
            }
            return;
        }

        AddTension(currentSettings.wrongBrainZoneTension);
    }

    private void HandleWrongCommandDelivered()
    {
        if (tutorialActive)
        {
            if (!tutorialFinishing &&
                tutorialStep == TutorialStep.ReturnCommand)
            {
                ShowTutorialInstruction(tutorialTexts.wrongStation);
            }
            return;
        }

        AddTension(currentSettings.wrongBrainZoneTension);
    }

    private void TryFillAvailableOrderSlots()
    {
        if (!roundRunning || isPaused)
            return;

        if (activeStations.Count == 0)
            TryStartRandomOrder();
    }

    // =========================================================
    // PASSIVE TENSION
    // =========================================================

    private void UpdatePassiveTension()
    {
        if (currentSettings == null || activeStations.Count <= 0)
            return;

        float rate = currentSettings.passiveTensionPerActiveOrderPerSecond;

        if (rate <= 0f)
            return;

        float tensionToAdd = rate * activeStations.Count * Time.deltaTime;
        AddTension(tensionToAdd);
    }

    // =========================================================
    // TENSION
    // =========================================================

    private void AddTension(float amount)
    {
        if (!roundRunning || isPaused)
            return;

        nervousTension = Mathf.Clamp(nervousTension + amount, 0f, 100f);
        UpdateTensionUI();

        if (nervousTension >= 100f)
            LoseRound();
    }

    private void UpdateTensionUI()
    {
        if (tensionFillMask != null)
            tensionFillMask.SetFill(nervousTension / 100f);

        if (tensionText != null)
            tensionText.text = Mathf.RoundToInt(nervousTension) + "%";

        UpdateTensionBrain();
    }

    private void UpdateTensionBrain()
    {
        if (tensionBrainRenderer == null)
            return;

        int newState;

        if (nervousTension >= 80f)
        {
            newState = 2;
            if (criticalBrainSprite != null)
                tensionBrainRenderer.sprite = criticalBrainSprite;
        }
        else if (nervousTension >= 50f)
        {
            newState = 1;
            if (stressedBrainSprite != null)
                tensionBrainRenderer.sprite = stressedBrainSprite;
        }
        else
        {
            newState = 0;
            if (calmBrainSprite != null)
                tensionBrainRenderer.sprite = calmBrainSprite;
        }

        if (newState > currentTensionBrainState &&
            tensionStateAnimation != null)
        {
            tensionStateAnimation.Play();
        }

        currentTensionBrainState = newState;
    }

    // =========================================================
    // ROUND TIMER
    // =========================================================

    private void UpdateRoundTimer()
    {
        remainingRoundTime -= Time.deltaTime;

        if (remainingRoundTime <= 0f)
        {
            remainingRoundTime = 0f;
            UpdateRoundTimeUI();
            WinRound();
            return;
        }

        UpdateRoundTimerPulse();
        UpdateRoundTimeUI();
    }

    private void UpdateRoundTimerPulse()
    {
        if (roundTimerPulse == null ||
            remainingRoundTime > roundTimerPulseStartTime)
            return;

        int currentSecond = Mathf.CeilToInt(remainingRoundTime);

        if (currentSecond <= 0 ||
            currentSecond == lastRoundTimerPulseSecond)
            return;

        lastRoundTimerPulseSecond = currentSecond;
        roundTimerPulse.Play();
    }

    private void UpdateRoundTimeUI()
    {
        if (roundTimeText == null)
            return;

        // The tutorial has no time limit, just like the tutorial order.
        if (tutorialActive)
        {
            roundTimeText.text = "∞";
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingRoundTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        roundTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    // =========================================================
    // WIN / LOSE
    // =========================================================

    private void WinRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;
        minigameStarted = false;
        isPaused = false;

        if (BrainScoreManager.Instance != null)
        {
            BrainScoreManager.Instance.SubmitMinigameResult(
                nervousTension,
                100f);

            Debug.Log(
                $"Brain Minigame Score submitted. " +
                $"Nervous Tension: {nervousTension:F1}% | " +
                $"Performance: {(100f - nervousTension):F1}%");
        }
        else
        {
            Debug.LogWarning(
                "BrainScoreManager.Instance липсва. " +
                "Резултатът от Brain Minigame не беше записан.");
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopAllOrders();
        HidePauseWorld();

        if (successObject != null)
            successObject.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("Brain minigame survived successfully.");
    }

    private void LoseRound()
    {
        if (!roundRunning)
            return;

        roundRunning = false;
        minigameStarted = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        StopAllOrders();
        HidePauseWorld();

        if (gameOverObject != null)
            gameOverObject.SetActive(true);

        Debug.Log("Brain minigame lost. Nervous tension reached 100%.");
    }

    private void StopAllOrders()
    {
        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station != null)
                station.CancelOrder();
        }

        activeStations.Clear();
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private DifficultySettings GetCurrentDifficultySettings()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                return easySettings;
            case 2:
                return hardSettings;
            default:
                return mediumSettings;
        }
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        StopTutorialCompletionCoroutine();
        UnsubscribeFromDropZones();

        if (stations == null)
            return;

        foreach (BrainOrganStation station in stations)
        {
            if (station == null)
                continue;

            station.OrderCompleted -= HandleOrderCompleted;
            station.OrderFailed -= HandleOrderFailed;
            station.WrongCommandDelivered -= HandleWrongCommandDelivered;
            station.ProblemPickedUp -= HandleTutorialProblemPickedUp;
        }
    }
}

