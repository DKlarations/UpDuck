using UnityEngine;

public class PushAwayOnCollision : MonoBehaviour
{
    [SerializeField] private float pushForce = 30f;  // Adjust the force as needed
    [SerializeField] private float cooldownTime = 0.2f;  // Adjust the time as needed
    public AudioSource audioPlayer;
    public AudioClip boingSound;


    public void Start()
    {

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Use the collision normal to determine the push direction
            Vector2 pushDirection = collision.GetContact(0).normal; // Gets the normal at the point of contact

            Ducky playerController = collision.gameObject.GetComponent<Ducky>();

            if (playerController != null)
            {
                // Apply the force in the opposite direction of the collision normal
                playerController.ApplyPushForce(-pushDirection * pushForce, cooldownTime);
                
                // Play Boing sound
                audioPlayer.PlayOneShot(boingSound);
            }
        }
    }
}

