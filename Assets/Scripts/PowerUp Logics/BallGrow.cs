using UnityEngine;

public class BallGrow : PowerUp
{
	protected override void PlaySound()
	{
		if (GameManager.Instance.BallSize == BallSize.Big)
			SoundManager.Instance.PlaySfx(powerUpName);
		else
			SoundManager.Instance.PlaySfx("Megajocke");
	}

	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseBall();
		Debug.Log("Ball Grow!");
	}
}