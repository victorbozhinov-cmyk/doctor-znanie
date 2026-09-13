using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OrganPurchaseController : MonoBehaviour
{
    public enum OrganType
    {
        Heart,
        Liver,
        Lungs,
        Stomach,
        Brain
    }

    // =========================
    // ORGAN
    // =========================

    [Header("Organ")]
    [SerializeField] private OrganType organType;

    // =========================
    // PRICE
    // =========================

    [Header("Price")]
    [SerializeField] private int price = 100;

    // =========================
    // UI
    // =========================

    [Header("UI")]
    [SerializeField] private GameObject purchasePanel;
    [SerializeField] private UIPopupCloseAnimation purchasePanelCloseAnimation;

    // =========================
    // NOT ENOUGH VITAMINS
    // =========================

    [Header("Not Enough Vitamins")]
    [SerializeField] private UIShakeAnimation purchasePanelShake;

    // =========================
    // VISUAL
    // =========================

    [Header("Visual")]
    [SerializeField] private OrganLockVisual organLockVisual;
    [SerializeField] private OrganUnlockAnimation organUnlockAnimation;

    // =========================
    // AUDIO
    // =========================

    [Header("Audio")]
    [SerializeField] private AudioClip unlockOrganSfx;
    [SerializeField] private AudioClip lockedOrganTrySfx;

    [Header("Audio Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float unlockOrganVolume = 0.85f;

    [Range(0f, 1f)]
    [SerializeField] private float lockedOrganTryVolume = 0.75f;

    // =========================
    // RUNTIME
    // =========================

    private bool isClosingPanel;
    private bool pointerOnOrgan;

    private AudioSource sfxAudioSource;

    private static OrganPurchaseController activePurchaseController;
    private static OrganPurchaseController pendingPurchaseController;

    // =========================
    // UNITY
    // =========================

    private void Awake()
    {
        sfxAudioSource = GetComponent<AudioSource>();

        if (sfxAudioSource != null)
        {
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            sfxAudioSource.spatialBlend = 0f;
        }
    }

    // =========================
    // HOVER STATE
    // =========================

    public void SetPointerOnOrgan(bool isInside)
    {
        pointerOnOrgan = isInside;

        // Ако сме напуснали органа, докато той чака
        // да се отвори, махаме го от pending.
        if (!pointerOnOrgan &&
            pendingPurchaseController == this)
        {
            pendingPurchaseController = null;
        }
    }

    // =========================
    // OPEN PANEL
    // =========================

    public void OpenPanel()
    {
        if (IsUnlocked())
        {
            return;
        }

        if (purchasePanel == null)
        {
            return;
        }

        if (isClosingPanel)
        {
            return;
        }

        // Има друг отворен/затварящ се панел.
        // Запомняме този орган и ще го отворим
        // веднага след като старият приключи.
        if (activePurchaseController != null &&
            activePurchaseController != this)
        {
            pendingPurchaseController = this;
            return;
        }

        purchasePanel.SetActive(true);

        activePurchaseController = this;

        if (pendingPurchaseController == this)
        {
            pendingPurchaseController = null;
        }
    }

    // =========================
    // CLOSE PANEL
    // =========================

    public void ClosePanel()
    {
        if (purchasePanel == null)
        {
            ReleaseActiveController();
            return;
        }

        if (!purchasePanel.activeSelf)
        {
            ReleaseActiveController();
            return;
        }

        if (isClosingPanel)
        {
            return;
        }

        if (purchasePanelCloseAnimation != null)
        {
            isClosingPanel = true;

            purchasePanelCloseAnimation.PlayClose(() =>
            {
                purchasePanel.SetActive(false);

                isClosingPanel = false;

                ReleaseActiveController();
            });
        }
        else
        {
            purchasePanel.SetActive(false);

            ReleaseActiveController();
        }
    }

    // =========================
    // PURCHASE
    // =========================

    public void TryUnlock()
    {
        if (IsUnlocked())
        {
            ClosePanel();
            return;
        }

        if (VitaminManager.Instance == null)
        {
            Debug.LogWarning(
                "VitaminManager не е намерен."
            );

            return;
        }

        // =============================================
        // НЯМА ДОСТАТЪЧНО ВИТАМИНИ
        // =============================================

        if (!VitaminManager.Instance.CanAfford(price))
        {
            Debug.Log(
                "Няма достатъчно витамини."
            );

            PlayLockedOrganTrySound();

            if (purchasePanelShake != null)
            {
                purchasePanelShake.Shake();
            }

            return;
        }

        // =============================================
        // ПЛАЩАНЕ
        // =============================================

        if (!VitaminManager.Instance.SpendVitamins(price))
        {
            Debug.LogWarning(
                "Витамините не могат да бъдат похарчени."
            );

            return;
        }

        // =============================================
        // UNLOCK
        // =============================================

        UnlockOrgan();

        PlayUnlockOrganSound();

        // Ако този орган е чакал като pending,
        // вече няма нужда да бъде отварян.
        if (pendingPurchaseController == this)
        {
            pendingPurchaseController = null;
        }

        ClosePanel();

        if (organUnlockAnimation != null)
        {
            organUnlockAnimation.PlayUnlockAnimation();
        }
        else if (organLockVisual != null)
        {
            organLockVisual.RefreshVisual();
        }
    }

    // =========================
    // AUDIO
    // =========================

    private void PlayUnlockOrganSound()
    {
        if (sfxAudioSource == null ||
            unlockOrganSfx == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(
            unlockOrganSfx,
            unlockOrganVolume
        );
    }

    private void PlayLockedOrganTrySound()
    {
        if (sfxAudioSource == null ||
            lockedOrganTrySfx == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(
            lockedOrganTrySfx,
            lockedOrganTryVolume
        );
    }

    // =========================
    // ENTER LEVEL
    // =========================

    public bool CanEnterLevel()
    {
        return IsUnlocked();
    }

    // =========================
    // ACTIVE / PENDING PANEL
    // =========================

    private void ReleaseActiveController()
    {
        if (activePurchaseController != this)
        {
            return;
        }

        activePurchaseController = null;

        TryOpenPendingPanel();
    }

    private static void TryOpenPendingPanel()
    {
        if (pendingPurchaseController == null)
        {
            return;
        }

        OrganPurchaseController nextController =
            pendingPurchaseController;

        pendingPurchaseController = null;

        // Отваряме го само ако курсорът все още
        // е върху съответния орган.
        if (!nextController.pointerOnOrgan)
        {
            return;
        }

        if (nextController.IsUnlocked())
        {
            return;
        }

        nextController.OpenPanel();
    }

    // =========================
    // UNLOCK CHECK
    // =========================

    private bool IsUnlocked()
    {
        if (OrganUnlockManager.Instance == null)
        {
            return false;
        }

        switch (organType)
        {
            case OrganType.Heart:
                return OrganUnlockManager.Instance.IsHeartUnlocked();

            case OrganType.Liver:
                return OrganUnlockManager.Instance.IsLiverUnlocked();

            case OrganType.Lungs:
                return OrganUnlockManager.Instance.IsLungsUnlocked();

            case OrganType.Stomach:
                return OrganUnlockManager.Instance.IsStomachUnlocked();

            case OrganType.Brain:
                return OrganUnlockManager.Instance.IsBrainUnlocked();

            default:
                return false;
        }
    }

    // =========================
    // UNLOCK ORGAN
    // =========================

    private void UnlockOrgan()
    {
        if (OrganUnlockManager.Instance == null)
        {
            return;
        }

        switch (organType)
        {
            case OrganType.Heart:
                OrganUnlockManager.Instance.UnlockHeart();
                break;

            case OrganType.Liver:
                OrganUnlockManager.Instance.UnlockLiver();
                break;

            case OrganType.Lungs:
                OrganUnlockManager.Instance.UnlockLungs();
                break;

            case OrganType.Stomach:
                OrganUnlockManager.Instance.UnlockStomach();
                break;

            case OrganType.Brain:
                OrganUnlockManager.Instance.UnlockBrain();
                break;
        }
    }
}