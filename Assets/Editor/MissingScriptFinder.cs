using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptFinder
{
    [MenuItem("Tools/Find Missing Scripts In Open Scene")]
    private static void FindMissingScripts()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        int missingScriptCount = 0;
        GameObject firstProblemObject = null;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            ScanObject(
                rootObject.transform,
                ref missingScriptCount,
                ref firstProblemObject
            );
        }

        if (missingScriptCount == 0)
        {
            Debug.Log(
                "Не са открити липсващи скриптове " +
                "в отворената сцена."
            );

            return;
        }

        Debug.LogWarning(
            "Общо открити липсващи скриптове: " +
            missingScriptCount
        );

        if (firstProblemObject != null)
        {
            Selection.activeGameObject = firstProblemObject;
            EditorGUIUtility.PingObject(firstProblemObject);
        }
    }

    [MenuItem("Tools/Remove Missing Scripts From Selected Object")]
    private static void RemoveFromSelectedObject()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning(
                "Първо избери проблемния GameObject в Hierarchy."
            );

            return;
        }

        Undo.RegisterCompleteObjectUndo(
            selectedObject,
            "Remove Missing Scripts"
        );

        int removedCount =
            GameObjectUtility
                .RemoveMonoBehavioursWithMissingScript(
                    selectedObject
                );

        if (removedCount == 0)
        {
            Debug.Log(
                "Върху избрания обект няма липсващи скриптове.",
                selectedObject
            );

            return;
        }

        EditorUtility.SetDirty(selectedObject);
        EditorSceneManager.MarkSceneDirty(
            selectedObject.scene
        );

        Debug.Log(
            "Премахнати липсващи скриптове: " +
            removedCount +
            " от обекта " +
            selectedObject.name,
            selectedObject
        );
    }

    private static void ScanObject(
        Transform currentTransform,
        ref int missingScriptCount,
        ref GameObject firstProblemObject
    )
    {
        GameObject currentObject =
            currentTransform.gameObject;

        int missingOnCurrentObject =
            GameObjectUtility
                .GetMonoBehavioursWithMissingScriptCount(
                    currentObject
                );

        if (missingOnCurrentObject > 0)
        {
            missingScriptCount += missingOnCurrentObject;

            if (firstProblemObject == null)
            {
                firstProblemObject = currentObject;
            }

            Debug.LogWarning(
                "Липсващ скрипт върху: " +
                GetFullPath(currentTransform),
                currentObject
            );
        }

        foreach (Transform child in currentTransform)
        {
            ScanObject(
                child,
                ref missingScriptCount,
                ref firstProblemObject
            );
        }
    }

    private static string GetFullPath(
        Transform currentTransform
    )
    {
        string path = currentTransform.name;

        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;

            path =
                currentTransform.name +
                "/" +
                path;
        }

        return path;
    }
}