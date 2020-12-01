using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Bullet : MonoBehaviour, IBrickBuster
{
	private const float FirstWallAngle = 95.0f;
	private BoxCollider2D ballBarrier;
	public Vector2 VelocityBeforeHit { get; private set; }
	private int bouncePoints;

	public Vector2 LastFrameVelocity { get; set; }
	public Vector3 LastFramePosition { get; set; }
	public Vector2 CurrentVelocity { get; set; }
	public Vector3 LastHitPoint { get; set; }
	public Vector2 LastHitNormal { get; set; }

	public bool Teleport { get; set; }

	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	[SerializeField]
	private LayerMask layerMaskWithoutBallBarrier;
	[SerializeField]
	private LayerMask layerMaskWithBallBarrier;

	private GameObject oblivion;
	private Bounds bulletColliderBounds;

    void Start()
    {
		ballBarrier = GameObject.Find("BallBarrier").GetComponent<BoxCollider2D>();
		bouncePoints = GameManager.Instance.ShooterLevel;
		CurrentVelocity = new Vector2(0, 0.08f);
		//if (GameManager.Instance.ShooterLevel < 2)
			//Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), ballBarrier.GetComponent<Collider2D>());

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();
		oblivion = GameObject.Find("Oblivion");
		bulletColliderBounds = GetComponent<BoxCollider2D>().bounds;
	}

	private void FixedUpdate()
	{
		LastFrameVelocity = CurrentVelocity;
		LastFramePosition = transform.position;
		CheckIfInsideLeftWall();
		CheckIfInsideRightWall();
		CheckIfOutOfBoard();
		OnCollision();
	}

	private void OnCollision()
	{
		VelocityBeforeHit = CurrentVelocity;

		int layerMask = bouncePoints > 1 ? layerMaskWithBallBarrier : layerMaskWithoutBallBarrier;
		RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position, GetComponent<BoxCollider2D>().size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask).ToArray();
		if (boxCastHit.Length > 0)
		{
			Vector2 totalNormal = Vector2.zero;
			RaycastHit2D firstRaycast = boxCastHit[0];
			float x = firstRaycast.normal.x != 0 ? firstRaycast.point.x + (GetComponent<BoxCollider2D>().bounds.extents.x + .01f) * firstRaycast.normal.x : firstRaycast.centroid.x;
			float y = firstRaycast.normal.y != 0 ? firstRaycast.point.y + (GetComponent<BoxCollider2D>().bounds.extents.y + .01f) * firstRaycast.normal.y : firstRaycast.centroid.y;
			transform.position = new Vector2(x, y);

			//if (boxCastHit[0].collider.GetComponent<Brick>() && totalNormal.normalized.y > 0)
			//Debug.Break();

			if (firstRaycast.collider.GetComponent<Paddle>())
				firstRaycast.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
			else
			{
				bool isColliderBrick = firstRaycast.collider.GetComponent<Brick>();
				foreach (RaycastHit2D raycastHit2D in boxCastHit)
				{
					LastHitNormal = raycastHit2D.normal;
					LastHitPoint = raycastHit2D.point;
					raycastHit2D.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
					if (isColliderBrick || firstRaycast.collider == ballBarrier)
					{
						bouncePoints--;
						if (bouncePoints == 0)
							Destroy(gameObject);
						else
						{
							if (isColliderBrick && !GameManager.Instance.PenetratingBall)
								CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
							else if (!isColliderBrick)
							{
								SoundManager.Instance.PlaySfx("Bullet Bounce");
								BounceWithRandomAngle(1);
							}
						}
						//Debug.Log($"Bounce points: {bouncePoints}");
					}
					else
						CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
				}
			}
			Debug.Log($"Normal: {totalNormal.normalized}");
		}
		else
			transform.position = new Vector2(transform.position.x + CurrentVelocity.x, transform.position.y + CurrentVelocity.y);
	}

	public void BounceWithRandomAngle(float normalY)
	{
		float add = normalY > 0 ? 180 : 0;
		float angle = Random.Range(20, 160) + add;
		CurrentVelocity = PhysicsHelper.GetAngledVelocity(angle) * CurrentVelocity.magnitude;
	}

	private void CheckIfOutOfBoard()
	{
		if (bulletColliderBounds.min.y > ballBarrier.GetComponent<BoxCollider2D>().bounds.max.y || bulletColliderBounds.max.y < oblivion.GetComponent<BoxCollider2D>().bounds.min.y)
			Destroy(gameObject);
	}

	private void CheckIfInsideLeftWall()
	{
		if (leftWallCollider.bounds.Contains(new Vector3(bulletColliderBounds.min.x, bulletColliderBounds.min.y, leftWallCollider.transform.position.z)))
		{
			CurrentVelocity = PhysicsHelper.GetAngledVelocity(FirstWallAngle) * CurrentVelocity.magnitude;
			transform.position = new Vector3(leftWallCollider.bounds.max.x + bulletColliderBounds.extents.x + .09f, transform.position.y, transform.position.z);
		}
	}

	private void CheckIfInsideRightWall()
	{
		if (rightWallCollider.bounds.Contains(new Vector3(bulletColliderBounds.max.x, bulletColliderBounds.max.y, rightWallCollider.transform.position.z)))
		{
			CurrentVelocity = PhysicsHelper.GetAngledVelocity(FirstWallAngle - 10.0f) * CurrentVelocity.magnitude;
			transform.position = new Vector3(rightWallCollider.bounds.min.x - bulletColliderBounds.extents.x - .09f, transform.position.y, transform.position.z);
		}
	}
}
