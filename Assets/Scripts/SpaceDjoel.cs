using System.Collections;
using System.Linq;
using UnityEngine;

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

	private int animationIndex = 20;
	private int increment = 1;
	private int firstFrameBound = 16;
	private int SecondFrameBound
	{
		get
		{
			int raw = firstFrameBound + rotateFrames - 1;
			return raw < djoelFrames ? raw : raw - djoelFrames;
		}
	}

	private SpriteRenderer djoelSpriteRenderer;

	public Vector2 LastFrameVelocity { get; set; } = Vector2.zero;
	public Vector3 LastFramePosition { get; set; }
	public Vector2 CurrentVelocity { get; set; } = Vector2.zero;
	public Vector3 LastHitPoint { get; set; }
	public Vector2 LastHitNormal { get; set; }

	public bool Teleport { get; set; }

	private BoxCollider2D thisCollider;
	private BoxCollider2D leftWallCollider;
	private BoxCollider2D rightWallCollider;

	private BoxCollider2D oblivion;

	/*private List<Vector2> lastHitNormals;
	private bool shouldReflectVector;*/

	public const float maxDjoelVelocityMagnitude = 1.5f;
	public const float acceleration = 1.002f;

	#region tmp
	private static int s_id;
	private int id;
	#endregion

	public void Start()
	{
		id = s_id;
		s_id++;

		allSprites = new Sprite[][] { yellowDjoelSprites, redDjoelSprites, greenDjoelSprites, blueDjoelSprites };
		djoelSpriteRenderer = GetComponent<SpriteRenderer>();
		chosenSprites = allSprites[Random.Range(0, allSprites.Length)];
		oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>();

		leftWallCollider = GameObject.Find("LeftSideWall").GetComponent<BoxCollider2D>();
		rightWallCollider = GameObject.Find("RightSideWall").GetComponent<BoxCollider2D>();

		thisCollider = GetComponent<BoxCollider2D>();

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
			if (IsOnRangeBounds(animationIndex))
			{
				increment *= -1;
				yield return new WaitForSeconds(0.1f);
			}
			else
				yield return new WaitForSeconds(0.06f);
			Debug.Log($"Djoel no. {id} animation index: {animationIndex}");
			djoelSpriteRenderer.sprite = chosenSprites[animationIndex];
			animationIndex += increment;
			ValidateIndices();
		}
	}

	private int GetIndexFromAngle(float angle) => Mathf.FloorToInt(angle / 9.0f);

	private float GetAngleInFullTurnRange(Vector2 vector)
	{
		float rawAngle = Vector2.Angle(Vector2.up, vector);
		return vector.x >= 0 ? rawAngle : 360 - rawAngle;
	}

	private bool IsInsideAnimationRangeBounds(int index) => index >= firstFrameBound && index <= SecondFrameBound;

	private bool IsOnRangeBounds(int index) => index == firstFrameBound || index == SecondFrameBound;

	private void FixedUpdate()
	{
		LastFrameVelocity = CurrentVelocity;
		//UpdateVelocityAfterHittingBrick();
		LastFramePosition = transform.position;

		RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position, GetComponent<BoxCollider2D>().size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask);
		if (boxCastHit.Length > 0 && boxCastHit.Any(bch => !bch.collider.isTrigger))
		{
			Vector2 totalNormal = Vector2.zero;
			//RaycastHit2D raycastHitX = boxCastHit.FirstOrDefault(bch => bch.normal.x != 0);
			//RaycastHit2D raycastHitY = boxCastHit.FirstOrDefault(bch => bch.normal.y != 0);
			RaycastHit2D firstRaycast = boxCastHit[0];
			float x = firstRaycast.normal.x != 0 ? firstRaycast.point.x + (GetComponent<BoxCollider2D>().bounds.extents.x + .01f) * firstRaycast.normal.x : firstRaycast.centroid.x;
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

			boxCastHit = boxCastHit.Where(bch => !bch.collider.GetComponent<Brick>() || (bch.normal == firstRaycast.normal && (firstRaycast.normal.x != 0 && bch.collider.GetComponent<Brick>().x == firstRaycast.collider.GetComponent<Brick>().x || firstRaycast.normal.y != 0 && bch.collider.GetComponent<Brick>().y == firstRaycast.collider.GetComponent<Brick>().y))).ToArray();
			if (firstRaycast.normal == Vector2.up)
			{
				float velocityX = LastFrameVelocity.x != 0 ? CurrentVelocity.x : CurrentVelocity.x + 0.1f * new int[] { -1, 1 }[Random.Range(0, 2)];
				CurrentVelocity = new Vector2(velocityX, Mathf.Max(-CurrentVelocity.y / 2, 0.04f));
			}
			else
				CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, firstRaycast.normal);
			foreach (RaycastHit2D raycastHit2D in boxCastHit)
			{
				LastHitNormal = raycastHit2D.normal;
				LastHitPoint = raycastHit2D.point;
				raycastHit2D.collider.gameObject.SendMessage("Collision", gameObject, SendMessageOptions.DontRequireReceiver);
			}
			Debug.Log($"Normal: {totalNormal.normalized}");
			//if (boxCastHit[0].collider.GetComponent<Brick>() && totalNormal.normalized.y > 0)
			//Debug.Break();
		}
		else
		{
			CurrentVelocity = new Vector2(CurrentVelocity.x, Mathf.Max(-SpaceDjoelManager.paddleBounceForce, CurrentVelocity.y - 0.001f));
			transform.position = new Vector2(transform.position.x + CurrentVelocity.x, transform.position.y + CurrentVelocity.y);
		}
		CheckIfOblivion();
	}

	private void ValidateIndices()
	{
		int newIndex = GetIndexFromAngle(GetAngleInFullTurnRange(LastFrameVelocity));
		firstFrameBound = newIndex - 4;
		if (!IsInsideAnimationRangeBounds(animationIndex))
		{
			animationIndex = newIndex;
			firstFrameBound = animationIndex - rotateFrames / 2;
			Debug.Log($"Angle: {GetAngleInFullTurnRange(LastFrameVelocity)}");
		}
		if (firstFrameBound < 0)
			firstFrameBound = djoelFrames + firstFrameBound;//I used the addition since firstFrameBound in this line is negative, therefore it changes from addition to subtraction
		else if (firstFrameBound >= djoelFrames)
			firstFrameBound = djoelFrames - firstFrameBound;
	}

	/*private void UpdateVelocityAfterHittingBrick()
	{
		if (shouldReflectVector)
		{
			Vector2 outputNormal = new Vector2(0, 0);
			foreach (Vector2 normal in lastHitNormals)
				outputNormal += normal;
			outputNormal = outputNormal.normalized;
			if (Mathf.Abs(outputNormal.x) > Mathf.Abs(outputNormal.y))
				outputNormal = new Vector2(outputNormal.x, 0).normalized;
			else if (Mathf.Abs(outputNormal.x) < Mathf.Abs(outputNormal.y))
				outputNormal = new Vector2(0, outputNormal.y).normalized;
			else
				outputNormal = new Vector2(outputNormal.x, outputNormal.y).normalized;
			CurrentVelocity = PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, outputNormal);//.normalized * 1.2f;
			Debug.Log($"Djoel Body velocity: {CurrentVelocity}");
			if (outputNormal == Vector2.up)
			{
				float velocityX = LastFrameVelocity.x != 0 ? CurrentVelocity.x : CurrentVelocity.x + 0.1f * new int[] { -1, 1 }[Random.Range(0, 2)];
				CurrentVelocity = new Vector2(velocityX, CurrentVelocity.y / 3.5f + 4.0f);
			}
			//	djoelBody.velocity = (PhysicsHelper.GenerateReflectedVelocity(LastFrameVelocity, outputNormal) * SpaceDjoelManager.Instance.initialForce);

			shouldReflectVector = false;
		}
	}*/

	/*private void Reflect(Vector2 collisionNormal)
	{
		if (!shouldReflectVector)
		{
			lastHitNormals = new List<Vector2>();
			shouldReflectVector = true;
		}
		lastHitNormals.Add(collisionNormal);
		//if (djoelBody.velocity.magnitude < maxDjoelVelocityMagnitude)
			//djoelBody.velocity *= acceleration;
	}*/

	//private void OnCollisionEnter2D(Collision2D collision)
	//{
	//	if (collision.gameObject.tag == "ballCollidable")
	//	{
	//		Reflect(collision.GetContact(0).normal);
	//	}
	//}

	private void Remove()
	{
		SpaceDjoelManager.Instance.Remove(gameObject);
	}

	private void CheckIfOblivion()
	{
		if (thisCollider.bounds.max.y < oblivion.GetComponent<BoxCollider2D>().bounds.min.y)
		{
			SoundManager.Instance.PlaySfx("Space Djoel Fall");
			Remove();
		}
	}
}
