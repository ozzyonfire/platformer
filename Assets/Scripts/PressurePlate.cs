using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
  public LayerMask interactableMask;
  private Animator animator;
  public bool activated = false;
  public List<Gate> gates;

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

  private void OnTriggerStay2D(Collider2D collision)
  {
    if (interactableMask == (interactableMask | 1 << collision.gameObject.layer))
    {
      this.activated = true;
      this.OpenGates();
    }
  }

  private void OnTriggerExit2D(Collider2D collision)
  {
    if (interactableMask == (interactableMask | 1 << collision.gameObject.layer))
    {
      this.activated = false;
      this.CloseGates();
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

  void OpenGates()
  {
    foreach (Gate gate in gates)
    {
      gate.Open();
    }
  }

  void CloseGates()
  {
    foreach (Gate gate in gates)
    {
      gate.Close();
    }
  }
}
