using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerPlatform : MonoBehaviour
{
    public GameObject[] platformsToEnable;
    private SpriteRenderer[] renderers;
    private BoxCollider2D[] colliders;
    public Animator[] hiddenPlatformAnimators;
    [SerializeField] private float platformsEnabledTime = 2f;
    public AudioSource audioPlayer;
    public AudioClip platformOnSounds;
    public AudioClip TimerTickingSound;
    public AudioClip platformOffSounds;
    public Animator animator;
    
    HiddenPlatform hiddenPlatform;
    private bool platformsAreOn = false;
    //================//
    //Animation States//
    //================//
    const string ON_ANIMATION = "TimedPlatformOn";
    const string OFF_ANIMATION = "TimedPlatformOff";
    const string HIDDEN_PLATFORM_ON_ANIMATION = "HiddenPlatformReveal";
    const string HIDDEN_PLATFORM_OFF_ANIMATION = "HiddenPlatformDisappear";
    const string HIDDEN_PLATFORM_IDLE_ANIMATION = "HiddenPlatformInvisible";

    void Start()
    {
        animator = GetComponent<Animator>();

        renderers = new SpriteRenderer[platformsToEnable.Length];
        colliders = new BoxCollider2D[platformsToEnable.Length];
        hiddenPlatformAnimators = new Animator[platformsToEnable.Length];

        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i] = platformsToEnable[i].GetComponent<SpriteRenderer>();
            colliders[i] = platformsToEnable[i].GetComponent<BoxCollider2D>();
            hiddenPlatformAnimators[i] = platformsToEnable[i].GetComponent<Animator>();

            hiddenPlatformAnimators[i].Play(HIDDEN_PLATFORM_IDLE_ANIMATION);
        }

        PreloadAudio();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky" && !platformsAreOn)
        {
            StartCoroutine(EnablePlatforms());

            platformsAreOn = true;
        }
    }

    IEnumerator EnablePlatforms()
    {
        SetPlatformsActive();

        audioPlayer.clip = TimerTickingSound;
        audioPlayer.Play();

        float interval = platformsEnabledTime / 8f;
        float elapsedTime = 0f;
        int currentInterval = 8;  // Start from the last animation

        while (elapsedTime < platformsEnabledTime)
        {
            elapsedTime += Time.deltaTime;

            // Trigger animations in reverse order
            if (elapsedTime >= interval * (8 - currentInterval))
            {
                animator.Play(ON_ANIMATION + currentInterval.ToString());
                currentInterval--;
            }

            // Pitch change logic (adjust as needed)
            if (elapsedTime >= platformsEnabledTime - 1.5f)
            {
                // Adjust the pitch over the last 1.5 seconds
                audioPlayer.pitch = Mathf.Lerp(1.0f, 1.5f, (elapsedTime - (platformsEnabledTime - 1.5f)) / 1.5f);
            }

            yield return null;
        }

        SetPlatformsInactive();

        audioPlayer.Stop();
        audioPlayer.pitch = 1f; // Reset pitch

        audioPlayer.PlayOneShot(platformOffSounds);

        // Play OFF Animation
        animator.Play(OFF_ANIMATION);

        platformsAreOn = false;
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
        audioPlayer.PlayOneShot(TimerTickingSound);

        // Immediately stop the audio player
        audioPlayer.Stop();

        // Restore the original volume
        audioPlayer.volume = originalVolume;
    }
    private void SetPlatformsActive()
    {
        foreach (var platform in platformsToEnable)
        {
            platform.GetComponent<SpriteRenderer>().enabled = true;
            platform.GetComponent<BoxCollider2D>().enabled = true;
            platform.GetComponent<Animator>().Play(HIDDEN_PLATFORM_ON_ANIMATION);    
        }
    }
    private void SetPlatformsInactive()
    {
        foreach (var platform in platformsToEnable)
        {
            platform.GetComponent<Animator>().Play(HIDDEN_PLATFORM_OFF_ANIMATION);
        }
    }
    void TriggerAnimationAtInterval(int intervalIndex)
    {
        // Trigger the appropriate animation based on the interval index
        // This is where you'll decide how to trigger the correct animation
        // Example: animator.Play("AnimationState_" + intervalIndex);
    } 
}
