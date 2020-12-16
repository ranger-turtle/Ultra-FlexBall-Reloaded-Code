using System.Collections;
using System.Linq;
using UnityEngine;

//FIXME prevent getting Djoel out of walls (e.g. after teleport, as you have already done with ball)
public class SpaceDjoel : MonoBehaviour, IBrickBuster
{
	[SerializeField]
	private LayerMask layerMask;

	private const int rotateFrames = 9;
	private const int djoelFrames = 40;

	[SerializeField]
	private Sprite[] yellowDjoelSprites;
	[SerializeField]
	private Sprite[] redDjoelSprites;
	[SerializeField]
	private Sprite[] greenDjoelSprites;
	[SerializeField]
	private Sprite[] blueDjoelSprites;

	private Sprite[][] allSprites;

	private Sprite[] chosenSprites;

	private int rawAnimationIndex = 20;//index which may be negative
	private int animationIndex = 20;
	private int increment = 1;
	private int firstFrameBound = 16;
	private int SecondFrameBound => firstFrameBound + rotateFrames - 1;

	private SpriteRenderer djoelSpriteRenderer;

	public Vector2 LastFrameVelocity { get; set; } = Vector2.zero;
	public Vector3 LastFramePosition { get; set; }
	public Vector2 CurrentVelocity { get; set; } = Vector2.zero;
	public Vector3 LastHitPoint { get; set; }
	public Vector2 LastHitNormal { get; set; }

	public bool Teleport { get; set; }

	private Bounds thisBounds;
	private BoxCollider2D thisCollider;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private Bounds oblivion;

	public const float maxDjoelVelocityMagnitude = 1.5f;
	public const float acceleration = 1.002f;

	public void Start()
	{
		allSprites = new Sprite[][] { yellowDjoelSprites, redDjoelSprites, greenDjoelSprites, blueDjoelSprites };
		djoelSpriteRenderer = GetComponent<SpriteRenderer>();
		chosenSprites = allSprites[Random.Range(0, allSprites.Length)];
		oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>().bounds;

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();

		thisCollider = GetComponent<BoxCollider2D>();
		thisBounds = thisCollider.bounds;

		float initialForce = SpaceDjoelManager.initialForce;
		Vector2[] initialVelocities = new Vector2[] { new Vector2(-initialForce, -initialForce), new Vector2(-0.07f, -initialForce), new Vector2(0.07f, -initialForce), new Vector2(initialForce, -initialForce) };
		CurrentVelocity = initialVelocities[Random.Range(0, initialVelocities.Length)];

		ValidateIndices();
		StartCoroutine(Animation());
	}

	private IEnumerator Animation()
	{
		while (true)
		{
			if (IsOnRangeBounds(rawAnimationIndex))
			{
				increment *= -1;
				yield return new WaitForSeconds(0.1f);
			}
			else
				yield return new WaitForSeconds(0.06f);
			djoelSpriteRenderer.sprite = chosenSprites[animationIndex];
			rawAnimationIndex += increment;
			ValidateIndices();
		}
	}

	private int GetIndexFromAngle(float angle) => Mathf.FloorToInt(angle / 9.0f);

	private float GetAngleInFullTurnRange(Vector2 vector)
	{
		float rawAngle = Vector2.Angle(Vector2.up, vector);
		//Debug.Log($"Raw angle: {rawAngle}, vector.x: {vector.x}");
		return vector.x >= 0 ? rawAngle : 360 - rawAngle;
	}

	private bool IsInsideAnimationRangeBounds(int index) => index >= firstFrameBound && index <= SecondFrameBound;

	private bool IsOnRangeBounds(int index) => index == firstFrameBound || index == SecondFrameBound;

	private void FixedUpdate()
	{
		LastFrameVelocity = CurrentVelocity;
		LastFramePosition = transform.position;

		OnCollision();
		CheckIfOblivion();
	}

	private void OnCollision()
	{
		RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position, thisCollider.size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask);
		if (boxCastHit.Length > 0)
		{
			RaycastHit2D firstRaycast = boxCastHit[0];
			float x = firstRaycast.normal.x != 0 ? firstRaycast.point.x + (thisBounds.extents.x + .01f) * firstRaycast.normal.x : firstRaycast.centroid.x;
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
					if (firstRaycast.normal == Vector2.up)
					{
						float velocityX = LastFrameVelocity.x != 0 ? CurrentVelocity.x : CurrentVelocity.x + 0.1f * new int[] { -1, 1 }[Random.Range(0, 2)];
						CurrentVelocity = new Vector2(velocityX, Mathf.Max(-CurrentVelocity.y * 0.75f, 0.04f));
					}
					else
						CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
				}
			}
		}
		else
		{
			CurrentVelocity = new Vector2(CurrentVelocity.x, Mathf.Max(-SpaceDjoelManager.paddleBounceForce, CurrentVelocity.y - 0.001f));
			transform.position = new Vector2(transform.position.x + CurrentVelocity.x, transform.position.y + CurrentVelocity.y);
		}
	}

	private void ValidateIndices()
	{
		int newIndex = GetIndexFromAngle(GetAngleInFullTurnRange(CurrentVelocity));
		firstFrameBound = newIndex - 4;
		if (!IsInsideAnimationRangeBounds(rawAnimationIndex))
		{
			rawAnimationIndex = newIndex;
			firstFrameBound = rawAnimationIndex - rotateFrames / 2;
			//Debug.Log($"Angle: {GetAngleInFullTurnRange(CurrentVelocity)}");
		}
		animationIndex = rawAnimationIndex % 40;
		if (animationIndex < 0)
			animationIndex = djoelFrames + animationIndex;
	}

	private void Remove()
	{
		SpaceDjoelManager.Instance.Remove(gameObject);
	}

	private void CheckIfOblivion()
	{
		if (thisCollider.bounds.max.y < oblivion.min.y)
		{
			SoundManager.Instance.PlaySfx("Space Djoel Fall");
			Remove();
		}
	}
}
