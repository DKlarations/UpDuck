using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWell : MonoBehaviour
{
    public float pullStrength = 5f;
    public float pushStrength = 10f;
    public float pullRadius = 5f;
    public float pushRadius = 1f;

    private void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                Vector2 directionToCenter = (transform.position - collider.transform.position).normalized;
                float distanceToCenter = Vector2.Distance(transform.position, collider.transform.position);

                // Attraction phase
                if (distanceToCenter > pushRadius)
                {
                    collider.attachedRigidbody.AddForce(directionToCenter * pullStrength);
                }
                // Expulsion phase
                else
                {
                    collider.attachedRigidbody.AddForce(-directionToCenter * pushStrength, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the pull radius for design-time visualization
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        // Draw the push radius for design-time visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
}
