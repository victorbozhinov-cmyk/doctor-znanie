using System.Collections;
using UnityEngine;

public class StomachStateCardController : MonoBehaviour
{
    [Header("Food Cards")]
    [SerializeField] private GameObject lightFoodCard;
    [SerializeField] private GameObject mediumFoodCard;
    [SerializeField] private GameObject heavyFoodCard;

    [Header("Digestion State Cards")]
    [SerializeField] private GameObject juicesMixedCard;
    [SerializeField] private GameObject breakingDownCard;
    [SerializeField] private GameObject chymeCard;
    [SerializeField] private GameObject readyForIntestineCard;

    [Header("Temporary State")]
    [SerializeField] private GameObject irritatedCard;
    [SerializeField] private float irritatedDuration = 2f;

    [Header("Panel Animation")]
    [SerializeField] private StomachStatePanelAnimation statePanelAnimation;

    [Header("Irritation Animation")]
    [SerializeField] private StomachIrritationAnimation irritationAnimation;

    private GameObject currentPermanentCard;
    private Coroutine irritatedCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (statePanelAnimation == null)
        {
            statePanelAnimation =
                GetComponent<StomachStatePanelAnimation>();
        }
    }

    private void Start()
    {
        // ВАЖНО:
        // Началната карта вече НЕ се показва автоматично.
        // Тя ще се покаже чак след затваряне
        // на WelcomePanel.
        HideAllCards();

        currentPermanentCard = null;
    }

    // =========================================================
    // STARTING CARD
    // =========================================================

    public void ShowStartingFoodCard()
    {
        int difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        switch (difficulty)
        {
            case 0:
                ShowLightFood();
                break;

            case 2:
                ShowHeavyFood();
                break;

            default:
                ShowMediumFood();
                break;
        }
    }

    // =========================================================
    // CARD VISIBILITY
    // =========================================================

    private void HideAllCards()
    {
        SetCardActive(lightFoodCard, false);
        SetCardActive(mediumFoodCard, false);
        SetCardActive(heavyFoodCard, false);

        SetCardActive(juicesMixedCard, false);
        SetCardActive(breakingDownCard, false);
        SetCardActive(chymeCard, false);
        SetCardActive(readyForIntestineCard, false);

        SetCardActive(irritatedCard, false);
    }

    private void ShowPermanentCard(
        GameObject card)
    {
        if (card == null)
            return;

        bool cardActuallyChanged =
            currentPermanentCard != card ||
            !card.activeSelf;

        if (irritatedCoroutine != null)
        {
            StopCoroutine(
                irritatedCoroutine
            );

            irritatedCoroutine = null;
        }

        HideAllCards();

        card.SetActive(true);

        currentPermanentCard = card;

        if (cardActuallyChanged)
        {
            PlayPanelChangeAnimation();
        }
    }

    // =========================================================
    // PERMANENT STATES
    // =========================================================

    public void ShowLightFood()
    {
        ShowPermanentCard(lightFoodCard);
    }

    public void ShowMediumFood()
    {
        ShowPermanentCard(mediumFoodCard);
    }

    public void ShowHeavyFood()
    {
        ShowPermanentCard(heavyFoodCard);
    }

    public void ShowJuicesMixed()
    {
        ShowPermanentCard(juicesMixedCard);
    }

    public void ShowBreakingDown()
    {
        ShowPermanentCard(breakingDownCard);
    }

    public void ShowChyme()
    {
        ShowPermanentCard(chymeCard);
    }

    public void ShowReadyForIntestine()
    {
        ShowPermanentCard(
            readyForIntestineCard
        );
    }

    // =========================================================
    // IRRITATED
    // =========================================================

    public void ShowIrritated()
    {
        if (currentPermanentCard == null)
            return;

        if (irritationAnimation != null)
        {
            irritationAnimation
                .PlayIrritation();
        }

        if (irritatedCoroutine != null)
        {
            StopCoroutine(
                irritatedCoroutine
            );
        }

        irritatedCoroutine =
            StartCoroutine(
                ShowIrritatedRoutine()
            );
    }

    private IEnumerator ShowIrritatedRoutine()
    {
        GameObject previousCard =
            currentPermanentCard;

        HideAllCards();

        if (irritatedCard != null)
        {
            irritatedCard.SetActive(true);

            PlayPanelChangeAnimation();
        }

        yield return new WaitForSeconds(
            irritatedDuration
        );

        SetCardActive(
            irritatedCard,
            false
        );

        if (previousCard != null)
        {
            previousCard.SetActive(true);

            currentPermanentCard =
                previousCard;

            PlayPanelChangeAnimation();
        }

        irritatedCoroutine = null;
    }

    // =========================================================
    // PANEL ANIMATION
    // =========================================================

    private void PlayPanelChangeAnimation()
    {
        if (statePanelAnimation != null)
        {
            statePanelAnimation
                .PlayCardChange();
        }
    }

    // =========================================================
    // HELPER
    // =========================================================

    private void SetCardActive(
        GameObject card,
        bool active)
    {
        if (card != null)
        {
            card.SetActive(active);
        }
    }
}