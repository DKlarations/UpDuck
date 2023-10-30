using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ducky : MonoBehaviour
{
    enum DuckyState { Idle, Running, Jumping, Flapping, Rolling, Falling, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;
    public AudioSource audioPlayer;
    public AudioClip[] jumpSounds;
    public AudioClip[] impactSounds;

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
    private bool jumpInput = false;
    public float horizontalPush = 0f;
    
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
 
        UpdateMovement();



        
        if (hit.collider != null 
        && currentState == DuckyState.Falling
        && body.velocity.y == 0)
        {
            OnLanding();
            currentState = DuckyState.Dead;
            AudioClip clipToPlay = impactSounds[Random.Range(0, impactSounds.Length)];
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
            
        }
        else if (hit.collider != null 
        && body.velocity.y == 0
        && currentState != DuckyState.Dead) 
        {
            OnLanding();
            currentState = DuckyState.Idle;
        } 
        else
        {
            isGrounded = false;
        }


        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            currentState = DuckyState.Running;
        }

        if (Input.GetKeyDown(KeyCode.Space) 
        && airborneTime <= coyoteTime)
        {
            jumpInput = true;
            shouldJump = true;
            currentState = DuckyState.Jumping;

            AudioClip clipToPlay = jumpSounds[Random.Range(0, jumpSounds.Length)];
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
        }
        
        if (Input.GetKey(KeyCode.Space) 
        && !isGrounded 
        && flapDuration < maxFlapDuration 
        && body.velocity.y < 0  
        && airborneTime > coyoteTime)
        {
            jumpInput = true;
            currentState = DuckyState.Flapping;
        }

        //If Spacebar is released, Release the flap while saving flap time.
        if (Input.GetKeyUp(KeyCode.Space)
        && currentState == DuckyState.Flapping
        && flapDuration < maxFlapDuration)
        {
            currentState = DuckyState.Falling;
        }

        if (flapDuration >= maxFlapDuration)
        {
            currentState = DuckyState.Falling;
        }

        if (Input.GetKey(KeyCode.S)
        && (currentState == DuckyState.Jumping || currentState == DuckyState.Falling))
        {
            currentState = DuckyState.Rolling;
        }


        if (body.velocity.y < 0 
        && currentState != DuckyState.Flapping  
        && currentState != DuckyState.Dead
        && airborneTime > deadThreshold)
        {
            currentState = DuckyState.Falling;
        }
          
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void FixedUpdate()
    {



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
            case DuckyState.Running:
                animator.SetBool("IsDead", false);
                break;

            case DuckyState.Jumping:
                if (shouldJump)
                {
                    animator.SetBool("IsDead", false);
                    body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                    animator.SetBool("IsJumping", true);
                    animator.SetBool("IsRolling", false);
                    shouldJump = false;
                }
                break;

            case DuckyState.Rolling:
                animator.SetBool("IsRolling", true);
                break;

            case DuckyState.Flapping:
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                animator.SetBool("IsFlapping", true);
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsRolling", false);
                break;

            case DuckyState.Falling:
                animator.SetBool("IsFalling", true);
                animator.SetBool("IsFlapping", false);
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsRolling", false);
                break;

            case DuckyState.Dead:
                animator.SetBool("IsDead", true);
                
                
                break;

            default:
                animator.SetBool("IsDead", false);
                OnLanding();
                break;
        }

        
    }
    public void UpdateMovement()
    {
        horizontalPush = Mathf.SmoothStep(0, 1, .75f)*horizontalPush;
        if (Mathf.Abs(horizontalPush) < 1)
        {
            horizontalPush = 0;
        }


        float horizontalInput = Input.GetAxis("Horizontal");
        body.velocity = new Vector2(horizontalInput * runSpeed + horizontalPush, body.velocity.y);


        //change the character facing
        if ((horizontalInput > 0 && !facingRight) || (horizontalInput < 0 && facingRight))
        {
            FlipCharacter();
        }
     }
    public void SetHorizontalPush(float push)
    {
        horizontalPush += push;
    }

    public void OnLanding()
    {
        flapDuration = 0.0f;
        isGrounded = true;
        jumpInput = false;  // Reset jump input flag

        animator.SetBool("IsJumping", false);
        animator.SetBool("IsFlapping", false);
        animator.SetBool("IsFalling", false);
    }

    public void FlipCharacter()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingRight = !facingRight;
    }
    public void Bounce(float bounceForce)
    {
        float totalForce = bounceForce;
        if (jumpInput)
        {
            totalForce += jumpSpeed;
            jumpInput = false;  // Reset jump input flag
        }
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force
    }
    
}
