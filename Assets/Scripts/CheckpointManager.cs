using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour
{
    public TextMeshProUGUI mainTimerText;      // Reference to the main timer TextMeshProUGUI
    public TextMeshProUGUI checkpointTimerPrefab; // Prefab for individual checkpoint timers
    public Transform checkpointTimerContainer; // UI container to hold checkpoint timers
    public static float startTime;                // Static so it's shared across all checkpoints
    private static bool isTimerRunning = true;    // Shared across all checkpoints
    public Sprite newSprite; // The new sprite to switch to
    private SpriteRenderer spriteRenderer;
    public ParticleSystem checkpointParticles;
    public AudioSource checkpointAudio;
    public int checkpointID;
    private static bool checkpointTimesLoaded = false;
    private List<float> checkpointTimes = new List<float>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        StartTimer();
        if (!checkpointTimesLoaded)
        {
            ReloadAndDisplayCheckpointTimes();
            checkpointTimesLoaded = true; 
        }

        if (PlayerPrefs.HasKey(GetPrefsKey()))
        {
            DisableCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") && isTimerRunning)
        {
            DisableCheckpoint();
            PlayCheckpointParticles();
            PlayCheckpointAudio();
            RecordCheckpointTime();
        }           
    }
    private static string GetPrefsKey(int checkpointID){return $"checkpoint_{checkpointID}";}
    private string GetPrefsKey(){return CheckpointManager.GetPrefsKey(this.checkpointID);}

    private void DisableCheckpoint()
    {
        if (newSprite != null && spriteRenderer != null)
            {   
                spriteRenderer.sprite = newSprite; // Change the sprite                
            }
            GetComponent<Collider2D>().enabled = false;
    }
    public void RecordCheckpointTime()
    {
        float checkpointTime = Time.time - startTime;
        
        InstantiateAndPositionCheckpointTimer(checkpointID, checkpointTime);

        //Save the checkpoint time in Player Prefs
        PlayerPrefs.SetFloat(GetPrefsKey(checkpointID), checkpointTime);
    }

    private void ReloadAndDisplayCheckpointTimes()
    {
        if (checkpointTimesLoaded) 
        {
            return;
        }
        // Dictionary to store checkpoint times and their IDs
        var checkpointTimes = new Dictionary<int, float>();

        // Assuming you have a known maximum number of checkpoints
        int maxCheckpoints = 4; // Adjust this to your actual maximum number of checkpoints

        // Load the times for all checkpoints
        for (int i = 1; i <= maxCheckpoints; i++)
        {
            float time = PlayerPrefs.GetFloat(GetPrefsKey(i), -1f);
            if (time >= 0f)
            {
                checkpointTimes.TryAdd(i, time);
            }
        }

        // Sort the checkpoints by their times in ascending order
        var sortedCheckpointTimes = checkpointTimes.OrderBy(kvp => kvp.Value);

        // Display the sorted checkpoint times
        foreach (var checkpoint in sortedCheckpointTimes)
        {
            InstantiateAndPositionCheckpointTimer(checkpoint.Key, checkpoint.Value);
        }
    }

    private void InstantiateAndPositionCheckpointTimer(int checkpointID, float time)
    {
        TextMeshProUGUI newCheckpointTimer = Instantiate(checkpointTimerPrefab, checkpointTimerContainer);
        newCheckpointTimer.text = "Checkpoint " + checkpointID + ": " + FormatTime(time);
        
        // Position the new timer in the UI
        float checkpointUIVerticalSpacing = 20f; 
        Vector3 newPosition = new Vector3(0, -checkpointTimerContainer.childCount * checkpointUIVerticalSpacing, 0);
        newCheckpointTimer.transform.localPosition = newPosition;
    }

    private string FormatTime(float timeInSeconds)
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
    private void PlayCheckpointParticles()
    {
        if (checkpointParticles != null)
        {
            checkpointParticles.Play();
        }
    }
    private void PlayCheckpointAudio()
    {
        if (checkpointAudio != null)
        {
            checkpointAudio.Play(); // Play the audio
        }
    }

    // Call this method when the player hits a checkpoint
    public void OnCheckpointReached()
    {
        RecordCheckpointTime();
    }
    public static void StartTimer()
    {
        startTime = Time.time - PlayerPrefs.GetFloat("TimeElapsed");
        isTimerRunning = true;
    }

    public static void StopTimer()
    {
        isTimerRunning = false;
    }
    public static void ResetCheckpointTimesLoaded()
    {
        checkpointTimesLoaded = false;
    }
}
