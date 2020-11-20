using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PowerUp : MonoBehaviour
{
	public int Score { get; set; } = 500;
	[SerializeField]
	protected AudioClip collectSound;
	public Vector2 CurrentVelocity;
	private Vector2 LastFrameVelocity;
	private readonly float maxSpeed = 0.2f;

	private BoxCollider2D oblivion;

	private void Start() => oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>();

	protected virtual void TriggerAction() => Destroy(gameObject);

	protected virtual void PlaySound() => SoundManager.Instance.PlaySfx(collectSound);

	private void FixedUpdate()
	{
		CurrentVelocity -= new Vector2(0, 0.0015f);
		LastFrameVelocity = CurrentVelocity;
		if (CurrentVelocity.magnitude < maxSpeed)
			transform.position += new Vector3(CurrentVelocity.x, CurrentVelocity.y);
		CheckIfBelowOblivion();
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
		else
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
}
