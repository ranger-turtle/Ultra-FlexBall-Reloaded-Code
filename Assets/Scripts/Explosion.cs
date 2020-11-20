﻿using System.Collections;
using UnityEngine;

//TODO make bang sound only after first subsequent explosion
[RequireComponent(typeof(BoxCollider2D))]
public class Explosion : MonoBehaviour
{
	protected virtual string ExplosionSoundName { get; } = "Explosion";

	private void Start() => SoundManager.Instance.PlaySfx(ExplosionSoundName);

	protected void Destroy() => Destroy(gameObject);

	protected virtual void OnTriggerEnter2D(Collider2D collider) => StartCoroutine(WaitForExplosion(collider));

	private IEnumerator WaitForExplosion(Collider2D collider)
	{
		yield return new WaitForSeconds(0.010f);
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
