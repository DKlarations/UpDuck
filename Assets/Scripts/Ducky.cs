using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Ducky : MonoBehaviour
{
    enum DuckyState { Idle, Running, Jumping, Flapping, Rolling, Falling, Tired, TiredFall, Dead } // The state machine variable
    private DuckyState currentState = DuckyState.Idle;
    private CinemachineImpulseSource impulseSource;
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
    [SerializeField] private float faceplantInputLockTime = .5f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallVelocity = 30f;
    [SerializeField] private float yVelocityBuffer = .1f;
    private float jumpBufferCounter;

    public Animator animator;
    private string currentAnimation = "Ducky Idle";
    private Rigidbody2D body;
    public ParticleSystem moveDust;
    public ParticleSystem bounceDust;
    private bool shouldJump;
    private bool onGround;
    private bool facingRight = true;
    private bool canInput = true;
    
    public float horizontalPush = 0f;
    
    private float airborneTime;
    [SerializeField] private float deadThreshold = 1f;
    public Vector3 raycastOffset;
    
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
    const string DUCKY_ROLL = "Ducky Roll";
    const string DUCKY_WALK = "Ducky Walk";

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.515f, groundLayer);

        //New Raycast logic, tying it to a Boolean, returns true if either ray hits groundlayer
        onGround = Physics2D.Raycast(transform.position + raycastOffset, Vector2.down, 0.515f, groundLayer) 
                 || Physics2D.Raycast(transform.position - raycastOffset, Vector2.down, 0.515f, groundLayer);
        Debug.DrawRay(transform.position + raycastOffset, Vector2.down * 0.515f, Color.red);
        Debug.DrawRay(transform.position - raycastOffset, Vector2.down * 0.515f, Color.red);
 

        UpdateMovement();
        
        //Landing logic, check for faceplant, check for idle, otherwise Ducky is not on ground.
        if (onGround
        && (currentState == DuckyState.TiredFall || currentState == DuckyState.Falling)
        && body.velocity.y <= yVelocityBuffer
        && body.velocity.y >= -yVelocityBuffer
        )
        {
            OnLanding();
            FacePlant();
            
            currentState = DuckyState.Dead;

            moveDust.Play(); //Dust on dead landing

            AudioClip clipToPlay = impactSounds[Random.Range(0, impactSounds.Length)];
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play(); 
            
        }
        else if (onGround
        && currentState != DuckyState.Dead)  //This was changed to not include y velocity into account
        {
            OnLanding();
            currentState = DuckyState.Idle;
        } 
        else if (onGround)  //This is because the player would not be able to input with dead state on moving platform
        {
            OnLanding();
        } 
        else
        {
            airborneTime += Time.deltaTime;
        }

        //Swap to jumping if in dead and falling after dead delay.
        if(currentState == DuckyState.Dead && body.velocity.y != 0 && canInput)
        {
            currentState = DuckyState.Jumping;
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 
        && onGround 
        && canInput 
        && currentState != DuckyState.Jumping) //Added this to help with overriding the jump
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
        if ( jumpBufferCounter > 0f
        && canInput
        && onGround
        && airborneTime <= coyoteTime
        && shouldJump)
        {
            shouldJump = true;

            if (shouldJump) /////////////////////////////////////////////TAKE A LOOK AT THIIIIIS
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
            currentState = DuckyState.TiredFall;
        }

        //Start Roll animation 
/*         if (Input.GetKey(KeyCode.S)
        && (currentState == DuckyState.Jumping || currentState == DuckyState.Falling))
        {
            currentState = DuckyState.Rolling;
        } */

        //       ???????????
        if (body.velocity.y < 0 
        && (currentState != DuckyState.Flapping || currentState != DuckyState.Dead || currentState != DuckyState.TiredFall)
        && airborneTime > deadThreshold)
        {
            currentState = DuckyState.Falling;
        }

        //Fall Speed Clamping
        if(body.velocity.y < 0)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxFallVelocity);
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
            case DuckyState.Running:
                ChangeAnimationState(DUCKY_WALK);
                break;

            case DuckyState.Jumping:                
                ChangeAnimationState(DUCKY_JUMP);
                break;

            case DuckyState.Rolling:                //DELETE IF UNUSED
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

            case DuckyState.TiredFall:
                ChangeAnimationState(DUCKY_TIRED_FALL);
                flapDuration += Time.fixedDeltaTime;
                body.velocity = new Vector2(body.velocity.x, body.velocity.y + flapStrength * Time.fixedDeltaTime);
                break;

            case DuckyState.Dead:
                body.velocity = new Vector2(0,0);  //Stop all momentum when faceplanting
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

        if (canInput)
        {
            horizontalPush = Mathf.SmoothStep(0, 1, .6f)*horizontalPush;
            if (Mathf.Abs(horizontalPush) < 1)
            {
                horizontalPush = 0;
            }
            body.velocity = new Vector2(horizontalInput * runSpeed + horizontalPush, body.velocity.y);
        }


        //change the character facing
        if (canInput && ((horizontalInput > 0 && !facingRight) || (horizontalInput < 0 && facingRight)))
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
        airborneTime = 0f;   // Reset airborneTime to Zero
        shouldJump = true;   // Reset the Should Jump Flag
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

        //Create Dust Particles on direction change if on ground
        if (onGround)
            {
                moveDust.Play();
            }
    }
    public void Bounce(float bounceForce)
    {
        //Create Bounce effect.
        float totalForce = bounceForce;
        bounceDust.Play(); //Create Bounce Particles

        if (jumpBufferCounter > 0f)
        { 
            totalForce += jumpSpeed;
        }
        currentState = DuckyState.Jumping;
        body.velocity = new Vector2(body.velocity.x, 0);  // Reset vertical velocity
        
       body.AddForce(new Vector2(0, totalForce), ForceMode2D.Impulse);  // Apply combined force
       // body.velocity = new Vector2(body.velocity.x, totalForce);
       //body.AddForce(Vector2.up*totalForce, ForceMode2D.Impulse);
    }
    
}
