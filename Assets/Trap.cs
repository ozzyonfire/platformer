using Assets.Scripts;
using UnityEngine;

namespace Assets
{
  public class Trap : Spike
  {
    public Trigger trigger;
    private new Rigidbody2D rigidbody;
    public PlayerController player;

    // Use this for initialization
    void Start()
    {
      this.SetStartingTransformProperties();
      rigidbody = this.GetComponent<Rigidbody2D>();
      trigger.target = player;
      trigger.AddTriggerAction(() =>
      {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
      });
    }

    public override void ResetObjectProperties()
    {
      this.ResetTransformProperties();
      this.rigidbody.bodyType = RigidbodyType2D.Static;
    }
  }
}