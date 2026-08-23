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
        // Ако двата скрипта са върху един и същи обект,
        // намираме анимацията автоматично.
        if (statePanelAnimation == null)
        {
            statePanelAnimation =
                GetComponent<StomachStatePanelAnimation>();
        }
    }

    private void Start()
    {
        HideAllCards();
        ShowStartingFoodCard();
    }

    // =========================================================
    // STARTING CARD
    // =========================================================

    private void ShowStartingFoodCard()
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

        // Проверяваме дали действително ще има
        // визуална смяна на карта.
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

        // Анимация на самия стомах.
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

        // ---------------------------------------------
        // IRRITATED CARD
        // ---------------------------------------------

        HideAllCards();

        if (irritatedCard != null)
        {
            irritatedCard.SetActive(true);

            PlayPanelChangeAnimation();
        }

        yield return new WaitForSeconds(
            irritatedDuration
        );

        // ---------------------------------------------
        // RETURN TO PREVIOUS CARD
        // ---------------------------------------------

        SetCardActive(
            irritatedCard,
            false
        );

        if (previousCard != null)
        {
            previousCard.SetActive(true);

            currentPermanentCard =
                previousCard;

            // Подскача отново при връщането
            // към предишната карта.
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