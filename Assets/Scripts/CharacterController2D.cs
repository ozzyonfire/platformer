using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class Timer
{
  public float time { get; set; }
  private float timer = 0;
  private Action onFinish;
  public bool isRunning = false;

  public Timer(float time, Action onFinish)
  {
    this.time = time;
    this.onFinish = onFinish;
  }

  public Timer(float time)
  {
    this.time = time;
  }

  public void Start()
  {
    this.timer = time;
    this.isRunning = true;
  }

  public void Pause()
  {
    this.isRunning = false;
  }

  public void Resume()
  {
    this.isRunning = true;
  }

  public void Stop()
  {
    this.timer = 0;
    this.isRunning = false;
  }

  public void Update()
  {
    if (this.isRunning)
    {
      if (this.timer > 0f)
      {
        this.timer -= Time.deltaTime;
      }
      if (this.timer <= 0f)
      {
        this.onFinish?.Invoke();
        this.isRunning = false;
      }
    }
  }
}

public class CharacterController2D : MonoBehaviour
{
  public enum State { Grounded, Airborne, Wallsliding, Dashing, Dead, Stopped }
  [Header("State")]
  public State controllerState;

  [Header("Movement")]
  public float speed = 35f;
  [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
  [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
  [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
  [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
  [SerializeField] private LayerMask m_WhatIsObject;                          // A mask determining what is an interactable object to the character
  [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
  [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
  [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
  public float hangtime = 0.2f;
  private float hangTimer = 0;
  public float gravityScale = 3f;
  private bool isDashing = false;
  [SerializeField] private float dashForce = 100f;
  private float dashTimer = 0;
  [SerializeField] private int numberOfDashes = 1;                            // Number of times we can dash in the air
  private int dashesUsed = 0;
  [SerializeField] private float dashTime = 0.5f;                             // how long should the dash last
  public GameObject currentObject;
  public FixedJoint2D grabJoint;

  // input flags
  public bool isJumping = false;
  public bool isCrouching = false;
  public bool isGrabbing = false;

  [Header("Jumping")]
  [SerializeField] private float jumpForce = 600f;                            // Amount of force added when the player jumps.
  [Range(0, 1)] [SerializeField] private float shortHopFactor = 0.5f;         // Factor reducing the players jump height on a short hop
  public float shortHopTime = 0.2f;                                           // Anything over this time is considered a long jump
  public Timer shortHopTimer;
  public int numberOfJumps = 1;
  public int jumpsUsed = 0;
  public float timeBetweenJumps = 0.2f;
  private float jumpTimer = 0f;
  public float jumpBuffer = 0.3f;                                             // the amount of lee-way given on a jump button press
  private Timer jumpBufferTimer; 
  private bool jumpButtonUp = false;

  [Header("Wall Sliding")]
  [SerializeField] private float wallJumpSpeed = 12f;              // Amount of force added when the player jumps.
  [SerializeField] private LayerMask m_WhatIsWall;              // A mask determining what is ground to the character
  [SerializeField] private Transform m_WallCheck;             // A position marking where to check if the player is grounded.
  [Range(0, 1)] [SerializeField] private float m_WallslidingSpeed = 0.75f;      // Amount of maxSpeed applied to crouching movement. 1 = 100%
  [SerializeField] private bool noGravityOnHang = false;
  [SerializeField] private float wallJumpTime = 0.2f; // the amount of time after a wall jump that you can't move the player
  private Timer wallJumpTimer; // While running, you can't control the player

  [Header("Effects")]
  public ParticleSystem footsteps;

  const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
  const float k_CeilingRadius = .05f; // Radius of the overlap circle to determine if the player can stand up
  private Rigidbody2D m_Rigidbody2D;
  private bool m_FacingRight = true;  // For determining which way the player is currently facing.
  private Vector3 m_Velocity = Vector3.zero;
  private Animator m_Animator;

  [System.Serializable]
  public class BoolEvent : UnityEvent<bool> { }

  [Header("Events")]
  [Space]
  public UnityEvent OnLandEvent;
  public UnityEvent OnFlip;
  public UnityEvent OnHitWall;
  public UnityEvent OnFallEvent;
  public UnityEvent OnJumpEvent;
  public BoolEvent OnCrouchEvent;
  private bool m_wasCrouching = false;


  private float horizontalMovement = 0f;
  private bool isPushing;

  private void Awake()
  {
    m_Animator = this.GetComponent<Animator>();
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    grabJoint = this.GetComponent<FixedJoint2D>();

    m_Rigidbody2D.gravityScale = this.gravityScale;
    if (OnLandEvent == null)
      OnLandEvent = new UnityEvent();
    if (OnCrouchEvent == null)
      OnCrouchEvent = new BoolEvent();
    if (OnFallEvent == null)
      OnFallEvent = new UnityEvent();

    wallJumpTimer = new Timer(wallJumpTime);
    jumpBufferTimer = new Timer(jumpBuffer);
    shortHopTimer = new Timer(shortHopTime);
  }

  private void Update()
  {
    horizontalMovement = Input.GetAxisRaw("Horizontal") * speed;

    if (Input.GetButtonUp("Jump")) {
      jumpButtonUp = true;
    }

    if (Input.GetButtonDown("Jump"))
    {
      jumpBufferTimer.Start();
    }

    isJumping = jumpBufferTimer.isRunning;

    if (Input.GetButtonDown("Crouch"))
    {
      isCrouching = true;
    }
    else if (Input.GetButtonUp("Crouch"))
    {
      isCrouching = false;
    }

    if (Input.GetButtonDown("Grab"))
    {
      isGrabbing = true;
    } else if (Input.GetButtonUp("Grab"))
    {
      isGrabbing = false;
    }

    if (Input.GetButtonDown("Dash"))
    {
      isDashing = true;
    }

    this.Animate();
    this.Timers();
  }

  private void FixedUpdate()
  {
    if (controllerState == State.Dead || controllerState == State.Stopped)
    {
      return;
    }
    if (controllerState == State.Dashing)
    {
      dashTimer -= Time.fixedDeltaTime;
      m_Rigidbody2D.gravityScale = 0f;
      this.m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
      if (dashTimer < 0f)
      {
        m_Rigidbody2D.gravityScale = this.gravityScale;
        CheckSurroundings();
        Move(horizontalMovement * Time.fixedDeltaTime, isCrouching, isJumping, isGrabbing, isDashing);
      }
    } else
    {
      CheckSurroundings();
      Move(horizontalMovement * Time.fixedDeltaTime, isCrouching, isJumping, isGrabbing, isDashing);
    }
    isDashing = false;
    isJumping = false;
    jumpButtonUp = false;
  }

  // Sets the state of the player
  private void CheckSurroundings()
  {
    CheckGround();
    CheckWall();
    GrabObject();
  }

  private void CheckGround()
  {
    bool foundGround = false;
    // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
    Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
    foreach (Collider2D collider in colliders)
    {
      if (collider.gameObject != this.gameObject)
      {
        switch (this.controllerState)
        {
          case State.Airborne:
            this.CreateDust();
            OnLandEvent.Invoke();
            break;
        }
        this.controllerState = State.Grounded; // now on the ground
        foundGround = true;
      }
    }

    if (!foundGround)
    {
      controllerState = State.Airborne; // airborne
    }
  }
  private void CheckWall()
  {
    // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
    Collider2D[] colliders = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsWall);
    foreach (Collider2D collider in colliders)
    {
      if (collider.gameObject != this.gameObject)
      {
        switch (this.controllerState)
        {
          case State.Airborne:
            OnHitWall.Invoke();
            this.controllerState = State.Wallsliding; // now wallsliding
            break;
          case State.Grounded:
            // do nothing
            break;
          default:
            this.controllerState = State.Wallsliding;
            break;
        }
      }
    }
  }

  private void GrabObject()
  {
    Collider2D collider = Physics2D.OverlapCircle(m_WallCheck.position, k_GroundedRadius, m_WhatIsObject);
    if (collider && collider.gameObject != this.gameObject)
    {
      // touching something 
      isPushing = true;
      if (isGrabbing)
      {
        FixedJoint2D objectJoint = collider.gameObject.GetComponent<FixedJoint2D>();
        objectJoint.enabled = true;
        objectJoint.connectedBody = m_Rigidbody2D;
        currentObject = collider.gameObject;
      } else if (currentObject != null)
      {
        FixedJoint2D objectJoint = currentObject.GetComponent<FixedJoint2D>();
        objectJoint.enabled = false;
        objectJoint.connectedBody = null;
        currentObject = null;
      }
    } else
    {
      isPushing = false;
    }
  }

  public void Move(float move, bool crouch, bool jump, bool grab, bool dash)
  {
    // If crouching, check to see if the character can stand up
    if (!crouch)
    {
      // If the character has a ceiling preventing them from standing up, keep them crouching
      if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
      {
        crouch = true;
      }
    }

    this.Movement(move, crouch);
    m_Rigidbody2D.gravityScale = this.gravityScale;
    switch (controllerState)
    {
      case State.Grounded:
        hangTimer = hangtime;
        jumpTimer -= Time.fixedDeltaTime;
        jumpsUsed = 0;
        dashesUsed = 0;
        if (jump)
        {
          this.Jump();
        }
        if (dash)
        {
          this.Dash();
        }
        break;
      case State.Airborne:
        hangTimer -= Time.fixedDeltaTime;
        jumpTimer -= Time.fixedDeltaTime;
        if (jump && (jumpsUsed < numberOfJumps) && (jumpTimer < 0f))
        {
          if (jumpsUsed == 0 && hangTimer > 0f) // first jump
          {
            this.CoyoteJump();
          } else if (jumpsUsed > 0) // double jump
          {
            this.Jump();
          }
        }
        if (jumpButtonUp && shortHopTimer.isRunning && m_Rigidbody2D.velocity.y > 0 && jumpsUsed > 0) // still jumping
        {
          this.m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * shortHopFactor);
          shortHopTimer.Stop();
        }
        if (dash)
        {
          this.Dash();
        }
        break;
      case State.Wallsliding:
        hangTimer = hangtime;
        jumpsUsed = 0;
        if (jump && !wallJumpTimer.isRunning)
        {
          this.WallJump();
        } else if(grab)
        {
          // slow the character on the wall
          m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * m_WallslidingSpeed);
          if (noGravityOnHang) m_Rigidbody2D.gravityScale = 0f;
        }
        //wallJumpTimer.Stop(); // in the air so reset the timer
        break;
    }
  }

  private void Movement(float move, bool crouch)
  {
    if (wallJumpTimer.isRunning)
    {
      return;
    }

    //only control the player if grounded or airControl is turned on
    if (controllerState == State.Grounded || m_AirControl)
    {
      // If crouching
      if (crouch)
      {
        if (!m_wasCrouching)
        {
          m_wasCrouching = true;
          OnCrouchEvent.Invoke(true);
        }

        // Reduce the speed by the crouchSpeed multiplier
        move *= m_CrouchSpeed;

        // Disable one of the colliders when crouching
        if (m_CrouchDisableCollider != null)
          m_CrouchDisableCollider.enabled = false;
      }
      else
      {
        // Enable the collider when not crouching
        if (m_CrouchDisableCollider != null)
          m_CrouchDisableCollider.enabled = true;

        if (m_wasCrouching)
        {
          m_wasCrouching = false;
          OnCrouchEvent.Invoke(false);
        }
      }

      // Move the character by finding the target velocity
      Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
      // And then smoothing it out and applying it to the character
      m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

      // If the input is moving the player right and the player is facing left...
      if (move > 0 && !m_FacingRight && currentObject == null)
      {
        // ... flip the player.
        Flip();
      }
      // Otherwise if the input is moving the player left and the player is facing right...
      else if (move < 0 && m_FacingRight && currentObject == null)
      {
        // ... flip the player.
        Flip();
      }
    }
  }

  private void _Jump(Vector2 speed, bool addForce)
  {
    this.controllerState = State.Airborne;
    jumpsUsed++;
    jumpTimer = timeBetweenJumps;
    hangTimer = 0;
    shortHopTimer.Start();
    this.CreateDust();
    if (addForce)
    {
      m_Rigidbody2D.AddForce(speed, ForceMode2D.Impulse);
    } else
    {
      m_Rigidbody2D.velocity = speed;
    }
    OnJumpEvent.Invoke();
  }

  private void Jump()
  {
    jumpBufferTimer.Stop();
    this._Jump(new Vector2(0f, jumpForce), true);
  }

  private void CoyoteJump()
  {
    jumpBufferTimer.Stop();
    this._Jump(new Vector2(m_Rigidbody2D.velocity.x, jumpForce), false);
  }

  private void WallJump()
  {
    this.Flip();
    wallJumpTimer.Start();
    this._Jump(new Vector2(this.wallJumpSpeed * (m_FacingRight ? 1 : -1), this.wallJumpSpeed), false);
  }

  private void Dash()
  {
    if (dashesUsed < numberOfDashes)
    {
      dashesUsed++;
      this.controllerState = State.Dashing;
      dashTimer = dashTime;
      //this.m_Rigidbody2D.velocity = new Vector2(this.dashForce * (m_FacingRight ? 1 : -1), 0f);
      this.m_Rigidbody2D.AddForce(new Vector2(this.dashForce * (m_FacingRight ? 1 : -1), 0f), ForceMode2D.Impulse);
    }
  }

  private void Flip()
  {
    // Switch the way the player is labelled as facing.
    m_FacingRight = !m_FacingRight;

    // Multiply the player's x local scale by -1.
    Vector3 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;

    // Effects
    theScale = footsteps.transform.localScale;
    theScale.x *= -1;
    footsteps.transform.localScale = theScale;

    if (this.controllerState != State.Airborne)
    {
      this.CreateDust(); // don't create dust in mid air
    }

    OnFlip.Invoke();
  }

  private void Animate()
  {
    bool isFalling = m_Rigidbody2D.velocity.y < 0;
    switch(this.controllerState)
    {
      case State.Grounded:
        if (isGrabbing && currentObject != null)
        {
          m_Animator.Play("Push");
        } else if (Math.Abs(horizontalMovement) > 0)
        {
          if (isPushing)
          {
            m_Animator.Play("Push");
          } else
          {
            m_Animator.Play("Run");
          }
        } else
        {
          m_Animator.Play("Idle");
        }
        break;
      case State.Airborne:
        if (isFalling)
        {
          m_Animator.Play("Fall");
        } else
        {
          m_Animator.Play("Jump");
        }
        break;
      case State.Wallsliding:
        if (isGrabbing)
        {
          m_Animator.Play("Wall Jump");
        } else if (isFalling)
        {
          m_Animator.Play("Fall");
        } else
        {
          m_Animator.Play("Jump");
        }
        break;
      case State.Dashing:
        m_Animator.Play("Dash");
        break;
      case State.Dead:
        m_Animator.Play("Die");
        break;
    }
  }

  private void CreateDust()
  {
    footsteps.Play();
  }

  // Timers that should be decremented every frame
  private void Timers()
  {
    wallJumpTimer.Update();
    jumpBufferTimer.Update();
    shortHopTimer.Update();
  }

  public void Die()
  {
    this.m_Rigidbody2D.velocity = Vector2.zero;
    this.m_Rigidbody2D.gravityScale = 0f;
    this.controllerState = State.Dead;
  }

  public void Stop()
  {
    this.controllerState = State.Stopped;
  }
}