using UnityEngine;

public class LiverGoToQuizButton : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private GameObject minigameSuccessPanel;
    [SerializeField] private GameObject liverQuizPanel;
    [SerializeField] private GameObject quizWelcomePanel;

    public void GoToQuiz()
    {
        // Връщаме нормалното време,
        // за да може куизът да работи.
        Time.timeScale = 1f;

        // Спираме ЦЯЛАТА миниигра.
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        // Скриваме success панела на минииграта.
        if (minigameSuccessPanel != null)
        {
            minigameSuccessPanel.SetActive(false);
        }

        // Пускаме куиза.
        if (liverQuizPanel != null)
        {
            liverQuizPanel.SetActive(true);
        }

        // Показваме Welcome панела на куиза.
        if (quizWelcomePanel != null)
        {
            quizWelcomePanel.SetActive(true);
        }
    }
}