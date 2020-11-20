using UnityEngine;

public class PaddleShrink : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.DecreasePaddleLength();
		Debug.Log("Paddle Shrink");
	}
}