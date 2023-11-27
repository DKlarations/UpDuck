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

    enum DuckyState { Idle, Walking, Running, Jumping, Flapping, Wave, Falling, Tired, TiredFall, Dead } // The state machine variable
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
    private float walkSpeed = 7f;
    [SerializeField] private LayerMask groundLayer;
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
    private bool canInput = true;
    [HideInInspector]public float horizontalPush = 0f;

    private float airborneTime;
    [Header("Ground Detection")]
    public Vector3 boxCastOffset;
    public Vector2 boxCastSize;
    public float castDistance;

    //======//
    //INPUTS//
    //======//
    private bool isJumpPressed;
    private bool isJumpReleased;
    private bool isJumpHeld;
    private bool isRunHeld;
    private float moveAxisInput;

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

/*         if (onGround)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            float slopeDifficultyFactor = CalculateSlopeDifficultyFactor(slopeAngle);
            walkSpeed = settings.originalWalkSpeed * slopeDifficultyFactor;
            Debug.Log("The Walkspeed is: " + walkSpeed);
        }
        else
        {
            walkSpeed = settings.originalWalkSpeed;
        } */





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
            currentState = DuckyState.Jumping;  //Swap to jumping animation if descending
        }
        else if (!onGround)
        {
            airborneTime += Time.deltaTime;
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
            body.velocity += Vector2.up * Physics2D.gravity.y * (settings.fallMultiplier - 1) * Time.deltaTime;
        }

        //Flapping code. 
        if (isJumpHeld
        && !onGround
        && flapDuration < settings.maxFlapDuration
        && body.velocity.y < 0
        && airborneTime > settings.coyoteTime)
        {
            currentState = DuckyState.Flapping;
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
        && airborneTime > settings.deadThreshold)
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
        if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return))
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
    }

    void FixedUpdate()
    {
        UpdateMovement();

        // Debugging feature: Fly upwards when 'P' key and 'I' are pressed together.
        if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.I))
        {
            body.AddForce(Vector2.up * 1f, ForceMode2D.Impulse);
            OnLanding();
        }


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
        //float horizontalInput = Input.GetAxis("Horizontal");
        float horizontalInput = moveAxisInput;
        float speedMultiplier = 1f;  // Default speed multiplier

        // Reduce the cooldown timer by the time since the last frame
        pushCooldownTimer -= Time.deltaTime;

        // Check if shift key is held down to increase speed
        /* if (onGround && (Input.GetKey(KeyCode.LeftShift)
        || Input.GetKey(KeyCode.RightShift)
        || Input.GetButton("Fire3"))) */

        if (onGround
        && isRunHeld)
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

        //Play Footsteps
        //HandleFootstepSounds();
    }
    private float CalculateSlopeDifficultyFactor(float slopeAngle)
    {
        if (slopeAngle <= 0f) return 1f; // NO SLOPE

        float difficultyFactor = Mathf.Clamp01(1 - (slopeAngle / settings.steepestClimbableAngle));
        return difficultyFactor;
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
        if (jumpBufferCounter > 0f
        && canInput
        && airborneTime <= settings.coyoteTime
        && shouldJump)
        {
            currentState = DuckyState.Jumping;

            // Cancel out any existing vertical velocity before applying the jump impulse
            body.velocity = new Vector2(body.velocity.x, 0);

            // Apply an impulse force upwards
            body.AddForce(Vector2.up * settings.jumpSpeed, ForceMode2D.Impulse);

            // Create Dust Particles
            moveDust.Play();

            // Play random jump sound.
            PlayRandomSound(jumpSounds);

            // Reset timers and flags
            shouldJump = false;
            jumpBufferCounter = 0f;
        }
    }
    private void HandleFlap()
    {
        flapDuration += Time.fixedDeltaTime;
        body.velocity = new Vector2(body.velocity.x, body.velocity.y + settings.flapStrength * Time.fixedDeltaTime);
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
        if (currentState == DuckyState.Idle)
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

        float totalForce = bounceForce;

        bounceDust.Play();  // Create Bounce Particles

        if (jumpBufferCounter > -settings.jumpBufferTime)
        {
            totalForce += settings.jumpSpeed;
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

        Color boxColor = onGround ? Color.green : Color.red;

        // Draw the box using Debug.DrawLine
        Debug.DrawLine(bottomLeft, bottomRight, boxColor); // Bottom
        Debug.DrawLine(topLeft, topRight, boxColor);       // Top
        Debug.DrawLine(bottomLeft, topLeft, boxColor);     // Left
        Debug.DrawLine(bottomRight, topRight, boxColor);   // Right

    }


}
