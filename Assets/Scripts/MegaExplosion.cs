using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MegaExplosion : Explosion
{
	protected override string ExplosionSoundName => "Mega Explosion";

	protected override void OnTriggerEnter2D(Collider2D collider)
	{
		Brick brick = collider.gameObject.GetComponent<Brick>();
		if (brick && !brick.Broken)
		{
			brick.TryIncreasePowerUpField();
			brick.Break(brick.brickType.Properties.Points / 4, force: true);
		}
	}
}
