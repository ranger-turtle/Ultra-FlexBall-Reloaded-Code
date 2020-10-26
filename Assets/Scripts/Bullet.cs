using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
	private BoxCollider2D ballBarrier;
	public Vector2 VelocityBeforeHit { get; private set; }
	private int bouncePoints;

	public Vector2 LastFrameVelocity { get; private set; }
	public Vector3 LastFramePosition { get; private set; }

	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private GameObject oblivion;

    void Start()
    {
		ballBarrier = GameObject.Find("BallBarrier").GetComponent<BoxCollider2D>();
		bouncePoints = GameManager.Instance.ShooterLevel;
		GetComponent<Rigidbody2D>().velocity = new Vector2(0, 5);
		if (GameManager.Instance.ShooterLevel < 2)
			Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), ballBarrier.GetComponent<Collider2D>());

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();
		oblivion = GameObject.Find("Oblivion");
	}

	private void Update()
	{
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		//if (!Mathf.Approximately(rb.velocity.x, 0) && !Mathf.Approximately(rb.velocity.y, 0))
		if (rb.velocity != Vector2.zero)
			LastFrameVelocity = rb.velocity;
		LastFramePosition = transform.position;

		BoxCollider2D thisCollider = GetComponent<BoxCollider2D>();
		//FIXME repair bullet stopping in midair next to wall
		//TODO examine possible bounce angles and do randomizing bounce angles
		if (leftWallCollider.bounds.max.x >= thisCollider.bounds.min.x || leftWallCollider.bounds.max.x >= thisCollider.bounds.max.x)
		{
			float offset = leftWallCollider.bounds.max.x + thisCollider.bounds.extents.x + 0.05f;
			transform.position = new Vector3(offset, transform.position.y);
			GetComponent<Rigidbody2D>().velocity = Vector2.Reflect(PhysicsHelper.GetAngledVelocity(85), Vector2.left) * LastFrameVelocity.magnitude;
		}
		else if (rightWallCollider.bounds.min.x <= thisCollider.bounds.max.x || rightWallCollider.bounds.min.x <= thisCollider.bounds.min.x)
		{
			float offset = rightWallCollider.bounds.min.x - thisCollider.bounds.extents.x - 0.05f;
			transform.position = new Vector3(offset, transform.position.y);
			GetComponent<Rigidbody2D>().velocity = Vector2.Reflect(PhysicsHelper.GetAngledVelocity(95), Vector2.right) * LastFrameVelocity.magnitude;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		VelocityBeforeHit = GetComponent<Rigidbody2D>().velocity;
		if (collision.gameObject.GetComponent<Brick>() || collision.collider == ballBarrier)
		{
			bouncePoints--;
			if (bouncePoints == 0)
				Destroy(gameObject);
			else
				GetComponent<Rigidbody2D>().velocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, collision);
			//Debug.Log($"Bounce points: {bouncePoints}");
		}
		else if (!collision.gameObject.GetComponent<Paddle>())
			GetComponent<Rigidbody2D>().velocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, collision);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.gameObject == oblivion)
		{
			BoxCollider2D collider = GetComponent<BoxCollider2D>();
			if (collision.bounds.max.y > collider.bounds.max.y)
				Destroy(gameObject);
		}
	}
}
