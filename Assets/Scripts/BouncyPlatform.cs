using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlledBounce : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f; // Set desired bounce force

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ensure the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Reset vertical velocity before applying bounce force
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(new Vector2(0, bounceForce), ForceMode2D.Impulse);
            }
        }
    }
}
