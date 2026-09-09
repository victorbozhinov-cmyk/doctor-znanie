using UnityEngine;

public class VitaminManager : MonoBehaviour
{
    public static VitaminManager Instance { get; private set; }

    private const string VitaminsKey = "Vitamins";

    [Header("Starting Vitamins")]
    [SerializeField] private int startingVitamins = 150;

    public int CurrentVitamins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVitamins();
    }

    private void LoadVitamins()
    {
        if (PlayerPrefs.HasKey(VitaminsKey))
        {
            CurrentVitamins = PlayerPrefs.GetInt(VitaminsKey);
        }
        else
        {
            CurrentVitamins = startingVitamins;
            SaveVitamins();
        }
    }

    public void AddVitamins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentVitamins += amount;
        SaveVitamins();
    }

    public bool CanAfford(int amount)
    {
        return CurrentVitamins >= amount;
    }

    public bool SpendVitamins(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (!CanAfford(amount))
        {
            return false;
        }

        CurrentVitamins -= amount;
        SaveVitamins();

        return true;
    }

    private void SaveVitamins()
    {
        PlayerPrefs.SetInt(VitaminsKey, CurrentVitamins);
        PlayerPrefs.Save();
    }

    public void ResetVitamins()
    {
        CurrentVitamins = startingVitamins;
        SaveVitamins();
    }
}
