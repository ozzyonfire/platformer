using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class GameManager : MonoBehaviour
  {
    private static GameManager _instance;
    public PlayerController player;
    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(this);
      } else
      {
        _instance = this;
      }
    }

    public static void ResetObjects()
    {
      Debug.Log("Resetting Scene");
      ResetObject[] objects = FindObjectsOfType<ResetObject>();
      foreach (ResetObject resetObject in objects)
      {
        resetObject.ResetObjectProperties();
      }
    }

    public void KillPlayer()
    {
      this.player.Die();
    }
  }
}