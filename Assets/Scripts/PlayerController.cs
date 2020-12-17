using UnityEngine;
using System.Collections;
using System;

namespace Assets.Scripts
{
  public class PlayerController : MonoBehaviour 
  {
    public Vector2 respawnPoint;
    public Cinemachine.CinemachineVirtualCameraBase cinemachine;
    private CharacterController2D controller;
    public CharacterController2D.State ControllerState => controller.controllerState;
    private Animator animator;

    private void Start()
    {
      this.controller = this.GetComponent<CharacterController2D>();
      this.animator = this.GetComponent<Animator>();
    }

    public void Respawn()
    {
      this.controller.Die();
    }

    public void Die()
    {
      this.Respawn();
    }


    public void Teleport(Vector2 position)
    {
      Vector2 oldPosition = this.transform.position;
      this.transform.position = position;
      this.cinemachine.OnTargetObjectWarped(this.transform, position - oldPosition);
    }

    public void SetRespawnPoint(Vector2 point)
    {
      this.respawnPoint = point;
    }

    public void DieAnimationComplete()
    {
      GameManager.ResetObjects();
      this.Teleport(respawnPoint);
      this.controller.controllerState = CharacterController2D.State.Grounded;
    }

    public void Stop()
    {
      this.controller.Stop();
    }
  }
}