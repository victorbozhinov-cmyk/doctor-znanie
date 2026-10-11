using UnityEngine;
using System;

public class LungsPuzzlePanelTrace : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log(
            "PUZZLE PANEL ENABLED | Frame: " +
            Time.frameCount
        );
    }

    private void OnDisable()
    {
        Debug.LogWarning(
            "PUZZLE PANEL DISABLED | Frame: " +
            Time.frameCount +
            "\n" +
            Environment.StackTrace
        );
    }
}