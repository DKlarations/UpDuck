using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumper : MonoBehaviour
{

    [SerializeField] private float bumpStrength = 10f;  // The strength of the bump
    [SerializeField] private float cooldownTime = 0.2f;  // Adjust the time as needed
    public AudioSource audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))  // Ensure the collided object is the player
        {
            Ducky playerController = collision.gameObject.GetComponent<Ducky>();

            if (playerController != null)
            {
                
                // Get the normal of the contact point
                Vector2 contactNormal = collision.GetContact(0).normal;

                // Apply the bump force
                //playerController.BumperBounce(bumpStrength,contactNormal);
                playerController.ApplyPushForce(-bumpStrength * contactNormal, cooldownTime);
                //playerController.SetHorizontalPush(contactNormal.x < 0 ? bumpStrength : -bumpStrength);

                
                PlayBumpSound();
                AnimateBumper();
            }
        }
    }


    private void PlayBumpSound()
    {
        audioSource.Play();
    }

    private void AnimateBumper()
    {
        // Trigger animation effect
    }
}
