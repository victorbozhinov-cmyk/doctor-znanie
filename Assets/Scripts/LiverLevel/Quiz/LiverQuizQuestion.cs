using System;
using UnityEngine;

public enum LiverQuizQuestionType
{
    MultipleChoice,
    QuestionImage,
    ImageAnswers,
    Written
}

[Serializable]
public class LiverQuizQuestion
{
    [Header("Question Type")]
    public LiverQuizQuestionType questionType =
        LiverQuizQuestionType.MultipleChoice;

    [Header("Question")]
    [TextArea(2, 5)]
    public string question;

    [Header("Text Answers")]
    public string answerA;
    public string answerB;
    public string answerV;
    public string answerG;

    [Range(0, 3)]
    public int correctAnswerIndex;

    [Header("Question Image")]
    public Sprite questionImage;

    [Header("Image Answers")]
    public Sprite answerAImage;
    public Sprite answerBImage;
    public Sprite answerVImage;
    public Sprite answerGImage;

    [Header("Written Answer")]
    public string correctWrittenAnswer;

    [Header("Hint")]
    [TextArea(2, 4)]
    public string hint;
}