using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Collectible : MonoBehaviour
  {
    private Animator animator;

    // Use this for initialization
    void Start()
    {
      this.animator = this.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.CompareTag("Player"))
      {
        this.Pickup();
      }
    }

    protected void Pickup()
    {
      // add to inventory
      this.animator.Play("Pickup");
    }

    private void OnPickupAnimationFinished()
    {
      Destroy(this.gameObject);
    }
  }
}