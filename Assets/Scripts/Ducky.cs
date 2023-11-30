using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Threading;
using static UnityEngine.InputSystem.InputAction;






#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
public class Ducky : MonoBehaviour
{

    enum DuckyState { Idle, Walking, Running, Jumping, Flapping, Wave, WallSlide, Falling, Tired, TiredFall, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;
    private CinemachineImpulseSource impulseSource;
    public UI_StatusIndicator status;
    public PlayerControls controls;
    public DuckSettings settings;

    [Header("Audio")]
    public AudioSource audioPlayer;
    public AudioClip[] jumpSounds;
    public AudioClip[] impactSounds;

    public AudioSource footstepsAudioSource;
    public List<AudioClip> GroundFootSteps;
    public List<AudioClip> GrassFootSteps;
    public List<AudioClip> MetalFootSteps;
    public List<AudioClip> WoodFootSteps;
    public List<AudioClip> IceFootSteps;
    enum groundMaterial
    {
        Ground, Grass, Metal, Wood, Ice, Empty
    }
    private groundMaterial footStepMaterial = groundMaterial.Empty;

    [Header("Animation")]
    public CameraFollow cameraFollow;
    public Animator animator;
    public ParticleSystem moveDust;
    public ParticleSystem bounceDust;
    [Header("Movement")]
    private float flapDuration = 0.0f;
    private float jumpBufferCounter;
    private float pushCooldownTimer = 0f;
    private float idleTimer = 0f;
    private float idleTimeBeforeWave = 8.1f; //Make sure to keep the .1 on any value

    private string currentAnimation = "Ducky Idle";
    private Rigidbody2D body;
    private bool shouldJump;
    private bool onGround;
    private bool facingRight = true;
    public bool canInput = true;
    [HideInInspector]public float horizontalPush = 0f;

    private float airborneTime;
    private float freefallTime;
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    public Vector3 boxCastOffset;
    public Vector2 boxCastSize;
    public float castDistance;
    [Header("Wall Detection")]
    public LayerMask wallLayer;
    [SerializeField] private Vector2 wallBoxCastOffset;
    [SerializeField] private float wallBoxCastDistance;
    [SerializeField] private Vector2 wallBoxCastSize;
    private bool isOnWall;

    //======//
    //INPUTS//
    //======//
    private bool isJumpPressed;
    private bool isJumpReleased;
    private bool isJumpHeld;
    private bool isRunHeld;
    private float moveAxisInput;
    private bool isExitPressed;

    //================//
    //Animation States//
    //================//
    const string DUCKY_IDLE = "Ducky Idle";
    const string DUCKY_JUMP = "Ducky Jump";
    const string DUCKY_FLAP = "Ducky Flap";
    const string DUCKY_FALL = "Ducky Fall";
    const string DUCKY_TIRED = "Ducky Tired";
    const string DUCKY_TIRED_FALL = "Ducky Tired Fall";
    const string DUCKY_WALL_SLIDE = "Ducky WallSlide";
    const string DUCKY_DEAD = "Ducky Dead";
    const string DUCKY_WALK = "Ducky Walk";
    const string DUCKY_RUN = "Ducky Run";
    const string DUCKY_WAVE = "Ducky Wave";

#if UNITY_EDITOR
    [UnityEditor.MenuItem("DuckStuff/SelectDuck #d")]
    public static void MoveDuck()
    {
        //   Selection.activeGameObject = GameObject.FindObjectOfType<Ducky>().gameObject;
        Selection.activeGameObject = GameObject.FindFirstObjectByType<Ducky>().gameObject;
    }
#endif

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        controls = new PlayerControls();

        #if UNITY_EDITOR
            transform.position = transform.position;
        #else
            transform.position = new Vector3 (PlayerPrefs.GetFloat("PlayerXLocation"), PlayerPrefs.GetFloat("PlayerYLocation"), 0);
        #endif  
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
    

