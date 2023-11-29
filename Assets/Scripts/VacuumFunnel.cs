using System.Collections;
using UnityEngine;

public class VacuumFunnel : MonoBehaviour
{
    public CircleCollider2D circleCollider;
    public PolygonCollider2D polygonCollider;
    public ParticleSystem suctionParticles; 
    public ParticleSystem ejectionParticles; 
    public Animator animator;
    [SerializeField] private float attractorRange = 5f;
    [SerializeField] private float attractorStrength = 5f;
    [SerializeField] [Range(0, 360)] private float attractorCenterAngle = 0f; // Center of the attractor zone
    [SerializeField] [Range(0, 360)] private float attractorSpanAngle = 90f; // Span of the attractor zone
    [SerializeField] private Vector3 ejectionAngleOffset;

    [SerializeField] private float ejectionForce = 10f;
    [SerializeField] [Range(0, 360)] private float ejectionAngleDegrees = 45f;
    //================//
    //AUDIO COMPONENTS//
    //================//
    [Header("AUDIO")]
    public AudioSource vacuumAudio;
    public AudioSource otherAudio;
    public AudioClip vacuumStart;
    public AudioClip vacuumOn;
    public AudioClip vacuumStop;
    public AudioClip suckedUpSound;
    public AudioClip ejectionSound;

    //================//
    //Animation States//
    //================//
    const string IDLE_ANIMATION = "VacuumFunnelIdle";
    const string ACTIVATED_ANIMATION = "VacuumFunnelActivated";
    const string CAPTURED_ANIMATION = "VacuumFunnelCaptured";


    private void Start()
    {
        animator = GetComponent<Animator>();
        ConfigureParticleSystem();
    }   

    private void OnTriggerEnter2D(Collider2D collider)
    {
            StartCoroutine(HandlePlayerInteraction(collider.gameObject));
    }
    private IEnumerator HandlePlayerInteraction(GameObject player)
    {
        // Access the required components
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Ducky playerScript = player.GetComponent<Ducky>();

        // Disable player's sprite, movement, and input
        if (playerSprite != null && playerRb != null && playerScript != null)
        {
            
            vacuumAudio.pitch = 1.25f;  //Change pitch of vacuum audio to simulate lack of suction

            polygonCollider.enabled = false;
            playerSprite.enabled = false;
            animator.Play(CAPTURED_ANIMATION);
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
            player.transform.position = transform.position; // Move player to the center
            playerScript.canInput = false; // Disable player input
        
            // Move player to the ejection origin
            Vector3 ejectionOrigin = transform.position + ejectionAngleOffset;
            player.transform.position = ejectionOrigin;

             // Wait for .5 seconds
            yield return new WaitForSeconds(.25f);
            AudioClip clipToPlay = suckedUpSound;
            otherAudio.clip = clipToPlay;
            otherAudio.Play();

            yield return new WaitForSeconds(.25f);

            playerSprite.enabled = true;
            playerRb.isKinematic = false;
            ApplyEjectionForce(player);
            playerScript.canInput = true; // Re-enable player input

            vacuumAudio.pitch = 1f;  //Change pitch back to normal
        

            // Wait for .5 seconds before reenable
            yield return new WaitForSeconds(.5f);
            polygonCollider.enabled = true;
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
    public void HandleEnterTrigger(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Check if the collider is the player
        {
            animator.Play(ACTIVATED_ANIMATION); 

            //Play Vacuum Start Sounds
            AudioClip clipToPlay = vacuumStart;
            vacuumAudio.clip = clipToPlay;
            vacuumAudio.Play(); 

            if (suctionParticles != null)
            {
                suctionParticles.Play(); // Turn on the particle system
            }
        }
    }

    public void HandleExitTrigger(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Check if the collider is the player
        {
            animator.Play(IDLE_ANIMATION);

            //Play Vacuum Start Sounds
            AudioClip clipToPlay = vacuumStop;
            vacuumAudio.clip = clipToPlay;
            vacuumAudio.Play();

            if (suctionParticles != null)
            {
                suctionParticles.Stop(); // Turn off the particle system
            }
        }
    }
    public void HandleStayTrigger(Collider2D collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

        if (!vacuumAudio.isPlaying)
        {
            //Play Vacuum On Sounds
            AudioClip clipToPlay = vacuumOn;
            vacuumAudio.clip = clipToPlay;
            vacuumAudio.Play();
        }
        

        if (rb != null)
        {
            Vector2 directionToCenter = (transform.position - collider.transform.position).normalized;
            float distanceToCenter = Vector2.Distance(transform.position, collider.transform.position);

            if (distanceToCenter <= 0) 
                return;

            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
            angleToCenter = NormalizeAngle(angleToCenter);

            // Check if the object is within the specified angle range
            float startAngle = attractorCenterAngle - (attractorSpanAngle / 2f);
            float endAngle = attractorCenterAngle + (attractorSpanAngle / 2f);
            if (IsAngleWithinRange(angleToCenter, startAngle, endAngle))
            {
                float scaledForce = attractorStrength / distanceToCenter;
                rb.AddForce(directionToCenter * scaledForce, ForceMode2D.Force);
            }
        }
    }
    public void ActivateEjectionParticles()
    {
        if (ejectionParticles != null)
        {
            ejectionParticles.Play();
        }
    }
    private bool IsAngleWithinRange(float angle, float startAngle, float endAngle)
    {
        if (startAngle < endAngle)
            return angle >= startAngle && angle <= endAngle;
        return angle >= startAngle || angle <= endAngle;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle > 360) angle -= 360;
        return angle;
    }

    private Vector2 CalculateEjectionDirection()
    {
        float angleInRadians = ejectionAngleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)).normalized;
    }
    private void ConfigureParticleSystem()
    {
        if (suctionParticles != null)
        {
            var shape = suctionParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;

            shape.radius = attractorRange;
            shape.arc = attractorSpanAngle;

            // Calculate and set the rotation angle
            float rotationAngle = attractorCenterAngle - (attractorSpanAngle / 2f);
            shape.rotation = new Vector3(0, 0, rotationAngle);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attractorRange);

        float startAngle = attractorCenterAngle - (attractorSpanAngle / 2f);
        float endAngle = attractorCenterAngle + (attractorSpanAngle / 2f);

        DrawAngleGizmos(startAngle, endAngle, attractorRange, Color.red);

        Vector2 ejectionDirection = CalculateEjectionDirection();
        Vector3 ejectionOrigin = transform.position + ejectionAngleOffset; // Calculate ejection origin
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(ejectionOrigin, ejectionOrigin + (Vector3)ejectionDirection * 2); // Draw line from ejection origin
    }

    private void DrawAngleGizmos(float startAngle, float endAngle, float range, Color color)
    {
        Gizmos.color = color;
        int lineCount = 18;
        for (int i = 0; i <= lineCount; i++)
        {
            float currentAngle = Mathf.Lerp(startAngle, endAngle, (float)i / lineCount);
            Vector2 direction = AngleToVector(currentAngle);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * range);
        }
    }
    private Vector2 AngleToVector(float angleDegrees)
    {
        // Convert from degrees to radians
        float angleInRadians = (angleDegrees+180) * Mathf.Deg2Rad;

        // Adjust angle interpretation (if needed)
        // Unity's coordinate system uses clockwise rotation, with 0 degrees pointing to the right.
        // If your angles are defined differently, you might need to adjust this conversion.
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
}