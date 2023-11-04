using UnityEngine;

public class PushAwayOnCollision : MonoBehaviour
{
    [SerializeField] private float pushForce = 30f;  // Adjust the force as needed
    public AudioSource audioPlayer;
    public AudioClip boingSounds;

    void OnCollisionEnter2D(Collision2D collision)
    {
        Ducky playerController = collision.gameObject.GetComponent<Ducky>();
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 pushDirection = collision.transform.position - transform.position;
            pushDirection.y = 0;
            pushDirection.Normalize();

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                playerController.SetHorizontalPush(pushDirection.x > 0 ? pushForce : -pushForce);   
                //Play Boing 
                AudioClip clipToPlay = boingSounds;
                audioPlayer.clip = clipToPlay;
                audioPlayer.Play();
            }
        }
    }
}
