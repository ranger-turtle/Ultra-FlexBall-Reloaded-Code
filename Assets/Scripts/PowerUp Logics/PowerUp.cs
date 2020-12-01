using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PowerUp : MonoBehaviour
{
	public int Score { get; set; } = 500;
	[SerializeField]
	protected AudioClip collectSound;
	public Vector2 CurrentVelocity;
	private Vector2 LastFrameVelocity;
	private readonly float minVelY = -BallManager.minBallSpeed;

	private BoxCollider2D oblivion;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private void Start()
	{
		oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>();
		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();
	}

	protected virtual void TriggerAction() => Destroy(gameObject);

	protected virtual void PlaySound() => SoundManager.Instance.PlaySfx(collectSound);

	private void FixedUpdate()
	{
		LastFrameVelocity = CurrentVelocity;
		if (CurrentVelocity.y > minVelY)
			CurrentVelocity -= new Vector2(0, 0.0025f);
		transform.position += new Vector3(CurrentVelocity.x, CurrentVelocity.y);
		CheckIfBelowOblivion();
		CheckIfOutsideWall();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.GetComponent<Paddle>())
		{
			Debug.Log("Power Up");
			GameManager.Instance.AddToScore(Score);
			TriggerAction();
			PlaySound();
		}
		else if (!collision.gameObject.GetComponent<PaddleSideCollider>())
			CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, collision.GetContact(0).normal);
	}

	private void CheckIfBelowOblivion()
	{
		Bounds thisBounds = GetComponent<BoxCollider2D>().bounds;
		if (thisBounds.max.y <= oblivion.bounds.max.y)
		{
			SoundManager.Instance.PlaySfx("Power Up Fall");
			Destroy(gameObject);
		}
	}

	private void CheckIfOutsideWall()
	{
		Bounds powerUpBounds = GetComponent<BoxCollider2D>().bounds;
		Vector2 normal = Vector2.zero;
		Bounds leftWallColliderBounds = leftWallCollider.GetComponent<BoxCollider2D>().bounds;
		Bounds rightWallColliderBounds = rightWallCollider.GetComponent<BoxCollider2D>().bounds;
		if (powerUpBounds.min.x < leftWallColliderBounds.max.x)
		{
			transform.position = new Vector3(leftWallColliderBounds.max.x + 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(1, 0);
		}
		else if (powerUpBounds.max.x > rightWallColliderBounds.min.x)
		{
			transform.position = new Vector3(rightWallColliderBounds.min.x - powerUpBounds.size.x - 0.01f, transform.position.y, transform.position.z);
			normal = new Vector2(-1, 0);
		}
		if (normal != Vector2.zero)
		{
			float x = LastFrameVelocity.x == 0 ? 0.14f : LastFrameVelocity.x;
			CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(new Vector2(x * normal.x, LastFrameVelocity.y), normal);
		}
	}
}
