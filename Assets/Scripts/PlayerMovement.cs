using UnityEngine;
using System.Collections;
using System;

namespace Assets.Scripts
{
  public class PlayerMovement : MonoBehaviour
  {
    private CharacterController2D controller;
    // Movement
    new private Rigidbody2D rigidbody;    
    public float gravityScale = 3f;
    private Animator animator;
    public ParticleSystem footsteps;

    // Use this for initialization
    void Start()
    {
      rigidbody = this.GetComponent<Rigidbody2D>();
      rigidbody.gravityScale = this.gravityScale;
      controller = this.GetComponent<CharacterController2D>();
      animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
      animator.SetFloat("Horizontal", Mathf.Abs(rigidbody.velocity.x));
      animator.SetFloat("Vertical Velocity", rigidbody.velocity.y);
      animator.SetBool("Grounded", controller.controllerState == CharacterController2D.State.Grounded);

      //animator.SetBool("Crouching", isCrouching);
    }

    public void OnFalling()
    {
      
    }

    public void OnLanding()
    {
      this.CreateDust();
    }

    public void OnJumping()
    {
      this.CreateDust();
    }

    public void OnCrouch(bool isCrouch)
    {
      animator.SetBool("Crouching", isCrouch);
    }

    public void OnFlip()
    {
      Vector3 theScale = footsteps.transform.localScale;
      theScale.x *= -1;
      footsteps.transform.localScale = theScale;

      if (controller.controllerState != CharacterController2D.State.Airborne)
      {
        this.CreateDust(); // don't create dust in mid air
      }
    }

    private void CreateDust()
    {
      footsteps.Play();
    }
  }
}