using LevelSetData;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ball : MonoBehaviour, IBrickBuster
{
	public const float MinThrustSeconds = 1.0f;
	public const float MaxThrustSeconds = 1.7f;

	[SerializeField]
	private LayerMask layerMask;

	public Vector2 LastFrameVelocity { get; set; }
	public Vector2 CurrentVelocity { get; set; }
	public Vector3 LastFramePosition { get; set; }
	public Vector2 VelocityToLaunch { get; private set; }
	public Vector3 LastHitPoint { get; set; }
	public Vector2 LastHitNormal { get; set; }

	private float differenceFromPaddleCenter;

	public float StickXPosition { get; private set; }
	public bool StuckToPaddle { get; set; }

	private Bounds thisBounds;
	private BoxCollider2D thisCollider;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private Bounds oblivion;

	public bool Teleport { get; set; }

	public bool Thrust { get; private set; }
	private Vector2 velocityAfterThrust;
	public Coroutine ThrustCoroutine { get; private set; }

	public int BallSize
	{
		get => GetComponent<Animator>().GetInteger("BallSize");
		set => GetComponent<Animator>().SetInteger("BallSize", value);
	}

	private void Awake()
	{
		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();

		oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>().bounds;

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
			ParticleManager.Instance.UpdateAllBallParticles(gameObject);
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

		StuckToPaddle = true;

		Paddle.Instance.SetMagnetZapVisibility(true);
	}

	public void LaunchFromPaddle()
	{
		CurrentVelocity = VelocityToLaunch;

		StuckToPaddle = false;
	}

	public void UpdateBallOnPaddle()
	{
		if (StuckToPaddle)
		{
			float stuckY = Paddle.Instance.GetComponent<BoxCollider2D>().bounds.max.y + thisBounds.extents.y + 0.025f;
			transform.position = new Vector3(Paddle.Instance.transform.position.x - differenceFromPaddleCenter, stuckY);
		}
	}

	public void CloneProperties(Ball originalBall)
	{
		SetBallSizeWithoutAnimation((int)GameManager.Instance.BallSize);
		StuckToPaddle = originalBall.StuckToPaddle;
		VelocityToLaunch = originalBall.VelocityToLaunch;
	}

	private void FixedUpdate()
	{
		if (/*!Paddle.Instance.MagnetActive || */!StuckToPaddle)
			LastFrameVelocity = CurrentVelocity;
		LastFramePosition = transform.position;
		thisBounds = thisCollider.bounds;
		OnCollision();
	}

	private void OnCollision()
	{
		if (!StuckToPaddle)
		{
			float originSafetyOffsetX = CurrentVelocity.x != 0 ? .01f * Mathf.Sign(CurrentVelocity.x) : 0;
			float originSafetyOffsetY = CurrentVelocity.y != 0 ? .01f * Mathf.Sign(CurrentVelocity.y) : 0;
			RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position + new Vector3(originSafetyOffsetX, originSafetyOffsetY, 0), thisCollider.size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask).ToArray();
			if (boxCastHit.Length > 0)
			{
				RaycastHit2D firstRaycast = boxCastHit[0];
				float x = firstRaycast.normal.x != 0 ? firstRaycast.point.x + (thisBounds.extents.x + .01f) * firstRaycast.normal.x : firstRaycast.centroid.x;//Rounding in false condition is made to make bouncing in straight line possible
				float y = firstRaycast.normal.y != 0 ? firstRaycast.point.y + (thisBounds.extents.y + .01f) * firstRaycast.normal.y : firstRaycast.centroid.y;
				transform.position = new Vector2(x, y);

				RaycastHit2D paddleRaycast = boxCastHit.FirstOrDefault(bch => bch.collider.GetComponent<Paddle>());
				if (paddleRaycast)
					paddleRaycast.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
				else
				{
					RaycastHit2D paddleSideRaycast = boxCastHit.FirstOrDefault(bch => bch.collider.GetComponent<PaddleSideCollider>());
					if (paddleSideRaycast)
						paddleSideRaycast.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
					else
					{
						RaycastHit2D firstBrickRaycast = boxCastHit.FirstOrDefault(bch => bch.collider.GetComponent<Brick>());
						boxCastHit = boxCastHit.Where(bch => !bch.collider.GetComponent<Brick>() || (bch.normal == firstBrickRaycast.normal && (firstBrickRaycast.normal.x != 0 && bch.collider.GetComponent<Brick>().x == firstBrickRaycast.collider.GetComponent<Brick>().x || firstBrickRaycast.normal.y != 0 && bch.collider.GetComponent<Brick>().y == firstBrickRaycast.collider.GetComponent<Brick>().y))).ToArray();
						foreach (RaycastHit2D raycastHit2D in boxCastHit)
						{
							LastHitNormal = raycastHit2D.normal;
							LastHitPoint = raycastHit2D.point;
							Brick brick = raycastHit2D.collider.GetComponent<Brick>();
							if (brick)
							{
								RaycastHit2D lastThrustingBrickRaycast = boxCastHit.LastOrDefault(bch => bch.collider.GetComponent<Brick>()?.brickType.Properties.IsBallThrusting == true);
								if (!lastThrustingBrickRaycast && (!GameManager.Instance.PenetratingBall || brick.BrickProperties.PenetrationResistant))
								{
									CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
									CurrentVelocity = Vector2.ClampMagnitude(CurrentVelocity * BallManager.acceleration, BallManager.maxBallSpeed);
								}
								else if (lastThrustingBrickRaycast)
								{
									Brick lastThrustingBrick = lastThrustingBrickRaycast.collider.GetComponent<Brick>();
									ThrustBall(lastThrustingBrick);
									RaycastHit2D LastHitBrickRaycast = boxCastHit.LastOrDefault(bch => bch.collider.GetComponent<Brick>());
									if (LastHitBrickRaycast == lastThrustingBrickRaycast)
										PlaceBallAccordingDirection(lastThrustingBrick);
								}
							}
							else
								CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
							raycastHit2D.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
							if (Teleport)
							{
								Teleport = false;
								break;
							}
						}
					}
				}
			}
			else
			{
				transform.position = new Vector2(transform.position.x + CurrentVelocity.x, transform.position.y + CurrentVelocity.y);
				CheckIfOblivion();
			}
			CheckIfOutsideWall();
		}
	}

	public void ThrustBall(Brick brick)
	{
		Direction direction = brick.BrickProperties.BallThrustDirection;
		SoundManager.Instance.PlaySfx("Ball Thrust");
		int[] signs = new int[] { -1, 1 };
		Vector2 oldVelocity = CurrentVelocity;
		Vector2 velocityBeforeThrust = Vector2.zero;
		velocityAfterThrust = PhysicsHelper.GetAngledVelocity(Random.Range(20, 160) * signs[Random.Range(0, 2)]) * oldVelocity.magnitude;
		switch (direction)
		{
			case Direction.Up:
				velocityBeforeThrust = Vector2.up * oldVelocity.magnitude;
				break;
			case Direction.Right:
				velocityBeforeThrust = Vector2.right * oldVelocity.magnitude;
				break;
			case Direction.Down:
				velocityBeforeThrust = Vector2.down * oldVelocity.magnitude;
				break;
			case Direction.Left:
				velocityBeforeThrust = Vector2.left * oldVelocity.magnitude;
				break;
			default:
				break;
		}
		CurrentVelocity = velocityBeforeThrust;
		if (!Thrust)
		{
			Thrust = true;
			WaitForThrustFinish();
		}
	}

	private void PlaceBallAccordingDirection(Brick brick)
	{
		Direction direction = brick.BrickProperties.BallThrustDirection;
		Bounds brickBounds = brick.GetComponent<BoxCollider2D>().bounds;
		float x = 0;
		float y = 0;
		switch (direction)
		{
			case Direction.Up:
				x = brickBounds.center.x;
				y = brickBounds.max.y + thisBounds.extents.y + 0.05f;
				break;
			case Direction.Right:
				x = brickBounds.max.x + thisBounds.extents.x + 0.05f;
				y = brickBounds.center.y;
				break;
			case Direction.Down:
				x = brickBounds.center.x;
				y = brickBounds.min.y - thisBounds.extents.y - 0.05f;
				break;
			case Direction.Left:
				x = brickBounds.min.x - thisBounds.extents.x - 0.05f;
				y = brickBounds.center.y;
				break;
			default:
				break;
		}
		transform.position = new Vector3(x, y, transform.position.z);
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

	private void CheckIfOblivion()
	{
		if (thisCollider.bounds.max.y < oblivion.min.y)
		{
			SoundManager.Instance.PlaySfx("Ball Fall");
			Remove();
		}
	}

	private void CheckIfOutsideWall()
	{
		Vector2 normal = Vector2.zero;
		if (thisBounds.max.x < leftWallCollider.bounds.min.x)
		{
			transform.position = new Vector3(leftWallCollider.bounds.max.x + thisBounds.extents.x + 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(1, 0);
		}
		else if (thisBounds.min.x > rightWallCollider.bounds.max.x)
		{
			transform.position = new Vector3(rightWallCollider.bounds.min.x - thisBounds.extents.x - 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(-1, 0);
		}
		if (normal != Vector2.zero)
			CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, normal);
	}
}