using UnityEngine;

public class PaddleShrink : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.DecreasePaddleLength();
		Debug.Log("Paddle Shrink");
	}
}