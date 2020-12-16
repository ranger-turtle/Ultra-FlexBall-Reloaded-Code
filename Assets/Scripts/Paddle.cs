using UnityEngine;

public class Paddle : MonoBehaviour
{
	#region Singleton
	public static Paddle Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private Camera mainCamera;
	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.leftBound' is never assigned to, and will always have its default value null
	private GameObject leftBound;
	[SerializeField]
	private GameObject rightBound;
	[SerializeField]
	private GameObject magnet;
	[SerializeField]
	private GameObject shooter;
	[SerializeField]
	private GameObject megaMissileTurret;
	[SerializeField]
	private GameObject paddleElectrodes;
	[SerializeField]
	private GameObject sideElectrodes;
	[SerializeField]
	private GameObject magnetZap;

	private float currentMouseX;

	[SerializeField]
	private SoundManager soundManager;
	[SerializeField]
	private HUDManager hudManager;

	[SerializeField]
	private SpriteRenderer spriteRenderer;
#pragma warning restore CS0649 // Field 'Paddle.hudManager' is never assigned to, and will always have its default value null

	public bool MagnetActive
	{
		get => magnet.activeSelf;
		set => magnet.SetActive(value);
	}

	public bool ShooterActive
	{
		get => shooter.activeSelf;
		set => shooter.SetActive(value);
	}

	public bool MegaMissileActive
	{
		get => megaMissileTurret.activeSelf;
		set => megaMissileTurret.SetActive(value);
	}

	public bool ProtectiveBarrierActive
	{
		get => paddleElectrodes.activeSelf;
		set
		{
			paddleElectrodes.SetActive(value);
			sideElectrodes.SetActive(value);
		}
	}

	private void Start()
	{
		mainCamera = FindObjectOfType<Camera>();
		SetLength(GameManager.Instance.PaddleLengthLevel);
		spriteRenderer = GetComponent<SpriteRenderer>();
		//Debug.Log($"Paddle Length: {GameManager.Instance.PaddleLengthLevel}");
		//Screen.SetResolution(640, 480, true);
		//currentMouseX = transform.position.x;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		PaddleMovement();
		BallManager.Instance.UpdateBallPositionsWhenStuckToPaddle();
	}

	internal void SetLength(int length)
	{
		GetComponent<Animator>().SetInteger("Length", length);
	}

