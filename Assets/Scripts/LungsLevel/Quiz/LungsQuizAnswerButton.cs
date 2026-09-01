using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LungsQuizAnswerButton : MonoBehaviour
{
    [SerializeField] private int answerIndex;
    [SerializeField] private LungsQuizManager quizManager;

    private Button button;
    private QuizAnswerFeedback feedback;

    private void Awake()
    {
        button = GetComponent<Button>();
        feedback = GetComponent<QuizAnswerFeedback>();

        if (quizManager == null)
        {
            quizManager =
                FindFirstObjectByType<LungsQuizManager>();
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (quizManager == null)
        {
            Debug.LogError(
                "LungsQuizManager не е намерен!"
            );

            return;
        }

        quizManager.SelectAnswer(
            answerIndex,
            feedback
        );
    }
}