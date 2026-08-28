using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StomachQuizAnswerButton : MonoBehaviour
{
    [SerializeField] private int answerIndex;
    [SerializeField] private StomachQuizManager quizManager;

    private Button button;
    private QuizAnswerFeedback feedback;

    private void Awake()
    {
        button = GetComponent<Button>();
        feedback = GetComponent<QuizAnswerFeedback>();

        if (quizManager == null)
        {
            quizManager =
                FindFirstObjectByType<StomachQuizManager>();
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (quizManager == null)
        {
            Debug.LogError(
                "StomachQuizManager не е намерен!"
            );

            return;
        }

        quizManager.SelectAnswer(
            answerIndex,
            feedback
        );
    }
}