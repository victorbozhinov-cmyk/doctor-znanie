using UnityEngine;

public class AntibodyManager : MonoBehaviour
{
    public static AntibodyManager Instance { get; private set; }

    public enum OrganType
    {
        Heart,
        Stomach,
        Liver,
        Brain,
        Lungs
    }

    private const string HeartBestKey = "HeartBestAntibodies";
    private const string StomachBestKey = "StomachBestAntibodies";
    private const string LiverBestKey = "LiverBestAntibodies";
    private const string BrainBestKey = "BrainBestAntibodies";
    private const string LungsBestKey = "LungsBestAntibodies";

    public int HeartBestScore => PlayerPrefs.GetInt(HeartBestKey, 0);
    public int StomachBestScore => PlayerPrefs.GetInt(StomachBestKey, 0);
    public int LiverBestScore => PlayerPrefs.GetInt(LiverBestKey, 0);
    public int BrainBestScore => PlayerPrefs.GetInt(BrainBestKey, 0);
    public int LungsBestScore => PlayerPrefs.GetInt(LungsBestKey, 0);

    public int TotalBestScore =>
        HeartBestScore +
        StomachBestScore +
        LiverBestScore +
        BrainBestScore +
        LungsBestScore;

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

    public int GetBestScore(OrganType organ)
    {
        return organ switch
        {
            OrganType.Heart => HeartBestScore,
            OrganType.Stomach => StomachBestScore,
            OrganType.Liver => LiverBestScore,
            OrganType.Brain => BrainBestScore,
            OrganType.Lungs => LungsBestScore,
            _ => 0
        };
    }

    public bool SubmitScore(OrganType organ, int currentScore)
    {
        int previousBest = GetBestScore(organ);

        if (currentScore <= previousBest)
        {
            return false;
        }

        string key = GetKeyForOrgan(organ);

        PlayerPrefs.SetInt(key, currentScore);
        PlayerPrefs.Save();

        return true;
    }

    private string GetKeyForOrgan(OrganType organ)
    {
        return organ switch
        {
            OrganType.Heart => HeartBestKey,
            OrganType.Stomach => StomachBestKey,
            OrganType.Liver => LiverBestKey,
            OrganType.Brain => BrainBestKey,
            OrganType.Lungs => LungsBestKey,
            _ => ""
        };
    }

    public void ResetAllScores()
    {
        PlayerPrefs.DeleteKey(HeartBestKey);
        PlayerPrefs.DeleteKey(StomachBestKey);
        PlayerPrefs.DeleteKey(LiverBestKey);
        PlayerPrefs.DeleteKey(BrainBestKey);
        PlayerPrefs.DeleteKey(LungsBestKey);

        PlayerPrefs.Save();
    }
}
