using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RevealerPlatform : MonoBehaviour
{
    public GameObject[] platformsToEnable;
    private SpriteRenderer[] renderers;
    public AudioSource audioPlayer;
    public AudioClip platformOnSounds;
    public AudioClip platformOffSounds;
    public Animator animator;
    //================//
    //Animation States//
    //================//
    const string ON_ANIMATION = "RevealerPlatformOn";
    const string OFF_ANIMATION = "RevealerPlatformOff";

    void Start()
    {
        // Initialize Collider2D and SpriteRenderer arrays
        renderers = new SpriteRenderer[platformsToEnable.Length];

        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i] = platformsToEnable[i].GetComponent<SpriteRenderer>();
            
        }
        PreloadAudio();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Enable platforms when something enters the trigger
        if (collider.gameObject.name == "Ducky")
        {
            SetPlatformsVisible(true);

            animator.Play(ON_ANIMATION);

            AudioClip clipToPlay = platformOnSounds;
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky")
        {
            SetPlatformsVisible(false);

            animator.Play(OFF_ANIMATION);

            AudioClip clipToPlay = platformOffSounds;
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
        }
    }
    private void SetPlatformsVisible(bool active)
    {
        foreach (var platform in platformsToEnable)
        {
            platform.GetComponent<SpriteRenderer>().enabled = active;
        }
    }
    private void PreloadAudio()
    {
        // Temporarily save the original volume
        float originalVolume = audioPlayer.volume;

        // Set volume to zero to mute the audio
        audioPlayer.volume = 0f;

        // Play and stop the audio clips to preload them
        audioPlayer.PlayOneShot(platformOnSounds);
        audioPlayer.PlayOneShot(platformOffSounds);

        // Immediately stop the audio player
        audioPlayer.Stop();

        // Restore the original volume
        audioPlayer.volume = originalVolume;
    }
}



