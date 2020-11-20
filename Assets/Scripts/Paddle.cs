using UnityEngine;

//TODO make side bounce
//TODO make sparkles sprinkling from paddle side
//TODO make sparkles sprinkling from paddle when ball hit when moving fast
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
#pragma warning restore CS0649 // Field 'Paddle.leftBound' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.rightBound' is never assigned to, and will always have its default value null
	private GameObject rightBound;
#pragma warning restore CS0649 // Field 'Paddle.rightBound' is never assigned to, and will always have its default value null
	private GameObject magnet;
	private GameObject shooter;
	private GameObject megaMissileTurret;
	private GameObject paddleElectrodes;
	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.sideElectrodes' is never assigned to, and will always have its default value null
	private GameObject sideElectrodes;
#pragma warning restore CS0649 // Field 'Paddle.sideElectrodes' is never assigned to, and will always have its default value null
	private SoundManager soundManager;
	private float ballBounceFactor;

	private float currentMouseX;

	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.hudManager' is never assigned to, and will always have its default value null
	private HUDManager hudManager;
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
		Cursor.visible = false;
		soundManager = GameObject.Find("Game").GetComponent<SoundManager>();
		SetLength(GameManager.Instance.PaddleLengthLevel);
		Debug.Log($"Paddle Length: {GameManager.Instance.PaddleLengthLevel}");
		magnet = transform.Find("magnet").gameObject;
		shooter = transform.Find("shooter").gameObject;
		megaMissileTurret = transform.Find("megaMissileTurret").gameObject;
		paddleElectrodes = transform.Find("barrierElectrodes").gameObject;
		//Screen.SetResolution(640, 480, true);
		//currentMouseX = transform.position.x;
	}

	// Update is called once per frame
	void Update()
	{
		if (!hudManager.Paused)
		{
			PaddleMovement();
			BallManager.Instance.UpdateBallPositionsWhenStuckToPaddle();
		}
	}

	internal void SetLength(int length)
	{
		GetComponent<Animator>().SetInteger("Length", length);
	}

	private void PaddleMovement()
	{
		float mousePositionX = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0)).x;
		float leftBoundX = leftBound.transform.position.x + GetComponent<SpriteRenderer>().size.x / 2;
		float rightBoundX = rightBound.transform.position.x - GetComponent<SpriteRenderer>().size.x / 2;
		if (mousePositionX != currentMouseX)
		{
			float xPositionForCheck = transform.position.x + mousePositionX - currentMouseX;
			float finalMouseX = Mathf.Clamp(xPositionForCheck, leftBoundX, rightBoundX);
			currentMouseX += mousePositionX - currentMouseX;
			transform.position = new Vector3(finalMouseX, transform.position.y);
			ballBounceFactor += currentMouseX - mousePositionX;
		}
	}

	private float RoundToTwoPlaces(float number) => Mathf.Round(number * 100.0f) / 100.0f;

	private float EvaluateAngleForFutureVelocity(float difference, float paddleLengthAndColliderBoundSum, float maxBounceAngleRange)
	{
		float maxDifference = paddleLengthAndColliderBoundSum / 2.0f;
		float differenceRatio = difference / maxDifference;
		return 90.00f - (maxBounceAngleRange * differenceRatio);
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
				Vector2 futureVelocity;
				//Ball ball = collision.gameObject.GetComponent<Ball>();
				if (ball.BallSize != (int)GameManager.Instance.BallSize)
					soundManager.PlaySfx("Ball Size Change");
				ball.UpdateSize();
				ball.FinishThrust();
				ParticleManager.Instance.RemoveThrustingFlame(ball.gameObject);
				if (!GameManager.Instance.MagnetPaddle || brickBuster.CurrentVelocity.magnitude >= BallManager.maxBallSpeed * 0.8f)
				{
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * Mathf.Max(brickBuster.CurrentVelocity.magnitude * 0.80f, BallManager.minBallSpeed);
					futureVelocity = new Vector2(RoundToTwoPlaces(futureVelocity.x), futureVelocity.y);
					if (brickBuster.CurrentVelocity.magnitude < BallManager.maxBallSpeed * 0.8f)
						soundManager.PlaySfx("Normal Ball Bounce");
					else
						soundManager.PlaySfx(DefaultSoundLibrary.Instance.quickBallBounce);
					brickBuster.CurrentVelocity = futureVelocity;
				}
				else
				{
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * BallManager.minBallSpeed;
					futureVelocity = new Vector2(RoundToTwoPlaces(futureVelocity.x), futureVelocity.y);
					if (brickBuster.CurrentVelocity.magnitude < BallManager.maxBallSpeed * 0.5f)
						soundManager.PlaySfx("Magnet Stick");
					else
						soundManager.PlaySfx(DefaultSoundLibrary.Instance.quickBallBounce);
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
				brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(angle) * brickBuster.CurrentVelocity.magnitude;
			}
			else if (bouncedObject.GetComponent<SpaceDjoel>())
			{
				Vector2 futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * Mathf.Max(brickBuster.CurrentVelocity.magnitude * 0.750f, SpaceDjoelManager.paddleBounceForce);
				soundManager.PlaySfx("Normal Ball Bounce");
				brickBuster.CurrentVelocity = futureVelocity;
			}
		}
	}
}
