using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class GameManager : MonoBehaviour
  {
    private static GameManager _instance;
    public PlayerController player;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private float gameTimer = 0;
    public Text timerText;
    public Text messageText;
    public Text inputNameText;
    public int maxOrbs = 4; // once this number is reached the game is over
    private bool gameOver = false;
    public GameObject submissionFields;
    public Button giveUpButton;
    Action<string, int> inventoryAdded;

    public void OnInventoryAdded(Action<string, int> action)
    {
      inventoryAdded += action;
    }

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

      inventory.Add("Coin", 0);
      inventory.Add("Orb", 0);
      inventory.Add("Death", 0);
      gameTimer = 0;
    }

    private void Start()
    {
      inventoryAdded += (name, value) =>
       {
         if (name == "Orb" && value == maxOrbs)
         {
           Debug.Log("Game over");
           gameOver = true;
           //Application.ExternalCall("GameOver", inventory["Coin"], inventory["Orb"], inventory["Death"], gameTimer);
         } 
       };
    }

    private void Update()
    {
      if (!gameOver)
      {
        gameTimer += Time.deltaTime;
        timerText.text = gameTimer.ToString("0.0");
      } else
      {
        this.player.Stop();
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
      if (this.player.ControllerState != CharacterController2D.State.Dead)
      {
        this.player.Die();
        this.AddInventory("Death");
      }
    }

    public void AddInventory(string name)
    {
      if (inventory.ContainsKey(name))
      {
        inventory[name]++;
        inventoryAdded(name, inventory[name]);
      }
    }

    public int GetInventoy(string name)
    {
      return inventory[name];
    }

    public void GiveUp()
    {
      this.submissionFields.SetActive(true);
      this.giveUpButton.gameObject.SetActive(false);
      this.gameOver = true;
      this.player.Stop();
    }

    public void SubmitScore()
    {
      if (inputNameText.text == "")
      {
        messageText.text = "Provide a name";
        return;
      }

    }
  }
}