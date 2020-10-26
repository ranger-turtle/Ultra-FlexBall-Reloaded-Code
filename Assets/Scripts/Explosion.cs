using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Explosion : MonoBehaviour
{
	private LevelSoundLibrary levelSoundLibrary = LevelSoundLibrary.Instance;

	private void Start()
	{
		Debug.Log("Explosion begin");
		levelSoundLibrary.PlaySfx(levelSoundLibrary.explosion);
		//GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 0.3f));
	}

	private void Activate() => GetComponent<BoxCollider2D>().enabled = true;

	private void Destroy() => Destroy(gameObject);

	private void OnTriggerEnter2D(Collider2D collider)
	{
		//Debug.Break();
		Debug.Log("Explosion hit");
		StartCoroutine(WaitForExplosion(collider));
	}

	private IEnumerator WaitForExplosion(Collider2D collider)
	{
		yield return new WaitForSeconds(0.050f);
		//BONUS try finding better way how to support destroyed bricks
		Brick brick = collider.gameObject.GetComponent<Brick>();
		if (brick && !brick.Broken)
		{
			//TODO make power-up to jump to far-left, near-left, near-right and far-right
			PowerUpManager.Instance.IncreaseMeter(1, brick.transform.position, new Vector2(0.3f, 0.5f));
			if (!brick.brickType.Properties.ExplosionResistant)
				brick.Break(brick.brickType.Properties.Points / 2, true);
		}
	}
}
