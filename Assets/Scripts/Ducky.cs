using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Ducky : MonoBehaviour
{
    enum DuckyState { Idle, Walking, Running, Jumping, Flapping, Rolling, Falling, Tired, TiredFall, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;
    private CinemachineImpulseSource impulseSource;
    [Header("Audio")]
    public AudioSource audioPlayer;
    public AudioClip[] jumpSounds;
    public AudioClip[] impactSounds;
    [Header("Animation")]
    public CameraFollow cameraFollow;
    public Animator animator;
    public ParticleSystem moveDust;
    public ParticleSystem bounceDust;
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float flapStrength = 2.0f;
    private float flapDuration = 0.0f;
    [SerializeField] private float maxFlapDuration = 2.0f;
    [SerializeField] private float faceplantInputLockTime = .5f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallVelocity = 30f;
    [SerializeField] private float yVelocityBuffer = .1f;
    private float jumpBufferCounter;

    private string currentAnimation = "Ducky Idle";
    private Rigidbody2D body;

    private bool shouldJump;
    private bool onGround;
    private bool facingRight = true;
    private bool canInput = true;
    
    public float horizontalPush = 0f;
    
    private float airborneTime;
    [SerializeField] private float deadThreshold = 1f;
    [Header("Ground Detection")]
    public Vector3 raycastOffset;
    public Vector3 boxcastOffset;
    public Vector2 boxCastSize;
    public float castDistance;
    
    //================//
    //Animation States//
    //================//
    const string DUCKY_IDLE = "Ducky Idle";
    const string DUCKY_JUMP = "Ducky Jump";
    const string DUCKY_FLAP = "Ducky Flap";
    const string DUCKY_FALL = "Ducky Fall";
    const string DUCKY_TIRED = "Ducky Tired";
    const string DUCKY_TIRED_FALL = "Ducky Tired Fall";
    const string DUCKY_DEAD = "Ducky Dead";
    const string DUCKY_WALK = "Ducky Walk";
    const string DUCKY_RUN = "Ducky Run";


    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.515f, groundLayer);

        //Old Raycast logic, tying it to a Boolean, returns true if either ray hits groundlayer
/*         onGround = Physics2D.Raycast(transform.position + raycastOffset, Vector2.down, 0.515f, groundLayer) 
                || Physics2D.Raycast(transform.position - raycastOffset, Vector2.down, 0.515f, groundLayer);*/
        Debug.DrawRay(transform.position + raycastOffset, Vector2.down * 0.515f, Color.red);
        Debug.DrawRay(transform.position - raycastOffset, Vector2.down * 0.515f, Color.red); 

        //Vector2 boxCastSize = new Vector2(1f, 0.1f); // Set the width and height of the box
        //float castDistance = 0.1f; // How far down we cast our box

        // Cast a box downward to check for ground layer
        RaycastHit2D hit = Physics2D.BoxCast(transform.position + boxcastOffset, boxCastSize, 0f, Vector2.down, castDistance, groundLayer);
        onGround = hit.collider != null;


        UpdateMovement();

        // Debugging feature: Fly upwards when 'P' key and 'W' are pressed together.
        if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.I))
        {
            body.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }

        
        //Landing logic, check for faceplant, check for idle, otherwise Ducky is not on ground.
        if (onGround
        && IsFallingState()
        && body.velocity.y <= yVelocityBuffer
        && body.velocity.y >= -yVelocityBuffer
        )
        {
            OnLanding();  //Reset all airborne stats and flags
            FacePlant();  //Faceplant including input pause

            if (currentState != DuckyState.Dead)
            {
                body.velocity = new Vector2(0,0);  //Stop all momentum when faceplanting
            }
            
            currentState = DuckyState.Dead;

            moveDust.Play(); //Dust on dead landing

            //Play random impact sound
            PlayRandomSound(impactSounds);
        }
        else if (onGround
        && body.velocity.y <= yVelocityBuffer
        && body.velocity.y >= -yVelocityBuffer
        && currentState != DuckyState.Dead)  
        {
            OnLanding();
            currentState = DuckyState.Idle;
        } 
        else if (!onGround
        && body.velocity.y < 0
        && IsFallingState())
        {
            airborneTime += Time.deltaTime;
            currentState = DuckyState.Jumping;  //Swap to jumping animation if descending
        }
        else
        {
            airborneTime += Time.deltaTime;
        }

        //If moving on ground play walking animation
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 
        && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        && onGround 
        && canInput 
        && currentState != DuckyState.Jumping) 
        {
            currentState = DuckyState.Running;
        }
        else if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0 
        && onGround 
        && canInput 
        && currentState != DuckyState.Jumping
        )
        {
            currentState = DuckyState.Walking;
        }


        //Jump Buffer Logic.  
        UpdateJumpBuffer();

        //Jump Logic
        HandleJump();

        //Small Jump Code. 
        if(Input.GetButtonUp("Jump") && body.velocity.y > 0)
        {
            currentState = DuckyState.Jumping;
            body.velocity = new Vector2(body.velocity.x, body.velocity.y*.5f);
        }

        //Higher Gravity when past apex of jump
        if (body.velocity.y < 0 || (body.velocity.y > 0 && !Input.GetButton("Jump")))
        {
            body.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }


        
        
        
        //Flapping code. 
        if (Input.GetButton("Jump") 
        && !onGround
        && flapDuration < maxFlapDuration 
        && body.velocity.y < 0  
        && airborneTime > coyoteTime)
        {
            currentState = DuckyState.Flapping;
        }

        //If Jump is released, Release the flap while saving flap time.
         if (Input.GetButtonUp("Jump")
        && currentState == DuckyState.Flapping
        && flapDuration < maxFlapDuration)
        {
            currentState = DuckyState.Jumping;
        }

        //Go to tired as approaching maxFlapDuration
        if (flapDuration + 0.5f >= maxFlapDuration)
        {
            currentState = DuckyState.Tired;
        }

        //Go to Tired Fall if past maxFlapDuration
        if (flapDuration >= maxFlapDuration)
        {
            Debug.Log("Past max flap duration");
            currentState = DuckyState.TiredFall;
        }

        //This is the ducky going into a fall animation if going to faceplant.
        if (body.velocity.y < 0 
        && currentState != DuckyState.TiredFall
        && airborneTime > deadThreshold)
        {
            currentState = DuckyState.Falling;
        }

        //Fall Speed Clamping
        if(body.velocity.y < 0)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxFallVelocity);
        }

        if(body.velocity.y < 0 && !onGround)
        {
            cameraFollow.AdjustCameraForJump(!onGround);
        }
        
        //Exit the program with Escape
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
       
    }

    void FixedUpdate()
    {
        // Handle state-specific logic
        switch (currentState)
        {
            case DuckyState.Walking:
                ChangeAnimationState(DUCKY_WALK);
                break;
            
            case DuckyState.Running:
                moveDust.Play();
                ChangeAnimationState(DUCKY_RUN);
                break;

            case DuckyState.Jumping:                
                ChangeAnimationState(DUCKY_JUMP);
                break;

            case DuckyState.Flapping:
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                ChangeAnimationState(DUCKY_FLAP);
                break;

            case DuckyState.Falling:
                ChangeAnimationState(DUCKY_FALL);
                break;

            case DuckyState.Tired:
                Debug.Log("Ducky is Tired");
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                ChangeAnimationState(DUCKY_TIRED);
                break;

            case DuckyState.TiredFall:
                ChangeAnimationState(DUCKY_TIRED_FALL);
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
        float horizontalInput = Input.GetAxis("Horizontal");
        float speedMultiplier = 1f;  // Default speed multiplier

        // Check if shift key is held down to increase speed
        if (onGround && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            speedMultiplier = 1.5f;
        }

        if (canInput)
        {
            horizontalPush = Mathf.SmoothStep(0, 1, .6f) * horizontalPush;

            // Apply deadzone to horizontalPush just before it's used
            horizontalPush = ApplyDeadzone(horizontalPush, 1f);

            // OLD Apply the speed multiplier to the walkSpeed
            body.velocity = new Vector2(horizontalInput * walkSpeed * speedMultiplier + horizontalPush, body.velocity.y);
        }

        // Change the character facing direction
        if (canInput && ((horizontalInput > 0 && !facingRight) || (horizontalInput < 0 && facingRight)))
        {
            FlipCharacter();
        }
    }
    private float ApplyDeadzone(float value, float threshold)
    {
        if (Mathf.Abs(value) < threshold)
        {
            return 0f;
        }
        return value;
    }


    public void SetHorizontalPush(float push)
    {
        horizontalPush += push;
    }

    public void OnLanding()
    {
        Debug.Log("Landed");
        cameraFollow.AdjustCameraForJump(!onGround);
        flapDuration = 0.0f; // Reset flapDuration to maximum
        airborneTime = 0f;   // Reset airborneTime to Zero
        shouldJump = true;   // Reset the Should Jump Flag
    }
    private void UpdateJumpBuffer()
    {
        //Jump Buffer Logic.  
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    private void HandleJump()
    {
        //Jump Logic
        if (jumpBufferCounter > 0f
        && canInput
        && onGround
        && airborneTime <= coyoteTime
        && shouldJump)
        { 
            currentState = DuckyState.Jumping;
            body.velocity = new Vector2(body.velocity.x, jumpSpeed);

            //Create Dust Particles
            moveDust.Play();

            //Play random jump sound.
            PlayRandomSound(jumpSounds);

            //reset timers and flags
            shouldJump = false;
            jumpBufferCounter = 0f;
        }
    }
    private bool IsFallingState() 
    {
        return currentState == DuckyState.Falling || currentState == DuckyState.TiredFall;
    }
    private void PlayRandomSound(AudioClip[] clips) 
    {
        if (clips.Length == 0) return;
        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
        audioPlayer.clip = clipToPlay;
        audioPlayer.Play();
    }
    public void FacePlant()
    {
        CameraShakeManager.instance.CameraShake(impulseSource);
        StartCoroutine(InputDelayCoroutine());
    }

    IEnumerator InputDelayCoroutine()
    {
        
        //disable further Input
        canInput = false;

        //wait for specified delay
        yield return new WaitForSeconds(faceplantInputLockTime);

        //Re-enable Input
        canInput = true;
    }

    public void FlipCharacter()
    {
        // flip character's facing.
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingRight = !facingRight;

        if (cameraFollow != null)
        {
            cameraFollow.AdjustCameraForDirection(facingRight);
        }

        //Create Dust Particles on direction change if on ground
        if (onGround)
            {
                moveDust.Play();
            }
    }
    public void VerticalBounce(float bounceForce)
    {
        // Reset flapDuration to 0 to avoid Tired and TiredFall behavior
        flapDuration = 0.0f;

        currentState = DuckyState.Idle;
        currentState = DuckyState.Jumping;

        airborneTime = 0f;   // Reset airborneTime to Zero
        
        float totalForce = bounceForce;

        bounceDust.Play(); //Create Bounce Particles

        if (jumpBufferCounter > 0f)
        { 
            totalForce += jumpSpeed;
            
            Debug.Log("Should be Super Jump");
        }
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force

        jumpBufferCounter = 0f;
    }
    public void BumperBounce(float bounceForce, Vector2 bounceDirection)
    {   
        float bounceDirectionX = -bounceDirection.x * bounceForce;
        float bounceDirectionY = -bounceDirection.y * bounceForce;
        Debug.Log("Bounce Force: " + bounceDirectionX + ", " + bounceDirectionY);
        body.AddForce(new Vector2(bounceDirectionX, bounceDirectionY), ForceMode2D.Impulse);
    }
    
    private void OnDrawGizmos()
    {
        // Use the same size for the box as you use in the BoxCast
        //Vector2 boxCastSize = new Vector2(1f, 0.1f);
        //float castDistance = 0.1f;

        // The center of the box cast, slightly above the player to ensure it starts inside the player collider
        Vector2 castOrigin = (Vector2)transform.position + (Vector2)boxcastOffset + Vector2.up * 0.05f;

        // Calculate the four corners of the box
        Vector2 bottomLeft = castOrigin + new Vector2(-boxCastSize.x / 2, -boxCastSize.y / 2 - castDistance);
        Vector2 bottomRight = castOrigin + new Vector2(boxCastSize.x / 2, -boxCastSize.y / 2 - castDistance);
        Vector2 topLeft = castOrigin + new Vector2(-boxCastSize.x / 2, boxCastSize.y / 2);
        Vector2 topRight = castOrigin + new Vector2(boxCastSize.x / 2, boxCastSize.y / 2);

        // Draw the box using Debug.DrawLine
        Debug.DrawLine(bottomLeft, bottomRight, Color.red); // Bottom
        Debug.DrawLine(topLeft, topRight, Color.red);       // Top
        Debug.DrawLine(bottomLeft, topLeft, Color.red);     // Left
        Debug.DrawLine(bottomRight, topRight, Color.red);   // Right
    }

    
}
