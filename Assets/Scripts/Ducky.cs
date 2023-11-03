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
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallVelocity = 30f;
    private float jumpBufferCounter;

    public Animator animator;
    private string currentAnimation = "Ducky Idle";
    private Rigidbody2D body;
    public ParticleSystem moveDust;
    public ParticleSystem bounceDust;
    private bool shouldJump;
    private bool isGrounded = false;
    private bool facingRight = true;
    private bool jumpInput = false;
    
    public float horizontalPush = 0f;
    
    private float airborneTime;
    [SerializeField] private float deadThreshold = 1f;

    //================
    //Animation States
    //================
    const string DUCKY_IDLE = "Ducky Idle";
    const string DUCKY_JUMP = "Ducky Jump";
    const string DUCKY_FLAP = "Ducky Flap";
    const string DUCKY_FALL = "Ducky Fall";
    const string DUCKY_DEAD = "Ducky Dead";
    const string DUCKY_ROLL = "Ducky Roll";
    const string DUCKY_WALK = "Ducky Walk";

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Determine current state based on input and conditions
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 0.6f, Color.red);
 
        UpdateMovement();
        
        //Landing logic, check for faceplant, check for idle, otherwise not on ground.
        if (hit.collider != null 
        && currentState == DuckyState.Falling
        && body.velocity.y == 0)
        {
            OnLanding();
            currentState = DuckyState.Dead;

            moveDust.Play(); //Dust on dead landing

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
        else if (hit.collider != null)
        {
            OnLanding();
        }
        else
        {
            isGrounded = false;
        }


        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 && isGrounded)
        {
            currentState = DuckyState.Running;
        }

        //Jump Buffer Logic.  
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        //Jump Logic
        if (jumpBufferCounter > 0f 
        && airborneTime <= coyoteTime)
        {
            jumpInput = true;
            shouldJump = true;

            if (shouldJump)
                {
                    currentState = DuckyState.Jumping;
                    body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                    shouldJump = false;
                }
            //Create Dust Particles
            moveDust.Play();

            //Play random jump sound.
            AudioClip clipToPlay = jumpSounds[Random.Range(0, jumpSounds.Length)];
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 

            //reset jump buffer
            jumpBufferCounter = 0f;
        }

        //Small Jump Code. 
        if(Input.GetButtonUp("Jump") && body.velocity.y > 0)
        {
            currentState = DuckyState.Jumping;
            body.velocity = new Vector2(body.velocity.x, body.velocity.y*.5f);
        }

        //Higher Gravity when at end of jump
        if (body.velocity.y < 0 || (body.velocity.y > 0 && !Input.GetButton("Jump")))
        {
            body.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        
        //Flapping code. 
        if (Input.GetButton("Jump") 
        && !isGrounded 
        && flapDuration < maxFlapDuration 
        && body.velocity.y < 0  
        && airborneTime > coyoteTime)
        {
            jumpInput = true;
            currentState = DuckyState.Flapping;
        }

        //If Jump is released, Release the flap while saving flap time.
        if (Input.GetButtonUp("Jump")
        && currentState == DuckyState.Flapping
        && flapDuration < maxFlapDuration)
        {
            currentState = DuckyState.Falling;
        }

        //Start fall animation if past maxFlapDuration
        if (flapDuration >= maxFlapDuration)
        {
            currentState = DuckyState.Falling;
        }

        //Start Roll animation 
        if (Input.GetKey(KeyCode.S)
        && (currentState == DuckyState.Jumping || currentState == DuckyState.Falling))
        {
            currentState = DuckyState.Rolling;
        }

        //UNCERTAIN IF THIS IS NEEDED
        if (body.velocity.y < 0 
        && currentState != DuckyState.Flapping  
        && currentState != DuckyState.Dead
        && airborneTime > deadThreshold)
        {
            currentState = DuckyState.Falling;
        }

        //FALL SPEED CLAMPING
        body.velocity = Vector3.ClampMagnitude(body.velocity, maxFallVelocity);
        Debug.Log("Velocity is: " + body.velocity);
        
        //Exit the program with Escape
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
                ChangeAnimationState(DUCKY_WALK);
                break;

            case DuckyState.Jumping:                
                ChangeAnimationState(DUCKY_JUMP);
                break;

            case DuckyState.Rolling:
                ChangeAnimationState(DUCKY_ROLL);
                break;

            case DuckyState.Flapping:
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                ChangeAnimationState(DUCKY_FLAP);
                break;

            case DuckyState.Falling:
                ChangeAnimationState(DUCKY_FALL);
                break;

            case DuckyState.Dead:
                ChangeAnimationState(DUCKY_DEAD);
                break;

            default:
                ChangeAnimationState(DUCKY_IDLE);
                OnLanding();
                break;
        }

        
    }
    void ChangeAnimationState(string newAnimation)
    {
        //stop the same animation from interrupting itself
        if (currentAnimation == newAnimation) return;

        //play the animation
        animator.Play(newAnimation);

        //reassign the current animation
        currentAnimation = newAnimation;

    }
    public void UpdateMovement()
    {

        horizontalPush = Mathf.SmoothStep(0, 1, .6f)*horizontalPush;
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
        flapDuration = 0.0f; // Reset flapDuration to maximum
        isGrounded = true;   // Reset Grounded Flag 
        jumpInput = false;   // Reset jump input flag
    }

    public void FlipCharacter()
    {
        // flip character's facing.
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingRight = !facingRight;

        //Create Dust Particles on direction change if on ground
        if (isGrounded)
            {
                moveDust.Play();
            }
    }
    public void Bounce(float bounceForce)
    {
        //Create Bounce effect.
        float totalForce = bounceForce; 
        bounceDust.Play(); //Create Bounce Particles

        if (jumpInput)
        {
            totalForce += jumpSpeed;
            jumpInput = false;  // Reset jump input flag
        }
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force
    }
    
}