    void Update()
    {
        UpdateInputs();
        // Cast a box downward to check for ground layer
        RaycastHit2D hit = Physics2D.BoxCast(transform.position + boxCastOffset, boxCastSize, 0f, Vector2.down, castDistance, groundLayer);
        onGround = hit.collider != null;

        //Change what the ground material is:
        ChangeGroundMaterial(hit);

        //Landing logic, check for faceplant, check for idle, otherwise Ducky is not on ground.
        if (onGround
        && IsFallingState
        && body.velocity.y <= settings.yVelocityBuffer
        && body.velocity.y >= -settings.yVelocityBuffer
        )
        {
            OnLanding();  //Reset all airborne stats and flags
            FacePlant();  //Faceplant including input pause

            if (currentState != DuckyState.Dead)
            {
                body.velocity = new Vector2(0, 0);  //Stop all momentum when faceplanting
            }

            currentState = DuckyState.Dead;

            moveDust.Play(); //Dust on dead landing

            //Play random impact sound
            PlayRandomSound(impactSounds);
        }
        else if (onGround
        && body.velocity.y <= settings.yVelocityBuffer
        && body.velocity.y >= -settings.yVelocityBuffer
        && currentState != DuckyState.Dead
        && currentState != DuckyState.Wave)
        {
            if (shouldJump == false)
            {
                HandleFootstepSounds();
            }
            OnLanding();
            currentState = DuckyState.Idle;
        }
        else if (!onGround
        && body.velocity.y < 0
        && IsFallingState)
        {
            airborneTime += Time.deltaTime;
            freefallTime += Time.deltaTime;
            currentState = DuckyState.Jumping;  //Swap to jumping animation if descending
        }
        else if (!onGround)
        {
            airborneTime += Time.deltaTime;
            freefallTime += Time.deltaTime;
        }

        //If moving on ground play Running or Walking animation
        if (Mathf.Abs(moveAxisInput) > 0
        && isRunHeld
        && onGround
        && canInput
        && currentState != DuckyState.Jumping)
        {
            currentState = DuckyState.Running;
        }
        else if (Mathf.Abs(moveAxisInput) > 0
        && onGround
        && canInput
        && currentState != DuckyState.Jumping
        )
        {
            currentState = DuckyState.Walking;
        }

        //IDLE WAVE ANIMATION METHOD
        HandleIdleWaveAnimation();

        //Jump Buffer Logic.  
        UpdateJumpBuffer();

        //Debugging Jumping:
        /* if (Input.GetButton("Jump"))
        {
           Debug.Log("Y Velocity: " + body.velocity.y  + "\n" + 
                     "jumpBufferCounter:" + jumpBufferCounter + "\n" + 
                     "canInput:" + canInput + "\n" + 
                     "onGround:" + onGround + "\n" +
                     "airborneTime:" + airborneTime + "\n" +
                     "settings.coyoteTime:" + settings.coyoteTime + "\n" + 
                     "shouldJump:" + shouldJump + "\n" + 
                     "all together:" + (
                        jumpBufferCounter >= 0f 
                        && canInput 
                        && airborneTime <= settings.coyoteTime 
                        && shouldJump));
        } */

        //Jump Logic
        if (isOnWall && jumpBufferCounter > 0f)
        {
            HandleWallJump();
        }
        
        HandleJump();
        

        //Small Jump Code. 
        if (isJumpReleased && body.velocity.y > 0)
        {
            currentState = DuckyState.Jumping;
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * .5f);
        }

