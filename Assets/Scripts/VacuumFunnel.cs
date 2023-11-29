using System.Collections;
using UnityEngine;

public class VacuumFunnel : MonoBehaviour
{
    public CircleCollider2D circleCollider;
    public PolygonCollider2D polygonCollider;
    public ParticleSystem suctionParticles; 
    [SerializeField] private float attractorRange = 5f;
    [SerializeField] private float attractorStrength = 5f;
    [SerializeField] [Range(0, 360)] private float attractorCenterAngle = 0f; // Center of the attractor zone
    [SerializeField] [Range(0, 360)] private float attractorSpanAngle = 90f; // Span of the attractor zone

    [SerializeField] private float ejectionForce = 10f;
    [SerializeField] [Range(0, 360)] private float ejectionAngleDegrees = 45f;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        
    }
    private void Update()
    {
        ConfigureParticleSystem();
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
            StartCoroutine(HandlePlayerInteraction(collision.gameObject));
    }
    private IEnumerator HandlePlayerInteraction(GameObject player)
    {
        // Access the required components
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        // Assuming the player script is named 'PlayerScript'
        Ducky playerScript = player.GetComponent<Ducky>();

        // Disable player's sprite, movement, and input
        if (playerSprite != null && playerRb != null && playerScript != null)
        {
            polygonCollider.enabled = false;
            playerSprite.enabled = false;
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
            player.transform.position = transform.position; // Move player to the center
            playerScript.canInput = false; // Disable player input
        }

        // Wait for .5 seconds
        yield return new WaitForSeconds(.5f);

        // Re-enable player's sprite, physics, and input
        if (playerSprite != null && playerRb != null && playerScript != null)
        {
            playerSprite.enabled = true;
            playerRb.isKinematic = false;
            ApplyEjectionForce(player);
            playerScript.canInput = true; // Re-enable player input
        }

        // Wait for .5 seconds
        yield return new WaitForSeconds(.5f);
        polygonCollider.enabled = true;
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
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Check if the collider is the player
        {
            if (suctionParticles != null)
            {
                suctionParticles.Play(); // Turn on the particle system
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Check if the collider is the player
        {
            if (suctionParticles != null)
            {
                suctionParticles.Stop(); // Turn off the particle system
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)ejectionDirection * 2);
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