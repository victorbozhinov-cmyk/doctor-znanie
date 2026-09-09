using UnityEngine;

public class OrganLockVisual : MonoBehaviour
{
    public enum OrganType
    {
        Heart,
        Liver,
        Lungs,
        Stomach,
        Brain
    }

    [Header("Organ")]
    [SerializeField] private OrganType organType;

    [Header("Visual Objects")]
    [SerializeField] private GameObject unlockedObject;
    [SerializeField] private GameObject lockedObject;

    private void Start()
    {
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        bool unlocked = IsUnlocked();

        if (unlockedObject != null)
        {
            unlockedObject.SetActive(unlocked);
        }

        if (lockedObject != null)
        {
            lockedObject.SetActive(!unlocked);
        }
    }

    private bool IsUnlocked()
    {
        if (OrganUnlockManager.Instance == null)
        {
            return false;
        }

        switch (organType)
        {
            case OrganType.Heart:
                return OrganUnlockManager.Instance.IsHeartUnlocked();

            case OrganType.Liver:
                return OrganUnlockManager.Instance.IsLiverUnlocked();

            case OrganType.Lungs:
                return OrganUnlockManager.Instance.IsLungsUnlocked();

            case OrganType.Stomach:
                return OrganUnlockManager.Instance.IsStomachUnlocked();

            case OrganType.Brain:
                return OrganUnlockManager.Instance.IsBrainUnlocked();

            default:
                return false;
        }
    }
}