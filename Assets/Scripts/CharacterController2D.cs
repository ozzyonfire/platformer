using UnityEngine;
using UnityEngine.Events;
using System;

public class CharacterController2D : MonoBehaviour
{
  public enum State { Grounded, Airborne, Wallsliding, Dashing }
  [Header("State")]
  public State controllerState;

  [Header("Movement")]
  public float speed = 35f;
  [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;      // Amount of maxSpeed applied to crouching movement. 1 = 100%
  [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
  [SerializeField] private bool m_AirControl = false;             // Whether or not a player can steer while jumping;
  [SerializeField] private LayerMask m_WhatIsGround;              // A mask determining what is ground to the character
  [SerializeField] private Transform m_GroundCheck;             // A position marking where to check if the player is grounded.
  [SerializeField] private Transform m_CeilingCheck;              // A position marking where to check for ceilings
  [SerializeField] private Collider2D m_CrouchDisableCollider;        // A collider that will be disabled when crouching
  public float hangtime = 0.2f;
  private float hangTimer = 0;
  public float gravityScale = 3f;
  private bool isDashing = false;
  [SerializeField] private float dashForce = 100f;
  private float dashTimer = 0;
  [SerializeField] private float dashTime = 0.5f; // how long should the dash last

  // input flags
  private bool isJumping = false;
  private bool isCrouching = false;
  private bool isGrabbing = false;

  [Header("Jumping")]
  [SerializeField] private float jumpForce = 600f;              // Amount of force added when the player jumps.
  [Range(0, 1)] [SerializeField] private float shortHopFactor = 0.5f;              // Amount of force added when the player jumps.
  public float shortHopTime = 0.2f; // anything over this time is considered a long jump
  private float shortHopTimer = 0f;
  public int numberOfJumps = 1;
  public int jumpsUsed = 0;
  public float timeBetweenJumps = 0.2f;
  private float jumpTimer = 0f;
  public float jumpBuffer = 0.3f;
  private float jumpBufferCounter = 0f;
  private bool jumpButtonUp = false;

  [Header("Wall Sliding")]
  [SerializeField] private float wallJumpSpeed = 12f;              // Amount of force added when the player jumps.
  [SerializeField] private LayerMask m_WhatIsWall;              // A mask determining what is ground to the character
  [SerializeField] private Transform m_WallCheck;             // A position marking where to check if the player is grounded.
  [Range(0, 1)] [SerializeField] private float m_WallslidingSpeed = 0.75f;      // Amount of maxSpeed applied to crouching movement. 1 = 100%
  [SerializeField] private bool noGravityOnHang = false;

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

  private void Awake()
  {
    m_Animator = this.GetComponent<Animator>();
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    m_Rigidbody2D.gravityScale = this.gravityScale;
    if (OnLandEvent == null)
      OnLandEvent = new UnityEvent();
    if (OnCrouchEvent == null)
      OnCrouchEvent = new BoolEvent();
    if (OnFallEvent == null)
      OnFallEvent = new UnityEvent();
  }

  private void Update()
  {
    horizontalMovement = Input.GetAxisRaw("Horizontal") * speed;
    if (Input.GetButton("Jump"))
    {
      shortHopTimer += Time.deltaTime;
    }

    if (Input.GetButtonUp("Jump")) {
      jumpButtonUp = true;
    } else
    {
      jumpButtonUp = false;
    }

    if (Input.GetButtonDown("Jump"))
    {
      shortHopTimer = 0f;
      jumpBufferCounter = jumpBuffer;
    }
    else
    {
      jumpBufferCounter -= Time.deltaTime;
    }

    if (jumpBufferCounter > 0f)
    {
      isJumping = true;
    }
    else
    {
      isJumping = false;
    }

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
  }

  private void FixedUpdate()
  {
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
  }

  // Sets the state of the player
  private void CheckSurroundings()
  {
    CheckGround();
    CheckWall();
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
      if (move > 0 && !m_FacingRight)
      {
        // ... flip the player.
        Flip();
      }
      // Otherwise if the input is moving the player left and the player is facing right...
      else if (move < 0 && m_FacingRight)
      {
        // ... flip the player.
        Flip();
      }
    }

    m_Rigidbody2D.gravityScale = this.gravityScale;
    switch (controllerState)
    {
      case State.Grounded:
        hangTimer = hangtime;
        jumpTimer -= Time.fixedDeltaTime;
        jumpsUsed = 0;
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
            this.Jump();
          } else if (jumpsUsed > 0) // double jump
          {
            this.Jump();
          }
        }
        if (jumpButtonUp && (shortHopTimer < shortHopTime) && m_Rigidbody2D.velocity.y > 0) // still jumping
        {
          this.m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * shortHopFactor);
        }
        if (dash)
        {
          this.Dash();
        }
        break;
      case State.Wallsliding:
        hangTimer = hangtime;
        jumpsUsed = 0;
        if (jump)
        {
          this.WallJump();
        } else if(grab)
        {
          // slow the character on the wall
          m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * m_WallslidingSpeed);
          if (noGravityOnHang) m_Rigidbody2D.gravityScale = 0f;
        }
        break;
    }
  }

  private void Jump()
  {
    this.controllerState = State.Airborne;
    // Add a vertical force to the player.
    m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    //m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, this.jumpSpeed);
    jumpsUsed++;
    jumpTimer = timeBetweenJumps;
    hangTimer = 0;
    jumpBufferCounter = 0;
    this.CreateDust();
    OnJumpEvent.Invoke();
  }

  private void WallJump()
  {
    this.controllerState = State.Airborne;
    this.Flip();
    jumpsUsed++;
    jumpTimer = timeBetweenJumps;
    hangTimer = 0;
    this.CreateDust();
    this.m_Rigidbody2D.AddForce(new Vector2(this.wallJumpSpeed * (m_FacingRight ? 1 : -1), this.wallJumpSpeed), ForceMode2D.Impulse); // kick off the wall in the opposite direction
  }

  private void Dash()
  {
    this.controllerState = State.Dashing;
    dashTimer = dashTime;
    //this.m_Rigidbody2D.velocity = new Vector2(this.dashForce * (m_FacingRight ? 1 : -1), 0f);
    this.m_Rigidbody2D.AddForce(new Vector2(this.dashForce * (m_FacingRight ? 1 : -1), 0f), ForceMode2D.Impulse);
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
        if (Math.Abs(horizontalMovement) > 0)
        {
          m_Animator.Play("Run");
        } else
        {
          m_Animator.Play("Idle");
        }
        break;
      case State.Airborne:
        if (isFalling)
        {
          m_Animator.Play("Jump");
        } else
        {
          m_Animator.Play("Fall");
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
    }
  }
  private void CreateDust()
  {
    footsteps.Play();
  }
}