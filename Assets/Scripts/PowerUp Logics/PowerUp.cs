using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PowerUp : MonoBehaviour
{
	public int Score { get; set; } = 500;
	public int probability;
	[SerializeField]
	protected AudioClip collectSound;

	private BoxCollider2D oblivion;

	private void Start() => oblivion = GameObject.Find("Oblivion").GetComponent<BoxCollider2D>();

	public virtual void TriggerAction()
	{
		Destroy(gameObject);
	}

	public virtual void PlaySound() => LevelSoundLibrary.Instance.PlaySfx(collectSound);

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.GetComponent<Paddle>())
		{
			Debug.Log("Power Up");
			GameManager.Instance.AddToScore(Score);
			TriggerAction();
			PlaySound();
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		if (collider == oblivion)
		{
			LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.powerUpFall);
			Destroy(gameObject);
		}
	}
}
