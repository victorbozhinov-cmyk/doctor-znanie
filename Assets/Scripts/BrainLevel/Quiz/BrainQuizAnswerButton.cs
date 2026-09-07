using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BrainQuizAnswerButton : MonoBehaviour
{
    [SerializeField] private int answerIndex;
    [SerializeField] private BrainQuizManager quizManager;

    private Button button;
    private QuizAnswerFeedback feedback;

    private void Awake()
    {
        button = GetComponent<Button>();
        feedback = GetComponent<QuizAnswerFeedback>();

        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<BrainQuizManager>();
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (quizManager == null)
        {
            Debug.LogError("BrainQuizManager не е намерен!");
            return;
        }

        quizManager.SelectAnswer(answerIndex, feedback);
    }
}