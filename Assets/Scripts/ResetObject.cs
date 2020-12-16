using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class ResetObject : MonoBehaviour
  {
    protected Vector2 startingPosition;
    protected Quaternion startingRotation;
    protected Vector2 startingScale;

    private void Start()
    {
      this.SetStartingTransformProperties();
    }

    protected void SetStartingTransformProperties()
    {
      this.startingPosition = this.transform.position;
      this.startingRotation = this.transform.rotation;
      this.startingScale = this.transform.localScale;

    }

    protected void ResetTransformProperties()
    {
      this.transform.position = startingPosition;
      this.transform.rotation = startingRotation;
      this.transform.localScale = startingScale;
    }

    public virtual void ResetObjectProperties()
    {
      this.ResetTransformProperties();
    }
  }
}