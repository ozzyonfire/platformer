using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Gateway : MonoBehaviour
  {
    protected Animator animator;
    public enum State { Open, Close }
    protected State state = State.Close;
    public State startingState = State.Close;
    protected virtual void Start()
    {
      this.state = startingState;
      this.animator = this.GetComponent<Animator>();
    }

    public virtual void Open()
    {
      this.state = State.Open;
    }
    public virtual void Close()
    {
      this.state = State.Close;
    }

    public virtual void Switch()
    {
      switch (state)
      {
        case State.Open:
          state = State.Close;
          break;
        case State.Close:
          state = State.Open;
          break;
      }
    }

    private void Update()
    {
      switch (state)
      {
        case State.Close:
          animator.Play("Close");
          break;
        case State.Open:
          animator.Play("Open");
          break;
      }
    }
  }
}