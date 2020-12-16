using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Interactable : MonoBehaviour
  {
    protected bool ableToInteract = false;

    protected virtual void Update()
    {
      if (Input.GetButtonDown("Interact") && ableToInteract)
      {
        this.Interact();
      }
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.CompareTag("Player"))
      {
        ableToInteract = true;
      }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
      if (collision.gameObject.CompareTag("Player"))
      {
        ableToInteract = false;
      }
    }

    protected virtual void Interact() { }

  }
}