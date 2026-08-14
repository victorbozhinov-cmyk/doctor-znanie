using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HeartQuizAnswerButton : MonoBehaviour
{
    [SerializeField] private int answerIndex;
    [SerializeField] private HeartQuizManager quizManager;

    private Button button;
    private QuizAnswerFeedback feedback;

    private void Awake()
    {
        button = GetComponent<Button>();
        feedback = GetComponent<QuizAnswerFeedback>();

        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<HeartQuizManager>();
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (quizManager == null)
        {
            Debug.LogError("HeartQuizManager не е намерен!");
            return;
        }

        quizManager.SelectAnswer(answerIndex, feedback);
    }
}
