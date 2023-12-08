// BouncyPlatform.cs
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f; // Set desired bounce force
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip boingSounds;
    [SerializeField] private Animator animator;
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
        if (!collider.gameObject.CompareTag("Player")) return;
        if (!collider.TryGetComponent<Rigidbody2D>(out var rb)) return;
        if (!collider.TryGetComponent<Ducky>(out var playerController)) return;

        //Add Bounce
        playerController.VerticalBounce(bounceForce);

        //Play Bounce Animation
        animator.Play(BOUNCE_ANIMATION);

        //Play Boing 
        AudioClip clipToPlay = boingSounds;
        audioPlayer.clip = clipToPlay;
        audioPlayer.Play();
    }

    private void BackToIdleAnimation()
    {
        animator.Play(IDLE_ANIMATION);
    }
}
