using System;
using UnityEngine;

[Serializable]
public class HeartQuizQuestion
{
    [TextArea(2, 5)]
    public string question;

    public string answerA;
    public string answerB;
    public string answerV;
    public string answerG;

    [Range(0, 3)]
    public int correctAnswerIndex;

    [TextArea(2, 4)]
    public string hint;
}
