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
        }

        PreloadAudio();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky" && !platformsAreOn)
        {
            StartCoroutine(EnablePlatforms());

            //Play ON Animation
            animator.Play(ON_ANIMATION);

            platformsAreOn = true;
        }
    }

    IEnumerator EnablePlatforms()
    {

        SetPlatformsActive();

        audioPlayer.clip = TimerTickingSound;
        audioPlayer.Play();

        yield return new WaitForSeconds(platformsEnabledTime - 1.5f); 

        yield return StartCoroutine(ChangePitchOverTime(audioPlayer, 1.0f, 1.5f, 1.5f));

        SetPlatformsInactive();

        audioPlayer.Stop();
        audioPlayer.pitch = 1f;

        AudioClip clipToPlay = platformOffSounds;
        audioPlayer.clip = clipToPlay;
        audioPlayer.Play();

        //Play OFF Animation
        animator.Play(OFF_ANIMATION);

        platformsAreOn = false;
    }
    IEnumerator ChangePitchOverTime(AudioSource audioSource, float startPitch, float endPitch, float duration)
    {
        float currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newPitch = Mathf.Lerp(startPitch, endPitch, currentTime / duration);
            audioSource.pitch = newPitch;
            yield return null; // Wait for the next frame
        }

        audioSource.pitch = endPitch; 
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
}
