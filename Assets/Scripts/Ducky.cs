using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ducky : MonoBehaviour
{
    enum DuckyState { Idle, Running, Jumping, Flapping, Rolling, Falling, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;

    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float flapStrength = 2.0f;
    private float flapDuration = 0.0f;
    [SerializeField] private float maxFlapDuration = 2.0f;

    public Animator animator;
    private Rigidbody2D body;
    private bool shouldJump;
    private bool isGrounded = false;
    private bool facingRight = true;
    private float airborneTime;
    [SerializeField] private float deadThreshold = 1f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Determine current state based on input and conditions
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 0.6f, Color.red);
 
 
        if (hit.collider != null 
        && body.velocity.y == 0
        /* && currentState != DuckyState.Falling */)
        {
            OnLanding();
            currentState = DuckyState.Idle;
            flapDuration = 0.0f;
            isGrounded = true;
        } else
        {
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) 
        && airborneTime <= coyoteTime)
        {
            shouldJump = true;
            currentState = DuckyState.Jumping;
        }

        if (Input.GetKey(KeyCode.Space) 
        && !isGrounded 
        && flapDuration < maxFlapDuration 
        && body.velocity.y < 0  
        && airborneTime > coyoteTime)
        {
            currentState = DuckyState.Flapping;
        }

        //If Spacebar is released, Release the flap while saving flap time.
        if (Input.GetKeyUp(KeyCode.Space)
        && currentState == DuckyState.Flapping
        && flapDuration < maxFlapDuration)
        {
            currentState = DuckyState.Falling;
        }


        if (body.velocity.y < 0 
        && currentState != DuckyState.Flapping  
        && currentState != DuckyState.Dead
        && airborneTime > deadThreshold)
        {
            currentState = DuckyState.Falling;
        }

        //While landed, not moving, and past the threshold, activate faceplant
/*         if (hit.collider != null
        && currentState == DuckyState.Falling
        && body.velocity.y == 0)
        {
            Debug.Log("SHOULD BE FACEPLANTED");
            OnLanding();
            currentState = DuckyState.Dead;
        } */
        
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        body.velocity = new Vector2(horizontalInput * runSpeed, body.velocity.y);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        
        if (hit.collider != null)
        {
            airborneTime = 0f;
            OnLanding();
        }
        else
        {
            airborneTime += Time.fixedDeltaTime;
        }

        // Handle state-specific logic
        switch (currentState)
        {
            case DuckyState.Jumping:
                if (shouldJump)
                {
                    body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                    animator.SetBool("IsJumping", true);
                    shouldJump = false;
                }
                break;

            case DuckyState.Flapping:
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                animator.SetBool("IsFlapping", true);
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", false);
                if (flapDuration >= maxFlapDuration)
                {
                    currentState = DuckyState.Falling;
                }
                break;

            case DuckyState.Falling:
                animator.SetBool("IsFalling", true);
                animator.SetBool("IsFlapping", false);
                animator.SetBool("IsJumping", false);
                break;

            case DuckyState.Dead:
                animator.SetBool("IsDead", true);
                break;

            default:
                OnLanding();
                break;
        }

        if (horizontalInput > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            FlipCharacter();
        }
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsFlapping", false);
        animator.SetBool("IsFalling", false);
    }

    void FlipCharacter()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingRight = !facingRight;
    }
}