        //Higher Gravity when past apex of jump
        if (body.velocity.y < 0 || (body.velocity.y > 0 && !isJumpPressed))
        {
            body.velocity += (settings.fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }

        //Flapping code. 
        if (isJumpHeld && !onGround && flapDuration < settings.maxFlapDuration)
        {
            // Check if Ducky is either in Falling state or airborne beyond coyote time
            if ((currentState == DuckyState.Falling || airborneTime > settings.coyoteTime) && body.velocity.y < 0)
            {
                freefallTime = 0f; // Reset freefall time
                currentState = DuckyState.Flapping;
            }
        }

        //If Jump is released, Release the flap while saving flap time.
        if (isJumpReleased
       && (currentState == DuckyState.Flapping || currentState == DuckyState.Tired)
       && flapDuration < settings.maxFlapDuration)
        {
            currentState = DuckyState.Jumping;
        }
        //Go to tired as approaching settings.maxFlapDuration
        if (isJumpHeld
        && flapDuration + .5f >= settings.maxFlapDuration)
        {
            currentState = DuckyState.Tired;
        }

        //Go to Tired Fall if past settings.maxFlapDuration
        if (flapDuration >= settings.maxFlapDuration)
        {
            currentState = DuckyState.TiredFall;
        }

        //This is the ducky going into a fall animation if going to faceplant.
        if (body.velocity.y < 0
        && currentState != DuckyState.TiredFall
        && freefallTime > settings.deadThreshold)
        {
            currentState = DuckyState.Falling;
        }

        //Fall Speed Clamping
        if (body.velocity.y < -10)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, settings.maxFallVelocity);
        }

        //Adjust camera with a jump
        if (body.velocity.y < 0 && !onGround)
        {
            cameraFollow.AdjustCameraForJump(!onGround);
        }

