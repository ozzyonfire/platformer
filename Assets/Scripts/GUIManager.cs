using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class GUIManager : MonoBehaviour
  {
    public Text orbsText;
    public Text coinsText; 
    public Text deathsText;
    public Text timerText;
    private float timer = 0;

    // Use this for initialization
    void Start()
    {
      orbsText.text = "0";
      coinsText.text = "0";
      deathsText.text = "0";
      timerText.text = "0";
      timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
      timer += Time.deltaTime;
      timerText.text = timer.ToString("0.0");
    }


  }
}