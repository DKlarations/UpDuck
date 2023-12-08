using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] [Range(0f, 250f)] private float conveyorBeltTopSpeed = 1f; // Top Speed of the treadmill
    private float currentSpeed = 0f;
    [SerializeField] [Range(0f, 2f)] private float conveyorBeltRevTime = .85f;
    private float timeSinceAccelerationStart = 0f;
    private bool isAccelerating = false;
    private bool isOnStraights = false;
    [SerializeField] private bool conveyorBeltMovingClockwise;
    [SerializeField] private CircleCollider2D leftSideCircleCollider;
    [SerializeField] private CircleCollider2D rightSideCircleCollider;
    [SerializeField] private Animator animator;
    // Animation States
    const string IDLE_ANIMATION = "ConveyorBeltIdle";
    const string CLOCKWISE_STARTUP_ANIMATION = "ConveyorBeltClockwiseStartUp";
    const string CLOCKWISE_ANIMATION = "ConveyorBeltMovingClockwise";
    const string CLOCKWISE_REVDOWN_ANIMATION = "ConveyorBeltClockwiseRevDown";
    const string COUNTERCLOCKWISE_STARTUP_ANIMATION = "ConveyorBeltCounterClockwiseStartUp";
    const string COUNTERCLOCKWISE_ANIMATION = "ConveyorBeltMovingCounterClockwise";
    const string COUNTERCLOCKWISE_REVDOWN_ANIMATION = "ConveyorBeltCounterClockwiseRevDown";
    private void Start()
    {
        animator.Play(IDLE_ANIMATION);
    }
    private void Update()
    {
        if (isAccelerating)
        {
            if (timeSinceAccelerationStart < conveyorBeltRevTime)
            {
                timeSinceAccelerationStart += Time.deltaTime;
                float t = timeSinceAccelerationStart / conveyorBeltRevTime;
                currentSpeed = conveyorBeltTopSpeed * Mathf.Sin(t * Mathf.PI * 0.5f);
            }
            else
            {
                currentSpeed = conveyorBeltTopSpeed;
                isAccelerating = false;
            }
        }
    }
    public void ChangeToClockwiseAnimation()
    {
        animator.Play(CLOCKWISE_ANIMATION);
    }
    public void ChangeToCounterClockwiseAnimation()
    {
        animator.Play(COUNTERCLOCKWISE_ANIMATION);
    }
    public void ChangeToIdleAnimation()
    {
        animator.Play(IDLE_ANIMATION);
    }
    public void HandleProximityEnter(Collider2D collider)
    {
        animator.Play(conveyorBeltMovingClockwise ? CLOCKWISE_STARTUP_ANIMATION : COUNTERCLOCKWISE_STARTUP_ANIMATION);
        timeSinceAccelerationStart = 0.0f;
        isAccelerating = true;
    }
    public void HandleProximityExit(Collider2D collider)
    {
        animator.Play(conveyorBeltMovingClockwise ? CLOCKWISE_REVDOWN_ANIMATION : COUNTERCLOCKWISE_REVDOWN_ANIMATION);
        currentSpeed = 0.0f;
        isAccelerating = false;
    }
    public void HandleTopAndBottomOfConveyorEnter(Collider2D collider)
    {
        if (collider.CompareTag("Player")){isOnStraights = true;}
    }
    public void HandleTopAndBottomOfConveyorExit(Collider2D collider)
    {
        if (collider.CompareTag("Player")){isOnStraights = false;}
    }
    public void HandleTopOfConveyorStay(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;
        if (!collider.TryGetComponent<Rigidbody2D>(out var playerRb)) return;

        Vector2 direction = conveyorBeltMovingClockwise ? transform.right : -transform.right;
        Vector2 force = direction * currentSpeed;
        playerRb.AddForce(force, ForceMode2D.Force);
    }
    public void HandleBottomOfConveyorStay(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;
        if (!collider.TryGetComponent<Rigidbody2D>(out var playerRb)) return;

        Vector2 direction = conveyorBeltMovingClockwise ? -transform.right : transform.right;
        Vector2 force = direction * currentSpeed;
        playerRb.AddForce(force, ForceMode2D.Force);
    }
    public void HandleLeftOfConveyorStay(Collider2D collider)
    {
        if (collider.CompareTag("Player") != !isOnStraights) return;
        if (!collider.TryGetComponent<Rigidbody2D>(out var playerRb)) return;

        // Calculate tangent force
        Vector2 contactPoint = collider.ClosestPoint(leftSideCircleCollider.bounds.center);
        Vector2 centerToPoint = contactPoint - (Vector2)leftSideCircleCollider.bounds.center;
        Vector2 tangent = conveyorBeltMovingClockwise ?
                          new Vector2(centerToPoint.y, -centerToPoint.x) :
                          new Vector2(-centerToPoint.y, centerToPoint.x);
        tangent.Normalize();

        // Apply the force
        Vector2 force = tangent * currentSpeed;
        playerRb.AddForce(force, ForceMode2D.Force);
    }
    public void HandleRightOfConveyorStay(Collider2D collider)
    {
        if (collider.CompareTag("Player") != !isOnStraights) return;
        if (!collider.TryGetComponent<Rigidbody2D>(out var playerRigidbody)) return;

        // Calculate tangent force
        Vector2 contactPoint = collider.ClosestPoint(rightSideCircleCollider.bounds.center);
        Vector2 centerToPoint = contactPoint - (Vector2)rightSideCircleCollider.bounds.center;
        Vector2 tangent = conveyorBeltMovingClockwise ?
                          new Vector2(centerToPoint.y, -centerToPoint.x) :
                          new Vector2(centerToPoint.y, centerToPoint.x);
        tangent.Normalize();

        // Apply the force
        Vector2 force = tangent * currentSpeed;
        playerRigidbody.AddForce(force, ForceMode2D.Force);
    }
}
