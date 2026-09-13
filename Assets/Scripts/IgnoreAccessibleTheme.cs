using UnityEngine;

public class IgnoreAccessibleTheme : MonoBehaviour
{
    [Header("Ignore Settings")]

    [Tooltip(
        "Ако е включено, този обект и всички негови деца " +
        "няма да бъдат променяни от достъпния цветен режим."
    )]
    [SerializeField]
    private bool ignoreChildren = true;

    // =========================================================
    // PUBLIC
    // =========================================================

    public bool IgnoreChildren
    {
        get
        {
            return ignoreChildren;
        }
    }
}