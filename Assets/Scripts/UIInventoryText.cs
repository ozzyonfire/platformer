using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class UIInventoryText : MonoBehaviour
  {
    Text textComponent;
    public string itemName;
    // Use this for initialization
    void Start()
    {
      textComponent = this.GetComponent<Text>();

      GameManager.Instance.OnInventoryAdded((name, value) =>
      {
        if (name == itemName)
        {
          textComponent.text = value.ToString();
        }
      });
    }
  }
}