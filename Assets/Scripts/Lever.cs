using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Lever : Interactable
  {
    private bool isLeft = true;
    private Animator animator;
    public Gateway[] gateways;

    private void Start()
    {
      this.animator = this.GetComponent<Animator>();
    }

    protected override void Update()
    {
      base.Update();
      if (this.isLeft)
      {
        animator.Play("Left");
      } else
      {
        animator.Play("Right");
      }
    }

    protected override void Interact()
    {
      this.isLeft = !isLeft;
      foreach (Gateway gate in gateways)
      {
        gate.Switch();
      }
    }
  }
}