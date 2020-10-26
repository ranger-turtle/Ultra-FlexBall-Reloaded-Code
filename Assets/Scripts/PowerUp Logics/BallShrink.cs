using UnityEngine;

public class BallShrink : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.DecreaseBall();
		Debug.Log("Ball Shrink");
	}
}