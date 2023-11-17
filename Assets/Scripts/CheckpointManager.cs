using System.Collections;
using System.Collections.Generic;
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
    private List<float> checkpointTimes = new List<float>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startTime = Time.time;
        StartTimer();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") && isTimerRunning)
        {
            if (newSprite != null && spriteRenderer != null)
            {   
                Debug.Log("Sprite Changed");
                spriteRenderer.sprite = newSprite; // Change the sprite                
            }
            
            PlayCheckpointParticles();
            PlayCheckpointAudio();

            RecordCheckpointTime();
            GetComponent<Collider2D>().enabled = false;
        }           
    }

    public void RecordCheckpointTime()
    {
        float checkpointTime = Time.time - startTime;
        
        TextMeshProUGUI newCheckpointTimer = Instantiate(checkpointTimerPrefab, checkpointTimerContainer);
        newCheckpointTimer.text = "Checkpoint " + checkpointID + ": " + FormatTime(checkpointTime);

        // Position the new timer in the UI
        // Calculate the position based on the number of children in checkpointTimerContainer
        float checkpointUIVerticalSpacing = 20f; // Adjust as needed
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
        startTime = Time.time;
        isTimerRunning = true;
    }

    public static void StopTimer()
    {
        isTimerRunning = false;
    }
}
