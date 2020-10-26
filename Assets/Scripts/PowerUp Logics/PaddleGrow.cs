using UnityEngine;

public class PaddleGrow : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreasePaddleLength();
		Debug.Log("Paddle Grow!");
	}
}