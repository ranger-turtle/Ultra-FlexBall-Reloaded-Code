using UnityEngine;

public class PenetratingBall : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.PenetratingBall = true;
		BallManager.Instance.ApplyParticlesToPenetratingBalls();
		Debug.Log("Penetrating Ball");
	}
}
