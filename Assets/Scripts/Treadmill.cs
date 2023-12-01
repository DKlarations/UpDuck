using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treadmill : MonoBehaviour
{
    [SerializeField] [Range(0f, 100f)] private float treadmillForceStrength = 1f; // Speed of the treadmill
    [SerializeField] private bool treadmillMovingRight = true;
    private Rigidbody2D playerRigidbody;
    private bool isPlayerOnTreadmill = false; // To check if the player is on the treadmill

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) // Replace "Player" with the tag of your player
        {
            isPlayerOnTreadmill = true;
            playerRigidbody = collider.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerOnTreadmill = false;
        }
    }

    private void FixedUpdate()
    {
        if (isPlayerOnTreadmill)
        {
            float movementStrength = treadmillMovingRight ? treadmillForceStrength : -treadmillForceStrength;
            Vector2 force = new Vector2(movementStrength, 0);
            playerRigidbody.AddForce(force, ForceMode2D.Force);
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        // Implement the logic to move the player sideways
        // This can be done by directly modifying the player's transform or using Rigidbody2D
        // Example: playerTransform.position += Vector3.right * speed * Time.fixedDeltaTime;
    }
}
