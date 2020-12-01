using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WallCollider : MonoBehaviour
{
	private void Collision(GameObject gameObject)
	{
		if (gameObject.GetComponent<IBrickBuster>() != null || gameObject.GetComponent<PowerUp>())
			SoundManager.Instance.PlaySfx("Hit Wall");
	}

	private void OnCollisionEnter2D(Collision2D collision) => Collision(collision.gameObject);
}
