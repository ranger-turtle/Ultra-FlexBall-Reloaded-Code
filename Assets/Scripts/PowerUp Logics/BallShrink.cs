using UnityEngine;

public class BallShrink : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.DecreaseBall();
		Debug.Log("Ball Shrink");
	}
}