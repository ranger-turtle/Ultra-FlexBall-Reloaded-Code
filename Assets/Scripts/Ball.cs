using LevelSetData;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ball : MonoBehaviour, IBrickBuster
{
	public const float MinThrustSeconds = 0.7f;
	public const float MaxThrustSeconds = 1.7f;

	[SerializeField]
	private LayerMask layerMask;

	//private Rigidbody2D ballbody;

	public Vector2 LastFrameVelocity { get; set; }
	public Vector2 CurrentVelocity { get; set; }
	public Vector3 LastFramePosition { get; set; }
	public Vector2 VelocityToLaunch { get; private set; }
	public Vector3 LastHitPoint { get; set; }
	public Vector2 LastHitNormal { get; set; }

	private float differenceFromPaddleCenter;
	/*private List<Vector2> lastHitNormals;
	private bool shouldReflectVector;
	private Vector2 futureNormal;*/

	public float StickXPosition { get; private set; }
	public bool StuckToPaddle { get; set; }

	public bool Thrust { get; set; }

	private BoxCollider2D thisCollider;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private BoxCollider2D oblivion;

	public bool Teleport { get; set; }

	private Vector2 velocityAfterThrust;

	public int BallSize
	{
		get => GetComponent<Animator>().GetInteger("BallSize");
		set => GetComponent<Animator>().SetInteger("BallSize", value);
	}
	public Coroutine ThrustCoroutine { get; private set; }

	private void Awake()
	{
		//ballbody = GetComponent<Rigidbody2D>();
		//levelSoundLibrary = GameObject.Find("GameSoundLibrary").GetComponent<DefaultSoundLibrary>();

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();

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
		CurrentVelocity = Vector3.zero;
		VelocityToLaunch = futureVelocity;

		differenceFromPaddleCenter = difference;
		//Physics2D.IgnoreCollision(thisCollider, leftWallCollider);
		//Physics2D.IgnoreCollision(thisCollider, rightWallCollider);

		StuckToPaddle = true;
	}

	public void LaunchFromPaddle()
	{
		CurrentVelocity = VelocityToLaunch;
		//Physics2D.IgnoreCollision(thisCollider, leftWallCollider, false);
		//Physics2D.IgnoreCollision(thisCollider, rightWallCollider, false);

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

	public void CloneProperties(Ball originalBall)
	{
		SetBallSizeWithoutAnimation((int)GameManager.Instance.BallSize);
		StuckToPaddle = originalBall.GetComponent<Ball>().StuckToPaddle;
		VelocityToLaunch = originalBall.GetComponent<Ball>().VelocityToLaunch;
	}

	//TODO do low-level ball physics (do not use rigidbody and compare collisions with BoxCast and RayCast)
	private void FixedUpdate()
	{
		//PreventPassingThrough();
		if (!Paddle.Instance.MagnetActive || !StuckToPaddle)
			LastFrameVelocity = CurrentVelocity;
		//UpdateVelocityAfterHitting();
		LastFramePosition = transform.position;
		OnCollision();
	}

	private void OnCollision()
	{
		if (!StuckToPaddle)
		{
			float originSafetyOffsetX = CurrentVelocity.x != 0 ? .01f * Mathf.Sign(CurrentVelocity.x) : 0;
			float originSafetyOffsetY = CurrentVelocity.y != 0 ? .01f * Mathf.Sign(CurrentVelocity.y) : 0;
			RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position + new Vector3(originSafetyOffsetX, originSafetyOffsetY, 0), GetComponent<BoxCollider2D>().size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask).Where(bch => !bch.collider.isTrigger).ToArray();
			if (boxCastHit.Length > 0)
			{
				//RaycastHit2D raycastHitX = boxCastHit.FirstOrDefault(bch => bch.normal.x != 0);
				//RaycastHit2D raycastHitY = boxCastHit.FirstOrDefault(bch => bch.normal.y != 0);
				RaycastHit2D firstRaycast = boxCastHit[0];
				float x = firstRaycast.normal.x != 0 ? firstRaycast.point.x + (GetComponent<BoxCollider2D>().bounds.extents.x + .01f) * firstRaycast.normal.x : firstRaycast.centroid.x;//Rounding in false condition is made to make bouncing in straight line possible
				float y = firstRaycast.normal.y != 0 ? firstRaycast.point.y + (GetComponent<BoxCollider2D>().bounds.extents.y + .01f) * firstRaycast.normal.y : firstRaycast.centroid.y;
				transform.position = new Vector2(x, y);

				/*foreach (Vector2 normal in boxCastHit.Select(bch => bch.normal))
				{
					totalNormal = new Vector2(totalNormal.x + normal.x, totalNormal.y + normal.y);
				}

				RaycastHit2D firstVerticalHit = boxCastHit.FirstOrDefault(bch => bch.normal.y != 0);
				Brick firstVerticalHitBrick = firstVerticalHit.collider?.GetComponent<Brick>();
				if (firstVerticalHitBrick)
				{
					Brick[] bricksHitWithWrongYNormal = boxCastHit.Where(bch => bch.normal.x != 0 && bch.collider.GetComponent<Brick>().y == firstVerticalHitBrick.y).Select(bch => bch.collider.GetComponent<Brick>()).ToArray();
					totalNormal -= new Vector2(Mathf.Sign(totalNormal.x) * bricksHitWithWrongYNormal.Length, 0);
				}*/

				RaycastHit2D firstBrickRaycast = boxCastHit.FirstOrDefault(bch => bch.collider.GetComponent<Brick>());
				boxCastHit = boxCastHit.Where(bch => !bch.collider.GetComponent<Brick>() || (bch.normal == firstBrickRaycast.normal && (firstBrickRaycast.normal.x != 0 && bch.collider.GetComponent<Brick>().x == firstBrickRaycast.collider.GetComponent<Brick>().x || firstBrickRaycast.normal.y != 0 && bch.collider.GetComponent<Brick>().y == firstBrickRaycast.collider.GetComponent<Brick>().y))).ToArray();
				if (boxCastHit.Any(bch => bch.collider.GetComponent<Brick>()))
				{
					//transform.position = LastFramePosition;
					RaycastHit2D firstThrustingBrick = boxCastHit.FirstOrDefault(bch => bch.collider.GetComponent<Brick>()?.brickType.Properties.IsBallThrusting == true);
					//BrickProperties brickProperties = collision.gameObject.GetComponent<Brick>()?.brickType.Properties;
					if (!firstThrustingBrick && !GameManager.Instance.PenetratingBall)
					{
						CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
						CurrentVelocity = Vector2.ClampMagnitude(CurrentVelocity * BallManager.acceleration, BallManager.maxBallSpeed);
					}
					else if (firstThrustingBrick)
					{
						Brick thrustingBrick = firstThrustingBrick.collider.GetComponent<Brick>();
						ThrustBall(thrustingBrick, thrustingBrick.BrickProperties.BallThrustDirection);
					}
				}
				else
					CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
				foreach (RaycastHit2D raycastHit2D in boxCastHit)
				{
					LastHitNormal = raycastHit2D.normal;
					LastHitPoint = raycastHit2D.point;
					raycastHit2D.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
					if (Teleport)
					{
						Teleport = false;
						break;
					}
				}
				//if (boxCastHit[0].collider.GetComponent<Brick>() && totalNormal.normalized.y > 0)
				//Debug.Break();
			}
			else
				transform.position = new Vector2(transform.position.x + CurrentVelocity.x, transform.position.y + CurrentVelocity.y);
			CheckIfOblivion();
			CheckIfOutsideWall();
		}
	}

	public void ThrustBall(Brick brick, Direction direction)
	{
		SoundManager.Instance.PlaySfx("Ball Thrust");
		int[] signs = new int[] { -1, 1 };
		Vector2 oldVelocity = CurrentVelocity;
		Vector2 velocityBeforeThrust = Vector2.zero;
		velocityAfterThrust = PhysicsHelper.GetAngledVelocity(Random.Range(20, 160) * signs[Random.Range(0, 2)]) * oldVelocity.magnitude;
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
		CurrentVelocity = velocityBeforeThrust;
		transform.position = new Vector3(x, y, transform.position.z);
		if (!Thrust)
		{
			Thrust = true;
			WaitForThrustFinish();
		}
	}

	private void WaitForThrustFinish()
	{
		float seconds = Random.Range(MinThrustSeconds, MaxThrustSeconds);
		float playbackPosition = (MaxThrustSeconds - seconds) * (1.0f / (MaxThrustSeconds - MinThrustSeconds));
		ThrustCoroutine = StartCoroutine(ParticleManager.Instance.GenerateThrustedBallFlame(gameObject, seconds, playbackPosition));
	}

	public void FinishThrust(bool force = true)
	{
		Thrust = false;
		if (ThrustCoroutine != null && force)
			StopCoroutine(ThrustCoroutine);
		else
			CurrentVelocity = velocityAfterThrust;
	}

	/*private void UpdateVelocityAfterHitting()
	{
		if (shouldReflectVector)
		{
			Vector2 outputNormal = lastHitNormals[0];//new Vector2(0, 0);//FIXME try improving collisions with Physics2D.Raycast
			//foreach (Vector2 normal in lastHitNormals)
			//{
			//	Debug.Log($"Vecn: {normal}");
			//	outputNormal += normal;
			//}
			//Debug.Log($"Vec1: {outputNormal}");
			//outputNormal = outputNormal.normalized;
			//Debug.Log($"Vec after normalization: {outputNormal}");
			/*if (Mathf.Abs(outputNormal.x) > Mathf.Abs(outputNormal.y))
				outputNormal = new Vector2(outputNormal.x, 0).normalized;
			else if (Mathf.Abs(outputNormal.x) < Mathf.Abs(outputNormal.y))
				outputNormal = new Vector2(0, outputNormal.y).normalized;
			else
				outputNormal = new Vector2(outputNormal.x, outputNormal.y).normalized;
			ballbody.velocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, outputNormal);

			shouldReflectVector = false;
		}
	}*/

	/*	private void Reflect(Vector2 collisionNormal)
		{
			/*if (!shouldReflectVector)
			{
				lastHitNormals = new List<Vector2>();
				shouldReflectVector = true;
			}
			lastHitNormals.Add(collisionNormal);
			ballbody.velocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, collisionNormal);
		}*/

	private void OnCollisionEnter2D(Collision2D collision)
	{
		/*if (!shouldReflectVector)
		{
			Debug.Log($"Vec1: {collision.GetContact(0).normal}, Vec2: {collision.GetContact(1).normal}");
			//Debug.Break();
		}*/
		//Debug.Log($"Normal on collision: {futureNormal}");
		#region oldCollision
		/*if (collision.gameObject.tag == "ballCollidable")
		{
			//if (futureNormal == Vector2.zero)
				futureNormal = collision.GetContact(0).normal;
			if (collision.gameObject.GetComponent<Brick>())
			{
				//transform.position = LastFramePosition;
				BrickProperties brickProperties = collision.gameObject.GetComponent<Brick>()?.brickType.Properties;
				if (brickProperties.BallThrustDirection == Direction.None)
				{
					if (GameManager.Instance.PenetratingBall)
						ballbody.velocity = LastFrameVelocity;
					else
					{
						Reflect(futureNormal);
						ballbody.velocity = Vector2.ClampMagnitude(ballbody.velocity * BallManager.acceleration, BallManager.maxBallSpeed);
					}
				}
				if (brickProperties.BallThrustDirection != Direction.None)
					ThrustBall(collision.gameObject.GetComponent<Brick>(), brickProperties.BallThrustDirection);
			}
			else
				Reflect(futureNormal);
		}*/
		#endregion
		//Debug.Log($"Ball velocity angle: {Vector2.SignedAngle(Vector2.up, GetComponent<Rigidbody2D>().velocity)}");
		//Debug.Log($"Ball velocity normal: {GetComponent<Rigidbody2D>().velocity.normalized}");
		//Debug.Log("Collision End");
	}

	private void CheckIfOblivion()
	{
		if (GetComponent<BoxCollider2D>().bounds.max.y < oblivion.GetComponent<BoxCollider2D>().bounds.min.y)
		{
			SoundManager.Instance.PlaySfx("Ball Fall");
			Remove();
		}
	}

	private void CheckIfOutsideWall()
	{
		Bounds ballBounds = GetComponent<BoxCollider2D>().bounds;
		Vector2 normal = Vector2.zero;
		if (ballBounds.max.x < leftWallCollider.GetComponent<BoxCollider2D>().bounds.min.x)
		{
			transform.position = new Vector3(leftWallCollider.GetComponent<BoxCollider2D>().bounds.max.x + ballBounds.extents.x + 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(1, 0);
		}
		else if (ballBounds.min.x > rightWallCollider.GetComponent<BoxCollider2D>().bounds.max.x)
		{
			transform.position = new Vector3(rightWallCollider.GetComponent<BoxCollider2D>().bounds.min.x - ballBounds.extents.x - 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(-1, 0);
		}
		if (normal != Vector2.zero)
			CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, normal);
	}
}