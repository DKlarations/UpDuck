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

    void Start()
    {
        // Initialize Collider2D and SpriteRenderer arrays
        renderers = new SpriteRenderer[platformsToEnable.Length];

        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i] = platformsToEnable[i].GetComponent<SpriteRenderer>();
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Enable platforms when something enters the trigger
        if (collider.gameObject.name == "Ducky")
        {
            for (int i = 0; i < platformsToEnable.Length; i++)
            {
                renderers[i].enabled = true;
            }
            AudioClip clipToPlay = platformOnSounds;
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky")
        {
            for (int i = 0; i < platformsToEnable.Length; i++)
            {
                renderers[i].enabled = false;
            }
            AudioClip clipToPlay = platformOffSounds;
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
        }
    }
}



