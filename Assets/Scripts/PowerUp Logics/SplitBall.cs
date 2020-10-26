using UnityEngine;

public class SplitBall : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		BallManager.Instance.SplitBall();
		Debug.Log("Ball Split");
	}
}
