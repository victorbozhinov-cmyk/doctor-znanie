using System;
using UnityEngine;

public enum SerumOrgan
{
    Heart,
    Stomach,
    Liver,
    Brain,
    Lungs
}

public class SerumManager : MonoBehaviour
{
    public static SerumManager Instance { get; private set; }

    public event Action<int> OnSerumChanged;

    private const int PART_PERCENT = 20;

    private const string HEART_KEY = "Serum_Heart";
    private const string STOMACH_KEY = "Serum_Stomach";
    private const string LIVER_KEY = "Serum_Liver";
    private const string BRAIN_KEY = "Serum_Brain";
    private const string LUNGS_KEY = "Serum_Lungs";

    // =========================================================
    // PATIENT CURED PANEL
    // =========================================================

    private const string PATIENT_CURED_PANEL_SHOWN_KEY =
        "PatientCuredPanelShown";

    // =========================================================
    // PENDING FILL ANIMATION
    // =========================================================

    private bool hasPendingFillAnimation = false;

    private int pendingFromPercent = 0;
    private int pendingToPercent = 0;

    // =========================================================
    // UNITY
    // =========================================================

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

    // =========================================================
    // GIVE SERUM PART
    // =========================================================

    public bool GiveSerumPart(SerumOrgan organ)
    {
        string key = GetKey(organ);

        // Органът вече е давал своята част.
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            Debug.Log(
                $"[SerumManager] {organ} вече е дал своята част от серума."
            );

            return false;
        }

        // Запомняме прогреса ПРЕДИ наградата.
        int oldPercent = GetSerumPercent();

        // Даваме новата част.
        PlayerPrefs.SetInt(key, 1);

        PlayerPrefs.Save();

        int newPercent = GetSerumPercent();

        // Подготвяме анимация за BodyMap.
        pendingFromPercent = oldPercent;
        pendingToPercent = newPercent;

        hasPendingFillAnimation = true;

        Debug.Log(
            $"[SerumManager] Получена част от {organ}. " +
            $"Серум: {oldPercent}% -> {newPercent}%"
        );

        OnSerumChanged?.Invoke(newPercent);

        return true;
    }

    // =========================================================
    // FILL ANIMATION
    // =========================================================

    public bool TryConsumeFillAnimation(
        out int fromPercent,
        out int toPercent
    )
    {
        if (!hasPendingFillAnimation)
        {
            fromPercent = GetSerumPercent();
            toPercent = GetSerumPercent();

            return false;
        }

        fromPercent = pendingFromPercent;
        toPercent = pendingToPercent;

        hasPendingFillAnimation = false;

        return true;
    }

    // =========================================================
    // PATIENT CURED PANEL
    // =========================================================

    public bool HasPatientCuredPanelBeenShown()
    {
        return PlayerPrefs.GetInt(
            PATIENT_CURED_PANEL_SHOWN_KEY,
            0
        ) == 1;
    }

    public void MarkPatientCuredPanelAsShown()
    {
        if (HasPatientCuredPanelBeenShown())
        {
            return;
        }

        PlayerPrefs.SetInt(
            PATIENT_CURED_PANEL_SHOWN_KEY,
            1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[SerumManager] Patient Cured panel marked as shown."
        );
    }

    // =========================================================
    // CHECK ORGAN
    // =========================================================

    public bool HasSerumPart(SerumOrgan organ)
    {
        string key = GetKey(organ);

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // =========================================================
    // PROGRESS
    // =========================================================

    public int GetCollectedParts()
    {
        int parts = 0;

        if (HasSerumPart(SerumOrgan.Heart))
            parts++;

        if (HasSerumPart(SerumOrgan.Stomach))
            parts++;

        if (HasSerumPart(SerumOrgan.Liver))
            parts++;

        if (HasSerumPart(SerumOrgan.Brain))
            parts++;

        if (HasSerumPart(SerumOrgan.Lungs))
            parts++;

        return parts;
    }

    public int GetSerumPercent()
    {
        return GetCollectedParts() * PART_PERCENT;
    }

    public bool IsSerumComplete()
    {
        return GetCollectedParts() >= 5;
    }

    // =========================================================
    // PLAYER PREFS KEY
    // =========================================================

    private string GetKey(SerumOrgan organ)
    {
        switch (organ)
        {
            case SerumOrgan.Heart:
                return HEART_KEY;

            case SerumOrgan.Stomach:
                return STOMACH_KEY;

            case SerumOrgan.Liver:
                return LIVER_KEY;

            case SerumOrgan.Brain:
                return BRAIN_KEY;

            case SerumOrgan.Lungs:
                return LUNGS_KEY;

            default:
                return "";
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    [ContextMenu("Reset Serum Progress")]
    public void ResetSerumProgress()
    {
        PlayerPrefs.DeleteKey(HEART_KEY);
        PlayerPrefs.DeleteKey(STOMACH_KEY);
        PlayerPrefs.DeleteKey(LIVER_KEY);
        PlayerPrefs.DeleteKey(BRAIN_KEY);
        PlayerPrefs.DeleteKey(LUNGS_KEY);

        PlayerPrefs.DeleteKey(
            PATIENT_CURED_PANEL_SHOWN_KEY
        );

        PlayerPrefs.Save();

        hasPendingFillAnimation = false;

        pendingFromPercent = 0;
        pendingToPercent = 0;

        OnSerumChanged?.Invoke(0);

        Debug.Log(
            "[SerumManager] Serum progress reset."
        );
    }
}