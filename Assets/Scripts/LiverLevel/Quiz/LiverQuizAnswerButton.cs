using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LiverQuizAnswerButton : MonoBehaviour
{
    [SerializeField] private int answerIndex;
    [SerializeField] private LiverQuizManager quizManager;

    private Button button;
    private QuizAnswerFeedback feedback;

    private void Awake()
    {
        button = GetComponent<Button>();
        feedback = GetComponent<QuizAnswerFeedback>();

        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<LiverQuizManager>();
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (quizManager == null)
        {
            Debug.LogError("LiverQuizManager не е намерен!");
            return;
        }

        quizManager.SelectAnswer(answerIndex, feedback);
    }
}