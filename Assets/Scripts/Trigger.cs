using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
  public class Trigger : MonoBehaviour
  {
    public List<Action> triggerActions;
    public PlayerController target;
    private void Awake()
    {
      triggerActions = new List<Action>();
    }

    public void AddTriggerAction(Action action)
    {
      triggerActions.Add(action);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject == target.gameObject)
      {
        triggerActions.ForEach(action => action());
      }
    }
  }
}