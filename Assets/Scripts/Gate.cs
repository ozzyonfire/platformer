using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Gate : MonoBehaviour
  {
    public enum State { Locked, Open, Close };
    private Animator animator;
    public State gateState = State.Locked;
    public int numberOfFrames = 12;
    public int currentFrame = 0;
    public float animationStartTime = 0f;
    private bool allowedToCheck = false;

    private void Start()
    {
      this.animator = this.GetComponent<Animator>();
    }

    private void Update()
    {
      this.Animate();
    }
    public void Open()
    {
      switch (gateState)
      {
        case State.Locked:
          animator.Play("Open");
          break;
        case State.Close:
          this.GetStartTime();
          animator.Play("Open", 0, 1 - animationStartTime);
          break;
      }
      this.gateState = State.Open;
    }

    public void Close()
    {
      switch(gateState)
      {
        case State.Open:
          this.GetStartTime();
          animator.Play("Close", 0, 1 - animationStartTime);
          allowedToCheck = false;
          StartCoroutine(CheckForEndOfAnimation());
          break;
        default:
          this.gateState = State.Close;
          break;
      }
      this.gateState = State.Close;
    }

    private void GetStartTime()
    {
      AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
      animationStartTime = info.normalizedTime;
      if (animationStartTime > 1f)
      {
        animationStartTime = 1f;
      }
    }

    public void Animate()
    {
      AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
      switch (gateState)
      {
        case State.Locked:
          animator.Play("Idle");
          break;
        case State.Open:
          break;
        case State.Close:
          if (allowedToCheck && info.normalizedTime > 1)
          {
            this.gateState = State.Locked;
          }
          break;
      }
    }

    private IEnumerator CheckForEndOfAnimation()
    {
      yield return null;
      allowedToCheck = true;
    }
  }
}