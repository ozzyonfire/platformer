using UnityEngine;

namespace Assets.Scripts
{
  public class SavePoint : Interactable
  {
    public PlayerController player;
    public Animator animator;

    // Use this for initialization
    void Start()
    {
      this.animator = this.GetComponent<Animator>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
      base.OnTriggerEnter2D(collision);
      if (collision.gameObject == player.gameObject)
      {
        player.SetRespawnPoint(this.transform.position);
        animator.Play("Saving");
      }
    }

    protected override void Interact()
    {
      player.Respawn();
    }
  }
}