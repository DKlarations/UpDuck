using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumper : MonoBehaviour
{

    public float bumpStrength = 10f;  // The strength of the bump

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))  // Ensure the collided object is the player
        {
            Ducky playerController = collision.gameObject.GetComponent<Ducky>();

            if (playerController != null)
            {
                
                // Get the normal of the contact point
                Vector2 contactNormal = collision.GetContact(0).normal;
                Debug.Log("Collision Detected at " + contactNormal);

                // Apply the bump force
                playerController.BumperBounce(bumpStrength,contactNormal);
                playerController.SetHorizontalPush(contactNormal.x < 0 ? bumpStrength : -bumpStrength);

                
                PlayBumpSound();
                AnimateBumper();
            }
        }
    }


    private void PlayBumpSound()
    {
        // Play bump sound effect
    }

    private void AnimateBumper()
    {
        // Trigger animation effect
    }
}
