using LevelSetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ball : MonoBehaviour
{
	private Rigidbody2D ballbody;
	private LevelSoundLibrary levelSoundLibrary;

	public Vector2 LastFrameVelocity { get; private set; }
	public Vector3 LastFramePosition { get; private set; }
	public Vector3 VelocityToLaunch { get; private set; }

	private float differenceFromPaddleCenter;
	private List<Vector2> lastHitNormals;
	private bool shouldReflectVector;

	public float StickXPosition { get; private set; }
	public bool StuckToPaddle { get; set; }

	[SerializeField]
	public bool Thrust { get; set; }

	private BoxCollider2D thisCollider;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;
	private BoxCollider2D ballBarrierCollider;

	private BoxCollider2D oblivion;

	public const float maxBallVelocityMagnitude = 16.0f;
	public const float acceleration = 1.005f;

	public int BallSize
	{
		get => GetComponent<Animator>().GetInteger("BallSize");
		set => GetComponent<Animator>().SetInteger("BallSize", value);
	}

	private void Awake()
	{
		ballbody = GetComponent<Rigidbody2D>();
		levelSoundLibrary = GameObject.Find("Game").GetComponent<LevelSoundLibrary>();

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();
		ballBarrierCollider = GameObject.Find("BallBarrier").GetComponent<BoxCollider2D>();

		oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>();

		thisCollider = GetComponent<BoxCollider2D>();
	}

	public void SetBallSizeWithoutAnimation(int ballSize)
	{
		GetComponent<Animator>().SetBool("NoAnimate", true);
		BallSize = ballSize;
	}

	public void UpdateSize()
	{
		if ((int)GameManager.Instance.BallSize != this.BallSize)
		{
			BallSize = (int)GameManager.Instance.BallSize;
		}
	}

	public void Remove()
	{
		BallManager.Instance.Remove(gameObject);
	}

	public void StickToPaddle(Vector2 futureVelocity, float difference)
	{
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		VelocityToLaunch = futureVelocity;

		differenceFromPaddleCenter = difference;
		Physics2D.IgnoreCollision(thisCollider, leftWallCollider);
		Physics2D.IgnoreCollision(thisCollider, rightWallCollider);

		StuckToPaddle = true;
	}

	public void LaunchFromPaddle()
	{
		Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.velocity = VelocityToLaunch;
		Physics2D.IgnoreCollision(thisCollider, leftWallCollider, false);
		Physics2D.IgnoreCollision(thisCollider, rightWallCollider, false);

		StuckToPaddle = false;
	}

	public void UpdateBallOnPaddle()
	{
		if (Paddle.Instance.MagnetActive && StuckToPaddle)
		{
			float stuckY = Paddle.Instance.GetComponent<BoxCollider2D>().bounds.max.y + GetComponent<BoxCollider2D>().bounds.extents.y + 0.025f;
			transform.position = new Vector3(Paddle.Instance.transform.position.x - differenceFromPaddleCenter, stuckY);
		}
	}

	private void FixedUpdate()
	{
		if (!Paddle.Instance.MagnetActive || !StuckToPaddle)
			LastFrameVelocity = ballbody.velocity;
		UpdateVelocityAfterHittingBrick();
		LastFramePosition = transform.position;

		Rigidbody2D rb = GetComponent<Rigidbody2D>();

		if (leftWallCollider.bounds.max.x > thisCollider.bounds.max.x || leftWallCollider.bounds.max.x - 0.1f > thisCollider.bounds.min.x)
		{
			float offset = leftWallCollider.bounds.max.x + thisCollider.bounds.extents.x + 0.05f;
			rb.MovePosition(new Vector2(offset, transform.position.y));
			//rb.velocity = Vector2.Reflect(PhysicsHelper.GetAngledVelocity(40), Vector2.right) * LastFrameVelocity.magnitude;
			rb.velocity = Vector2.Reflect(rb.velocity, Vector2.right);
		}
		else if (rightWallCollider.bounds.min.x < thisCollider.bounds.min.x || rightWallCollider.bounds.min.x + 0.1f < thisCollider.bounds.max.x)
		{
			float offset = rightWallCollider.bounds.min.x - thisCollider.bounds.extents.x - 0.05f;
			rb.MovePosition(new Vector2(offset, transform.position.y));
			//rb.velocity = Vector2.Reflect(PhysicsHelper.GetAngledVelocity(130), Vector2.left) * LastFrameVelocity.magnitude;
			rb.velocity = Vector2.Reflect(rb.velocity, Vector2.left);
		}
		else if (ballBarrierCollider.bounds.min.y < thisCollider.bounds.min.y)
		{
			float offset = ballBarrierCollider.bounds.min.y - thisCollider.bounds.extents.x - 0.05f;
			rb.MovePosition(new Vector2(transform.position.x, offset));
			//rb.velocity = Vector2.Reflect(PhysicsHelper.GetAngledVelocity(130), Vector2.down) * LastFrameVelocity.magnitude;
			rb.velocity = Vector2.Reflect(rb.velocity, Vector2.down);
		}
	}

	public void ThrustBall(Brick brick, Direction direction)
	{
		levelSoundLibrary.PlaySfx(levelSoundLibrary.ballPush);
		int[] signs = new int[] { -1, 1 };
		Vector2 oldVelocity = GetComponent<Rigidbody2D>().velocity;
		Vector2 velocityBeforeThrust = Vector2.zero;
		Vector3 velocityAfterThrust = PhysicsHelper.GetAngledVelocity(Random.Range(20, 160) * signs[Random.Range(0, 2)]) * oldVelocity.magnitude;
		Bounds brickBounds = brick.GetComponent<BoxCollider2D>().bounds;
		Bounds brickBusterBounds = GetComponent<BoxCollider2D>().bounds;
		float x = 0;
		float y = 0;
		switch (direction)
		{
			case Direction.Up:
				x = brickBounds.center.x;
				y = brickBounds.max.y + brickBusterBounds.extents.y + 0.05f;
				velocityBeforeThrust = Vector2.up * oldVelocity.magnitude;
				break;
			case Direction.Right:
				x = brickBounds.max.x + brickBusterBounds.extents.x + 0.05f;
				y = brickBounds.center.y;
				velocityBeforeThrust = Vector2.right * oldVelocity.magnitude;
				break;
			case Direction.Down:
				x = brickBounds.center.x;
				y = brickBounds.min.y - brickBusterBounds.extents.y - 0.05f;
				velocityBeforeThrust = Vector2.down * oldVelocity.magnitude;
				break;
			case Direction.Left:
				x = brickBounds.min.x - brickBusterBounds.extents.x - 0.05f;
				y = brickBounds.center.y;
				velocityBeforeThrust = Vector2.left * oldVelocity.magnitude;
				break;
			default:
				break;
		}
		GetComponent<Rigidbody2D>().velocity = velocityBeforeThrust;
		transform.position = new Vector3(x, y, transform.position.z);
		if (!Thrust)
		{
			Thrust = true;
			StartCoroutine(WaitForThrustFinish(velocityAfterThrust));
		}
	}

	private IEnumerator WaitForThrustFinish(Vector2 oldVelocity)
	{
		yield return new WaitForSeconds(Random.Range(1.0f, 1.7f));
		GetComponent<Rigidbody2D>().velocity = oldVelocity;
		Thrust = false;
	}

	private void UpdateVelocityAfterHittingBrick()
	{
		if (shouldReflectVector)
		{
			Vector2 outputNormal = new Vector2(0, 0);
			foreach (Vector2 normal in lastHitNormals)
			{
				outputNormal += normal;
			}
			outputNormal = outputNormal.normalized;
			ballbody.velocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, outputNormal);

			shouldReflectVector = false;
		}
	}

	private void Reflect(Vector2 collisionNormal)
	{
		if (!shouldReflectVector)
		{
			lastHitNormals = new List<Vector2>();
			shouldReflectVector = true;
		}
		lastHitNormals.Add(collisionNormal);
		if (ballbody.velocity.magnitude < maxBallVelocityMagnitude)
			ballbody.velocity *= acceleration;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Debug.Log("Collision Begin");
		if (collision.gameObject.tag == "ballCollidable")
		{
			if (collision.gameObject.GetComponent<Brick>())
			{
				//transform.position = LastFramePosition;
				BrickProperties brickProperties = collision.gameObject.GetComponent<Brick>()?.brickType.Properties;
				if (brickProperties.BallThrustDirection == Direction.None)
				{
					if (GameManager.Instance.PenetratingBall)
						ballbody.velocity = LastFrameVelocity;
					else
						Reflect(collision.GetContact(0).normal);
				}
				if (brickProperties.BallThrustDirection != Direction.None)
					ThrustBall(collision.gameObject.GetComponent<Brick>(), brickProperties.BallThrustDirection);
			}
			else
				Reflect(collision.GetContact(0).normal);
		}
		Debug.Log($"Ball velocity angle: {Vector2.SignedAngle(Vector2.up, GetComponent<Rigidbody2D>().velocity)}");
		Debug.Log($"Ball velocity normal: {GetComponent<Rigidbody2D>().velocity.normalized}");
		//Debug.Log("Collision End");
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision == oblivion)
		{
			if (GetComponent<BoxCollider2D>().bounds.max.y < oblivion.GetComponent<BoxCollider2D>().bounds.min.y)
			{
				LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.ballFall);
				Remove();
			}
		}
	}
}