        //Go to Menu with Escape
        if (isExitPressed)
        {
            SceneManager.LoadScene("Menu");
            PlayerPrefs.SetFloat("PlayerXLocation", transform.position.x);
            PlayerPrefs.SetFloat("PlayerYLocation", transform.position.y);
        }

    }

    private void UpdateInputs()
    {
        isJumpPressed = controls.Player.Jump.triggered;
        isJumpReleased = controls.Player.Jump.WasReleasedThisFrame();
        isJumpHeld =  controls.Player.Jump.ReadValue<float>() > 0;
        isRunHeld = controls.Player.Run.ReadValue<float>() > 0;
        moveAxisInput = controls.Player.Move.ReadValue<float>();
        isExitPressed = controls.Player.Exit.triggered;
    }

    void FixedUpdate()
    {
        UpdateMovement();

        RaycastHit2D hitRight = Physics2D.BoxCast(transform.position + (Vector3)wallBoxCastOffset, wallBoxCastSize, 0f, Vector2.right, wallBoxCastDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position + (Vector3)wallBoxCastOffset, wallBoxCastSize, 0f, Vector2.left, wallBoxCastDistance, wallLayer);
        isOnWall = (hitRight.collider != null || hitLeft.collider != null) && !onGround;

        if (isOnWall && (currentState != DuckyState.TiredFall || currentState == DuckyState.Dead))
        {
            HandleWallSlide();
        }
        else if (currentState == DuckyState.WallSlide)
        {
            currentState = DuckyState.Jumping;
        }

        // Debugging feature: Fly upwards when 'U' key and 'P' are pressed together.
        #if UNITY_EDITOR
        if (Input.GetKey(KeyCode.U) && Input.GetKey(KeyCode.P))
        {
            body.AddForce(Vector2.up, ForceMode2D.Impulse);
            OnLanding();
        }
        #endif

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
                HandleFlap();
                ChangeAnimationState(DUCKY_FLAP);
                break;

            case DuckyState.Falling:
                ChangeAnimationState(DUCKY_FALL);
                break;

            case DuckyState.WallSlide:
                ChangeAnimationState(DUCKY_WALL_SLIDE);
                break;

            case DuckyState.Tired:
                HandleFlap();
                ChangeAnimationState(DUCKY_TIRED);
                break;

            case DuckyState.TiredFall:
                ChangeAnimationState(DUCKY_TIRED_FALL);
                break;

            case DuckyState.Wave:
                ChangeAnimationState(DUCKY_WAVE);
                break;

            case DuckyState.Dead:
                ChangeAnimationState(DUCKY_DEAD);
                break;

            case DuckyState.Idle:
                ChangeAnimationState(DUCKY_IDLE);
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
        // Reduce the cooldown timer by the time since the last frame
        pushCooldownTimer -= Time.deltaTime;

        //float horizontalInput = Input.GetAxis("Horizontal");
        float horizontalInput = moveAxisInput;
        float targetSpeed = isRunHeld ? settings.maxRunSpeed : settings.maxWalkSpeed;
        float acceleration = isRunHeld ? settings.runAcceleration : settings.walkAcceleration;
        float deceleration = settings.deceleration;
        float velocityStopThreshold = settings.velocityStopThreshold;

        if (canInput && pushCooldownTimer <= 0f)
        {
            if(onGround)
            {   //GROUND MOVEMENT LOGIC
                if(Mathf.Abs(horizontalInput) > 0) 
                {   //ACCELERATE
                    if (Mathf.Abs(body.velocity.x) < targetSpeed)
                    {
                        // Apply an accelerating force based on the difference in speed
                        float targetVelocityX = horizontalInput * targetSpeed;
                        float speedDiff = targetVelocityX - body.velocity.x;
                        float force = speedDiff * acceleration;
                        body.AddForce(new Vector2(force, 0), ForceMode2D.Force);
                    }
                }
                else 
                {   //DECELERATE
                     HandleDeceleration(deceleration, velocityStopThreshold);
                }
            }
            else
            {   //AIR MOVEMENT LOGIC
                float targetVelocityX = horizontalInput * settings.airMaxSpeed;
                deceleration = settings.airDeceleration;
                if (Mathf.Abs(body.velocity.x) < settings.airMaxSpeed)
                {
                    float airControlForce = horizontalInput * settings.airControlStrength;
                    body.AddForce(new Vector2(airControlForce, 0), ForceMode2D.Force);
                }
                else //DECELERATE
                {
                    HandleDeceleration(deceleration, velocityStopThreshold);
                }
            }
            
        }

        // Change the character facing direction
        if (canInput && ((horizontalInput > 0 && !facingRight) || (horizontalInput < 0 && facingRight)))
        {
            FlipCharacter();
        }
    }
    private void HandleDeceleration(float deceleration, float velocityStopThreshold)
    {
        if (Mathf.Abs(body.velocity.x) > velocityStopThreshold)
        {
            float decelerationForce = -Mathf.Sign(body.velocity.x) * deceleration;
            body.AddForce(new Vector2(decelerationForce, 0), ForceMode2D.Force);
        }
        else
        {
            // Clamp velocity to zero if it's within the threshold
            body.velocity = new Vector2(0, body.velocity.y);
        }
    }
    public void ApplyPushForce(Vector2 force, float cooldownTime)
    {
        flapDuration = 0.0f;

        currentState = DuckyState.Idle;
        currentState = DuckyState.Jumping;
        airborneTime = 0f;   // Reset airborneTime to Zero
        freefallTime = 0f;

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
        freefallTime = 0f;   // Reset freefallTime to Zero
        shouldJump = true;   // Reset the Should Jump Flag
    }
     private void UpdateJumpBuffer()
    {
        //Jump Buffer Logic.  
        if (isJumpPressed)
        {
            jumpBufferCounter = settings.jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    private void HandleJump()
    {
        // Jump Logic
        if (currentState != DuckyState.WallSlide
        && jumpBufferCounter > 0f
        && canInput
        && airborneTime <= settings.coyoteTime
        && shouldJump)
        {
            currentState = DuckyState.Jumping;

            // Cancel out any existing vertical velocity before applying the jump impulse
            body.velocity = new Vector2(body.velocity.x, 0);

            // Apply an impulse force upwards
            body.AddForce(Vector2.up * settings.jumpForce, ForceMode2D.Impulse);

            // Create Dust Particles
            moveDust.Play();

            // Play random jump sound.
            PlayRandomSound(jumpSounds);

            // Reset timers and flags
            shouldJump = false;
            jumpBufferCounter = 0f;
        }
    }
    private void HandleWallJump()
    {
        Vector2 jumpDirection = CalculateWallJumpDirection();
        body.velocity = new Vector2(0, 0); // Reset existing velocity
        body.AddForce(jumpDirection * settings.wallJumpForce, ForceMode2D.Impulse);

        currentState = DuckyState.Jumping; // Transition to jumping state
        // Create Dust Particles
        moveDust.Play();
        // Play random jump sound.
        PlayRandomSound(jumpSounds);

        // Reset timers and flags
        shouldJump = false;
        jumpBufferCounter = 0f;
    }
    private Vector2 CalculateWallJumpDirection()
    {
        // Calculate the jump direction based on the wall slide direction
        float angleInRadians = settings.wallJumpAngle * Mathf.Deg2Rad;
        float x = facingRight ? -Mathf.Cos(angleInRadians) : Mathf.Cos(angleInRadians);
        float y = Mathf.Sin(angleInRadians);

        return new Vector2(x, y).normalized;
    }
    private void HandleFlap()
    {
        flapDuration += Time.fixedDeltaTime;
        freefallTime = 0f;  

        body.velocity = new Vector2(body.velocity.x, body.velocity.y + settings.flapStrength * Time.fixedDeltaTime);
    }
    private void HandleWallSlide()
    {
        currentState = DuckyState.WallSlide;

        Vector2 slideForce = new Vector2(0, settings.wallSlideSpeed);
        body.AddForce(slideForce, ForceMode2D.Force);

        flapDuration = 0.0f; // Reset flapDuration to maximum
        airborneTime = 0f;   // Reset airborneTime to Zero
        freefallTime = 0;    // Reset freefallTime to Zero
    }
    private bool IsFallingState => currentState == DuckyState.Falling || currentState == DuckyState.TiredFall;
    private AudioClip lastPlayedClip;
    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length == 0) return;

        AudioClip clipToPlay;
        do
        {
            clipToPlay = clips[Random.Range(0, clips.Length)];
        }
        while (clipToPlay == lastPlayedClip && clips.Length > 1); // Ensure there's an alternative clip to choose

        lastPlayedClip = clipToPlay; // Remember the last played clip

        audioPlayer.clip = clipToPlay;
        audioPlayer.Play();
    }
    private AudioClip lastPlayedFootstep;
    private void PlayRandomFootstep(List<AudioClip> footstepClips)
    {
        if (footstepClips == null || footstepClips.Count == 0) return;

        AudioClip clipToPlay;
        do
        {
            clipToPlay = footstepClips[Random.Range(0, footstepClips.Count)];
        }
        while (clipToPlay == lastPlayedClip && footstepClips.Count > 1); // Ensure there's an alternative clip to choose

        lastPlayedFootstep = clipToPlay; // Remember the last played clip

        footstepsAudioSource.clip = clipToPlay;
        footstepsAudioSource.pitch = Random.Range(0.9f, 1.1f);
        //footstepsAudioSource.Play();
        footstepsAudioSource.PlayOneShot(clipToPlay);
    }
    private void HandleFootstepSounds()
    {
        if (onGround)
        {
            SelectAndPlayFootstepSound();
        }
    }
    private void SelectAndPlayFootstepSound()
    {
        if (!onGround) return;

        List<AudioClip> selectedFootstepSounds = null;
        selectedFootstepSounds = footStepMaterial switch
        {
            groundMaterial.Ground => GroundFootSteps,
            groundMaterial.Grass => GrassFootSteps,
            groundMaterial.Metal => MetalFootSteps,
            groundMaterial.Wood => WoodFootSteps,
            groundMaterial.Ice => IceFootSteps,
            _ => GroundFootSteps,
        };
        if (selectedFootstepSounds != null)
        {
            PlayRandomFootstep(selectedFootstepSounds);
        }
    }
    private void FacePlant()
    {
        CameraShakeManager.instance.CameraShake(impulseSource);
        StartCoroutine(InputDelayCoroutine());
    }
    private void HandleIdleWaveAnimation()
    {
        // Update the idle timer if in Idle state
        if (currentState == DuckyState.Idle || currentState == DuckyState.Wave)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeBeforeWave)
            {
                // Trigger waving animation
                currentState = DuckyState.Wave;
                idleTimer = 0f; // Reset timer
                if (idleTimeBeforeWave > 1)
                {
                    idleTimeBeforeWave--; //Slowly increase how often Ducky Waves
                }
            }
        }
        else
        {
            idleTimer = 0f; // Reset timer if not idle
            idleTimeBeforeWave = 8.1f; // Reset to original value, should probably not hard code it
        }
    }

    IEnumerator InputDelayCoroutine()
    {
        //disable further Input
        canInput = false;

        //wait for specified delay
        yield return new WaitForSeconds(settings.faceplantInputLockTime);

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
        freefallTime = 0f;   // Reset freefallTime to Zero

        float totalForce = bounceForce;

        bounceDust.Play();  // Create Bounce Particles

        if (jumpBufferCounter > -settings.jumpBufferTime)
        {
            totalForce += settings.jumpForce;
        }
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force

        jumpBufferCounter = 0f;
    }
    public void ResetToIdleState() { currentState = DuckyState.Idle; }
    public void ChangeGroundMaterial(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            // Check the tag of the collider
            footStepMaterial = hit.collider.tag switch
            {
                "Ground" => groundMaterial.Ground,
                "Grass" => groundMaterial.Grass,
                "Metal" => groundMaterial.Metal,
                "Wood" => groundMaterial.Wood,
                "Ice" => groundMaterial.Ice,
                _ => groundMaterial.Empty,
            };
        }
        else
        {
            // No ground detected
            footStepMaterial = groundMaterial.Empty;
        }
    }

    private void OnDrawGizmos()
    {
        // GROUND BOX CAST
        // The center of the box cast, slightly above the player to ensure it starts inside the player collider
        Vector2 castOrigin = (Vector2)transform.position + (Vector2)boxCastOffset + Vector2.up * 0.05f;

        // Calculate the four corners of the box
        Vector2 bottomLeft = castOrigin + new Vector2(-boxCastSize.x / 2, -boxCastSize.y / 2 - castDistance);
        Vector2 bottomRight = castOrigin + new Vector2(boxCastSize.x / 2, -boxCastSize.y / 2 - castDistance);
        Vector2 topLeft = castOrigin + new Vector2(-boxCastSize.x / 2, boxCastSize.y / 2);
        Vector2 topRight = castOrigin + new Vector2(boxCastSize.x / 2, boxCastSize.y / 2);

        Color groundBoxColor = onGround ? Color.green : Color.red;

        // Draw the box using Debug.DrawLine
        Debug.DrawLine(bottomLeft, bottomRight, groundBoxColor); // Bottom
        Debug.DrawLine(topLeft, topRight, groundBoxColor);       // Top
        Debug.DrawLine(bottomLeft, topLeft, groundBoxColor);     // Left
        Debug.DrawLine(bottomRight, topRight, groundBoxColor);   // Right

        //-------------------------------------------------------//
        // WALL BOX CASTS
        // Set the color of the gizmo
        Gizmos.color = isOnWall ? Color.green : Color.red;

        // Right-side gizmo
        Vector3 boxCastOriginRight = transform.position + (Vector3)wallBoxCastOffset + Vector3.right * wallBoxCastDistance / 2;
        Vector3 sizeRight = new(wallBoxCastSize.x, wallBoxCastSize.y, 1);
        Gizmos.DrawWireCube(boxCastOriginRight, sizeRight);

        // Left-side gizmo
        Vector3 boxCastOriginLeft = transform.position + (Vector3)wallBoxCastOffset + Vector3.left * wallBoxCastDistance / 2;
        Vector3 sizeLeft = new(wallBoxCastSize.x, wallBoxCastSize.y, 1);
        Gizmos.DrawWireCube(boxCastOriginLeft, sizeLeft);

    }


}
