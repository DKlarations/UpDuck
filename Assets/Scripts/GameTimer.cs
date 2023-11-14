using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Namespace for TextMeshPro

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // Reference to TextMeshProUGUI component
    private float startTime;
    private bool isTimerRunning;

    void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        startTime = Time.time;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            float timeElapsed = Time.time - startTime;
            timerText.text = FormatTime(timeElapsed);
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        int milliseconds = (int)((timeInSeconds - (int)timeInSeconds) * 100); // Changed to 100 for two digits

        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

}
