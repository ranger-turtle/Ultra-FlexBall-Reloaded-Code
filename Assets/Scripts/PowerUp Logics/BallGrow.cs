using UnityEngine;

public class BallGrow : PowerUp
{
	[SerializeField]
#pragma warning disable CS0649 // Field 'BallGrow.megaBall' is never assigned to, and will always have its default value null
	private AudioClip megaBall;
#pragma warning restore CS0649 // Field 'BallGrow.megaBall' is never assigned to, and will always have its default value null

	protected override void PlaySound()
	{
		if (GameManager.Instance.BallSize == BallSize.Big)
			SoundManager.Instance.PlaySfx(collectSound);
		else
			SoundManager.Instance.PlaySfx(megaBall);
	}

	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseBall();
		Debug.Log("Ball Grow!");
	}
}