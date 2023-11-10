using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public Transform otherPortal;
    public float teleportCooldown = 0.5f; // Cooldown in seconds

    private float lastTeleportTime = -1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time - lastTeleportTime > teleportCooldown)
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Teleport the player
                collision.transform.position = otherPortal.position;

                // Calculate the angle difference in world space
                float angleDifference = otherPortal.eulerAngles.z - transform.eulerAngles.z;

                // Calculate the new exit direction
                Vector2 entryDirection = playerRb.velocity.normalized;
                Quaternion rotation = Quaternion.Euler(0, 0, angleDifference);
                Vector2 exitDirection = rotation * entryDirection;

                // Apply the player's original speed in the new direction
                playerRb.velocity = exitDirection * playerRb.velocity.magnitude;

                // Update the teleportation cooldown
                lastTeleportTime = Time.time;
                otherPortal.GetComponent<PortalTeleporter>().lastTeleportTime = Time.time;
            }
        }
    }
}
