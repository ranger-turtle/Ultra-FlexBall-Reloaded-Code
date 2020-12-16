using System.Collections;
using UnityEngine;

//BONUS make explosions in corners in case of large area explosions
[RequireComponent(typeof(BoxCollider2D))]
public class Explosion : MonoBehaviour
{
	protected virtual string ExplosionSoundName { get; } = "Explosion";

	private void Start() => SoundManager.Instance.PlaySfx(ExplosionSoundName);

	protected void Destroy() => Destroy(gameObject);

	protected virtual void OnTriggerEnter2D(Collider2D collider) => WaitForExplosion(collider); // StartCoroutine(WaitForExplosion(collider));

	private void WaitForExplosion(Collider2D collider)
	{
		//yield return new WaitForSeconds(Time.deltaTime * 1.7f);
		//BONUS try finding better way how to support destroyed bricks
		//BONUS try fixing uneven explosion assign
		if (collider)
		{
			Brick brick = collider.gameObject.GetComponent<Brick>();
			if (brick && !brick.Broken)
			{
				brick.TryIncreasePowerUpField();
				if (!brick.brickType.Properties.ExplosionResistant)
				{
					brick.Break(brick.brickType.Properties.Points / 2, force: true);
				}
			}
		}
	}
}