	private void PaddleMovement()
	{
		float mousePositionX = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0)).x;
		float leftBoundX = leftBound.transform.position.x + spriteRenderer.size.x / 2;
		float rightBoundX = rightBound.transform.position.x - spriteRenderer.size.x / 2;
		if (mousePositionX != currentMouseX)
		{
			float difference = (mousePositionX - currentMouseX) * 1.6f;
			float xPositionForCheck = transform.position.x + difference;
			float finalMouseX = Mathf.Clamp(xPositionForCheck, leftBoundX, rightBoundX);
			currentMouseX += mousePositionX - currentMouseX;
			transform.position = new Vector3(finalMouseX, transform.position.y);
		}
	}

	private float RoundToTwoPlaces(float number) => Mathf.Round(number * 100.0f) / 100.0f;

	private float EvaluateAngleForFutureVelocity(float difference, float paddleLengthAndColliderBoundSum, float maxBounceAngleRange)
	{
		float maxDifference = paddleLengthAndColliderBoundSum / 2.0f;
		float differenceRatio = difference / maxDifference;
		return 90.00f - (maxBounceAngleRange * differenceRatio);
	}

	public void SetMagnetZapVisibility(bool visible)
	{
		if (visible)
			MagnetActive = true;
		magnetZap.SetActive(visible);
	}

	private void Collision(GameObject bouncedObject)
	{
		if (bouncedObject.GetComponent<Ball>() || bouncedObject.GetComponent<Bullet>() || bouncedObject.GetComponent<SpaceDjoel>())
		{
			IBrickBuster brickBuster = bouncedObject.GetComponent<IBrickBuster>();
			//Vector3 hitPoint1 = collision.GetContact(0).point;
			//Vector3 hitPoint2 = collision.GetContact(1).point;
			Vector3 paddleCenter = new Vector3(transform.position.x, transform.position.y);

			//float hitPointX = (hitPoint2.x + hitPoint1.x) / 2.0f;
			//float difference = Mathf.Round((paddleCenter.x - hitPointX) * 100.0f) / 100.0f;
			float difference = RoundToTwoPlaces(paddleCenter.x - bouncedObject.transform.position.x);
			float maxBounceAngleRange = bouncedObject.GetComponent<Ball>() ? 50.00f : 20.00f;
			float paddleWidth = GetComponent<BoxCollider2D>().size.x;
			float ballWidth = bouncedObject.GetComponent<BoxCollider2D>().size.x;
			float angle = EvaluateAngleForFutureVelocity(difference, paddleWidth + ballWidth, maxBounceAngleRange);

			if (bouncedObject.GetComponent<Ball>())
			{
				Ball ball = brickBuster as Ball;
				Bounds paddleColliderBounds = GetComponent<BoxCollider2D>().bounds;
				Vector3 particlePosition = new Vector3(bouncedObject.transform.position.x, paddleColliderBounds.max.y + .001f, bouncedObject.transform.position.z);
				Vector2 futureVelocity;
				//Ball ball = collision.gameObject.GetComponent<Ball>();
				if (ball.BallSize != (int)GameManager.Instance.BallSize)
					soundManager.PlaySfx("Ball Size Change");
				ball.UpdateSize();
				ball.FinishThrust();
				ParticleManager.Instance.RemoveThrustingFlame(ball.gameObject);
				float ballSpeedLerp = Mathf.Lerp(BallManager.minBallSpeed, BallManager.maxBallSpeed, 0.7f);
				if (!GameManager.Instance.MagnetPaddle || brickBuster.LastFrameVelocity.magnitude >= ballSpeedLerp)
				{
					ball.transform.position = new Vector3(ball.transform.position.x, paddleColliderBounds.max.y + ball.GetComponent<BoxCollider2D>().bounds.extents.y + .001f, ball.transform.position.z);
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * Mathf.Max(brickBuster.LastFrameVelocity.magnitude * 0.80f, BallManager.minBallSpeed);
					futureVelocity = new Vector2(RoundToTwoPlaces(futureVelocity.x), futureVelocity.y);
					if (brickBuster.LastFrameVelocity.magnitude < ballSpeedLerp)
						soundManager.PlaySfx("Normal Ball Bounce");
					else
						BounceWithStrongForce(particlePosition);
					brickBuster.CurrentVelocity = futureVelocity;
				}
				else
				{
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * BallManager.minBallSpeed;
					futureVelocity = new Vector2(RoundToTwoPlaces(futureVelocity.x), futureVelocity.y);
					ballSpeedLerp = Mathf.Lerp(BallManager.minBallSpeed, BallManager.maxBallSpeed, 0.4f);
					if (brickBuster.LastFrameVelocity.magnitude < ballSpeedLerp)
						soundManager.PlaySfx("Magnet Stick");
					else
						BounceWithStrongForce(particlePosition);
					ball.StickToPaddle(futureVelocity, difference);
				}

				if (GameManager.Instance.DescendingBricks)
				{
					soundManager.PlaySfx("Brick Descend");
					GameManager.Instance.DescendBrickRows();
				}
			}
			else if (bouncedObject.GetComponent<Bullet>())
			{
				soundManager.PlaySfx("Bullet Bounce");
				bouncedObject.GetComponent<Bullet>().BounceWithRandomAngle(-1);
			}
			else if (bouncedObject.GetComponent<SpaceDjoel>())
			{
				Vector2 futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * Mathf.Max(brickBuster.LastFrameVelocity.magnitude * 0.50f, SpaceDjoelManager.paddleBounceForce);
				soundManager.PlaySfx("Normal Ball Bounce");
				brickBuster.CurrentVelocity = futureVelocity;
			}
		}
	}

	private void BounceWithStrongForce(Vector3 position)
	{
		ParticleManager.Instance.GenerateHighSpeedPaddleBounceSparkles(position);
		soundManager.PlaySfx("Fast Ball Bounce");
	}
}
