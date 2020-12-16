using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Spike : ResetObject
  {

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.CompareTag("Player"))
      {
        GameManager.Instance.KillPlayer();
      }
    }
  }
}