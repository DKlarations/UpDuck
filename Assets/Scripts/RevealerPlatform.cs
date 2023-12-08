using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RevealerPlatform : MonoBehaviour
{
    [SerializeField] private List<GameObject> platformsToEnable = new List<GameObject>();
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip platformOnSounds;
    [SerializeField] private AudioClip platformOffSounds;
    [SerializeField] private Animator animator;
    //================//
    //Animation States//
    //================//
    const string ON_ANIMATION = "RevealerPlatformOn";
    const string OFF_ANIMATION = "RevealerPlatformOff";

    void Start()
    {
        // Initialize SpriteRenderer list
        foreach (GameObject platform in platformsToEnable)
        {
            SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderers.Add(renderer);
            }
        }
        PreloadAudio();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player")) return;

        // Enable platforms when something enters the trigger
        SetPlatformsVisible(true);

        animator.Play(ON_ANIMATION);

        audioPlayer.clip = platformOnSounds;
        audioPlayer.Play(); 
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player")) return;

        SetPlatformsVisible(false);

        animator.Play(OFF_ANIMATION);

        audioPlayer.clip = platformOffSounds;
        audioPlayer.Play(); 
    }

    private void SetPlatformsVisible(bool active)
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = active;
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
