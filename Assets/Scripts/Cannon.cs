using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Animator animator;
    public PolygonCollider2D polygonCollider;
    [SerializeField] private Vector3 ejectionAngleOffset;
    [SerializeField] private float ejectionForce = 10f;
    [SerializeField] [Range(0, 360)] private float ejectionAngleDegrees = 45f;

    public ParticleSystem featherParticles;
    public ParticleSystem duckCannonSmokeParticles;
    // Audio Components
    [Header("AUDIO")]
    public AudioSource cannonAudio;
    public AudioClip cannonLoad;
    public AudioClip cannonFire;

    // Animation States
    const string IDLE_ANIMATION = "CannonIdle";
    const string FIRING_ANIMATION = "CannonFiring";

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Check if the collider is the player
        {
            StartCoroutine(PlayerGetsLaunched(collider.gameObject));
        }
    }

    private IEnumerator PlayerGetsLaunched(GameObject player)
    {
        // Access the required components
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Ducky playerScript = player.GetComponent<Ducky>();

        // Disable player's sprite, movement, and input
        if (playerSprite != null && playerRb != null && playerScript != null)
        {
            playerSprite.enabled = false;
            animator.Play(FIRING_ANIMATION);
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
            polygonCollider.enabled = false;// Turn off Cannon's Collider to avoid Grounded state
            playerScript.canInput = false; // Disable player input

            // Move player to the ejection origin
            Vector3 ejectionOrigin = new Vector3 (transform.position.x,transform.position.y, 0) + ejectionAngleOffset;
            player.transform.position = ejectionOrigin;

            // Play cannon load audio
            cannonAudio.clip = cannonLoad;
            cannonAudio.Play();

            playerScript.ChangeToCannonball();
            playerScript.ResetAirTime(); //Change air variables to 0
            
            // POSSIBLY Wait for the loading audio to finish (cannonLoad.Length)
            yield return new WaitForSeconds(1f);

            
            playerSprite.enabled = true;
            playerRb.isKinematic = false;
            IncreaseDuckParticles();
            

            ApplyEjectionForce(player); // Launch the player
            duckCannonSmokeParticles.Play();
            featherParticles.Play();
            playerScript.ResetAirTime(); //Resetting this twice to avoid TiredFall Behavior
            playerScript.shouldJump = false;
            playerScript.canInput = true; // Re-enable player input
            

            // Play cannon fire audio
            cannonAudio.clip = cannonFire;
            cannonAudio.Play();

            // Change animation back to idle
            animator.Play(IDLE_ANIMATION);

            yield return new WaitForSeconds(1.5f);
            DecreaseDuckParticles();
            polygonCollider.enabled = true;// Turn on Cannon's Collider
        }
    }

    private void ApplyEjectionForce(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Convert angle to radians and calculate direction
            float angleInRadians = ejectionAngleDegrees * Mathf.Deg2Rad;
            Vector2 forceDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            // Apply the force
            rb.AddForce(forceDirection.normalized * ejectionForce, ForceMode2D.Impulse);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // Set the color of the Gizmo line
        Vector3 ejectionOrigin = transform.position + ejectionAngleOffset; // Calculate ejection origin

        // Calculate ejection direction
        Vector2 ejectionDirection = CalculateEjectionDirection();

        // Draw line from ejection origin in the direction of ejection
        Gizmos.DrawLine(ejectionOrigin, ejectionOrigin + (Vector3)ejectionDirection * ejectionForce);
    }

    private Vector2 CalculateEjectionDirection()
    {
        float angleInRadians = ejectionAngleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)).normalized;
    }
    private void IncreaseDuckParticles()
    {
        //Change Suction Particles to Simulate the Vacuum Being "stuck"
        duckCannonSmokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);//Stop so I can change duration
        float scaleFactor = ejectionForce / 50f; //Calculate scale based on arbitrary number

        var main = duckCannonSmokeParticles.main;
        main.startLifetime = scaleFactor * .5f;
        main.duration = scaleFactor * 1f;

        var emission = duckCannonSmokeParticles.emission;
        emission.rateOverTime = scaleFactor * 400f;
    }
    private void DecreaseDuckParticles()
    {
        //Change Suction Particles to Simulate the Vacuum Being "stuck"
        duckCannonSmokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);//Stop so I can change duration
        var main = duckCannonSmokeParticles.main;
        main.startLifetime = .5f;
        main.duration = .1f;
        var emission = duckCannonSmokeParticles.emission;
        emission.rateOverTime = 100f;
    }

}
