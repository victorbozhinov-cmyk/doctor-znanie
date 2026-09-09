using UnityEngine;

public class OrganUnlockManager : MonoBehaviour
{
    public static OrganUnlockManager Instance { get; private set; }

    private const string HeartUnlockedKey = "HeartUnlocked";
    private const string LiverUnlockedKey = "LiverUnlocked";
    private const string LungsUnlockedKey = "LungsUnlocked";
    private const string StomachUnlockedKey = "StomachUnlocked";
    private const string BrainUnlockedKey = "BrainUnlocked";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsHeartUnlocked()
    {
        return PlayerPrefs.GetInt(HeartUnlockedKey, 0) == 1;
    }

    public void UnlockHeart()
    {
        PlayerPrefs.SetInt(HeartUnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public bool IsLiverUnlocked()
    {
        return PlayerPrefs.GetInt(LiverUnlockedKey, 0) == 1;
    }

    public void UnlockLiver()
    {
        PlayerPrefs.SetInt(LiverUnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public bool IsLungsUnlocked()
    {
        return PlayerPrefs.GetInt(LungsUnlockedKey, 0) == 1;
    }

    public void UnlockLungs()
    {
        PlayerPrefs.SetInt(LungsUnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public bool IsStomachUnlocked()
    {
        return PlayerPrefs.GetInt(StomachUnlockedKey, 0) == 1;
    }

    public void UnlockStomach()
    {
        PlayerPrefs.SetInt(StomachUnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public bool IsBrainUnlocked()
    {
        return PlayerPrefs.GetInt(BrainUnlockedKey, 0) == 1;
    }

    public void UnlockBrain()
    {
        PlayerPrefs.SetInt(BrainUnlockedKey, 1);
        PlayerPrefs.Save();
    }

    public void ResetAllUnlocks()
    {
        PlayerPrefs.DeleteKey(HeartUnlockedKey);
        PlayerPrefs.DeleteKey(LiverUnlockedKey);
        PlayerPrefs.DeleteKey(LungsUnlockedKey);
        PlayerPrefs.DeleteKey(StomachUnlockedKey);
        PlayerPrefs.DeleteKey(BrainUnlockedKey);

        PlayerPrefs.Save();
    }
}