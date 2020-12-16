using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MegaMissile : MonoBehaviour
{
	public Vector2 CurrentVelocity { get; set; }
	private BoxCollider2D ballBarrier;
	private float velocityIncrement = 0.008f;

	[SerializeField]
	private GameObject megaExplosionPrefab;

	[SerializeField]
	private LayerMask layerMask;

    void Start()
    {
		CurrentVelocity = new Vector2(0, 0.005f);
		ballBarrier = GameObject.Find("BallBarrier").GetComponent<BoxCollider2D>();
	}

	private void FixedUpdate()
	{
		OnCollision();
	}

	private void OnCollision()
	{
		RaycastHit2D boxCastHit = Physics2D.BoxCast(transform.position, GetComponent<BoxCollider2D>().size, 0, CurrentVelocity, CurrentVelocity.magnitude, layerMask);
		if (boxCastHit)
		{
			Instantiate(megaExplosionPrefab, boxCastHit.centroid, Quaternion.identity);
			Destroy(gameObject);
		}
		else
		{
			transform.position += new Vector3(0, velocityIncrement, 0);
			velocityIncrement += 0.001f;
		}

		CheckIfAboveBallBarrier();
	}

	private void CheckIfAboveBallBarrier()
	{
		if (GetComponent<BoxCollider2D>().bounds.min.y > ballBarrier.GetComponent<BoxCollider2D>().bounds.max.y)
			Destroy(gameObject);
	}
}
