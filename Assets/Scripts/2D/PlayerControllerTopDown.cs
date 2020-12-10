using UnityEngine;

public class PlayerControllerTopDown : MonoBehaviour
{

  public float speed = 5f;
  new private Rigidbody2D rigidbody;
  public Animator animator;

  private Vector2 movement;
  public enum Direction { North, East, South, West };
  public Direction facing = Direction.South;
  public CircleCollider2D coll;
  public Vector2 northPosition;
  public Vector2 eastPosition;
  public Vector2 southPosition;
  public Vector2 westPosition;

  // Use this for initialization
  void Start()
  {
    movement = new Vector2(0, 0);
    rigidbody = GetComponent<Rigidbody2D>();
  }

  // Update is called once per frame
  void Update()
  {
    switch (facing)
    {
      case Direction.North:
        coll.offset = northPosition;
        break;
      case Direction.East:
        coll.offset = eastPosition;
        break;
      case Direction.South:
        coll.offset = southPosition;
        break;
      case Direction.West:
        coll.offset = westPosition;
        break;
    }
  }

  void FixedUpdate()
  {
    //Store the current horizontal input in the float moveHorizontal.
    float moveHorizontal = Input.GetAxis("Horizontal");

    //Store the current vertical input in the float moveVertical.
    float moveVertical = Input.GetAxis("Vertical");

    //Use the two store floats to create a new Vector2 variable movement.
    movement = new Vector2(moveHorizontal, moveVertical);
    float movementAmount = Mathf.Clamp(movement.sqrMagnitude, 0, 1);

    if (movementAmount > 0)
    {
      float angle = Vector2.SignedAngle(movement, Vector2.up);

      animator.SetFloat("LastVectorX", moveHorizontal);
      animator.SetFloat("LastVectorY", moveVertical);
      if (angle > -45 && angle < 45)
      {
        facing = Direction.North;
      }
      else if (angle > 45 && angle < 135)
      {
        facing = Direction.East;
      }
      else if ((angle > 135 && angle <= 180) || (angle < -135 && angle >= -180))
      {
        facing = Direction.South;
      }
      else if (angle > -135 && angle < -45)
      {
        facing = Direction.West;
      }
    }

    animator.SetFloat("Vertical", moveVertical);
    animator.SetFloat("Horizontal", moveHorizontal);

    float movementSpeed = speed;
    animator.SetFloat("Speed", movementAmount * movementSpeed);
    rigidbody.MovePosition(rigidbody.position + (movement * movementSpeed * Time.fixedDeltaTime));
  }
}
