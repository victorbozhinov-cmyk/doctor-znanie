#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LiverQuizHintSetupTool
{
    [MenuItem("Tools/Quiz/Fill Liver Quiz Hints")]
    public static void FillLiverQuizHints()
    {
        GameObject selectedObject =
            Selection.activeGameObject;

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
            "Fill Liver Quiz Hints"
        );

        SerializedObject serializedManager =
            new SerializedObject(manager);

        serializedManager.Update();

        SerializedProperty questions =
            serializedManager.FindProperty("questions");

        if (questions == null)
        {
            Debug.LogError(
                "Не беше намерено полето questions."
            );
            return;
        }

        if (questions.arraySize < 23)
        {
            Debug.LogError(
                "В LiverQuizManager няма 23 въпроса."
            );
            return;
        }

        string[] hints =
        {
            "Помисли коя течност свързваме с работата на черния дроб, а не със стомаха или слюнчените жлези.",

            "Организмът може да запази част от глюкозата като енергиен резерв за по-късно.",

            "Черният дроб участва в обработването на два важни вида хранителни вещества, свързани с енергията.",

            "Черният дроб може да пази запас от някои полезни вещества, от които организмът се нуждае в малки количества.",

            "Помисли за вещество, което при прекомерна употреба натоварва силно черния дроб.",

            "Черният дроб помага на организма да обработи някои вещества, които приемаме при лечение.",

            "Тези вещества имат важна роля за изграждането и работата на организма.",

            "Този орган се намира под ребрата, близо до стомаха.",

            "От изброените заболявания избери това, което засяга черния дроб, а не стомаха, бронхите или зъбите.",

            "Една от изброените дейности принадлежи на сърцето.",

            "Това е голям орган в горната част на коремната кухина, който преработва и съхранява различни вещества.",

            "Погледни малкото зелено мехурче непосредствено под черния дроб.",

            "Помисли дали редовното движение е полезен или вреден навик за организма.",

            "Този орган има важна роля в обработването на хранителните вещества след хранене.",

            "Търси голям червено-кафяв орган с характерна широка форма.",

            "Един от изборите може силно да натовари черния дроб при прекомерна употреба.",

            "Помисли коя от храните обикновено се приготвя от брашно.",

            "Помисли кой от изборите подпомага организма в дългосрочен план, вместо да го натоварва.",

            "Това е веществото, в което черният дроб превръща част от глюкозата, за да я запази като енергиен резерв.",

            "То не е заболяване на стомаха, бронхите или зъбите — свързано е конкретно с черния дроб.",

            "Това е процесът, чрез който вредното действие на дадено вещество се намалява или премахва.",

            "Тези вещества са важни за изграждането и правилната работа на организма.",

            "Тази течност не се произвежда в стомаха, въпреки че участва в храносмилането."
        };

        for (int i = 0; i < hints.Length; i++)
        {
            SerializedProperty question =
                questions.GetArrayElementAtIndex(i);

            SerializedProperty hint =
                question.FindPropertyRelative("hint");

            if (hint != null)
            {
                hint.stringValue = hints[i];
            }
        }

        serializedManager.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);

        EditorSceneManager.MarkSceneDirty(
            manager.gameObject.scene
        );

        Debug.Log(
            "Готово! Добавени са 23 хинта към Liver Quiz."
        );
    }
}

#endif