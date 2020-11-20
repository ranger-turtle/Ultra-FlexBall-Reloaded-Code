using UnityEngine;

public class SplitBall : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		BallManager.Instance.SplitBall();
		Debug.Log("Ball Split");
	}
}
