using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float delayBeforeDisable = 2.0f; // Delay before disabling (adjust as needed)
    [SerializeField] private float delayBeforeEnable = 2.0f; // Delay before re-enabling (adjust as needed)
    private bool isNotBroken = true;

    public AudioSource audioPlayer;
    public AudioClip iceCrackingSounds;
    public AudioClip iceReturningSounds;
    public Animator animator;
    //================
    //Animation States
    //================
    const string IDLE_ANIMATION = "IcePlatformIdle";
    const string CRACKING_ANIMATION = "IcePlatformCrack";
    const string REFORMING_ANIMATION = "IcePlatformReform";
    

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

        
    public void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky" && isNotBroken)
        {
            StartCoroutine(DisableAndEnablePlatform());
        }
    } 
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky" && isNotBroken)
        {
            StartCoroutine(DisableAndEnablePlatform());
        }
    }

    public IEnumerator DisableAndEnablePlatform()
    {
        //Set Condition to Broken
        isNotBroken = false;

        //Play Cracking Animation
        animator.Play(CRACKING_ANIMATION);

        // Delay before disabling the platform
        yield return new WaitForSeconds(delayBeforeDisable);

        // Disable the Box Collider
        // boxCollider.enabled = false;

        //Play Ice Cracking Sounds
        AudioClip clipToPlay = iceCrackingSounds;
        audioPlayer.clip = clipToPlay;
        audioPlayer.Play(); 

        // Delay before re-enabling the platform
        yield return new WaitForSeconds(delayBeforeEnable);

        // Re-enable the Box Collider
        // boxCollider.enabled = true;

        //Play Reforming Animation
        animator.Play(REFORMING_ANIMATION);

        //Play Ice Returning Sounds
        audioPlayer.clip = iceReturningSounds;
        audioPlayer.Play();

        //Set to Not broken 
        isNotBroken = true;
    }

    private void BackToIdleAnimation()
    {
        animator.Play(IDLE_ANIMATION);
    }
}
