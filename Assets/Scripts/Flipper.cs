using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Flipper : MonoBehaviour
{
    [SerializeField] private float bumpStrength = 35f;  // The strength of the bump
    [SerializeField] private float cooldownTime = 0.2f;  // Adjust the time as needed

    [Tooltip("Max Value = 1(up,right) Min Value = -1(down,left)")]
    public Vector2 Direction;
    public AudioSource audioSource;
    public Animator animator;
    //================
    //Animation States
    //================
    const string BOUNCE_ANIMATION = "PinballFlipperTriggered";
    const string IDLE_ANIMATION = "PinballFlipperIdle";

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        
        if (collider.gameObject.CompareTag("Player"))  // Ensure the collided object is the player
        {
            Ducky playerController = collider.gameObject.GetComponent<Ducky>();

            if (playerController != null)
            {
                // Apply the bump force
                playerController.ApplyPushForce(bumpStrength * Direction, cooldownTime);
                
                //Play Bounce Animation
                animator.Play(BOUNCE_ANIMATION);
                
                PlayBumpSound();
            }
        }
    }
    private void PlayBumpSound()
    {
        audioSource.Play();
    }
    private void BackToIdleAnimation()
    {
        animator.Play(IDLE_ANIMATION);
    }
}
