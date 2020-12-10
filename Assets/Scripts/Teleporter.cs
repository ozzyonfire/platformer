using UnityEngine;
using System.Collections;

namespace Assets.Scripts
{
  public class Teleporter : MonoBehaviour
  {
    public Transform target;
    public Transform player;
    public Cinemachine.CinemachineVirtualCameraBase cinemachine;
    public bool isTeleporting;

    // Update is called once per frame
    void Update()
    {
      if (isTeleporting)
      {
        Vector3 oldPosition = player.position;
        Vector3 playerOffset = oldPosition - this.transform.position;
        Vector3 newPosition = this.target.position + playerOffset;
        player.position = newPosition;
        this.cinemachine.OnTargetObjectWarped(player, newPosition - oldPosition);
        isTeleporting = false;
      }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.CompareTag("Player"))
      {
        isTeleporting = true;
      }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
      if (collision.CompareTag("Player"))
      {
        isTeleporting = false;
      }
    }
  }
}