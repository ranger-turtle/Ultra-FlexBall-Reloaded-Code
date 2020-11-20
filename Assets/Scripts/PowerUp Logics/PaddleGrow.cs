using UnityEngine;

public class PaddleGrow : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreasePaddleLength();
		Debug.Log("Paddle Grow!");
	}
}