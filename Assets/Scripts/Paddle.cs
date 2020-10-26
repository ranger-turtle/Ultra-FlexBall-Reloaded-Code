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
#pragma warning restore CS0649 // Field 'Paddle.leftBound' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.rightBound' is never assigned to, and will always have its default value null
	private GameObject rightBound;
#pragma warning restore CS0649 // Field 'Paddle.rightBound' is never assigned to, and will always have its default value null
	private GameObject magnet;
	private GameObject shooter;
	private GameObject paddleElectrodes;
	[SerializeField]
#pragma warning disable CS0649 // Field 'Paddle.sideElectrodes' is never assigned to, and will always have its default value null
	private GameObject sideElectrodes;
#pragma warning restore CS0649 // Field 'Paddle.sideElectrodes' is never assigned to, and will always have its default value null
	private LevelSoundLibrary levelSoundLibrary;
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
		levelSoundLibrary = GameObject.Find("Game").GetComponent<LevelSoundLibrary>();
		SetLength(GameManager.Instance.PaddleLengthLevel);
		Debug.Log($"Paddle Length: {GameManager.Instance.PaddleLengthLevel}");
		magnet = transform.Find("magnet").gameObject;
		shooter = transform.Find("shooter").gameObject;
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

	private float EvaluateAngleForFutureVelocity(float difference, float paddleLengthAndColliderBoundSum, float maxBounceAngleRange)
	{
		float maxDifference = paddleLengthAndColliderBoundSum / 2.0f;
		float differenceRatio = difference / maxDifference;
		return 90.00f - (maxBounceAngleRange * differenceRatio);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.GetComponent<Ball>() || collision.gameObject.GetComponent<Bullet>())
		{
			Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
			Vector3 hitPoint1 = collision.GetContact(0).point;
			Vector3 hitPoint2 = collision.GetContact(1).point;
			Vector3 paddleCenter = new Vector3(transform.position.x, transform.position.y);

			rb.velocity = Vector2.zero;

			//float hitPointX = (hitPoint2.x + hitPoint1.x) / 2.0f;
			//float difference = Mathf.Round((paddleCenter.x - hitPointX) * 100.0f) / 100.0f;
			float difference = Mathf.Round((paddleCenter.x - collision.gameObject.transform.position.x) * 100.0f) / 100.0f;
			float maxBounceAngleRange = collision.gameObject.GetComponent<Ball>() ? 50.00f : 20.00f;
			float paddleWidth = GetComponent<BoxCollider2D>().size.x;
			float ballWidth = collision.gameObject.GetComponent<BoxCollider2D>().size.x;
			float angle = EvaluateAngleForFutureVelocity(difference, paddleWidth + ballWidth, maxBounceAngleRange);


			if (collision.gameObject.GetComponent<Ball>())
			{
				Vector2 futureVelocity;
				Ball ball = collision.gameObject.GetComponent<Ball>();
				if (ball.BallSize != (int)GameManager.Instance.BallSize)
					levelSoundLibrary.PlaySfx(levelSoundLibrary.ballSizeChange);
				ball.UpdateSize();
				if (!GameManager.Instance.MagnetPaddle)
				{
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * Mathf.Max(rb.velocity.magnitude / 2, BallManager.Instance.initialForce);
					levelSoundLibrary.PlaySfx(levelSoundLibrary.normalBallBounce);
					rb.velocity = futureVelocity;
				}
				else
				{
					float stickXPosition = difference;
					//if (ball.transform.position.x < collision.GetContact(0).point.x || ball.transform.position.x > collision.GetContact(1).point.x)
						//difference = ball.transform.position.x;
					futureVelocity = PhysicsHelper.GetAngledVelocity(angle) * BallManager.Instance.initialForce;
					levelSoundLibrary.PlaySfx(levelSoundLibrary.magnetStick);
					ball.StickToPaddle(futureVelocity, difference);
				}

				if (GameManager.Instance.DescendingBricks)
				{
					levelSoundLibrary.PlaySfx(levelSoundLibrary.brickDescend);
					GameManager.Instance.DescendBrickRows();
				}
			}
			else if (collision.gameObject.GetComponent<Bullet>())
			{
				//TODO levelSoundLibrary.PlaySfx(levelSoundLibrary.bulletBounce);
				rb.velocity = PhysicsHelper.GetAngledVelocity(angle) * BallManager.Instance.initialForce;
			}
		}
	}
}
