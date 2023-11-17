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

        //Debug feature to increase the timer's time
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
        {
            startTime -= 600f;
        }
        #endif
    }

    string FormatTime(float timeInSeconds)
    {
        int hours = (int)(timeInSeconds / 3600);
        int minutes = (int)((timeInSeconds % 3600) / 60);
        int seconds = (int)(timeInSeconds % 60);
        int milliseconds = (int)((timeInSeconds - (int)timeInSeconds) * 100); // Two digits

        if (hours > 0)
        {
            return $"{hours}:{minutes:00}:{seconds:00}:{milliseconds:00}";
        }
        else
        {
            return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        }
    }


}
