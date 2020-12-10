using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

  // Movement
  public float speed = 10f;
  public float jumpForce = 3f;
  new public Rigidbody2D rigidbody;
  Vector2 m_Velocity = Vector2.zero;
  private float movement = 0f;
  private float verticalMovement = 0f;
  private float m_MovementSmoothing = 0.05f;
  private bool facingRight = true;
  private bool isClimbing = false;
  public float gravityScale = 3f;

  // Tools and Devices
  //public Laptop laptop;
  //public Scanner scanner;
  public float scannerRange = 10f;

  // Use this for initialization
  void Start()
  {
    rigidbody = this.GetComponent<Rigidbody2D>();
    rigidbody.gravityScale = this.gravityScale;
  }

  // Update is called once per frame
  void Update()
  {
    movement = Input.GetAxisRaw("Horizontal");
    verticalMovement = Input.GetAxisRaw("Vertical");

    if (!facingRight && movement > 0)
    {
      this.Flip();
    }
    else if (facingRight && movement < 0)
    {
      this.Flip();
    }

    if (Input.GetKeyDown(KeyCode.C))
    {
      //laptop.SendLocalData("192.168.0.101", "Hello, World");
    }

    if (Input.GetKeyDown(KeyCode.S))
    {
      //Device[] devices = scanner.ScanWifiDevices(scannerRange);
      //foreach (Device d in devices)
      //{
      //  Debug.Log(d.macAddress);
      //}
    }
  }

  private void FixedUpdate()
  {
    Vector2 targetVelocity = new Vector2(movement * speed, this.isClimbing ? verticalMovement * speed : rigidbody.velocity.y);
    if (isClimbing)
    {
      rigidbody.gravityScale = 0;
    }
    else
    {
      rigidbody.gravityScale = this.gravityScale;
    }
    rigidbody.velocity = Vector2.SmoothDamp(rigidbody.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
  }

  private void Flip()
  {
    facingRight = !facingRight;
    Vector3 scaler = this.transform.localScale;
    scaler.x *= -1;
    this.transform.localScale = scaler;
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.tag == "Stairs")
    {
      this.isClimbing = true;
    }
  }

  private void OnTriggerStay2D(Collider2D collider)
  {
    if (collider.gameObject.tag == "Stairs")
    {
      this.isClimbing = true;
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.tag == "Stairs")
    {
      this.isClimbing = false;
    }
  }
}
