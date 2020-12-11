using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
  public Transform block; // the corresponding block for the button
  public LayerMask interactableMask;
  private Animator animator;
  public bool activated = false;

  // Start is called before the first frame update
  void Start()
  {
    this.animator = this.GetComponent<Animator>();
  }

  // Update is called once per frame
  void Update()
  {
    this.Animate();
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (interactableMask == (interactableMask | 1 << collision.gameObject.layer))
    {
      this.activated = true;
    }
  }

  private void OnCollisionExit2D(Collision2D collision)
  {
    if (interactableMask == (interactableMask | 1 << collision.gameObject.layer))
    {
      this.activated = false;
    }
  }

  void Animate()
  {
    if (activated)
    {
      animator.Play("Pressed");
    } else
    {
      animator.Play("Idle");
    }
  }
}
