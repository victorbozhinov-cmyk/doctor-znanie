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

    private GameObject currentPermanentCard;
    private Coroutine irritatedCoroutine;

    private void Start()
    {
        HideAllCards();
        ShowStartingFoodCard();
    }

    private void ShowStartingFoodCard()
    {
        // Difficulty:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficulty = PlayerPrefs.GetInt(
            "Difficulty",
            1
        );

        switch (difficulty)
        {
            case 0:
                ShowLightFood();
                Debug.Log(
                    "Stomach difficulty: EASY → LightFoodCard"
                );
                break;

            case 2:
                ShowHeavyFood();
                Debug.Log(
                    "Stomach difficulty: HARD → HeavyFoodCard"
                );
                break;

            default:
                ShowMediumFood();
                Debug.Log(
                    "Stomach difficulty: MEDIUM → MediumFoodCard"
                );
                break;
        }
    }

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

    private void ShowPermanentCard(GameObject card)
    {
        if (card == null)
            return;

        if (irritatedCoroutine != null)
        {
            StopCoroutine(irritatedCoroutine);
            irritatedCoroutine = null;
        }

        HideAllCards();

        card.SetActive(true);

        currentPermanentCard = card;
    }

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
        ShowPermanentCard(readyForIntestineCard);
    }

    public void ShowIrritated()
    {
        if (currentPermanentCard == null)
            return;

        if (irritatedCoroutine != null)
        {
            StopCoroutine(irritatedCoroutine);
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
        }

        irritatedCoroutine = null;
    }

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