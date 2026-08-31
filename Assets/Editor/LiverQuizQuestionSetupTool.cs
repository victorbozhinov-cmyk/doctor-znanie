#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LiverQuizQuestionSetupTool
{
    [MenuItem("Tools/Quiz/Fill Liver Quiz Questions")]
    public static void FillLiverQuizQuestions()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError(
                "Първо избери GameObject-а LiverQuizManager в Hierarchy."
            );
            return;
        }

        LiverQuizManager manager =
            selectedObject.GetComponent<LiverQuizManager>();

        if (manager == null)
        {
            Debug.LogError(
                "Избраният GameObject няма LiverQuizManager компонент."
            );
            return;
        }

        Undo.RecordObject(
            manager,
            "Fill Liver Quiz Questions"
        );

        SerializedObject serializedManager =
            new SerializedObject(manager);

        serializedManager.Update();

        SerializedProperty questions =
            serializedManager.FindProperty("questions");

        if (questions == null)
        {
            Debug.LogError(
                "Не беше намерено полето 'questions' в LiverQuizManager."
            );
            return;
        }

        questions.arraySize = 23;

        // =====================================================
        // 1–10 MULTIPLE CHOICE
        // =====================================================

        SetQuestion(
            questions,
            0,
            LiverQuizQuestionType.MultipleChoice,
            "Каква течност произвежда черният дроб, която подпомага храносмилането на мазнините?",
            "Слюнка",
            "Стомашен сок",
            "Жлъчка",
            "Кръв",
            2
        );

        SetQuestion(
            questions,
            1,
            LiverQuizQuestionType.MultipleChoice,
            "Под каква форма черният дроб съхранява запас от въглехидрати?",
            "Като кислород",
            "Като гликоген",
            "Като жлъчка",
            "Като вода",
            1
        );

        SetQuestion(
            questions,
            2,
            LiverQuizQuestionType.MultipleChoice,
            "Кои хранителни вещества черният дроб участва в преработването?",
            "Само витамини",
            "Мазнини и въглехидрати",
            "Само вода",
            "Само минерали",
            1
        );

        SetQuestion(
            questions,
            3,
            LiverQuizQuestionType.MultipleChoice,
            "Кое от следните може да се съхранява в черния дроб?",
            "Кислород",
            "Витамини",
            "Слюнка",
            "Урина",
            1
        );

        SetQuestion(
            questions,
            4,
            LiverQuizQuestionType.MultipleChoice,
            "Кое вредно вещество черният дроб помага да бъде обезвредено?",
            "Вода",
            "Кислород",
            "Алкохол",
            "Калций",
            2
        );

        SetQuestion(
            questions,
            5,
            LiverQuizQuestionType.MultipleChoice,
            "Какво прави черният дроб с някои лекарства?",
            "Произвежда ги",
            "Преработва ги",
            "Съхранява ги завинаги",
            "Превръща ги в кислород",
            1
        );

        SetQuestion(
            questions,
            6,
            LiverQuizQuestionType.MultipleChoice,
            "Кои вещества може да произвежда черният дроб освен жлъчка?",
            "Кости",
            "Белтъци",
            "Кислород",
            "Слюнка",
            1
        );

        SetQuestion(
            questions,
            7,
            LiverQuizQuestionType.MultipleChoice,
            "Къде се намира черният дроб?",
            "В областта на шията",
            "В таза",
            "В горната дясна част на коремната кухина",
            "В главата",
            2
        );

        SetQuestion(
            questions,
            8,
            LiverQuizQuestionType.MultipleChoice,
            "Как се нарича заболяването, при което черният дроб се възпалява?",
            "Бронхит",
            "Гастрит",
            "Хепатит",
            "Кариес",
            2
        );

        SetQuestion(
            questions,
            9,
            LiverQuizQuestionType.MultipleChoice,
            "Кое от следните НЕ е функция на черния дроб?",
            "Съхранява гликоген",
            "Преработва мазнини и въглехидрати",
            "Произвежда жлъчка",
            "Изпомпва кръвта в тялото",
            3
        );

        // =====================================================
        // 11–14 QUESTION IMAGE
        // =====================================================

        SetQuestion(
            questions,
            10,
            LiverQuizQuestionType.QuestionImage,
            "Кой орган е показан на изображението?",
            "Стомах",
            "Сърце",
            "Черен дроб",
            "Бял дроб",
            2
        );

        SetQuestion(
            questions,
            11,
            LiverQuizQuestionType.QuestionImage,
            "Как се нарича малкият орган, показан под черния дроб?",
            "Стомах",
            "Бъбрек",
            "Жлъчен мехур",
            "Панкреас",
            2
        );

        SetQuestion(
            questions,
            12,
            LiverQuizQuestionType.QuestionImage,
            "Как влияе показаният навик върху здравето на черния дроб?",
            "Уврежда черния дроб",
            "Подпомага поддържането му здрав",
            "Спира работата му",
            "Няма никакво значение за организма",
            1
        );

        SetQuestion(
            questions,
            13,
            LiverQuizQuestionType.QuestionImage,
            "Коя от следните дейности е функция на показания орган?",
            "Изпомпва кръвта",
            "Поема кислород от въздуха",
            "Преработва мазнини и въглехидрати",
            "Образува урина",
            2
        );

        // =====================================================
        // 15–18 IMAGE ANSWERS
        // =====================================================

        SetQuestion(
            questions,
            14,
            LiverQuizQuestionType.ImageAnswers,
            "Кое изображение показва черния дроб?",
            "",
            "",
            "",
            "",
            2
        );

        SetQuestion(
            questions,
            15,
            LiverQuizQuestionType.ImageAnswers,
            "Кое от показаните може да навреди на черния дроб?",
            "",
            "",
            "",
            "",
            3
        );

        SetQuestion(
            questions,
            16,
            LiverQuizQuestionType.ImageAnswers,
            "Кое изображение показва храна, богата на въглехидрати?",
            "",
            "",
            "",
            "",
            0
        );

        SetQuestion(
            questions,
            17,
            LiverQuizQuestionType.ImageAnswers,
            "Кое изображение показва полезен навик за здравето на черния дроб?",
            "",
            "",
            "",
            "",
            2
        );

        // =====================================================
        // 19–23 WRITTEN
        // =====================================================

        SetWrittenQuestion(
            questions,
            18,
            "Как се нарича веществото, под формата на което черният дроб съхранява запас от въглехидрати?",
            "гликоген"
        );

        SetWrittenQuestion(
            questions,
            19,
            "Как се нарича заболяването, при което черният дроб се възпалява?",
            "хепатит"
        );

        SetWrittenQuestion(
            questions,
            20,
            "Как се нарича процесът, при който черният дроб помага за премахването на вредното действие на вещества като алкохола?",
            "обезвреждане"
        );

        SetWrittenQuestion(
            questions,
            21,
            "Какъв вид вещества, освен жлъчката, може да произвежда черният дроб?",
            "белтъци"
        );

        SetWrittenQuestion(
            questions,
            22,
            "Как се нарича течността, която се произвежда от черния дроб и подпомага обработването на мазнините?",
            "жлъчка"
        );

        serializedManager.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);

        EditorSceneManager.MarkSceneDirty(
            manager.gameObject.scene
        );

        Debug.Log(
            "Готово! Добавени са 23 въпроса към Liver Quiz."
        );
    }

    private static void SetQuestion(
        SerializedProperty questions,
        int index,
        LiverQuizQuestionType type,
        string question,
        string answerA,
        string answerB,
        string answerV,
        string answerG,
        int correctAnswerIndex)
    {
        SerializedProperty item =
            questions.GetArrayElementAtIndex(index);

        item.FindPropertyRelative("questionType").enumValueIndex =
            (int)type;

        item.FindPropertyRelative("question").stringValue =
            question;

        item.FindPropertyRelative("answerA").stringValue =
            answerA;

        item.FindPropertyRelative("answerB").stringValue =
            answerB;

        item.FindPropertyRelative("answerV").stringValue =
            answerV;

        item.FindPropertyRelative("answerG").stringValue =
            answerG;

        item.FindPropertyRelative("correctAnswerIndex").intValue =
            correctAnswerIndex;

        item.FindPropertyRelative("correctWrittenAnswer").stringValue =
            "";
    }

    private static void SetWrittenQuestion(
        SerializedProperty questions,
        int index,
        string question,
        string correctWrittenAnswer)
    {
        SerializedProperty item =
            questions.GetArrayElementAtIndex(index);

        item.FindPropertyRelative("questionType").enumValueIndex =
            (int)LiverQuizQuestionType.Written;

        item.FindPropertyRelative("question").stringValue =
            question;

        item.FindPropertyRelative("answerA").stringValue = "";
        item.FindPropertyRelative("answerB").stringValue = "";
        item.FindPropertyRelative("answerV").stringValue = "";
        item.FindPropertyRelative("answerG").stringValue = "";

        item.FindPropertyRelative("correctAnswerIndex").intValue = 0;

        item.FindPropertyRelative("correctWrittenAnswer").stringValue =
            correctWrittenAnswer;
    }
}

#endif