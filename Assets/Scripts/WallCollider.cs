using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WallCollider : MonoBehaviour
{
	private void OnCollisionEnter2D(Collision2D collision)
	{
		GameObject collisionObj = collision.gameObject;
		if (collisionObj.GetComponent<Ball>() || collisionObj.GetComponent<Bullet>() || collisionObj.GetComponent<PowerUp>())
			LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.hitWall);
	}
}
