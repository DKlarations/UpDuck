using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
public class Ducky : MonoBehaviour
{

    enum DuckyState { Idle, Walking, Running, Jumping, Flapping, Rolling, Falling, Tired, TiredFall, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;
    private CinemachineImpulseSource impulseSource;
    public UI_StatusIndicator status;

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
    private float pushCooldownTimer = 0f;

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

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("DuckStuff/MoveDuck #d")]
    public static void MoveDuck()
    {
        Selection.activeGameObject = GameObject.FindObjectOfType<Ducky>().gameObject;
    }
    #endif

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        // Cast a box downward to check for ground layer
        RaycastHit2D hit = Physics2D.BoxCast(transform.position + boxcastOffset, boxCastSize, 0f, Vector2.down, castDistance, groundLayer);
        onGround = hit.collider != null;


        // Debugging feature: Fly upwards when 'P' key and 'I' are pressed together.
        if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.I))
        {
            body.AddForce(Vector2.up * 1f, ForceMode2D.Impulse);
            OnLanding();
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
/*         && body.velocity.y <= yVelocityBuffer
        && body.velocity.y >= -yVelocityBuffer */
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

        //debugging seesaws:
        if (Input.GetButton("Jump"))
        {
           Debug.Log("jumpBufferCounter:" + jumpBufferCounter + "\n" + 
                     "canInput:" + canInput + "\n" + 
                     "onGround:" + onGround + "\n" +
                     "airborneTime:" + airborneTime + "\n" +
                     "coyoteTime:" + coyoteTime + "\n" + 
                     "shouldJump:" + shouldJump + "\n" + 
                     "all together:" + (
                        jumpBufferCounter >= 0f 
                        && canInput 
                        && onGround 
                        && airborneTime <= coyoteTime 
                        && shouldJump));
        }

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
        && (currentState == DuckyState.Flapping || currentState == DuckyState.Tired)
        && flapDuration < maxFlapDuration)
        {
            currentState = DuckyState.Jumping;
        }

        //Go to tired as approaching maxFlapDuration
        if (Input.GetButton("Jump")
        && flapDuration + .5f >= maxFlapDuration )
        {
            currentState = DuckyState.Tired;
        }

        //Go to Tired Fall if past maxFlapDuration
        if (flapDuration >= maxFlapDuration)
        {
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
        if(body.velocity.y < -10)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxFallVelocity);
        }

        //Adjust camera with a jump
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
        UpdateMovement();


        status.LabelTheDuck(currentState.ToString());
        
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

        // Reduce the cooldown timer by the time since the last frame
        pushCooldownTimer -= Time.deltaTime;

        // Check if shift key is held down to increase speed
        if (onGround && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            speedMultiplier = 1.5f;
        }

        if (canInput && pushCooldownTimer <= 0f)
        {
            // Calculate the desired velocity
            float targetVelocityX = horizontalInput * walkSpeed * speedMultiplier;
            
            // Calculate the difference between current velocity and desired velocity
            float velocityChangeX = targetVelocityX - body.velocity.x;

            // Apply force based on the difference. Using Impulse mode applies the force immediately
            body.AddForce(new Vector2(velocityChangeX, 0), ForceMode2D.Impulse);
        }

        // Change the character facing direction
        if (canInput && ((horizontalInput > 0 && !facingRight) || (horizontalInput < 0 && facingRight)))
        {
            FlipCharacter();
        }
    }
    public void ApplyPushForce(Vector2 force, float cooldownTime)
    {
        flapDuration = 0.0f;

        currentState = DuckyState.Idle;
        currentState = DuckyState.Jumping;
        airborneTime = 0f;   // Reset airborneTime to Zero

        // Apply the push force
        body.AddForce(force, ForceMode2D.Impulse);
        
        // Set the cooldown timer to prevent immediate input-based force application
        pushCooldownTimer = cooldownTime; // This is the cooldown period in seconds
        jumpBufferCounter = 0f;
    }

    private float ApplyDeadzone(float value, float threshold)
    {
        if (Mathf.Abs(value) < threshold)
        {
            return 0f;
        }
        return value;
    }

    public void OnLanding()
    {
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
        // Jump Logic
        if (jumpBufferCounter > 0f 
        && canInput 
        && onGround 
        && airborneTime <= coyoteTime 
        && shouldJump)
        { 
            currentState = DuckyState.Jumping;

            // Cancel out any existing vertical velocity before applying the jump impulse
            body.velocity = new Vector2(body.velocity.x, 0);

            // Apply an impulse force upwards
            body.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);

            // Create Dust Particles
            moveDust.Play();

            // Play random jump sound.
            PlayRandomSound(jumpSounds);

            // Reset timers and flags
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

        //flip the status readout of the ducky
        Vector3 statusScale = status.transform.localScale;
        statusScale.x *= -1;  
        status.transform.localScale = statusScale;

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
        }
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force

        jumpBufferCounter = 0f;
    }
    
    private void OnDrawGizmos()
    {
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
