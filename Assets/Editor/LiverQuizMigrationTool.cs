#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LiverQuizMigrationTool
{
    [MenuItem("Tools/Quiz/Migrate Heart Quiz To Liver")]
    public static void MigrateHeartQuizToLiver()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogError(
                "Първо избери LiverQuizPanel в Hierarchy."
            );
            return;
        }

        HeartQuizManager heartManager =
            root.GetComponentInChildren<HeartQuizManager>(true);

        if (heartManager == null)
        {
            Debug.LogError(
                "Не е намерен HeartQuizManager в избрания обект."
            );
            return;
        }

        // =========================
        // ADD / FIND LIVER MANAGER
        // =========================

        LiverQuizManager liverManager =
            heartManager.GetComponent<LiverQuizManager>();

        if (liverManager == null)
        {
            liverManager =
                Undo.AddComponent<LiverQuizManager>(
                    heartManager.gameObject
                );
        }

        // =========================
        // COPY SERIALIZED FIELDS
        // =========================

        SerializedObject heartSerialized =
            new SerializedObject(heartManager);

        SerializedObject liverSerialized =
            new SerializedObject(liverManager);

        heartSerialized.Update();
        liverSerialized.Update();

        SerializedProperty property =
            heartSerialized.GetIterator();

        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Не копираме самия script.
            if (property.propertyPath == "m_Script")
                continue;

            // Не копираме въпросите за сърцето.
            if (property.propertyPath == "questions")
                continue;

            SerializedProperty liverProperty =
                liverSerialized.FindProperty(
                    property.propertyPath
                );

            if (liverProperty == null)
                continue;

            if (liverProperty.propertyType !=
                property.propertyType)
            {
                continue;
            }

            liverSerialized.CopyFromSerializedProperty(
                property
            );
        }

        liverSerialized.ApplyModifiedProperties();

        // =========================
        // REPLACE REFERENCES
        // =========================

        int replacedReferences = 0;

        Component[] components =
            root.GetComponentsInChildren<Component>(true);

        foreach (Component component in components)
        {
            if (component == null)
                continue;

            // Не променяме стария manager.
            if (component == heartManager)
                continue;

            SerializedObject serializedComponent =
                new SerializedObject(component);

            serializedComponent.Update();

            SerializedProperty iterator =
                serializedComponent.GetIterator();

            bool changed = false;

            while (iterator.Next(true))
            {
                if (iterator.propertyType !=
                    SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                if (iterator.objectReferenceValue ==
                    heartManager)
                {
                    iterator.objectReferenceValue =
                        liverManager;

                    replacedReferences++;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedComponent
                    .ApplyModifiedProperties();

                EditorUtility.SetDirty(component);
            }
        }

        // =========================
        // DISABLE OLD MANAGER
        // =========================

        heartManager.enabled = false;

        EditorUtility.SetDirty(heartManager);
        EditorUtility.SetDirty(liverManager);

        EditorSceneManager.MarkSceneDirty(
            root.scene
        );

        Selection.activeGameObject =
            liverManager.gameObject;

        Debug.Log(
            "Liver Quiz migration приключи успешно! " +
            "Прехвърлени references: " +
            replacedReferences +
            ". HeartQuizManager е изключен."
        );
    }
}

#endif