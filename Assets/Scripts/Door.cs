using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Door : Interactable
  {
    public Transform destination;
    public PlayerController player;

    protected override void Interact()
    {
      player.Teleport(destination.position);
    }
  }
}