using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] [Range(0f, 250f)] private float conveyorBeltForceStrength = 1f; // Speed of the treadmill
    [SerializeField] private bool conveyorBeltMovingRight = true;
    private Rigidbody2D playerRigidbody;
    public Animator animator;
    // Animation States
    const string CLOCKWISE_ANIMATION = "ConveyorBeltMovingClockwise";

    private void Start()
    {
        animator.Play(CLOCKWISE_ANIMATION);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerRigidbody = collider.GetComponent<Rigidbody2D>();
            float movementStrength = conveyorBeltMovingRight ? conveyorBeltForceStrength : -conveyorBeltForceStrength;
            Vector2 force = new Vector2(movementStrength, 0);
            playerRigidbody.AddForce(force, ForceMode2D.Force);
        }
    }
}
