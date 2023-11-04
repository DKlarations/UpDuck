// BouncyPlatform.cs
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f; // Set desired bounce force
    public AudioSource audioPlayer;
    public AudioClip boingSounds;
    public Animator animator;
    //================
    //Animation States
    //================
    const string BOUNCE_ANIMATION = "MembranePlatform";
    const string IDLE_ANIMATION = "MembranePlatformIdle";

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public float GetBounceForce()
    {
        return bounceForce;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player")) 
        {
            Rigidbody2D rb = collider.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Ducky playerController = collider.gameObject.GetComponent<Ducky>();
                if (playerController != null)
                {
                    //Add Bounce
                    playerController.Bounce(bounceForce);

                    //Play Bounce Animation
                    animator.Play(BOUNCE_ANIMATION);

                    //Play Boing 
                    AudioClip clipToPlay = boingSounds;
                    audioPlayer.clip = clipToPlay;
                    audioPlayer.Play(); 
                }
            }
        }
    }

    private void BackToIdleAnimation()
    {
        animator.Play(IDLE_ANIMATION);
    }
}
