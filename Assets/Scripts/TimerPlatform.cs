using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerPlatform : MonoBehaviour
{
    public GameObject[] platformsToEnable;
    private SpriteRenderer[] renderers;
    private BoxCollider2D[] colliders;
    [SerializeField] private float platformsEnabledTime = 2f;
    public AudioSource audioPlayer;
    public AudioClip platformOnSounds;
    public AudioClip TimerTickingSound;
    public AudioClip platformOffSounds;
    public Animator animator;
    private bool platformsAreOn = false;
    //================
    //Animation States
    //================
    const string ON_ANIMATION = "TimedPlatformOn";
    const string OFF_ANIMATION = "TimedPlatformOff";

    void Start()
    {
        animator = GetComponent<Animator>();

        renderers = new SpriteRenderer[platformsToEnable.Length];
        colliders = new BoxCollider2D[platformsToEnable.Length];

        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i] = platformsToEnable[i].GetComponent<SpriteRenderer>();
            colliders[i] = platformsToEnable[i].GetComponent<BoxCollider2D>();
        }
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
        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i].enabled = true;
            colliders[i].enabled = true;
        }

        audioPlayer.clip = TimerTickingSound;
        audioPlayer.Play();

        yield return new WaitForSeconds(platformsEnabledTime - 1);        
        audioPlayer.pitch = 1.5f;
        yield return new WaitForSeconds(1);

        for (int i = 0; i < platformsToEnable.Length; i++)
        {
            renderers[i].enabled = false;
            colliders[i].enabled = false;
        }

        audioPlayer.Stop();
        audioPlayer.pitch = 1f;

        //Play OFF Animation
        animator.Play(OFF_ANIMATION);

        platformsAreOn = false;
    }
}