using UnityEngine;

public class AccountSystemsActivator : MonoBehaviour
{
    public static AccountSystemsActivator Instance { get; private set; }

    [Header("Account Systems")]
    [SerializeField] private GameObject[] accountSystems;

    private bool systemsActivated = false;

    public bool SystemsActivated => systemsActivated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ActivateSystems()
    {
        if (systemsActivated)
            return;

        if (accountSystems == null)
            return;

        foreach (GameObject system in accountSystems)
        {
            if (system == null)
                continue;

            system.SetActive(true);
        }

        systemsActivated = true;

        Debug.Log(
            "Account systems activated after cloud data load."
        );
    }
}
